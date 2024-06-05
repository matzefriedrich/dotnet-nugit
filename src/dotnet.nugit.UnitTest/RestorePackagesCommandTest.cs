namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Commands;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging.Abstractions;
    using Mocking;
    using Moq;
    using Services.Tasks;
    using static Commands.ExitCodes;

    public class RestorePackagesCommandTest
    {
        [Fact]
        public async Task RestorePackagesCommand_RestoreWorkspacePackagesAsync_build_head_tip_if_no_tags_available_Test()
        {
            // Arrange
            var feed = new LocalFeedInfo { LocalPath = "/tmp/this-path-does-not-exist", Name = "TestFeed" };
            RepositoryReference repositoryReference = new() { RepositoryUrl = "git@github.com:owner/repo.git", RepositoryType = "git" };

            var configurationFile = new NugitConfigurationFile
            {
                LocalFeed = feed,
                Repositories = new List<RepositoryReference> { repositoryReference }
            };

            var workspaceMock = new Mock<INugitWorkspace>();
            workspaceMock
                .Setup(workspace => workspace.TryReadConfiguration(out configurationFile))
                .Returns(true);

            var repositoryMock = new Mock<IRepository>();
            repositoryMock.SetupGet(repository => repository.Refs).Returns(new MockReferenceCollection());

            var openRepositoryTaskMock = new Mock<IOpenRepositoryTask>();
            openRepositoryTaskMock
                .Setup(task => task.OpenRepository(feed, It.IsAny<RepositoryUri>(), It.IsAny<TimeSpan?>(), true))
                .Returns(repositoryMock.Object)
                .Verifiable();

            var buildPackagesTaskMock = new Mock<IBuildRepositoryPackagesTask>();
            buildPackagesTaskMock
                .Setup(task => task.BuildRepositoryPackagesAsync(repositoryReference, feed, repositoryMock.Object, null, null, It.IsAny<CancellationToken>()))
                .Verifiable();

            var msBuildToolsLocatorMock = new Mock<IMsBuildToolsLocator>();
            
            var sut = new RestorePackagesCommand(
                workspaceMock.Object,
                msBuildToolsLocatorMock.Object,
                OpenRepositoryTaskFactory,
                BuildPackagesTaskFactory,
                new NullLogger<RestorePackagesCommand>());

            // Act
            int actual = await sut.RestoreWorkspacePackagesAsync(false, CancellationToken.None);

            // Assert
            Assert.Equal(Ok, actual);
            openRepositoryTaskMock.Verify(task => task.OpenRepository(feed, It.IsAny<RepositoryUri>(), It.IsAny<TimeSpan?>(), true), Times.Once);
            buildPackagesTaskMock.Verify(task => task.BuildRepositoryPackagesAsync(repositoryReference, feed, repositoryMock.Object, null, null, It.IsAny<CancellationToken>()), Times.Once);

            return;

            IOpenRepositoryTask OpenRepositoryTaskFactory()
            {
                return openRepositoryTaskMock.Object;
            }

            IBuildRepositoryPackagesTask BuildPackagesTaskFactory()
            {
                return buildPackagesTaskMock.Object;
            }
        }
    }
}