namespace dotnet.nugit.Abstractions
{
    public interface INuGetFeedService
    {
        Task<LocalFeedInfo?> GetConfiguredLocalFeedAsync(CancellationToken cancellationToken);
        
        Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync(CancellationToken cancellationToken);

        Task<LocalFeedInfo> CreateLocalFeedIfNotExistsAsync(CancellationToken cancellationToken);
    }
}