namespace dotnet.nugit.Commands
{
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
        private readonly IFindFilesService finder = finder ?? throw new ArgumentNullException(nameof(finder));
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
        private readonly IDotNetUtility dotNetUtility = dotNetUtility ?? throw new ArgumentNullException(nameof(dotNetUtility));

        public async Task<int> ProcessRepositoryAsync(string repositoryReference, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryReference)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryReference));

            LocalFeedInfo? feed = await this.nuGetFeedService.GetConfiguredLocalFeedAsync(cancellationToken);
            if (feed == null) return ErrLocalFeedNotFound;

            RepositoryUri? repositoryUri = RepositoryUri.FromString(repositoryReference);
            if (repositoryUri == null) return ErrInvalidRepositoryReference;

            using IRepository repository = OpenRepository(feed, repositoryUri, out string localRepositoryPath);
            
            IAsyncEnumerable<string> projectFileFinder = this.CreateDotNetProjectFileFinder(localRepositoryPath, cancellationToken);
            List<string> projectFiles = await projectFileFinder.ToListAsync(cancellationToken);
            foreach (string file in projectFiles)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(30);
                await this.dotNetUtility.PackAsync(file, feed, timeout, cancellationToken);
                Console.WriteLine(file);                
            }

            return await Task.FromResult(Ok);
        }

        private static IRepository OpenRepository(LocalFeedInfo feed, RepositoryUri repositoryUri, out string localRepositoryPath, bool allowClone = true)
        {
            localRepositoryPath = Path.Combine(feed.RepositoriesPath(), repositoryUri.RepositoryName);
            string hiddenGitFolderPath = Path.Combine(localRepositoryPath, ".git");
            if (Directory.Exists(hiddenGitFolderPath) == false)
            {
                if (!allowClone) throw new InvalidOperationException("The requested repository does not exist.");
                string cloneUrl = repositoryUri.CloneUrl();
                Repository.Clone(cloneUrl, localRepositoryPath, new CloneOptions { RecurseSubmodules = true });

                throw new InvalidOperationException("The requested repository does not exist.");
            }
            
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