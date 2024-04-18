namespace dotnet.nugit.Abstractions
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides functionality to read, create and update a local NuGit workspace.
    /// </summary>
    public interface INugitWorkspace
    {
        bool TryReadConfiguration(out NugitConfigurationFile? configurationFile);

        Task CreateOrUpdateConfigurationAsync(
            Func<NugitConfigurationFile>? create = null,
            Func<NugitConfigurationFile, NugitConfigurationFile>? update = null);

        Task AddRepositoryReferenceAsync(RepositoryUri repositoryUri);

        Task UpdateRepositoryReferenceAsync(RepositoryReference repositoryReference, RepositoryReference updatedRepositoryReference);
    }
}