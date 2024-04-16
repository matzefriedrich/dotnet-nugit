namespace dotnet.nugit.UnitTest
{
    using Services;

    public class CurrentDirectoryWorkspaceEnvironmentTest
    {
        [Fact]
        public void CurrentDirectoryWorkspaceEnvironment_WorkspaceConfigurationFilePath_Test()
        {
            // Arrange
            var sut = new CurrentDirectoryWorkspaceEnvironment();
            
            // Act
            string actual = sut.WorkspaceConfigurationFilePath();
            
            // Assert
            Assert.False(string.IsNullOrWhiteSpace(actual));
            Assert.StartsWith(Environment.CurrentDirectory, actual);
        }
        
    }
}