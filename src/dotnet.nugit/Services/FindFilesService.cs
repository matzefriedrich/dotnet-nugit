namespace dotnet.nugit.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Resources;

    public sealed class FindFilesService(IFileSystem fileSystem) : IFindFilesService
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        public async IAsyncEnumerable<string> FindAsync(string path, string pattern, Func<FileSystemEntry, Task<bool>> fetch, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(fetch);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(path));
            if ((this.fileSystem.Path.IsPathFullyQualified(path) && this.fileSystem.Path.Exists(path)) == false) throw new ArgumentException("Invalid path.", nameof(path));

            var directories = new Stack<string>();
            directories.Push(path);

            while (directories.Count > 0 && cancellationToken.IsCancellationRequested == false)
            {
                string next = directories.Pop();
                var directoryEntry = new FileSystemEntry(next, true);
                bool traverse = await fetch(directoryEntry);
                if (traverse == false) continue;

                string[] entries = this.fileSystem.Directory.GetFileSystemEntries(next, pattern, SearchOption.TopDirectoryOnly);
                foreach (string entryPath in entries)
                {
                    FileAttributes attributes = this.fileSystem.File.GetAttributes(entryPath);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        directories.Push(entryPath);
                        continue;
                    }

                    var entry = new FileSystemEntry(entryPath, false);
                    bool collect = await fetch(entry);
                    if (collect) yield return entryPath;
                }
            }
        }
    }
}