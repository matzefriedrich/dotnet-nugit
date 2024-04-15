namespace dotnet.nugit.Abstractions
{
    using Resources;

    public class LocalFeedInfo
    {
        public LocalFeedInfo()
        {
        }
        
        public required string Name { get; init; }
        public required string LocalPath { get; init; }

        public string PackagesPath()
        {
            return Path.Combine(this.LocalPath, "packages");
        }

        public string RepositoriesPath()
        {
            return Path.Combine(this.LocalPath, "repositories");
        }
    }
}