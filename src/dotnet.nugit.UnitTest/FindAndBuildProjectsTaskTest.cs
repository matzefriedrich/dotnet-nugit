namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using System.Security.Cryptography;
    using Abstractions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services.Tasks;

    public class FindAndBuildProjectsTaskTest
    {
        [Fact]
        public async Task FindAndBuildProjectsTask_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();

            byte[] hash = SHA256.HashData("sha"u8.ToArray());
            string commitHashString = Convert.ToHexString(hash);
            var qualifiedRepositoryReference = new RepositoryReference { RepositoryType = RepositoryTypes.Git, RepositoryUrl = "git@github.com:owner/repo.git", Hash = commitHashString };
            var feed = new LocalFeedInfo { Name = "TestFeed", LocalPath = fileSystem.Path.GetTempPath() };

            string projectFilePath = fileSystem.Path.Combine(feed.LocalPath, "src", "project1", "project1.csproj");
            fileSystem.AddFile(projectFilePath, new MockFileData(""));

            NugitConfigurationFile? configurationFile = null;
            var workspaceMock = new Mock<INugitWorkspace>();
            workspaceMock
                .Setup(workspace => workspace.TryReadConfiguration(out configurationFile))
                .Returns(false)
                .Verifiable();

            var dotNetUtilityMock = new Mock<IDotNetUtility>();
            dotNetUtilityMock
                .Setup(utility => utility.BuildAsync(projectFilePath, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            dotNetUtilityMock
                .Setup(utility => utility.TryPackAsync(projectFilePath, It.IsAny<string>(), It.IsAny<PackOptions>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var projectFilesList = new List<string> { projectFilePath };

            var finderMock = new Mock<IFindFilesService>();
            finderMock
                .Setup(service => service.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<FileSystemEntry, Task<bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(projectFilesList.ToAsyncEnumerable)
                .Verifiable();

            var sut = new FindAndBuildProjectsTask(
                workspaceMock.Object,
                dotNetUtilityMock.Object,
                finderMock.Object,
                fileSystem,
                new NullLogger<FindAndBuildProjectsTask>());

            // Act
            await sut.FindAndBuildPackagesAsync(qualifiedRepositoryReference, feed, CancellationToken.None);

            // Assert
            finderMock.Verify(service => service.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<FileSystemEntry, Task<bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
            dotNetUtilityMock.Verify(utility => utility.BuildAsync(projectFilePath, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
            dotNetUtilityMock.Verify(utility => utility.TryPackAsync(projectFilePath, It.IsAny<string>(), It.IsAny<PackOptions>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}