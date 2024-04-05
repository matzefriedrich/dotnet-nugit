namespace dotnet.nugit.Abstractions
{
    public interface INuGetFeedService
    {
        Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync();
        
        Task<LocalFeedInfo> CreateLocalFeedIfNotExistsAsync(string name, CancellationToken cancellationToken);
    }
}