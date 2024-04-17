namespace dotnet.nugit.Abstractions
{
    public interface ILibGit2SharpAdapter
    {
        bool TryCloneRepository(string cloneUrl, string repositoryPath, CancellationToken cancellationToken);
    }
}