namespace dotnet.nugit.Services
{
    using System.Xml.Linq;
    using Abstractions;

    public sealed class LocalNuGetFeedService : INuGetFeedService
    {
        private static readonly string DefaultLocalFeedName = "LocalNuGitFeed";
        private readonly INuGetInfoService infoService;
        private readonly IVariablesService variablesService;


        public LocalNuGetFeedService(
            IVariablesService variablesService,
            INuGetInfoService infoService)
        {
            this.variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));
            this.infoService = infoService ?? throw new ArgumentNullException(nameof(infoService));
        }

        public async Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync(CancellationToken cancellationToken)
        {
            string nugetConfigFilePath = this.infoService.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && File.Exists(nugetConfigFilePath))
            {
                await using Stream stream = File.OpenRead(nugetConfigFilePath);
                XDocument doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

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

            return [];
        }

        public async Task<LocalFeedInfo?> GetConfiguredLocalFeedAsync(CancellationToken cancellationToken)
        {
            await this.CreateLocalFeedIfNotExistsAsync(cancellationToken);
            PackageSource? source = (await this.GetConfiguredPackageSourcesAsync(cancellationToken)).SingleOrDefault(source => string.Compare(source.Key, DefaultLocalFeedName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (source == null) return null;
            return new LocalFeedInfo { Name = source.Key!, LocalPath = source.Value! };
        }

        public async Task<LocalFeedInfo> CreateLocalFeedIfNotExistsAsync(CancellationToken cancellationToken)
        {
            string feedName = DefaultLocalFeedName;

            string localFeedPath = this.variablesService.GetNugitHomeDirectoryPath();
            if (string.IsNullOrWhiteSpace(localFeedPath) == false && Directory.Exists(localFeedPath) == false) Directory.CreateDirectory(localFeedPath);
            
            IEnumerable<PackageSource> existingSources = await this.GetConfiguredPackageSourcesAsync(cancellationToken);
            Dictionary<string, PackageSource> dict = existingSources.ToDictionary(packageSource => packageSource.Key.ToLowerInvariant(), packageSource => packageSource);
            if (dict.TryGetValue(feedName.ToLowerInvariant(), out PackageSource? source)) return new LocalFeedInfo { Name = feedName, LocalPath = source.Value! };
            
            string nugetConfigFilePath = this.infoService.GetNuGetConfigFilePath();
            XDocument doc = XDocument.Load(nugetConfigFilePath);
            XElement? configurationElt = doc.Element("configuration");
            XElement? sourcesElt = configurationElt?.Element("packageSources");
            sourcesElt?.Add(new XElement("add", new XAttribute("key", feedName), new XAttribute("value", localFeedPath)));
            await doc.SaveAsync(File.OpenWrite(nugetConfigFilePath), SaveOptions.None, cancellationToken);

            return new LocalFeedInfo { Name = feedName, LocalPath = localFeedPath };
        }
    }
}