namespace dotnet.nugit.Abstractions
{
    using System.Threading;

    public interface ILibGit2SharpAdapter
    {
        bool TryCloneRepository(string cloneUrl, string repositoryPath, CancellationToken cancellationToken);
    }
}