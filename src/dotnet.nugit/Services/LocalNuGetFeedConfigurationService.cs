namespace dotnet.nugit.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Extensions.Logging;
    using Abstractions;

    public sealed class LocalNuGetFeedConfigurationService(
        IFileSystem fileSystem,
        IVariablesService variablesService,
        INuGetConfigurationAccessService configurationAccessService,
        ILogger<LocalNuGetFeedConfigurationService> logger)
        : INuGetFeedConfigurationService
    {
        private static readonly string DefaultLocalFeedName = "LocalNuGitFeed";

        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(IFileSystem));
        private readonly INuGetConfigurationAccessService configurationAccessService = configurationAccessService ?? throw new ArgumentNullException(nameof(configurationAccessService));
        private readonly ILogger<LocalNuGetFeedConfigurationService> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IVariablesService variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));

        public async Task<IEnumerable<PackageSource>> GetConfiguredPackageSourcesAsync(CancellationToken cancellationToken)
        {
            using TextReader reader = this.configurationAccessService.GetNuGetConfigReader();
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
            if (string.IsNullOrWhiteSpace(localFeedPath) == false && this.fileSystem.Directory.Exists(localFeedPath) == false) this.fileSystem.Directory.CreateDirectory(localFeedPath);

            IEnumerable<PackageSource> existingSources = await this.GetConfiguredPackageSourcesAsync(cancellationToken);
            Dictionary<string, PackageSource> dict = existingSources.ToDictionary(packageSource => packageSource.Key.ToLowerInvariant(), packageSource => packageSource);
            if (dict.TryGetValue(feedName.ToLowerInvariant(), out PackageSource? source)) return new LocalFeedInfo { Name = feedName, LocalPath = source.Value! };

            XDocument doc;
            using (TextReader reader = this.configurationAccessService.GetNuGetConfigReader())
            {
                if (reader == StreamReader.Null) return null;
                doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            }

            XElement? configurationElt = doc.Element("configuration");
            XElement? sourcesElt = configurationElt?.Element("packageSources");
            sourcesElt?.Add(new XElement("add", new XAttribute("key", feedName), new XAttribute("value", localFeedPath)));

            await using TextWriter writer = this.configurationAccessService.GetNuGetConfigWriter();
            await doc.SaveAsync(writer, SaveOptions.None, cancellationToken);
            await writer.FlushAsync(cancellationToken);

            return new LocalFeedInfo { Name = feedName, LocalPath = localFeedPath };
        }
    }
}