namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Services;

    public class NugitHomeVariableAccessorTest
    {
        [Fact]
        public void NugitHomeVariableAccessor_Value_Test()
        {
            // Arrange
            var sut = new NugitHomeVariableAccessor();
            
            // Act
            string actual = sut.Value();
            
            // Assert
            Assert.Equal(ApplicationVariableNames.NugitHome, sut.Name);
            Assert.False(string.IsNullOrWhiteSpace(actual));
        }
    }
}