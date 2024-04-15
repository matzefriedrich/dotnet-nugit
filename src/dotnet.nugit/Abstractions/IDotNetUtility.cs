namespace dotnet.nugit.Abstractions
{
    public interface IDotNetUtility
    {
        Task BuildAsync(string projectFile, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        
        Task<bool> TryPackAsync(string projectFile, string targetFolderPath, PackOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}