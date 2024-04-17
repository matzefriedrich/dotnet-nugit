namespace dotnet.nugit.Abstractions
{
    public class LocalFeedInfo : IEquatable<LocalFeedInfo>
    {
        // ReSharper disable once EmptyConstructor; the empty ctor is required by the Yaml deserializer
        public LocalFeedInfo()
        {
        }

        public required string Name { get; init; }
        public required string LocalPath { get; init; }

        public bool Equals(LocalFeedInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Name == other.Name && this.LocalPath == other.LocalPath;
        }

        public string PackagesPath()
        {
            return Path.Combine(this.LocalPath, "packages");
        }

        public string RepositoriesPath()
        {
            return Path.Combine(this.LocalPath, "repositories");
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((LocalFeedInfo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name, this.LocalPath);
        }
    }
}