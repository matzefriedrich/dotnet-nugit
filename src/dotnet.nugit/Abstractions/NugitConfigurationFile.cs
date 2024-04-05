namespace dotnet.nugit.Abstractions
{
    public class NugitConfigurationFile
    {
        public LocalFeedInfo? LocalFeed { get; init; }
        public IList<RepositoryReference> Repositories { get; } = new List<RepositoryReference>();
    }
}