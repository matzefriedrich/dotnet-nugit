namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using Abstractions;
    using Resources;

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
            if (string.IsNullOrWhiteSpace(projectPropertyName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectPropertyName));
            foreach (string? next in values)
            {
                if (string.IsNullOrWhiteSpace(next) == false)
                    this.fallbackValues[projectPropertyName] = next;
            }

            return this;
        }

        public async Task WriteNuspecFileAsync(Stream target, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(target);
            XDocument doc = this.CreateNuspecDocument();

            var settings = new XmlWriterSettings { Async = true, Encoding = Encoding.UTF8, Indent = true };
            await using var writer = XmlWriter.Create(target, settings);
            await doc.WriteToAsync(writer, cancellationToken);
            await writer.FlushAsync();
        }

        public bool Validate(out ICollection<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();

            NuspecFile file = this.CreateNuspecFileObject();

            var validationContext = new ValidationContext(file, null, null);
            return Validator.TryValidateObject(file, validationContext, validationResults, true);
        }

        internal static IEnumerable<ProjectPropertyName> GetPropertyNames()
        {
            ProjectPropertyName[] propertyNames = [AssemblyVersion, AssemblyTitle, Authors, Company, Copyright, Description, Owners, PackageDescription, PackageId, PackageVersion, RepositoryUrl, Tags];
            return propertyNames;
        }

        private NuspecFile CreateNuspecFileObject()
        {
            string? authorsOrCompany = map.GetValueOrDefault(Authors, UseFallbackValues("", Authors, Company));
            string? packageId = map.GetValueOrDefault(PackageId, UseFallbackValues("", PackageId, AssemblyTitle));
            string? packageVersion = map.GetValueOrDefault(PackageVersion, map.GetValueOrDefault(AssemblyVersion, UseFallbackValues("1.0.0", AssemblyVersion)));
            string? packageTitle = map.GetValueOrDefault(AssemblyTitle, UseFallbackValues("", AssemblyTitle));
            string? packageOwners = map.GetValueOrDefault(Owners, UseFallbackValues("", Owners));
            string? packageProjectUrl = map.GetValueOrDefault(RepositoryUrl, UseFallbackValues("", RepositoryUrl));
            string? packageDescription = map.GetValueOrDefault(PackageDescription, map.GetValueOrDefault(Description));
            string? copyrightString = map.GetValueOrDefault(Copyright, UseFallbackValues($"Copyright {DateTime.Today.Year}", Copyright));
            string? tags = map.GetValueOrDefault(Tags, UseFallbackValues("", Tags));

            return new NuspecFile
            {
                Authors = authorsOrCompany,
                Id = packageId,
                Version = packageVersion,
                Title = packageTitle,
                Owners = packageOwners,
                ProjectUrl = packageProjectUrl,
                Description = packageDescription,
                Copyright = copyrightString,
                Tags = tags
            };

            string UseFallbackValues(string defaultValue, params ProjectPropertyName[] names)
            {
                foreach (ProjectPropertyName propertyName in names)
                {
                    if (this.fallbackValues.TryGetValue(propertyName, out string? value) && string.IsNullOrWhiteSpace(value) == false)
                        return value;
                }

                return defaultValue;
            }
        }

        public XDocument CreateNuspecDocument()
        {
            NuspecFile package = this.CreateNuspecFileObject();

            var metadataElement = new XElement("metadata",
                new XElement("id", package.Id),
                new XElement("version", package.Version),
                new XElement("authors", package.Authors),
                new XElement("description", package.Description),
                new XElement("title", package.Title),
                new XElement("owners", package.Owners),
                new XElement("requireLicenseAcceptance", "false"),
                new XElement("copyright", package.Copyright),
                new XElement("tags", package.Tags));

            if (string.IsNullOrWhiteSpace(package.ProjectUrl) == false)
                metadataElement.Add(new XElement("projectUrl", package.ProjectUrl));
            
            var nuspec = new XDocument(
                new XElement("package",
                    metadataElement
                )
            );

            return nuspec;
        }

        public static IProjectPackageMetadata Empty()
        {
            return new ProjectPackageMetadata(new Dictionary<string, string?>());
        }

        internal sealed class NuspecFile
        {
            [Required] public string? Authors { get; init; }

            [Required] public string? Id { get; init; }

            [Required] public string? Version { get; init; }

            public string? Title { get; init; }
            public string? Owners { get; init; }
            
            public string? ProjectUrl { get; init; }

            [Required] public string? Description { get; init; }

            public string? Copyright { get; init; }
            public string? Tags { get; init; }
        }
    }
}