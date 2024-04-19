namespace dotnet.nugit.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using nugit.Abstractions;

    public abstract class DirectoryWorkspaceEnvironment(
        IFileSystem fileSystem) : IWorkspaceEnvironment
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        private const string WorkspaceConfigurationFileName = ".nugit";

        private static readonly Encoding ConfigurationFileEncoding = Encoding.UTF8;

        public TextReader CreateConfigurationFileReader()
        {
            string? nugitFile = this.WorkspaceConfigurationFilePath();
            if (string.IsNullOrWhiteSpace(nugitFile) || this.fileSystem.File.Exists(nugitFile) == false)
                return StreamReader.Null;

            Stream stream = this.fileSystem.FileStream.New(nugitFile, FileMode.Open, FileAccess.Read, FileShare.None);
            return new StreamReader(stream, ConfigurationFileEncoding);
        }

        public TextWriter GetConfigurationFileWriter()
        {
            string? nugitFile = this.WorkspaceConfigurationFilePath();
            if (string.IsNullOrWhiteSpace(nugitFile))
                nugitFile = this.DefaultWorkspaceConfigurationFilePath();

            Stream stream = this.fileSystem.FileStream.New(nugitFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            stream.SetLength(0);

            return new StreamWriter(stream, ConfigurationFileEncoding, 4096);
        }

        protected abstract string DirectoryPath();

        public string? WorkspaceConfigurationFilePath()
        {
            var stack = new Stack<string>();
            stack.Push(this.DirectoryPath());
            while (stack.Count > 0)
            {
                string nextPath = stack.Pop();
                string workspaceConfigurationFilePath = this.fileSystem.Path.Combine(nextPath, WorkspaceConfigurationFileName);
                if (this.fileSystem.File.Exists(workspaceConfigurationFilePath))
                    return workspaceConfigurationFilePath;

                IDirectoryInfo? parentPath = this.fileSystem.Directory.GetParent(nextPath);
                if (parentPath != null) stack.Push(parentPath.FullName);
            }

            return null;
        }

        private string DefaultWorkspaceConfigurationFilePath()
        {
            return this.fileSystem.Path.Combine(this.DirectoryPath(), WorkspaceConfigurationFileName);
        }
    }
}