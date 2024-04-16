namespace dotnet.nugit.Abstractions
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Resources;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class RepositoryUri : IEquatable<RepositoryUri>
    {
        private RepositoryUri(GitRepositoryUriScheme schemeOrProtocol, string host, string repositoryName, string? tag, string? path)
        {
            if (!Enum.IsDefined(typeof(GitRepositoryUriScheme), schemeOrProtocol)) throw new InvalidEnumArgumentException(nameof(schemeOrProtocol), (int)schemeOrProtocol, typeof(GitRepositoryUriScheme));
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(host));
            if (string.IsNullOrWhiteSpace(repositoryName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryName));

            this.SchemeOrProtocol = schemeOrProtocol;
            this.Host = host;
            this.RepositoryName = repositoryName;
            this.Tag = tag;
            this.Path = path;
        }

        public GitRepositoryUriScheme SchemeOrProtocol { get; }
        public string Host { get; }
        public string RepositoryName { get; }
        public string? Tag { get; }
        public string? Path { get; }

        public bool Equals(RepositoryUri? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.SchemeOrProtocol == other.SchemeOrProtocol && this.Host == other.Host && this.RepositoryName == other.RepositoryName && this.Tag == other.Tag && this.Path == other.Path;
        }

        public string CloneUrl()
        {
            return this.SchemeOrProtocol switch
            {
                GitRepositoryUriScheme.Https => $"https://{this.Host}/{this.RepositoryName}.git",
                GitRepositoryUriScheme.SecureSocket => $"git@{this.Host}:{this.RepositoryName}.git",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static RepositoryUri FromString(string uriString)
        {
            if (string.IsNullOrWhiteSpace(uriString)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(uriString));

            if (TryParseHttpsRepositoryUrl(uriString, out RepositoryUri? repositoryUri) && repositoryUri != null) return repositoryUri;
            if (TryParseSecureSocketRepositoryUrl(uriString, out repositoryUri) && repositoryUri != null) return repositoryUri;

            throw new ArgumentException("The given URI string is of an invalid format and this cannot be parsed.");
        }

        private static bool TryParseHttpsRepositoryUrl(string urlString, out RepositoryUri? repositoryUri)
        {
            repositoryUri = null;

            bool success = Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uri);
            if (!success || uri == null) return false;

            string? scheme = uri.Scheme;
            if (string.IsNullOrWhiteSpace(scheme) || string.Compare("https", scheme, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            string host = uri.Host;
            string pathAndQuery = uri.PathAndQuery;

            const string repositoryNameExtension = ".git";
            int repositoryNameEnd = pathAndQuery.IndexOf(repositoryNameExtension, StringComparison.Ordinal);
            if (repositoryNameEnd < 0) return false;

            string repositoryName = pathAndQuery[1..repositoryNameEnd];
            int tagAndPathStartIndex = repositoryNameEnd + repositoryNameExtension.Length;
            pathAndQuery = pathAndQuery[tagAndPathStartIndex..];
            (string? tag, string? path) = ParseTagAndRelativePath(pathAndQuery);

            repositoryUri = new RepositoryUri(GitRepositoryUriScheme.Https, host, repositoryName, tag, path);
            return true;
        }

        private static bool TryParseSecureSocketRepositoryUrl(string urlString, out RepositoryUri? repositoryUri)
        {
            repositoryUri = null;

            const string sshSchemePrefix = "ssh://";
            if (urlString.StartsWith(sshSchemePrefix, StringComparison.InvariantCultureIgnoreCase)) urlString = urlString[sshSchemePrefix.Length..];

            const string gitProtocolPrefix = "git@";
            if (urlString.StartsWith(gitProtocolPrefix, StringComparison.InvariantCultureIgnoreCase) == false) return false;
            urlString = urlString[gitProtocolPrefix.Length..];

            int hostNameEnd = urlString.IndexOf(':');
            if (hostNameEnd < 0) return false;
            string host = urlString[..hostNameEnd];
            string pathAndQuery = urlString[(hostNameEnd + 1)..].TrimStart('/');

            const string repositoryNameExtension = ".git";
            int repositoryNameEnd = pathAndQuery.IndexOf(repositoryNameExtension, StringComparison.Ordinal);
            if (repositoryNameEnd < 0) return false;

            string repositoryName = pathAndQuery[..repositoryNameEnd];
            int tagAndPathStartIndex = repositoryNameEnd + repositoryNameExtension.Length;
            pathAndQuery = pathAndQuery[tagAndPathStartIndex..];
            (string? tag, string? path) = ParseTagAndRelativePath(pathAndQuery);

            repositoryUri = new RepositoryUri(GitRepositoryUriScheme.SecureSocket, host, repositoryName, tag, path);
            return true;
        }

        private static (string?, string?) ParseTagAndRelativePath(string pathAndQuery)
        {
            int tagIndex = pathAndQuery.IndexOf('@');
            if (tagIndex < 0) return (null, null);

            int p0 = tagIndex + 1;
            int tagAndPathLength = pathAndQuery.Length - p0;

            string tagAndRelativePath = pathAndQuery.Substring(p0, tagAndPathLength);
            int tagEnd = tagAndRelativePath.IndexOf('/');
            if (tagEnd < 0) tagEnd = tagAndPathLength;

            string? path = null;

            string? tag = tagAndRelativePath[..tagEnd].Trim('/');
            int p1 = tagEnd + 1;
            if (p1 < tagAndRelativePath.Length)
            {
                path = tagAndRelativePath.Substring(p1, tagAndPathLength - p1).Trim('/');
                if (string.IsNullOrWhiteSpace(path.Trim())) path = null;
            }

            return (tag, path);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RepositoryUri)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.SchemeOrProtocol, this.Host, this.RepositoryName, this.Tag, this.Path);
        }

        public RepositoryReference AsReference()
        {
            return new RepositoryReference
            {
                RepositoryType = "git",
                RepositoryUrl = this.CloneUrl()
            };
        }

        public RepositoryUri SwitchProtocol(GitRepositoryUriScheme scheme)
        {
            if (!Enum.IsDefined(typeof(GitRepositoryUriScheme), scheme)) throw new InvalidEnumArgumentException(nameof(scheme), (int)scheme, typeof(GitRepositoryUriScheme));
            return new RepositoryUri(scheme, this.Host, this.RepositoryName, this.Tag, this.Path);
        }

        public override string ToString()
        {
            return this.SchemeOrProtocol switch
            {
                GitRepositoryUriScheme.Https => FormatUriString($"https://{this.Host}/{this.RepositoryName}.git"),
                GitRepositoryUriScheme.SecureSocket => FormatUriString($"git@{this.Host}:{this.RepositoryName}.git"),
                _ => throw new ArgumentOutOfRangeException()
            };

            string FormatUriString(string uriString)
            {
                var builder = new StringBuilder(uriString);

                if (string.IsNullOrWhiteSpace(this.Tag) == false) 
                    builder.Append($"@{this.Tag}");
                
                if (string.IsNullOrWhiteSpace(this.Path) == false) 
                    builder.Append($"/{this.Path}");

                return builder.ToString();
            }
        }
    }
}