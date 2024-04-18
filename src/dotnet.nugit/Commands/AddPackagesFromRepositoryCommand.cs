namespace dotnet.nugit.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
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
        Func<OpenRepositoryTask> openRepositoryTaskFactory,
        Func<FindAndBuildProjectsTask> projectBuilderTaskFactory,
        ILogger<AddPackagesFromRepositoryCommand> logger)
    {
        private readonly ILogger<AddPackagesFromRepositoryCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INuGetFeedConfigurationService nuGetFeedConfigurationService = nuGetFeedConfigurationService ?? throw new ArgumentNullException(nameof(nuGetFeedConfigurationService));
        private readonly Func<OpenRepositoryTask> openRepositoryTaskFactory = openRepositoryTaskFactory ?? throw new ArgumentNullException(nameof(openRepositoryTaskFactory));
        private readonly Func<FindAndBuildProjectsTask> projectBuilderTaskFactory = projectBuilderTaskFactory ?? throw new ArgumentNullException(nameof(projectBuilderTaskFactory));
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

            OpenRepositoryTask openRepositoryTask = this.openRepositoryTaskFactory();
            using IRepository? repository = openRepositoryTask.OpenRepository(feed, repositoryUri, TimeSpan.FromSeconds(60));
            if (repository == null) return ErrCannotOpen;

            FindAndBuildProjectsTask builderTask = this.projectBuilderTaskFactory();

            Commit? restoreHeadTip = repository.Head.Tip;
            RepositoryReference headReference = repositoryUri.AsReference(null, restoreHeadTip.Sha);
            await builderTask.FindAndBuildPackagesAsync(headReference, feed, cancellationToken);
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

                RepositoryReference repositoryReference = repositoryUri.AsReference(tagName, commit.Sha);
                await builderTask.FindAndBuildPackagesAsync(repositoryReference, feed, cancellationToken);
            }

            return Ok;
        }
    }
}