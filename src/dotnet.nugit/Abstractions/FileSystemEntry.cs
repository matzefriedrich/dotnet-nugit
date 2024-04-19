namespace dotnet.nugit.Abstractions
{
    using System;
    using Resources;

    public sealed class FileSystemEntry
    {
        public FileSystemEntry(string path, bool isDirectory)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(path));

            this.Path = path;
            this.IsDirectory = isDirectory;
        }

        public string Path { get; }
        public bool IsDirectory { get; }
    }
}