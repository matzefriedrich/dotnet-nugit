namespace dotnet.nugit.Abstractions
{
    using Services;

    public interface IFindFilesService
    {
        IAsyncEnumerable<string> FindAsync(string path, string pattern, Func<FileSystemEntry, Task<bool>> fetch, CancellationToken cancellationToken);
    }
}