namespace dotnet.nugit.Services
{
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Abstractions;
    using Resources;

    public sealed class LocalNuGetFeedService : INuGetFeedService
    {
        private readonly INuGetInfoService infoService;
        private readonly IVariablesService variablesService;

        public LocalNuGetFeedService(
            IVariablesService variablesService,
            INuGetInfoService infoService)
        {
            this.variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));
            this.infoService = infoService ?? throw new ArgumentNullException(nameof(infoService));
        }

        public async Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync()
        {
            string nugetConfigFilePath = this.infoService.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && File.Exists(nugetConfigFilePath))
            {
                await using Stream stream = File.OpenRead(nugetConfigFilePath);
                XDocument doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

                XElement? configurationElt = doc.Element("configuration");
                XElement? sourcesElt = configurationElt?.Element("packageSources");
                
                return sourcesElt?.Elements("add").Select(element =>
                {
                    string? protocolVersionString = element.Attribute("protocolVersion")?.Value;
                    int.TryParse(protocolVersionString, out int pv);
                    string? key = element.Attribute("key")?.Value;
                    string? value = element.Attribute("value")?.Value;
                    return new PackageSource(key, value)
                    {
                        ProtocolVersion = pv == 0 ? null : pv
                    };
                }).ToList().AsReadOnly() ?? Enumerable.Empty<PackageSource>();
            }

            return [];
        }

        public async Task<LocalFeedInfo> CreateLocalFeedIfNotExistsAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(name));
            }

            IEnumerable<PackageSource> existingSources = await this.GetConfiguredPackageSourcesAsync();
            Dictionary<string, PackageSource> dict = existingSources.ToDictionary(source => source.Key.ToLowerInvariant(), source => source);
            if (dict.TryGetValue(name.ToLowerInvariant(), out PackageSource? source) && source != null)
            {
                return new LocalFeedInfo(name) { LocalPath = source.Value };
            }

            string localFeedPath = this.variablesService.GetNugitHomeDirectoryPath();
            if (string.IsNullOrWhiteSpace(localFeedPath) == false && Directory.Exists(localFeedPath) == false)
            {
                Directory.CreateDirectory(localFeedPath);
            }

            string nugetConfigFilePath = this.infoService.GetNuGetConfigFilePath();
            XDocument doc = XDocument.Load(nugetConfigFilePath);
            XElement? configurationElt = doc.Element("configuration");
            XElement? sourcesElt = configurationElt?.Element("packageSources");
            sourcesElt?.Add(new XElement("add", new XAttribute("key", name), new XAttribute("value", localFeedPath)));
            await doc.SaveAsync(File.OpenWrite(nugetConfigFilePath), SaveOptions.None, cancellationToken);

            return new LocalFeedInfo(name)
            {
                LocalPath = localFeedPath
            };
        }
    }
}