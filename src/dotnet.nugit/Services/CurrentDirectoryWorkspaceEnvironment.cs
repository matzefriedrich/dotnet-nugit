namespace dotnet.nugit.Services
{
    using System;
    using System.IO.Abstractions;

    public sealed class CurrentDirectoryWorkspaceEnvironment(IFileSystem fileSystem) : DirectoryWorkspaceEnvironment(fileSystem)
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        protected override string DirectoryPath()
        {
            return this.fileSystem.Directory.GetCurrentDirectory(); // Environment.CurrentDirectory;
        }
    }
}