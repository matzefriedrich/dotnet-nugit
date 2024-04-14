namespace dotnet.nugit.Commands
{
    using System.Text;
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using static ExitCodes;

    public sealed class InitCommand(
        INuGetFeedService nuGetFeedService,
        ILogger<InitCommand> logger)
    {
        private readonly INuGetFeedService nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
        private readonly ILogger<InitCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<int> InitializeNewRepositoryAsync()
        {
            LocalFeedInfo feed = await this.nuGetFeedService.CreateLocalFeedIfNotExistsAsync(CancellationToken.None);

            await CreateNugitRepositoryFileIfNotExistsAsync(feed);

            this.logger.LogDebug("Feed: {0}, Path: {1}", feed.Name, feed.LocalPath);

            return Ok;
        }

        private static async Task CreateNugitRepositoryFileIfNotExistsAsync(LocalFeedInfo feed)
        {
            string nugitFile = Path.Combine(Environment.CurrentDirectory, ".nugit");
            if (File.Exists(nugitFile) == false)
            {
                var nugitConfigurationFile = new NugitConfigurationFile
                {
                    LocalFeed = feed
                };

                await using Stream stream = new FileStream(nugitFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                await using var writer = new StreamWriter(stream, Encoding.UTF8, 4096);
                ISerializer serializer = new SerializerBuilder()
                    .WithNamingConvention(HyphenatedNamingConvention.Instance)
                    .Build();

                serializer.Serialize(writer, nugitConfigurationFile);
            }
        }
    }
}