namespace dotnet.nugit.Services
{
    using System;
    using System.IO.Abstractions;

    public sealed class TemporaryDirectoryWorkspaceEnvironment(IFileSystem fileSystem) : DirectoryWorkspaceEnvironment(fileSystem), IDisposable
    {
        private readonly TempDirectory directory = new(fileSystem);

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