namespace dotnet.nugit.Services.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;

    public class BuildRepositoryPackagesTask(
        Func<FindAndBuildProjectsTask> buildProjectTaskFactory,
        ILogger<BuildRepositoryPackagesTask> logger)
    {
        private readonly Func<FindAndBuildProjectsTask> buildProjectTaskFactory = buildProjectTaskFactory ?? throw new ArgumentNullException(nameof(buildProjectTaskFactory));
        private readonly ILogger<BuildRepositoryPackagesTask> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<RepositoryReference> BuildRepositoryPackagesAsync(RepositoryReference buildReference, LocalFeedInfo feed, IRepository repo, Reference? reference = null, Commit? commit = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(buildReference);
            ArgumentNullException.ThrowIfNull(feed);
            ArgumentNullException.ThrowIfNull(repo);

            DirectReference? directReference = reference?.ResolveToDirectReference();
            if (directReference != null && commit == null) commit = directReference.Target as Commit;

            if (commit == null)
            {
                this.logger.LogWarning("Cannot checkout tag reference; using head tip instead.");
                buildReference = buildReference.AsHeadReference();
                commit = repo.Head.Tip;
            }

            var forceCheckoutOptions = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };

            Commands.Checkout(repo, commit, forceCheckoutOptions);
            FindAndBuildProjectsTask buildTask = this.buildProjectTaskFactory();

            RepositoryReference qualifiedBuildReference = buildReference.AsQualifiedReference(directReference?.CanonicalName, commit?.Sha);
            await buildTask.FindAndBuildPackagesAsync(qualifiedBuildReference, feed, cancellationToken);

            return qualifiedBuildReference;
        }
    }
}