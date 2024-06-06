namespace dotnet.nugit.Abstractions
{
    using System;
    using Resources;

    public class SdkVersion
    {
        public SdkVersion(Version version, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(path));
            this.Version = version ?? throw new ArgumentNullException(nameof(version));
            this.Path = path;
        }

        public Version Version { get; }
        public string Path { get; }
    }
}