namespace dotnet.nugit.Services
{
    using System;
    using System.IO.Abstractions;

    public sealed class TempDirectory : IDisposable
    {
        private readonly IFileSystem fileSystem;
        private readonly object syncObject = new();
        private bool disposed;

        public TempDirectory(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            string folderName = Guid.NewGuid().ToString()[..8];
            this.DirectoryPath = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), folderName);

            lock (this.syncObject)
            {
                if (this.fileSystem.Directory.Exists(this.DirectoryPath) == false) fileSystem.Directory.CreateDirectory(this.DirectoryPath);
            }
        }

        public string DirectoryPath { get; }

        public void Dispose()
        {
            lock (this.syncObject)
            {
                if (this.disposed) return;
                if (this.fileSystem.Directory.Exists(this.DirectoryPath)) this.fileSystem.Directory.Delete(this.DirectoryPath, true);
                this.disposed = true;
            }
        }
    }
}