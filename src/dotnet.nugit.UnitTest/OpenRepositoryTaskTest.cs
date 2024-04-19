namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services.Tasks;

    public class OpenRepositoryTaskTest
    {
        [Fact]
        public void OpenRepositoryTask_OpenRepository_clones_remote_repository_if_exists_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            string feedLocalPath = fileSystem.Path.GetTempPath();
            string repositoryPath = fileSystem.Path.Combine(feedLocalPath, "repositories/owner/repo").SanitizedPathString();

            var repositoryMock = new Mock<IRepository>();

            var gitMock = new Mock<ILibGit2SharpAdapter>();
            gitMock
                .Setup(adapter => adapter.OpenRepository(repositoryPath))
                .Returns(repositoryMock.Object)
                .Verifiable();

            gitMock
                .Setup(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()))
                .Returns(true)
                .Verifiable();

            var sut = new OpenRepositoryTask(
                gitMock.Object,
                fileSystem,
                new NullLogger<OpenRepositoryTask>());

            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = feedLocalPath };

            RepositoryUri repositoryUri = RepositoryUri.FromString("git@github.com:/owner/repo.git");

            // Act
            IRepository? actual = sut.OpenRepository(feed, repositoryUri, allowClone: true);

            // Assert
            Assert.NotNull(actual);

            gitMock.Verify(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()), Times.Once);
            gitMock.Verify(adapter => adapter.OpenRepository(repositoryPath), Times.Once);
        }

        [Fact]
        public void OpenRepositoryTask_OpenRepository_does_not_clone_if_exists_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            string feedLocalPath = fileSystem.Path.GetTempPath();
            string repositoryPath = fileSystem.Path.Combine(feedLocalPath, "repositories/owner/repo").SanitizedPathString();

            var repositoryMock = new Mock<IRepository>();

            var gitMock = new Mock<ILibGit2SharpAdapter>();
            gitMock
                .Setup(adapter => adapter.OpenRepository(repositoryPath))
                .Returns(repositoryMock.Object)
                .Verifiable();

            gitMock
                .Setup(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()))
                .Verifiable();

            var hiddenGitFolderPath = $"{repositoryPath}/.git";
            fileSystem.AddDirectory(hiddenGitFolderPath);

            var sut = new OpenRepositoryTask(
                gitMock.Object,
                fileSystem,
                new NullLogger<OpenRepositoryTask>());

            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = feedLocalPath };

            RepositoryUri repositoryUri = RepositoryUri.FromString("git@github.com:/owner/repo.git");

            // Act
            IRepository? actual = sut.OpenRepository(feed, repositoryUri);

            // Assert
            Assert.NotNull(actual);

            gitMock.Verify(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()), Times.Never);
            gitMock.Verify(adapter => adapter.OpenRepository(repositoryPath), Times.Once);
        }

        [Fact]
        public void OpenRepositoryTask_OpenRepository_returns_null_on_RepositoryNotFoundException_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            string feedLocalPath = fileSystem.Path.GetTempPath();
            string repositoryPath = fileSystem.Path.Combine(feedLocalPath, "repositories/owner/repo").SanitizedPathString();

            var gitMock = new Mock<ILibGit2SharpAdapter>();
            gitMock
                .Setup(adapter => adapter.OpenRepository(repositoryPath))
                .Throws<RepositoryNotFoundException>()
                .Verifiable();

            gitMock
                .Setup(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()))
                .Verifiable();

            var hiddenGitFolderPath = $"{repositoryPath}/.git";
            fileSystem.AddDirectory(hiddenGitFolderPath);

            var sut = new OpenRepositoryTask(
                gitMock.Object,
                fileSystem,
                new NullLogger<OpenRepositoryTask>());

            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = feedLocalPath };

            RepositoryUri repositoryUri = RepositoryUri.FromString("git@github.com:/owner/repo.git");

            // Act
            IRepository? actual = sut.OpenRepository(feed, repositoryUri);

            // Assert
            Assert.Null(actual);

            gitMock.Verify(adapter => adapter.TryCloneRepository(It.IsAny<string>(), repositoryPath, It.IsAny<CancellationToken>()), Times.Never);
            gitMock.Verify(adapter => adapter.OpenRepository(repositoryPath), Times.Once);
        }
    }
}