namespace dotnet.nugit.Abstractions
{
    public class RepositoryReference : IEquatable<RepositoryReference>
    {
        public string RepositoryType { get; init; } = "git";
        public string? RepositoryUrl { get; init; }
        public string? Hash { get; init; } = null;
        public string RepositoryPath { get; init; } = "/";
        public string? Tag { get; init; } = null;

        public bool Equals(RepositoryReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.RepositoryType == other.RepositoryType && this.RepositoryUrl == other.RepositoryUrl;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RepositoryReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.RepositoryType, this.RepositoryUrl);
        }
    }
}