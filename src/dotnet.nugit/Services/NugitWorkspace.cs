namespace dotnet.nugit.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    internal sealed class NugitWorkspace(
        IWorkspaceEnvironment workspaceEnvironment,
        ILogger<NugitWorkspace> logger) : INugitWorkspace
    {
        private readonly IWorkspaceEnvironment environment = workspaceEnvironment ?? throw new ArgumentNullException(nameof(workspaceEnvironment));
        private readonly ILogger<NugitWorkspace> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool TryReadConfiguration(out NugitConfigurationFile? configurationFile)
        {
            configurationFile = null;

            using TextReader reader = this.environment.CreateConfigurationFileReader();

            IDeserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            try
            {
                configurationFile = deserializer.Deserialize<NugitConfigurationFile>(reader);
                return true;
            }
            catch (YamlException e)
            {
                this.logger.LogError(e, "Failed to deserialize .nugit workspace configuration file.");
                return false;
            }
        }

        public async Task CreateOrUpdateConfigurationAsync(
            Func<NugitConfigurationFile>? create = null,
            Func<NugitConfigurationFile, NugitConfigurationFile>? update = null)
        {
            if (this.TryReadConfiguration(out NugitConfigurationFile? instance) && instance != null)
                instance = update?.Invoke(instance) ?? instance;

            instance ??= create?.Invoke();
            if (instance == null) return;

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            await using TextWriter writer = this.environment.GetConfigurationFileWriter();
            serializer.Serialize(writer, instance);
        }

        public async Task AddRepositoryReferenceAsync(RepositoryUri repositoryUri)
        {
            ArgumentNullException.ThrowIfNull(repositoryUri);
            await this.CreateOrUpdateConfigurationAsync(update: configurationFile =>
            {
                configurationFile.AddRepository(repositoryUri);
                return configurationFile;
            });
        }

        public Task UpdateRepositoryReferenceAsync(RepositoryReference repositoryReference, RepositoryReference updatedRepositoryReference)
        {
            throw new NotImplementedException();
        }
    }
}