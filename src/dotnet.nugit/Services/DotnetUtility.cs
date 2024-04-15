namespace dotnet.nugit.Services
{
    using System.Diagnostics;
    using System.Text;
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using Resources;

    internal sealed class DotNetUtility(ILogger<DotNetUtility> logger) : IDotNetUtility
    {
        private readonly ILogger<DotNetUtility> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task BuildAsync(string projectFile, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectFile));

            string? workingDirectoryPath = Path.GetDirectoryName(projectFile);

            var arguments = $"build \"{projectFile}\" --configuration Release";
            await this.RunDotNetProcessAsync(arguments, workingDirectoryPath, timeout, cancellationToken);
        }

        public async Task PackAsync(string projectFile, LocalFeedInfo target, PackOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectFile));

            string packageTargetFolderPath = target.PackagesPath();

            string? workingDirectoryPath = Path.GetDirectoryName(projectFile);

            // TODO: load the project, reflect all NuGet-specific properties
            // Emit a (temporary) .nuspec file and build the package using: dotnet pack ~/projects/app1/project.csproj -p:NuspecFile=~/projects/app1/project.nuspec -p:NuspecBasePath=~/projects/app1/nuget
            
            var arguments = $"pack \"{projectFile}\" --output \"{packageTargetFolderPath}\" --version-suffix \"{options.VersionSuffix}\"";
            await this.RunDotNetProcessAsync(arguments, workingDirectoryPath, timeout, cancellationToken);
        }

        private async Task RunDotNetProcessAsync(string arguments, string? workingDirectory, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));

            var p = new ProcessWrapper(this.logger, "dotnet");
            await p.StartAndWaitForExitAsync(arguments, workingDirectory, tokenSource.Token);
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

            public async Task StartAndWaitForExitAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
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
            }

            private async Task TraceStandardOutputAsync(Process? p, CancellationToken cancellationToken)
            {
                if (p == null) return;
                string output = await p.StandardOutput.ReadToEndAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(output)) return;
                this.logger.LogInformation(output);
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