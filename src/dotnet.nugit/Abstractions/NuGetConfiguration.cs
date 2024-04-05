namespace dotnet.nugit.Abstractions
{
    using System.Xml.Serialization;

    [XmlRoot("configuration")]
    public sealed class NuGetConfiguration
    {
        [XmlArray("packageSources", ElementName = "add")]
        [XmlArrayItem(typeof(PackageSource))]
        public List<PackageSource> PackageSources { get; init; } = new();
    }
}