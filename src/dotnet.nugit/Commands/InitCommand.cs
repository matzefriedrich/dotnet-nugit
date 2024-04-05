namespace dotnet.nugit.Commands
{
    using System.Text;
    using Abstractions;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public sealed class InitCommand
    {
        private readonly INuGetFeedService nuGetFeedService;

        public InitCommand(INuGetFeedService nuGetFeedService)
        {
            this.nuGetFeedService = nuGetFeedService ?? throw new ArgumentNullException(nameof(nuGetFeedService));
        }

        public async Task<int> InitializeNewRepositoryAsync()
        {
            const string feedName = "LocalNuGitFeed";
            LocalFeedInfo feed = await this.nuGetFeedService.CreateLocalFeedIfNotExistsAsync(feedName, CancellationToken.None);

            await CreateNugitRepositoryFileIfNotExistsAsync(feed);

            Console.WriteLine($"Feed: {feed.Name}, Path: {feed.LocalPath}");

            return 0;
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