namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging.Abstractions;
    using Mocking;
    using Moq;
    using Services.Tasks;

    public class BuildRepositoryPackagesTaskTest
    {
        [Fact]
        public async Task BuildRepositoryPackagesTask_BuildRepositoryPackagesAsync_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();

            var tip = new MockCommit("sha");
            var branch = new MockBranch(tip);
            var repoMock = new Mock<IRepository>();
            repoMock.Setup(repository => repository.Head).Returns(branch);

            var gitMock = new Mock<ILibGit2SharpAdapter>();
            gitMock
                .Setup(adapter => adapter.Checkout(repoMock.Object, tip, It.IsAny<CheckoutOptions>()))
                .Verifiable();

            var buildReference = new RepositoryReference { RepositoryType = "git", RepositoryUrl = "git@github.com:owner/repo.git" };
            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = fileSystem.Path.GetTempPath() };

            var buildProjectsTask = new Mock<IFindAndBuildProjectsTask>();
            buildProjectsTask
                .Setup(task => task.FindAndBuildPackagesAsync(It.IsAny<RepositoryReference>(), feed, It.IsAny<CancellationToken>()))
                .Verifiable();

            var sut = new BuildRepositoryPackagesTask(
                gitMock.Object,
                BuildProjectTaskFactory,
                new NullLogger<BuildRepositoryPackagesTask>());

            // Act
            RepositoryReference actual = await sut.BuildRepositoryPackagesAsync(buildReference, feed, repoMock.Object, null, null, CancellationToken.None);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(buildReference, actual);

            gitMock.Verify(adapter => adapter.Checkout(repoMock.Object, tip, It.IsAny<CheckoutOptions>()), Times.Once);

            return;

            IFindAndBuildProjectsTask BuildProjectTaskFactory()
            {
                return buildProjectsTask.Object;
            }
        }
    }
}