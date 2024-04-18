namespace dotnet.nugit.Services.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Threading;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using nugit.Abstractions;

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class OpenRepositoryTask(
        ILibGit2SharpAdapter git,
        IFileSystem fileSystem,
        ILogger<OpenRepositoryTask> logger)
    {
        private readonly ILibGit2SharpAdapter git = git ?? throw new ArgumentNullException(nameof(git));
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        private readonly ILogger<OpenRepositoryTask> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public IRepository? OpenRepository(LocalFeedInfo feed, RepositoryUri repositoryUri, TimeSpan? timeout = null, bool allowClone = true)
        {
            string projectFolderPath = feed.ProjectDirectoryPathFor(repositoryUri);
            string hiddenGitFolderPath = this.fileSystem.Path.Combine(projectFolderPath, ".git");
            if (this.fileSystem.Directory.Exists(hiddenGitFolderPath) == false)
            {
                if (!allowClone) throw new InvalidOperationException("The requested repository does not exist.");
                string cloneUrl = repositoryUri.CloneUrl();

                this.logger.LogInformation("Cloning the repository: {RepositoryUrl} into: {LocalRepositoryPath}.", cloneUrl, projectFolderPath);
                using var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout ?? TimeSpan.FromMilliseconds(60));

                if (this.git.TryCloneRepository(cloneUrl, projectFolderPath, cancellationTokenSource.Token) == false)
                    return null;
            }

            try
            {
                this.logger.LogInformation("Opening Git repository.");
                return new Repository(projectFolderPath);
            }
            catch (RepositoryNotFoundException e)
            {
                this.logger.LogError(e, "Cannot open repository.");
                return null;
            }
        }
    }
}