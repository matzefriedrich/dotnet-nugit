namespace dotnet.nugit.Abstractions
{
    public interface INugitWorkspace
    {
        bool TryReadConfiguration(out NugitConfigurationFile? configurationFile);

        Task CreateOrUpdateConfigurationAsync(
            Func<NugitConfigurationFile>? create = null,
            Func<NugitConfigurationFile, NugitConfigurationFile>? update = null);

        Task AddRepositoryReferenceAsync(RepositoryUri repositoryUri);
    }
}