namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Commands;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services.Tasks;
    using static Commands.ExitCodes;

    public class AddPackagesFromRepositoryCommandTest
    {
        [Fact]
        public async Task AddPackagesFromRepositoryCommand_ProcessRepositoryAsync_does_not_process_tags_if_head_only_specified_Test()
        {
            // Arrange
            const string repositoryReference = "git@github.com:/owner/repo.git";
            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = "/tmp/this-path-does-not-exist" };

            var feedService = new Mock<INuGetFeedConfigurationService>();
            feedService
                .Setup(service => service.GetConfiguredLocalFeedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => feed)
                .Verifiable();

            var workspace = new Mock<INugitWorkspace>();

            var repositoryMock = new Mock<IRepository>();
            repositoryMock.SetupGet(repository => repository.Refs).Verifiable();

            var openRepositoryMock = new Mock<IOpenRepositoryTask>();
            openRepositoryMock
                .Setup(task => task.OpenRepository(feed, It.IsAny<RepositoryUri>(), It.IsAny<TimeSpan?>(), true))
                .Returns(repositoryMock.Object)
                .Verifiable();

            var buildPackagesTaskMock = new Mock<IBuildRepositoryPackagesTask>();
            buildPackagesTaskMock
                .Setup(task => task.BuildRepositoryPackagesAsync(It.IsAny<RepositoryReference>(), feed, repositoryMock.Object, null, null, It.IsAny<CancellationToken>()))
                .Verifiable();

            var msBuildToolsLocatorMock = new Mock<IMsBuildToolsLocator>();
            
            var sut = new AddPackagesFromRepositoryCommand(
                feedService.Object,
                workspace.Object,
                msBuildToolsLocatorMock.Object,
                OpenRepositoryTaskFactory,
                BuildPackagesTaskFactory,
                new NullLogger<AddPackagesFromRepositoryCommand>());

            const bool headOnly = true;

            // Act
            int exitCode = await sut.ProcessRepositoryAsync(repositoryReference, headOnly, CancellationToken.None);

            // Assert
            Assert.Equal(Ok, exitCode);

            openRepositoryMock.Verify(task => task.OpenRepository(feed, It.IsAny<RepositoryUri>(), It.IsAny<TimeSpan?>(), true), Times.Once);
            buildPackagesTaskMock.Verify(task => task.BuildRepositoryPackagesAsync(It.IsAny<RepositoryReference>(), feed, repositoryMock.Object, null, null, It.IsAny<CancellationToken>()), Times.Once);
            repositoryMock.VerifyGet(repository => repository.Refs, Times.Never);
            return;

            IBuildRepositoryPackagesTask BuildPackagesTaskFactory()
            {
                return buildPackagesTaskMock.Object;
            }

            IOpenRepositoryTask OpenRepositoryTaskFactory()
            {
                return openRepositoryMock.Object;
            }
        }
    }
}