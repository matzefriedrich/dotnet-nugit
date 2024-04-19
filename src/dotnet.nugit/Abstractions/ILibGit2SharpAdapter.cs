namespace dotnet.nugit.Abstractions
{
    using System.Threading;
    using LibGit2Sharp;

    public interface ILibGit2SharpAdapter
    {
        bool TryCloneRepository(string cloneUrl, string repositoryPath, CancellationToken cancellationToken);

        IRepository OpenRepository(string path);
    }
}