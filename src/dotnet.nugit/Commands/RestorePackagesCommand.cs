namespace dotnet.nugit.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Services;
    using Services.Tasks;
    using static ExitCodes;

    /// <summary>
    ///     Reads a workspace file from the current working directory (or above) and tries to restore all packages from
    ///     referenced repositories.
    /// </summary>
    public class RestorePackagesCommand(
        INugitWorkspace workspace,
        Func<IOpenRepositoryTask> openRepositoryTaskFactory,
        Func<IBuildRepositoryPackagesTask> buildPackagesTaskFactory,
        ILogger<RestorePackagesCommand> logger)
    {
        private readonly Func<IBuildRepositoryPackagesTask> buildPackagesTaskFactory = buildPackagesTaskFactory ?? throw new ArgumentNullException(nameof(buildPackagesTaskFactory));
        private readonly ILogger<RestorePackagesCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly Func<IOpenRepositoryTask> openRepositoryTaskFactory = openRepositoryTaskFactory ?? throw new ArgumentNullException(nameof(openRepositoryTaskFactory));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task<int> RestoreWorkspacePackagesAsync(bool forceReinstall, CancellationToken cancellationToken)
        {
            LocalFeedInfo? feed = this.workspace.GetConfiguredLocalFeed();
            if (feed == null)
                return ErrLocalFeedNotFound;

            IEnumerable<RepositoryReference> repositories = this.workspace.GetWorkspaceRepositories();
            foreach (RepositoryReference repositoryReference in repositories)
            {
                RepositoryUri uri = repositoryReference.AsRepositoryUri();
                IOpenRepositoryTask openRepositoryTask = this.openRepositoryTaskFactory();
                using IRepository? repo = openRepositoryTask.OpenRepository(feed, uri, allowClone: true);
                if (repo == null)
                {
                    this.logger.LogError("Cannot open repository: {RepositoryUrl}", uri.CloneUrl());
                    continue;
                }

                IBuildRepositoryPackagesTask buildPackagesTask = this.buildPackagesTaskFactory();

                string? repositoryReferenceTag = repositoryReference.Tag;
                if (string.IsNullOrWhiteSpace(repositoryReferenceTag) == false)
                {
                    Reference reference = repo.Refs[repositoryReferenceTag];
                    await buildPackagesTask.BuildRepositoryPackagesAsync(repositoryReference, feed, repo, reference, null, cancellationToken);
                }
                else
                {
                    await buildPackagesTask.BuildRepositoryPackagesAsync(repositoryReference, feed, repo, null, null, cancellationToken);

                    List<Reference> tagReferences = repo.Refs.Where(reference => reference.IsTag).ToList();
                    foreach (Reference reference in tagReferences)
                    {
                        await buildPackagesTask.BuildRepositoryPackagesAsync(repositoryReference, feed, repo, reference, null, cancellationToken);
                    }
                }
            }

            return await Task.FromResult(Ok);
        }
    }
}