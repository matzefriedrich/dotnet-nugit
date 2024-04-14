namespace dotnet.nugit.Commands
{
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Resources;

    /// <summary>
    ///     Clones a remote repository, packages available projects and pushes package to the local feed.
    /// </summary>
    public class AddPackagesFromRepositoryCommand(
        INuGetFeedService nuGetFeedService,
        ILogger<AddPackagesFromRepositoryCommand> logger)
    {
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));

        public async Task<int> ProcessRepositoryAsync(string repositoryReference, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryReference)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryReference));

            LocalFeedInfo? feed = await this.nuGetFeedService.GetConfiguredLocalFeedAsync(cancellationToken);
            if (feed == null) return 1; // TODO: define exit code 
            
            RepositoryUri? repositoryUri = RepositoryUri.FromString(repositoryReference);
            if (repositoryUri == null) return 2;

            string cloneUrl = repositoryUri.CloneUrl();
            string localRepositoryPath = Path.Combine(feed.LocalPath, repositoryUri.RepositoryName);
            Repository.Clone(cloneUrl, localRepositoryPath);
            using IRepository repository = new Repository(localRepositoryPath);
            
            return await Task.FromResult(0);
        }
        
    }
}