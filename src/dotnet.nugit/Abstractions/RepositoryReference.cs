namespace dotnet.nugit.Abstractions
{
    using System;

    public class RepositoryReference : IEquatable<RepositoryReference>
    {
        public string RepositoryType { get; init; } = RepositoryTypes.Git;
        public string? RepositoryUrl { get; init; }
        public string? Hash { get; init; }
        public string RepositoryPath { get; init; } = "/";
        public string? Tag { get; init; }

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

        public RepositoryUri AsRepositoryUri()
        {
            string? repositoryUrl = this.RepositoryUrl;
            if (repositoryUrl != null) return RepositoryUri.FromString(repositoryUrl);
            throw new InvalidOperationException("Invalid repository URL.");
        }

        public RepositoryReference AsHeadReference()
        {
            return new RepositoryReference
            {
                RepositoryType = this.RepositoryType,
                RepositoryUrl = this.RepositoryUrl
            };
        }

        public RepositoryReference AsQualifiedReference(string? tagName, string? hash = null)
        {
            return new RepositoryReference
            {
                RepositoryType = this.RepositoryType,
                RepositoryUrl = this.RepositoryUrl,
                Tag = tagName,
                Hash = hash
            };
        }
    }
}