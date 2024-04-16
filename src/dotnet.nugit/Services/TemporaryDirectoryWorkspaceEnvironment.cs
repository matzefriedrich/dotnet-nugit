namespace dotnet.nugit.Services
{
    public sealed class TemporaryDirectoryWorkspaceEnvironment : DirectoryWorkspaceEnvironment, IDisposable
    {
        private readonly object syncObject = new();
        private readonly string path;

        public TemporaryDirectoryWorkspaceEnvironment()
        {
            string folderName = Guid.NewGuid().ToString()[..8];
            this.path = Path.Combine(Path.GetTempPath(), folderName);
        }
        
        protected override string DirectoryPath()
        {
            lock (this.syncObject)
            {
                if (Directory.Exists(this.path) == false) Directory.CreateDirectory(this.path);
            }

            return this.path;
        }

        public void Dispose()
        {
            lock (this.syncObject)
            {
                if (Directory.Exists(this.path)) Directory.Delete(this.path, true);
            }
        }
    }
}