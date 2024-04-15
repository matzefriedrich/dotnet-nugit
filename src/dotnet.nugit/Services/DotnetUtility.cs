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

        public async Task PackAsync(string projectFile, LocalFeedInfo target, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectFile));

            string packageTargetFolderPath = target.PackagesPath();

            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));

            string workingDirectoryPath = Path.GetDirectoryName(projectFile);

            var arguments = $"pack \"{projectFile}\" --output \"{packageTargetFolderPath}\"";
            var processStartInfo = new ProcessStartInfo("dotnet", arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectoryPath,
                RedirectStandardError = true
            };

            using Process? p = Process.Start(processStartInfo);

            await (p?.WaitForExitAsync(tokenSource.Token) ?? Task.CompletedTask);

            if (p.ExitCode != 0)
            {
                string error = await p.StandardError.ReadToEndAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(error) == false)
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("The dotnet pack command completed with errors: ");
                    builder.Append($"\t{error}");
                    this.logger.LogError(builder.ToString());
                }
            }

            this.logger.LogInformation("The dotnet pack command completed with exit code: {exitCode}.", p.ExitCode);
        }
    }
}