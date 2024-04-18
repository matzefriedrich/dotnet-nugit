namespace dotnet.nugit.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NuGet.Packaging;

    public class NugitConfigurationFile
    {
        private readonly object syncObject = new();

        public LocalFeedInfo? LocalFeed { get; init; }
        public IList<RepositoryReference> Repositories { get; init; } = new List<RepositoryReference>();

        /// <summary>
        ///     Gets or sets a value indicating whether build packages shall be copied to the local workspace.
        /// </summary>
        public bool CopyLocal { get; set; }

        public void AddRepository(RepositoryUri repositoryUri)
        {
            ArgumentNullException.ThrowIfNull(repositoryUri);

            lock (this.syncObject)
            {
                RepositoryReference reference = repositoryUri.AsReference();
                HashSet<RepositoryReference> hashSet = this.Repositories.ToHashSet();
                hashSet.Add(reference);

                this.Repositories.Clear();
                this.Repositories.AddRange(hashSet);
            }
        }
    }
}