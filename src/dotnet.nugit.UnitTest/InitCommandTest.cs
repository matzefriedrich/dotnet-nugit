namespace dotnet.nugit.UnitTest
{
    using System.Reflection;
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;

    public class InitCommandTest
    {
        [Fact]
        public async Task InitCommand_smoke_Test()
        {
            // Arrange
            var expectedLocalFeedInfo = new LocalFeedInfo
            {
                Name = "TestFeed",
                LocalPath = Environment.CurrentDirectory
            };

            var feedServiceMock = new Mock<INuGetFeedService>();
            feedServiceMock
                .Setup(service => service.CreateLocalFeedIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((CancellationToken _) => expectedLocalFeedInfo)
                .Verifiable();

            var workspaceMock = new Mock<INugitWorkspace>();
            workspaceMock.Setup(workspace => workspace.CreateOrUpdateConfigurationAsync(It.IsAny<Func<NugitConfigurationFile>>(), It.IsAny<Func<NugitConfigurationFile?, NugitConfigurationFile>>()))
                .Verifiable();

            var sut = new InitCommand(feedServiceMock.Object, workspaceMock.Object, new NullLogger<InitCommand>());

            const bool copyLocal = false;

            // Act
            int actual = await sut.InitializeNewRepositoryAsync(copyLocal);

            // Assert
            Assert.Equal(ExitCodes.Ok, actual);
            feedServiceMock.Verify(service => service.CreateLocalFeedIfNotExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
            workspaceMock.Verify(workspace => workspace.CreateOrUpdateConfigurationAsync(It.IsAny<Func<NugitConfigurationFile>>(), It.IsAny<Func<NugitConfigurationFile?, NugitConfigurationFile>>()), Times.Once);
        }
        
        [Fact]
        public async Task InitCommand_create_configuration_for_local_feed_if_missing_Test()
        {
            // Arrange
            var expectedLocalFeedInfo = new LocalFeedInfo
            {
                Name = "TestFeed",
                LocalPath = Environment.CurrentDirectory
            };

            var feedServiceMock = new Mock<INuGetFeedService>();
            feedServiceMock
                .Setup(service => service.CreateLocalFeedIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((CancellationToken _) => expectedLocalFeedInfo)
                .Verifiable();

            NugitConfigurationFile? configurationFile = null;
            var workspaceMock = new Mock<INugitWorkspace>();
            workspaceMock
                .Setup(workspace => workspace.CreateOrUpdateConfigurationAsync(It.IsAny<Func<NugitConfigurationFile>>(), It.IsAny<Func<NugitConfigurationFile?, NugitConfigurationFile>>()))
                .Callback<Func<NugitConfigurationFile>, Func<NugitConfigurationFile?, NugitConfigurationFile>>((create, update) =>
                {
                    configurationFile = create();
                });

            var sut = new InitCommand(feedServiceMock.Object, workspaceMock.Object, new NullLogger<InitCommand>());

            const bool copyLocal = false;

            // Act
            int actual = await sut.InitializeNewRepositoryAsync(copyLocal);

            // Assert
            Assert.Equal(ExitCodes.Ok, actual);
            Assert.NotNull(configurationFile);
            Assert.Equal(expectedLocalFeedInfo, configurationFile.LocalFeed);
            Assert.False(configurationFile.CopyLocal);
            
            feedServiceMock.Verify(service => service.CreateLocalFeedIfNotExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}