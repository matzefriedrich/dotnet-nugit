namespace dotnet.nugit.Services
{
    using System.Text;
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    internal sealed class NugitWorkspace(ILogger<NugitWorkspace> logger) : INugitWorkspace
    {
        private readonly ILogger<NugitWorkspace> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool TryReadConfigurationAsync(out NugitConfigurationFile? configurationFile)
        {
            configurationFile = null;
            string nugitFile = Path.Combine(Environment.CurrentDirectory, ".nugit");
            if (File.Exists(nugitFile) == false) return false;

            using Stream stream = new FileStream(nugitFile, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new StreamReader(stream, Encoding.UTF8);
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
            if (this.TryReadConfigurationAsync(out NugitConfigurationFile? instance) && instance != null)
                instance = update?.Invoke(instance) ?? instance;

            instance ??= create?.Invoke();
            if (instance == null) return;
            
            string nugitFile = Path.Combine(Environment.CurrentDirectory, ".nugit");
            await using Stream stream = new FileStream(nugitFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            stream.SetLength(0);
                
            await using var writer = new StreamWriter(stream, Encoding.UTF8, 4096);
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

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
    }
}