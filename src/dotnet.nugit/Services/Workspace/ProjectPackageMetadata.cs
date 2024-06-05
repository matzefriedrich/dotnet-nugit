namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using Abstractions;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class ProjectPackageMetadata(Dictionary<string, string?> map) : IProjectPackageMetadata
    {
        public static readonly ProjectPropertyName AssemblyTitle = "AssemblyTitle";
        public static readonly ProjectPropertyName AssemblyVersion = "AssemblyVersion";
        public static readonly ProjectPropertyName Authors = "Authors";
        public static readonly ProjectPropertyName Company = "Company";
        public static readonly ProjectPropertyName Copyright = "Copyright";
        public static readonly ProjectPropertyName Description = "Description";
        public static readonly ProjectPropertyName Owners = "Owners";
        public static readonly ProjectPropertyName PackageDescription = "PackageDescription";
        public static readonly ProjectPropertyName PackageId = "PackageId";
        public static readonly ProjectPropertyName PackageVersion = "PackageVersion";
        public static readonly ProjectPropertyName RepositoryUrl = "RepositoryUrl";
        public static readonly ProjectPropertyName Tags = "PackageTags";

        private readonly Dictionary<string, string?> fallbackValues = new();

        public IProjectPackageMetadata AddDefaultValues(string projectPropertyName, params string?[] values)
        {
            if (string.IsNullOrWhiteSpace(projectPropertyName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectPropertyName));
            foreach (string? next in values)
            {
                if (string.IsNullOrWhiteSpace(next) == false)
                    this.fallbackValues[projectPropertyName] = next;
            }

            return this;
        }

        internal static IEnumerable<ProjectPropertyName> GetPropertyNames()
        {
            ProjectPropertyName[] propertyNames = [AssemblyVersion, AssemblyTitle, Authors, Company, Copyright, Description, Owners, PackageDescription, PackageId, PackageVersion, RepositoryUrl, Tags];
            return propertyNames;
        }

        public XDocument CreateNuspecDocument()
        {
            string? authorsOrCompany = map.GetValueOrDefault(Authors, this.fallbackValues.GetValueOrDefault(Authors, map.GetValueOrDefault(Company, this.fallbackValues.GetValueOrDefault(Company, ""))));

            var nuspec = new XDocument(
                new XElement("package",
                    new XElement("metadata",
                        new XElement("id", map.GetValueOrDefault(PackageId, this.fallbackValues.GetValueOrDefault(PackageId, ""))),
                        new XElement("version", map.GetValueOrDefault(PackageVersion, map.GetValueOrDefault(AssemblyVersion, this.fallbackValues.GetValueOrDefault(AssemblyVersion, "1.0.0"))))),
                    new XElement("title", map.GetValueOrDefault(AssemblyTitle, this.fallbackValues.GetValueOrDefault(AssemblyTitle, ""))),
                    new XElement("authors", authorsOrCompany),
                    new XElement("owners", map.GetValueOrDefault(Owners, this.fallbackValues.GetValueOrDefault(Owners, ""))),
                    new XElement("projectUrl", map.GetValueOrDefault(RepositoryUrl, this.fallbackValues.GetValueOrDefault(RepositoryUrl, ""))),
                    new XElement("requireLicenseAcceptance", "false"),
                    new XElement("description", map.GetValueOrDefault(PackageDescription, map.GetValueOrDefault(Description))),
                    new XElement("copyright", map.GetValueOrDefault(Copyright, this.fallbackValues.GetValueOrDefault(Copyright, $"Copyright {DateTime.Today.Year}"))),
                    new XElement("tags", map.GetValueOrDefault(Tags, this.fallbackValues.GetValueOrDefault(Tags, "")))
                )
            );

            return nuspec;
        }

        public async Task WriteNuspecFileAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            XDocument doc = this.CreateNuspecDocument();
            
            Stream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            stream.SetLength(0);

            await using var writer = XmlWriter.Create(stream);
            await doc.WriteToAsync(writer, cancellationToken);
            await writer.FlushAsync();
        }

        public static IProjectPackageMetadata Empty()
        {
            return new ProjectPackageMetadata(new Dictionary<string, string?>());
        }
    }
}