namespace dotnet.nugit.Abstractions
{
    public interface IDotNetUtility
    {
        Task PackAsync(string projectFile, LocalFeedInfo target, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}