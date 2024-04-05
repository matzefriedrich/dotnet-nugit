namespace dotnet.nugit.Abstractions
{
    using System.Xml.Serialization;

    public sealed class PackageSource
    {
        [XmlAttribute("key")]
        public string Key { get; init; } = null!;

        [XmlAttribute("value")]
        public string? Value { get; init; }
        
        [XmlAttribute("protocolVersion", typeof(int?))]
        public int? ProtocolVersion { get; init; }
    }
}