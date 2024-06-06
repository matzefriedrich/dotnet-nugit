namespace dotnet.nugit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using Resources;
    using Workspace;

    internal sealed class DotNetUtility(
        IFileSystem fileSystem,
        ILogger<DotNetUtility> logger) : IDotNetUtility
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        private readonly ILogger<DotNetUtility> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task BuildAsync(IDotNetProject project, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);

            string? projectFile = project.GetProjectFilePath();
            string? workingDirectoryPath = this.fileSystem.Path.GetDirectoryName(projectFile);

            var arguments = $"build \"{projectFile}\" --configuration Release";
            await this.RunDotNetProcessAsync(arguments, workingDirectoryPath, timeout, cancellationToken);
        }

        public async Task<bool> TryPackAsync(IDotNetProject project, string packageTargetFolderPath, PackOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);
            if (string.IsNullOrWhiteSpace(packageTargetFolderPath)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(packageTargetFolderPath));

            string? projectFile = project.GetProjectFilePath();
            string? workingDirectoryPath = Path.GetDirectoryName(projectFile);

            IProjectPackageMetadata packageSpec = project.DerivePackageSpec();
            packageSpec.AddDefaultValues(ProjectPackageMetadata.Authors, $"{Environment.UserName}@{Environment.MachineName}");
            if (packageSpec.Validate(out ICollection<ValidationResult> validationResults) == false)
            {
                var errors = new StringBuilder();
                ImmutableList<string?> errorMessages = validationResults.Select(result => result.ErrorMessage).Where(s => string.IsNullOrWhiteSpace(s) == false).ToImmutableList();
                errors.AppendJoin(Environment.NewLine, errorMessages);
                this.logger.LogError("Failed to derive valid NuGet package specification.");
                this.logger.LogError(errors.ToString());
                return false;
            }
            
            string? nuspecFilePath = project.GetNuspecFilePath();
            if (string.IsNullOrWhiteSpace(nuspecFilePath))
                return false;

            await this.WriteNuspecFile(packageSpec, nuspecFilePath, cancellationToken);

            string? nuspecBasePath = project.GetNuspecBasePath();

            // dotnet pack ~/projects/app1/project.csproj -p:NuspecFile=~/projects/app1/project.nuspec -p:NuspecBasePath=~/projects/app1/nuget
            var arguments = $"pack \"{projectFile}\" -p:NuspecFile=\"{nuspecFilePath}\" -p:NuspecBasePath=\"{nuspecBasePath}\" --output \"{packageTargetFolderPath}\" --version-suffix \"{options.VersionSuffix}\"";
            int exitCode = await this.RunDotNetProcessAsync(arguments, workingDirectoryPath, timeout, cancellationToken);

            return exitCode == 0;
        }

        private async Task WriteNuspecFile(IProjectPackageMetadata packageSpec, string nuspecFilePath, CancellationToken cancellationToken)
        {
            await using Stream stream = this.fileSystem.File.Open(nuspecFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            stream.SetLength(0);
            await packageSpec.WriteNuspecFileAsync(stream, cancellationToken);
        }

        private async Task<int> RunDotNetProcessAsync(string arguments, string? workingDirectory, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));

            var p = new ProcessWrapper(this.logger, "dotnet");
            return await p.StartAndWaitForExitAsync(arguments, workingDirectory, tokenSource.Token);
        }

        private sealed class ProcessWrapper
        {
            private readonly string fileName;
            private readonly ILogger logger;

            public ProcessWrapper(ILogger logger, string fileName)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(fileName));

                this.fileName = fileName;
            }

            public async Task<int> StartAndWaitForExitAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(arguments));

                var processStartInfo = new ProcessStartInfo(this.fileName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using Process? p = Process.Start(processStartInfo);

                await (p?.WaitForExitAsync(cancellationToken) ?? Task.CompletedTask);
                await this.TraceStandardOutputAsync(p, cancellationToken);
                await this.TraceStandardErrorAsync(p, cancellationToken);

                return p?.ExitCode ?? 0;
            }

            private async Task TraceStandardOutputAsync(Process? p, CancellationToken cancellationToken)
            {
                if (p == null) return;
                string output = await p.StandardOutput.ReadToEndAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(output)) return;
                this.logger.LogDebug(output);
            }

            private async Task TraceStandardErrorAsync(Process? p, CancellationToken cancellationToken)
            {
                if (p == null) return;

                int exitCode = p.ExitCode;
                this.logger.LogInformation($"The {this.fileName} command completed with exit code: {{exitCode}}.", exitCode);
                if (exitCode == 0) return;

                string error = await p.StandardError.ReadToEndAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(error) == false)
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"The {this.fileName} command completed with errors: ");
                    builder.Append($"\t{error}");
                    this.logger.LogError(builder.ToString());
                }
            }
        }
    }
}