namespace dotnet.nugit.Abstractions
{
    public interface IFindFilesService
    {
        IAsyncEnumerable<string> FindAsync(string path, string pattern, Func<FileSystemEntry, Task<bool>> fetch, CancellationToken cancellationToken);
    }
}