namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using static Commands.ExitCodes;

    public class AddPackagesFromRepositoryCommandTest
    {
        [Fact]
        public async Task AddPackagesFromRepositoryCommand_ProcessRepositoryAsync_Test()
        {
            // Arrange
            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = "/tmp/this-path-does-not-exist" };

            var feedService = new Mock<INuGetFeedService>();
            feedService
                .Setup(service => service.GetConfiguredLocalFeedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => feed)
                .Verifiable();

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
                findFilesService.Object,
                dotnetUtility.Object,
                workspace.Object,
                git.Object,
                new NullLogger<AddPackagesFromRepositoryCommand>());

            const string repositoryReference = "git@github.com:/owner/repo.git";

            // Act
            int exitCode = await sut.ProcessRepositoryAsync(repositoryReference, true, CancellationToken.None);

            // Assert
            Assert.Equal(ErrCannotOpen, exitCode);

            git.Verify(adapter => adapter.TryCloneRepository(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}