namespace dotnet.nugit.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectWorkspaceManager
    {
        Task<IDotNetProject> LoadProjectAsync(string projectFile, string configurationName, CancellationToken cancellationToken);
    }
}