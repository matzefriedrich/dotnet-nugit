namespace dotnet.nugit.Abstractions
{
    public sealed class PackageSource(string? key, string? value) : IEquatable<PackageSource>
    {
        public string Key { get; } = key!;

        public string? Value { get; } = value;

        public int? ProtocolVersion { get; init; }

        public bool Equals(PackageSource? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Key == other.Key && this.Value == other.Value && this.ProtocolVersion == other.ProtocolVersion;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is PackageSource other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Key, this.Value, this.ProtocolVersion);
        }
    }
}