namespace dotnet.nugit.Abstractions
{
    public interface INuGetFeedService
    {
        Task<LocalFeedInfo?> GetConfiguredLocalFeedAsync();
        
        Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync();

        Task<LocalFeedInfo> CreateLocalFeedIfNotExistsAsync(CancellationToken cancellationToken);
    }
}