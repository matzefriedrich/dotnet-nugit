namespace dotnet.nugit.UnitTest
{
    using System.Text;
    using Abstractions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services;

    public class LocalNuGetFeedServiceTest
    {
        [Fact]
        public async Task LocalNuGetFeedService_GetConfiguredPackageSourcesAsync_returns_empty_sources_if_config_file_is_missing_Test()
        {
            // Arrange
            var variablesServiceMock = new Mock<IVariablesService>();
            var infoServiceMock = new Mock<INuGetInfoService>();
            infoServiceMock
                .Setup(service => service.GetNuGetConfigReader())
                .Returns(StreamReader.Null);

            var sut = new LocalNuGetFeedService(
                variablesServiceMock.Object,
                infoServiceMock.Object,
                new NullLogger<LocalNuGetFeedService>());

            // Act
            IEnumerable<PackageSource> actual = await sut.GetConfiguredPackageSourcesAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task LocalNuGetFeedService_CreateLocalFeedIfNotExistsAsync_Test()
        {
            // Arrange
            using var temporaryDirectory = new TempDirectory();

            const string localFeedName = "TestFeed";
            string? nugitHomeDirectoryVariableValue = Path.Combine(temporaryDirectory.DirectoryPath, localFeedName);

            string nugetConfig = "<configuration>" +
                                 "    <packageSources />" +
                                 "</configuration>";

            var buffer = new StringBuilder(nugetConfig);

            var variablesServiceMock = new Mock<IVariablesService>();
            variablesServiceMock
                .Setup(service => service.TryGetVariable(ApplicationVariableNames.NugitHome, out nugitHomeDirectoryVariableValue))
                .Returns(true)
                .Verifiable();

            var infoServiceMock = new Mock<INuGetInfoService>();
            infoServiceMock
                .Setup(service => service.GetNuGetConfigReader())
                .Returns(() => new StringReader(buffer.ToString()));

            infoServiceMock
                .Setup(service => service.GetNuGetConfigWriter())
                .Returns(CreateNugetConfigurationWriter);

            var sut = new LocalNuGetFeedService(
                variablesServiceMock.Object,
                infoServiceMock.Object,
                new NullLogger<LocalNuGetFeedService>());

            // Act
            LocalFeedInfo? createdFeed = await sut.CreateLocalFeedIfNotExistsAsync(CancellationToken.None);
            LocalFeedInfo? actual = await sut.GetConfiguredLocalFeedAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(createdFeed);
            Assert.Equal("LocalNuGitFeed", createdFeed.Name);
            Assert.Equal(nugitHomeDirectoryVariableValue, createdFeed.LocalPath);

            Assert.NotNull(actual);
            Assert.Equal(createdFeed, actual);
            return;

            TextWriter CreateNugetConfigurationWriter()
            {
                buffer.Clear();
                return new StringWriter(buffer);
            }
        }
    }
}