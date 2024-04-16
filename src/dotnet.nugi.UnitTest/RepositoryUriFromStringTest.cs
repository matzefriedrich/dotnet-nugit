namespace dotnet.nugi.UnitTest
{
    using nugit.Abstractions;

    public class RepositoryUriFromStringTest
    {
        [Theory]
        [InlineData("git@github.com")]
        [InlineData("git@github.com/")]
        [InlineData("git@github.com:matzefriedrich/command-line-api-extensions")]
        [InlineData("https://github.com")]
        [InlineData("https://github.com/")]
        public void RepositoryUri_FromString_parse_incomplete_url_string_Test(string referenceString)
        {
            // Arrange

            // Assert
            Assert.Throws<ArgumentException>(Act);
            return;

            // Act
            void Act() => RepositoryUri.FromString(referenceString);
        }

        [Theory]
        [InlineData("git@github.com:matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1/src/System.CommandLine.Extensions", "github.com", "matzefriedrich/command-line-api-extensions", "v2.0.0-beta4.22272.1", "src/System.CommandLine.Extensions")]
        [InlineData("ssh://git@github.com:matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1/src/System.CommandLine.Extensions", "github.com", "matzefriedrich/command-line-api-extensions", "v2.0.0-beta4.22272.1", "src/System.CommandLine.Extensions")]
        [InlineData("git@github.com:matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1", "github.com", "matzefriedrich/command-line-api-extensions", "v2.0.0-beta4.22272.1", null)]
        [InlineData("git@github.com:matzefriedrich/command-line-api-extensions.git", "github.com", "matzefriedrich/command-line-api-extensions", null, null)]
        public void RepositoryUri_FromString_parse_ssh_reference_Test(string referenceString, string expectedDomain, string expectedRepositoryName, string? expectedTag, string? expectedPath)
        {
            // Arrange

            // Act
            RepositoryUri actual = RepositoryUri.FromString(referenceString);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expectedDomain, actual.Host);
            Assert.Equal(expectedRepositoryName, actual.RepositoryName);
            Assert.Equal(expectedTag, actual.Tag);
            Assert.Equal(expectedPath, actual.Path);
        }

        [Theory]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1/src/System.CommandLine.Extensions/", "v2.0.0-beta4.22272.1", "src/System.CommandLine.Extensions")]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1/jj", "v2.0.0-beta4.22272.1", "jj")]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1/", "v2.0.0-beta4.22272.1", null)]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git@v2.0.0-beta4.22272.1", "v2.0.0-beta4.22272.1", null)]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git/", null, null)]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git", null, null)]
        public void RepositoryUri_FromString_parse_https_reference_Test(string referenceString, string? expectedTag, string? expectedPath)
        {
            // Arrange

            // Act
            RepositoryUri actual = RepositoryUri.FromString(referenceString);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(GitRepositoryUriScheme.Https, actual.SchemeOrProtocol);
            Assert.Equal("github.com", actual.Host);
            Assert.Equal("matzefriedrich/command-line-api-extensions", actual.RepositoryName);
            Assert.Equal(expectedTag, actual.Tag);
            Assert.Equal(expectedPath, actual.Path);
        }

        [Theory]
        [InlineData(GitRepositoryUriScheme.SecureSocket, "https://github.com/matzefriedrich/command-line-api-extensions.git", "git@github.com:matzefriedrich/command-line-api-extensions.git")]
        [InlineData(GitRepositoryUriScheme.Https, "git@github.com:matzefriedrich/command-line-api-extensions.git", "https://github.com/matzefriedrich/command-line-api-extensions.git")]
        public void RepositoryUri_SwitchProtocol_Test(GitRepositoryUriScheme targetScheme, string uriString, string expectedUriString)
        {
            // Arrange
            RepositoryUri uri = RepositoryUri.FromString(uriString);
            
            // Act
            RepositoryUri actual = uri.SwitchProtocol(targetScheme);
            var actualUriString = actual.ToString();
            
            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expectedUriString, actualUriString);
        }
        
        [Theory]
        [InlineData("https://github.com/matzefriedrich/command-line-api-extensions.git")]
        [InlineData("git@github.com:matzefriedrich/command-line-api-extensions.git")]
        public void RepositoryUri_AsReference_Test(string uriString)
        {
            // Arrange
            RepositoryUri uri = RepositoryUri.FromString(uriString);
            const string rootPathExpression = "/";
            
            // Act
            RepositoryReference actual = uri.AsReference();
            
            // Assert
            Assert.NotNull(actual);
            Assert.Equal("git", actual.RepositoryType);
            Assert.Equal(uri.CloneUrl(), actual.RepositoryUrl);
            Assert.Null(actual.Tag);
            Assert.Null(actual.Hash);
            Assert.Equal(rootPathExpression, actual.RepositoryPath);
        }
    }
}