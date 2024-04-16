namespace dotnet.nugit.Services
{
    public sealed class TempDirectory : IDisposable
    {
        private bool disposed;
        private readonly object syncObject = new();
        private readonly string path;

        public TempDirectory()
        {
            string folderName = Guid.NewGuid().ToString()[..8];
            this.path = Path.Combine(Path.GetTempPath(), folderName);

            lock (this.syncObject)
            {
                if (Directory.Exists(this.path) == false) Directory.CreateDirectory(this.path);
            }
        }

        public string DirectoryPath => this.path;

        public void Dispose()
        {
            lock (this.syncObject)
            {
                if (this.disposed) return;
                if (Directory.Exists(this.path)) Directory.Delete(this.path, true);
                this.disposed = true;
            }
        }
    }
}