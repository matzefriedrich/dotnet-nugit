namespace dotnet.nugit.Services
{
    using System.Xml.Linq;
    using Abstractions;
    using Microsoft.Extensions.Logging;

    public sealed class LocalNuGetFeedService(
        IVariablesService variablesService,
        INuGetInfoService infoService,
        ILogger<LocalNuGetFeedService> logger)
        : INuGetFeedService
    {
        private static readonly string DefaultLocalFeedName = "LocalNuGitFeed";

        private readonly INuGetInfoService infoService = infoService ?? throw new ArgumentNullException(nameof(infoService));
        private readonly ILogger<LocalNuGetFeedService> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IVariablesService variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));

        public async Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync(CancellationToken cancellationToken)
        {
            using TextReader reader = this.infoService.GetNuGetConfigReader();
            if (reader == StreamReader.Null) 
                return Array.Empty<PackageSource>();
            
            XDocument doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);

            XElement? configurationElt = doc.Element("configuration");
            XElement? sourcesElt = configurationElt?.Element("packageSources");

            return sourcesElt?.Elements("add").Select(element =>
            {
                string? protocolVersionString = element.Attribute("protocolVersion")?.Value;
                int.TryParse(protocolVersionString, out int pv);
                string key = element.Attribute("key")?.Value!;
                string? value = element.Attribute("value")?.Value!;
                return new PackageSource(key, value)
                {
                    ProtocolVersion = pv == 0 ? null : pv
                };
            }).ToList().AsReadOnly() ?? Enumerable.Empty<PackageSource>();
        }

        public async Task<LocalFeedInfo?> GetConfiguredLocalFeedAsync(CancellationToken cancellationToken)
        {
            await this.CreateLocalFeedIfNotExistsAsync(cancellationToken);
            PackageSource? source = (await this.GetConfiguredPackageSourcesAsync(cancellationToken)).SingleOrDefault(source => string.Compare(source.Key, DefaultLocalFeedName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (source == null) return null;
            return new LocalFeedInfo { Name = source.Key!, LocalPath = source.Value! };
        }

        public async Task<LocalFeedInfo?> CreateLocalFeedIfNotExistsAsync(CancellationToken cancellationToken)
        {
            string feedName = DefaultLocalFeedName;

            string localFeedPath = this.variablesService.GetNugitHomeDirectoryPath();
            if (string.IsNullOrWhiteSpace(localFeedPath) == false && Directory.Exists(localFeedPath) == false) Directory.CreateDirectory(localFeedPath);

            IEnumerable<PackageSource> existingSources = await this.GetConfiguredPackageSourcesAsync(cancellationToken);
            Dictionary<string, PackageSource> dict = existingSources.ToDictionary(packageSource => packageSource.Key.ToLowerInvariant(), packageSource => packageSource);
            if (dict.TryGetValue(feedName.ToLowerInvariant(), out PackageSource? source)) return new LocalFeedInfo { Name = feedName, LocalPath = source.Value! };

            XDocument doc;
            using (TextReader reader = this.infoService.GetNuGetConfigReader())
            {
                if (reader == StreamReader.Null) return null;
                doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            }
            
            XElement? configurationElt = doc.Element("configuration");
            XElement? sourcesElt = configurationElt?.Element("packageSources");
            sourcesElt?.Add(new XElement("add", new XAttribute("key", feedName), new XAttribute("value", localFeedPath)));

            await using TextWriter writer = this.infoService.GetNuGetConfigWriter();
            await doc.SaveAsync(writer, SaveOptions.None, cancellationToken);
            await writer.FlushAsync(cancellationToken);

            return new LocalFeedInfo { Name = feedName, LocalPath = localFeedPath };
        }
    }
}