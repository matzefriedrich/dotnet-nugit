namespace dotnet.nugit.Abstractions
{
    using LibGit2Sharp;

    public interface ILibGit2SharpAdapter
    {
        bool TryCloneRepository(string cloneUrl, string repositoryPath, CancellationToken cancellationToken);
    }
}