namespace dotnet.nugit.Abstractions
{
    public interface IDotNetUtility
    {
        Task BuildAsync(string projectFile, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        
        Task PackAsync(string projectFile, LocalFeedInfo target, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}