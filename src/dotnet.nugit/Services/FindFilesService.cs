namespace dotnet.nugit.Services
{
    using System.Runtime.CompilerServices;
    using Abstractions;
    using Resources;

    public sealed class FindFilesService : IFindFilesService
    {
        public async IAsyncEnumerable<string> FindAsync(string path, string pattern, Func<FileSystemEntry, Task<bool>> fetch, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(fetch);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(path));
            if ((Path.IsPathFullyQualified(path) && Path.Exists(path)) == false) throw new ArgumentException("Invalid path.", nameof(path));

            var directories = new Stack<string>();
            directories.Push(path);

            while (directories.Count > 0 && cancellationToken.IsCancellationRequested == false)
            {
                string next = directories.Pop();
                var directoryEntry = new FileSystemEntry(next, true);
                bool traverse = await fetch(directoryEntry);
                if (traverse == false) continue;

                string[] entries = Directory.GetFileSystemEntries(next, pattern, SearchOption.TopDirectoryOnly);
                foreach (string entryPath in entries)
                {
                    FileAttributes attributes = File.GetAttributes(entryPath);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        directories.Push(entryPath);
                        continue;
                    }
                    
                    var entry = new FileSystemEntry(entryPath, false);
                    bool collect = await fetch(entry);
                    if (collect)
                    {
                        yield return entryPath;
                    }
                }
            }
        }
    }
}