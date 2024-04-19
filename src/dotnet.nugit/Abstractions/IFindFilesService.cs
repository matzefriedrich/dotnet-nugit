namespace dotnet.nugit.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFindFilesService
    {
        IAsyncEnumerable<string> FindAsync(string path, string pattern, Func<FileSystemEntry, Task<bool>> fetch, CancellationToken cancellationToken);
    }
}