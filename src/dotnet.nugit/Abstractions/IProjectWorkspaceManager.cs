namespace dotnet.nugit.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectWorkspaceManager
    {
        string? LocateMsBuildToolsPath();
        
        Task<IProjectAccessor> LoadProjectAsync(string projectFile, string configurationName, CancellationToken cancellationToken);
    }
}