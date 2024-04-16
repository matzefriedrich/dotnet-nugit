namespace dotnet.nugit.Services
{
    public sealed class TemporaryDirectoryWorkspaceEnvironment : DirectoryWorkspaceEnvironment, IDisposable
    {
        private readonly TempDirectory directory = new();

        public void Dispose()
        {
            this.directory.Dispose();
        }

        protected override string DirectoryPath()
        {
            return this.directory.DirectoryPath;
        }
    }
}