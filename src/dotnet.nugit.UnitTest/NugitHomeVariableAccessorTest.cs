namespace dotnet.nugit.UnitTest
{
    using System.IO.Abstractions.TestingHelpers;
    using Abstractions;
    using Services;

    public class NugitHomeVariableAccessorTest
    {
        [Fact]
        public void NugitHomeVariableAccessor_Value_Test()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var sut = new NugitHomeVariableAccessor(fileSystem);

            // Act
            string actual = sut.Value();

            // Assert
            Assert.Equal(ApplicationVariableNames.NugitHome, sut.Name);
            Assert.False(string.IsNullOrWhiteSpace(actual));
        }
    }
}