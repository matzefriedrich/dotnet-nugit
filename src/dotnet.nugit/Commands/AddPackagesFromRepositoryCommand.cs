namespace dotnet.nugit.Commands
{
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Resources;

    /// <summary>
    ///     Clones a remote repository, packages available projects and pushes package to the local feed.
    /// </summary>
    public class AddPackagesFromRepositoryCommand
    {
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger;
        private readonly INuGetFeedService nuGetFeedService;

        public AddPackagesFromRepositoryCommand(
            INuGetFeedService nuGetFeedService,
            ILogger<AddPackagesFromRepositoryCommand> logger)
        {
            this.nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> ProcessRepositoryAsync(string repositoryUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryUrl));

            LocalFeedInfo? feed = await this.nuGetFeedService.GetConfiguredLocalFeedAsync();
            if (feed == null) return 1; // TODO: define exit code 

            // TODO: analyze repository URL (remove path, and reference)
            Repository.Clone(repositoryUrl, feed.LocalPath);
            using var repository = new Repository(feed.LocalPath);
            
            return await Task.FromResult(0);
        }
    }
}