namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services.Tasks;
    using static Commands.ExitCodes;

    public class AddPackagesFromRepositoryCommandTest
    {
        [Fact]
        public async Task AddPackagesFromRepositoryCommand_ProcessRepositoryAsync_Test()
        {
            // Arrange
            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = "/tmp/this-path-does-not-exist" };

            var feedService = new Mock<INuGetFeedConfigurationService>();
            feedService
                .Setup(service => service.GetConfiguredLocalFeedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => feed)
                .Verifiable();

            var fileSystem = new MockFileSystem();
            var findFilesService = new Mock<IFindFilesService>();
            var dotnetUtility = new Mock<IDotNetUtility>();
            var workspace = new Mock<INugitWorkspace>();

            var git = new Mock<ILibGit2SharpAdapter>();
            git
                .Setup(adapter => adapter.TryCloneRepository(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(true)
                .Verifiable();

            var sut = new AddPackagesFromRepositoryCommand(
                feedService.Object,
                workspace.Object,
                OpenRepositoryTaskFactory,
                BuildPackagesTaskFactory,
                new NullLogger<AddPackagesFromRepositoryCommand>());

            const string repositoryReference = "git@github.com:/owner/repo.git";

            // Act
            int exitCode = await sut.ProcessRepositoryAsync(repositoryReference, true, CancellationToken.None);

            // Assert
            Assert.Equal(ErrCannotOpen, exitCode);

            git.Verify(adapter => adapter.TryCloneRepository(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            return;

            BuildRepositoryPackagesTask BuildPackagesTaskFactory() => new(BuildProjectTaskFactory, new NullLogger<BuildRepositoryPackagesTask>());
            FindAndBuildProjectsTask BuildProjectTaskFactory() => new(workspace.Object, dotnetUtility.Object, findFilesService.Object, new NullLogger<FindAndBuildProjectsTask>());
            OpenRepositoryTask OpenRepositoryTaskFactory() => new(git.Object, fileSystem, new NullLogger<OpenRepositoryTask>());
        }
    }
}