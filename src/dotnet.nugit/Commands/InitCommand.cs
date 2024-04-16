namespace dotnet.nugit.Commands
{
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using static ExitCodes;

    public sealed class InitCommand(
        INuGetFeedService nuGetFeedService,
        INugitWorkspace workspace,
        ILogger<InitCommand> logger)
    {
        private readonly ILogger<InitCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task<int> InitializeNewRepositoryAsync(bool copyLocal)
        {
            LocalFeedInfo? feed = await this.nuGetFeedService.CreateLocalFeedIfNotExistsAsync(CancellationToken.None);
            await this.CreateNugitRepositoryFileIfNotExistsAsync(feed);

            if (feed == null) return ErrCannotCreateFeed;

            this.logger.LogDebug("Feed: {0}, Path: {1}", feed.Name, feed.LocalPath);
            
            return Ok;
        }

        private async Task CreateNugitRepositoryFileIfNotExistsAsync(LocalFeedInfo? feed, bool copyLocal = false)
        {
            await this.workspace.CreateOrUpdateConfigurationAsync(CreateNewConfigurationFile,UpdateConfigurationFile);

            return;

            NugitConfigurationFile UpdateConfigurationFile(NugitConfigurationFile current)
            {
                current.CopyLocal = copyLocal;
                return current;
            }

            NugitConfigurationFile CreateNewConfigurationFile()
            {
                return new NugitConfigurationFile
                {
                    LocalFeed = feed,
                    CopyLocal = copyLocal
                };
            }
        }
    }
}