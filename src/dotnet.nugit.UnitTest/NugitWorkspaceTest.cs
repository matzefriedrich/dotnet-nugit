namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using Abstractions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Services;

    public class NugitWorkspaceTest
    {
        [Fact]
        public async Task NugitWorkspace_invokes_create_callback_if_workspace_file_not_found_Test()
        {
            // Assert
            var localFeedInfo = new LocalFeedInfo
            {
                Name = "TestFeed",
                LocalPath = "/tmp/this-feed-does-not-exist"
            };

            var expectedConfigurationFile = new NugitConfigurationFile
            {
                LocalFeed = localFeedInfo
            };

            var mockFileSystem = new MockFileSystem();
            using var environment = new TemporaryDirectoryWorkspaceEnvironment(mockFileSystem);

            var sut = new NugitWorkspace(environment, new NullLogger<NugitWorkspace>());

            var createDelegateMock = new Mock<Func<NugitConfigurationFile>>();
            createDelegateMock
                .Setup(func => func.Invoke())
                .Returns(expectedConfigurationFile)
                .Verifiable();

            var updateDelegateMock = new Mock<Func<NugitConfigurationFile?, NugitConfigurationFile>>();
            updateDelegateMock
                .Setup(func => func.Invoke(It.IsAny<NugitConfigurationFile?>()))
                .Verifiable();

            // Act
            await sut.CreateOrUpdateConfigurationAsync(createDelegateMock.Object, updateDelegateMock.Object);

            // Arrange
            createDelegateMock.Verify(func => func.Invoke(), Times.Once);
            updateDelegateMock.Verify(func => func.Invoke(It.IsAny<NugitConfigurationFile?>()), Times.Never);
        }

        [Fact]
        public async Task NugitWorkspace_invokes_update_on_existing_workspace_file_Test()
        {
            // Assert
            var localFeedInfo = new LocalFeedInfo
            {
                Name = "TestFeed",
                LocalPath = "/tmp/this-feed-does-not-exist"
            };

            var expectedConfigurationFile = new NugitConfigurationFile
            {
                LocalFeed = localFeedInfo
            };

            var fileSystemMock = new MockFileSystem();
            using var environment = new TemporaryDirectoryWorkspaceEnvironment(fileSystemMock);
            var sut = new NugitWorkspace(environment, new NullLogger<NugitWorkspace>());

            var createDelegateMock = new Mock<Func<NugitConfigurationFile>>();
            createDelegateMock
                .Setup(func => func.Invoke())
                .Returns(expectedConfigurationFile)
                .Verifiable();

            var updateDelegateMock = new Mock<Func<NugitConfigurationFile?, NugitConfigurationFile>>();
            updateDelegateMock
                .Setup(func => func.Invoke(It.IsAny<NugitConfigurationFile?>()))
                .Verifiable();

            updateDelegateMock
                .Setup(func => func.Invoke(It.IsAny<NugitConfigurationFile?>()))
                .Callback((NugitConfigurationFile? config) =>
                {
                    RepositoryUri repositoryUri = RepositoryUri.FromString("https://github.com/owner/repo.git");
                    config?.AddRepository(repositoryUri);
                });

            await sut.CreateOrUpdateConfigurationAsync(createDelegateMock.Object, updateDelegateMock.Object);

            // Act
            await sut.CreateOrUpdateConfigurationAsync(createDelegateMock.Object, updateDelegateMock.Object);
            bool actual = sut.TryReadConfiguration(out NugitConfigurationFile? current);

            // Arrange
            Assert.True(actual);
            Assert.NotNull(current);
            Assert.Single(current.Repositories);

            createDelegateMock.Verify(func => func.Invoke(), Times.Once);
            updateDelegateMock.Verify(func => func.Invoke(It.IsAny<NugitConfigurationFile?>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task NugitWorkspace_AddRepositoryReferenceAsync_Test()
        {
            // Arrange
            var buffer = new StringBuilder();
            var environmentMock = new Mock<IWorkspaceEnvironment>();

            environmentMock.Setup(environment => environment.CreateConfigurationFileReader())
                .Returns(CreateReaderFunc)
                .Verifiable();

            environmentMock.Setup(environment => environment.GetConfigurationFileWriter())
                .Returns(CreateWriterFunc)
                .Verifiable();

            var sut = new NugitWorkspace(environmentMock.Object, new NullLogger<NugitWorkspace>());
            await sut.CreateOrUpdateConfigurationAsync(() => new NugitConfigurationFile());

            RepositoryUri repositoryUri = RepositoryUri.FromString("https://github.com/owner/repo.git");

            // Act
            await sut.AddRepositoryReferenceAsync(repositoryUri);
            bool actual = sut.TryReadConfiguration(out NugitConfigurationFile? configurationFile);

            // Assert
            Assert.True(actual);
            Assert.NotNull(configurationFile);
            Assert.Single(configurationFile.Repositories);
            return;

            TextReader CreateReaderFunc()
            {
                return new StringReader(buffer.ToString());
            }

            TextWriter CreateWriterFunc()
            {
                buffer.Clear();
                return new StringWriter(buffer);
            }
        }
    }
}