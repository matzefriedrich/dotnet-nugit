namespace dotnet.nugit.Commands
{
    using System;
    using System.Collections.Generic;
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
        Func<OpenRepositoryTask> openRepositoryTaskFactory,
        Func<FindAndBuildProjectsTask> buildProjectsTaskFactory,
        ILogger<RestorePackagesCommand> logger)
    {
        private readonly Func<FindAndBuildProjectsTask> buildProjectsTaskFactory = buildProjectsTaskFactory ?? throw new ArgumentNullException(nameof(buildProjectsTaskFactory));
        private readonly ILogger<RestorePackagesCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly Func<OpenRepositoryTask> openRepositoryTaskFactory = openRepositoryTaskFactory ?? throw new ArgumentNullException(nameof(openRepositoryTaskFactory));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task<int> RestoreWorkspacePackagesAsync(bool forceReinstall, CancellationToken cancellationToken)
        {
            LocalFeedInfo? feed = this.workspace.GetConfiguredLocalFeed();
            if (feed == null)
                return ErrLocalFeedNotFound;

            IEnumerable<RepositoryReference> repositories = this.workspace.GetWorkspaceRepositories();
            foreach (RepositoryReference repositoryReference in repositories)
            {
                RepositoryReference buildReference = repositoryReference;

                RepositoryUri uri = buildReference.AsRepositoryUri();
                OpenRepositoryTask openRepositoryTask = this.openRepositoryTaskFactory();
                using IRepository? repo = openRepositoryTask.OpenRepository(feed, uri, allowClone: true);
                if (repo == null)
                {
                    this.logger.LogError("Cannot open repository: {RepositoryUrl}", uri.CloneUrl());
                    continue;
                }

                string? repositoryReferenceTag = buildReference.Tag;
                if (string.IsNullOrWhiteSpace(repositoryReferenceTag) == false)
                {
                    Reference reference = repo.Refs[repositoryReferenceTag];
                    DirectReference? directReference = reference?.ResolveToDirectReference();
                    Commit? commit;
                    if ((commit = directReference?.Target as Commit) == null)
                    {
                        this.logger.LogWarning("Cannot checkout tag reference: {TagName}. Using head tip instead.", repositoryReferenceTag);
                        buildReference = buildReference.AsHeadReference();
                        await this.workspace.UpdateRepositoryReferenceAsync(repositoryReference, buildReference);
                        continue;
                    }

                    Commands.Checkout(repo, commit);
                }

                FindAndBuildProjectsTask buildTask = this.buildProjectsTaskFactory();
                await buildTask.FindAndBuildPackagesAsync(buildReference, feed, cancellationToken);
            }

            return await Task.FromResult(Ok);
        }
    }
}