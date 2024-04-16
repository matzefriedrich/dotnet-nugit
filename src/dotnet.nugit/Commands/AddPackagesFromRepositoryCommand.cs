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
        INugitWorkspace workspace,
        ILibGit2SharpAdapter git,
        ILogger<AddPackagesFromRepositoryCommand> logger)
    {
        private readonly IDotNetUtility dotNetUtility = dotNetUtility ?? throw new ArgumentNullException(nameof(dotNetUtility));
        private readonly IFindFilesService finder = finder ?? throw new ArgumentNullException(nameof(finder));
        private readonly ILibGit2SharpAdapter git = git ?? throw new ArgumentNullException(nameof(git));
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task<int> ProcessRepositoryAsync(string repositoryReference, bool headOnly, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryReference)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryReference));

            LocalFeedInfo? feed = await this.nuGetFeedService.GetConfiguredLocalFeedAsync(cancellationToken);
            if (feed == null) return ErrLocalFeedNotFound;

            RepositoryUri? repositoryUri;
            try
            {
                repositoryUri = RepositoryUri.FromString(repositoryReference);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, "Failed to parse repository reference.");
                return ErrInvalidRepositoryReference;
            }

            using IRepository? repository = this.OpenRepository(feed, repositoryUri, out string localRepositoryPath, TimeSpan.FromSeconds(60));
            if (repository == null) return ErrCannotOpen;

            Commit? restoreHeadTip = repository.Head.Tip;
            await this.FindAndBuildPackagesAsync(restoreHeadTip, localRepositoryPath, null, feed, cancellationToken);
            await this.workspace.AddRepositoryReferenceAsync(repositoryUri);

            if (headOnly) return Ok;

            var forceCheckoutOptions = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
            ReadOnlyCollection<Reference> tags = repository.Refs.Where(reference => reference.IsTag).ToList().AsReadOnly();
            foreach (Reference reference in tags)
            {
                DirectReference? directReference = reference.ResolveToDirectReference();
                string tagName = Path.GetFileName(directReference.CanonicalName);

                Commit? commit;
                if ((commit = directReference.Target as Commit) == null) continue;
                Commands.Checkout(repository, commit, forceCheckoutOptions);

                await this.FindAndBuildPackagesAsync(commit, localRepositoryPath, tagName, feed, cancellationToken);
            }

            return Ok;
        }

        private async Task FindAndBuildPackagesAsync(GitObject? commit, string localRepositoryPath, string? tageName, LocalFeedInfo feed, CancellationToken cancellationToken)
        {
            string? commitSha = commit?.Sha[..7];
            string versionSuffix = tageName ?? $"ref-{commitSha}";

            IAsyncEnumerable<string> projectFileFinder = this.CreateDotNetProjectFileFinder(localRepositoryPath, cancellationToken);
            List<string> projectFiles = await projectFileFinder.ToListAsync(cancellationToken);
            foreach (string file in projectFiles)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(30);

                this.logger.LogInformation("Building package for project: {ProjectFile}@{ReferenceName}", file, versionSuffix);
                await this.dotNetUtility.BuildAsync(file, timeout, cancellationToken);

                string packageTargetFolderPath = feed.PackagesPath();
                var packOptions = new PackOptions { VersionSuffix = versionSuffix };
                bool success = await this.dotNetUtility.TryPackAsync(file, packageTargetFolderPath, packOptions, timeout, cancellationToken);
                if (success == false) this.logger.LogWarning("Failed to create NuGet package for project: {ProjectFile}@{ReferenceName}", file, versionSuffix);
                if (this.workspace.TryReadConfiguration(out NugitConfigurationFile? config) && config is { CopyLocal: true })
                {
                    packageTargetFolderPath = Path.Combine(Environment.CurrentDirectory, "nupkg");
                    await this.dotNetUtility.TryPackAsync(file, packageTargetFolderPath, packOptions, timeout, cancellationToken);
                }
            }
        }

        private IRepository? OpenRepository(LocalFeedInfo feed, RepositoryUri repositoryUri, out string localRepositoryPath, TimeSpan? timeout = null, bool allowClone = true)
        {
            localRepositoryPath = null!;

            string repositoriesPath = feed.RepositoriesPath();
            string repositoryPath = Path.Combine(repositoriesPath, repositoryUri.RepositoryName.TrimStart('/'));

            string hiddenGitFolderPath = Path.Combine(repositoryPath, ".git");
            if (Directory.Exists(hiddenGitFolderPath) == false)
            {
                if (!allowClone) throw new InvalidOperationException("The requested repository does not exist.");
                string cloneUrl = repositoryUri.CloneUrl();

                this.logger.LogInformation("Cloning the repository: {RepositoryUrl} into: {LocalRepositoryPath}.", cloneUrl, localRepositoryPath);
                using var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout ?? TimeSpan.FromMilliseconds(60));

                if (this.git.TryCloneRepository(cloneUrl, repositoryPath, cancellationTokenSource.Token) == false)
                    return null;
            }

            try
            {
                localRepositoryPath = repositoryPath;
                this.logger.LogInformation("Opening Git repository.");
                return new Repository(repositoryPath);
            }
            catch (RepositoryNotFoundException e)
            {
                this.logger.LogError(e, "Cannot open repository.");
                return null;
            }
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