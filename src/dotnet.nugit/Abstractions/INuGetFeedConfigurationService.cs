namespace dotnet.nugit.Abstractions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides functionality to manage the required local package source.
    /// </summary>
    public interface INuGetFeedConfigurationService
    {
        Task<LocalFeedInfo?> GetConfiguredLocalFeedAsync(CancellationToken cancellationToken);

        Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync(CancellationToken cancellationToken);

        Task<LocalFeedInfo?> CreateLocalFeedIfNotExistsAsync(CancellationToken cancellationToken);
    }
}