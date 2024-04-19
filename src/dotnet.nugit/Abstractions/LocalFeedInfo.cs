namespace dotnet.nugit.Abstractions
{
    using System;
    using System.IO;

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

        /// <summary>
        ///     Gets a value indicating the local packages root folder path.
        /// </summary>
        public string PackagesRootPath()
        {
            return Path.Combine(this.LocalPath, "packages");
        }

        /// <summary>
        ///     Gets a value indicating the local repositories root folder path.
        /// </summary>
        public string RepositoriesRootPath()
        {
            return Path.Combine(this.LocalPath, "repositories");
        }

        /// <summary>
        ///     Gets a value indicating the local project folder path for the specified repository.
        /// </summary>
        /// <param name="repositoryUri">A <see cref="RepositoryUri" /> value representing the repository.</param>
        public string ProjectDirectoryPathFor(RepositoryUri repositoryUri)
        {
            string repositoriesRootPath = this.RepositoriesRootPath();
            string sanitizedRepositoryPathString = repositoryUri.RepositoryName.TrimStart('/');
            return Path.Combine(repositoriesRootPath, sanitizedRepositoryPathString).SanitizedPathString();
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