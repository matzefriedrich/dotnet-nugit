namespace dotnet.nugit.Commands
{
    using System.Collections.ObjectModel;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Resources;
    using static ExitCodes;

    /// <summary>
    ///     Clones a remote repository, packages available projects and pushes package to the local feed.
    /// </summary>
    public class AddPackagesFromRepositoryCommand(
        INuGetFeedService nuGetFeedService,
        IFindFilesService finder,
        IDotNetUtility dotNetUtility,
        ILogger<AddPackagesFromRepositoryCommand> logger)
    {
        private readonly IDotNetUtility dotNetUtility = dotNetUtility ?? throw new ArgumentNullException(nameof(dotNetUtility));
        private readonly IFindFilesService finder = finder ?? throw new ArgumentNullException(nameof(finder));
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));

        public async Task<int> ProcessRepositoryAsync(string repositoryReference, bool headOnly, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryReference)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryReference));

            LocalFeedInfo? feed = await this.nuGetFeedService.GetConfiguredLocalFeedAsync(cancellationToken);
            if (feed == null) return ErrLocalFeedNotFound;

            RepositoryUri? repositoryUri = RepositoryUri.FromString(repositoryReference);
            if (repositoryUri == null) return ErrInvalidRepositoryReference;

            using IRepository repository = this.OpenRepository(feed, repositoryUri, out string localRepositoryPath, TimeSpan.FromSeconds(60));
            
            Commit? restoreHeadTip = repository.Head.Tip;
            await this.FindAndBuildPackagesAsync(restoreHeadTip, localRepositoryPath, feed, cancellationToken);
            
            if (headOnly) return Ok;

            var forceCheckoutOptions = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
            ReadOnlyCollection<DirectReference> tags = repository.Refs.Where(reference => reference.IsTag).Select(reference => reference.ResolveToDirectReference()).ToList().AsReadOnly();
            foreach (DirectReference tag in tags)
            {
                Commit? commit;
                if ((commit = tag.Target as Commit) == null) continue;
                Commands.Checkout(repository, commit, forceCheckoutOptions);

                await this.FindAndBuildPackagesAsync(commit, localRepositoryPath, feed, cancellationToken);
            }

            return Ok;
        }

        private async Task FindAndBuildPackagesAsync(GitObject? commit, string localRepositoryPath, LocalFeedInfo feed, CancellationToken cancellationToken)
        {
            string? commitSha = commit?.Sha[..7];
            var versionSuffix = $"ref-{commitSha}";
            
            IAsyncEnumerable<string> projectFileFinder = this.CreateDotNetProjectFileFinder(localRepositoryPath, cancellationToken);
            List<string> projectFiles = await projectFileFinder.ToListAsync(cancellationToken);
            foreach (string file in projectFiles)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(30);

                await this.dotNetUtility.BuildAsync(file, timeout, cancellationToken);

                var packOptions = new PackOptions { VersionSuffix = versionSuffix };
                await this.dotNetUtility.PackAsync(file, feed, packOptions, timeout, cancellationToken);

                Console.WriteLine(file);
            }
        }

        private IRepository OpenRepository(LocalFeedInfo feed, RepositoryUri repositoryUri, out string localRepositoryPath, TimeSpan? timeout = null, bool allowClone = true)
        {
            localRepositoryPath = Path.Combine(feed.RepositoriesPath(), repositoryUri.RepositoryName);
            string hiddenGitFolderPath = Path.Combine(localRepositoryPath, ".git");
            if (Directory.Exists(hiddenGitFolderPath) == false)
            {
                if (!allowClone) throw new InvalidOperationException("The requested repository does not exist.");
                string cloneUrl = repositoryUri.CloneUrl();

                using var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout ?? TimeSpan.FromMilliseconds(60));

                using var mre = new ManualResetEventSlim();

                void RepositoryOperationCompleted(RepositoryOperationContext context)
                {
                    this.logger.LogInformation("Completed operating on the repository.");
                    mre.Set();
                }

                var cloneOptions = new CloneOptions
                {
                    RecurseSubmodules = true,
                    FetchOptions =
                    {
                        RepositoryOperationCompleted = RepositoryOperationCompleted
                    }
                };

                this.logger.LogInformation("Cloning the repository: {RepositoryUrl} into: {LocalRepositoryPath}.", cloneUrl, localRepositoryPath);
                Repository.Clone(cloneUrl, localRepositoryPath, cloneOptions);
                mre.Wait(cancellationTokenSource.Token);
            }

            this.logger.LogInformation("Opening Git repository.");
            return new Repository(localRepositoryPath);
        }

        private IAsyncEnumerable<string> CreateDotNetProjectFileFinder(string localRepositoryPath, CancellationToken cancellationToken)
        {
            const string csproj = ".csproj";
            const string vbproj = ".vbproj";

            return this.finder.FindAsync(localRepositoryPath, "*.*", async entry =>
            {
                if (entry.IsDirectory) return await Task.FromResult(true);
                string extension = Path.GetExtension(entry.Path);
                return extension switch
                {
                    csproj => true,
                    vbproj => true,
                    _ => false
                };
            }, cancellationToken);
        }
    }
}