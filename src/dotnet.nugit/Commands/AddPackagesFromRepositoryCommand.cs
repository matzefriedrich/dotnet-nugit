namespace dotnet.nugit.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Resources;
    using Services.Tasks;
    using static ExitCodes;

    /// <summary>
    ///     Clones a remote repository, packages available projects and pushes package to the local feed.
    /// </summary>
    public class AddPackagesFromRepositoryCommand(
        INuGetFeedConfigurationService nuGetFeedConfigurationService,
        INugitWorkspace workspace,
        Func<IOpenRepositoryTask> openRepositoryTaskFactory,
        Func<IBuildRepositoryPackagesTask> buildPackagesTaskFactory,
        ILogger<AddPackagesFromRepositoryCommand> logger)
    {
        private readonly Func<IBuildRepositoryPackagesTask> buildPackagesTaskFactory = buildPackagesTaskFactory ?? throw new ArgumentNullException(nameof(buildPackagesTaskFactory));
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedConfigurationService nuGetFeedConfigurationService = nuGetFeedConfigurationService ?? throw new ArgumentNullException(nameof(nuGetFeedConfigurationService));
        private readonly Func<IOpenRepositoryTask> openRepositoryTaskFactory = openRepositoryTaskFactory ?? throw new ArgumentNullException(nameof(openRepositoryTaskFactory));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task<int> ProcessRepositoryAsync(string repositoryReferenceString, bool headOnly, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(repositoryReferenceString)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryReferenceString));

            LocalFeedInfo? feed = await this.nuGetFeedConfigurationService.GetConfiguredLocalFeedAsync(cancellationToken);
            if (feed == null) return ErrLocalFeedNotFound;

            RepositoryUri? repositoryUri;
            try
            {
                repositoryUri = RepositoryUri.FromString(repositoryReferenceString);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, "Failed to parse repository reference.");
                return ErrInvalidRepositoryReference;
            }

            IOpenRepositoryTask openRepositoryTask = this.openRepositoryTaskFactory();
            using IRepository? repository = openRepositoryTask.OpenRepository(feed, repositoryUri, TimeSpan.FromSeconds(60));
            if (repository == null) return ErrCannotOpen;

            IBuildRepositoryPackagesTask buildRepositoryPackagesTask = this.buildPackagesTaskFactory();

            RepositoryReference headReference = repositoryUri.AsReference();
            await buildRepositoryPackagesTask.BuildRepositoryPackagesAsync(headReference, feed, repository, null, null, cancellationToken);
            await this.workspace.AddRepositoryReferenceAsync(repositoryUri);

            if (headOnly) return Ok;

            ReadOnlyCollection<Reference> tags = repository.Refs.Where(reference => reference.IsTag).ToList().AsReadOnly();
            foreach (Reference reference in tags)
            {
                await buildRepositoryPackagesTask.BuildRepositoryPackagesAsync(headReference, feed, repository, reference, null, cancellationToken);
            }

            return Ok;
        }
    }
}