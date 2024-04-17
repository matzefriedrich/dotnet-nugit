namespace dotnet.nugit.Services
{
    public sealed class TempDirectory : IDisposable
    {
        private readonly object syncObject = new();
        private bool disposed;

        public TempDirectory()
        {
            string folderName = Guid.NewGuid().ToString()[..8];
            this.DirectoryPath = Path.Combine(Path.GetTempPath(), folderName);

            lock (this.syncObject)
            {
                if (Directory.Exists(this.DirectoryPath) == false) Directory.CreateDirectory(this.DirectoryPath);
            }
        }

        public string DirectoryPath { get; }

        public void Dispose()
        {
            lock (this.syncObject)
            {
                if (this.disposed) return;
                if (Directory.Exists(this.DirectoryPath)) Directory.Delete(this.DirectoryPath, true);
                this.disposed = true;
            }
        }
    }
}