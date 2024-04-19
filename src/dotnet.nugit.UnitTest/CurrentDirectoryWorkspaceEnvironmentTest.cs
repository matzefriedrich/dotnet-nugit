namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Services;

    public class CurrentDirectoryWorkspaceEnvironmentTest
    {
        [Fact]
        public void CurrentDirectoryWorkspaceEnvironment_WorkspaceConfigurationFilePath_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            string expectedWorkspaceConfigurationFilePath = fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), ".nugit");
            fileSystem.AddFile(expectedWorkspaceConfigurationFilePath, new MockFileData(""));

            var sut = new CurrentDirectoryWorkspaceEnvironment(fileSystem);

            // Act
            string? actual = sut.WorkspaceConfigurationFilePath();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(actual));
            Assert.StartsWith(expectedWorkspaceConfigurationFilePath, actual);
        }
    }
}