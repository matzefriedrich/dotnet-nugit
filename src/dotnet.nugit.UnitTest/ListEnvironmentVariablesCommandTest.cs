namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using static Commands.ExitCodes;

    public class ListEnvironmentVariablesCommandTest
    {
        [Fact]
        public async Task ListEnvironmentVariablesCommand_ListEnvironmentVariablesAsync_Test()
        {
            // Arrange
            IEnumerable<string> variablesNames = new[] { "variable1", "variable2" };

            var variablesServiceMock = new Mock<IVariablesService>();
            variablesServiceMock
                .Setup(service => service.GetVariableNames())
                .Returns(variablesNames)
                .Verifiable();

            string? value = null;
            variablesServiceMock
                .Setup(service => service.TryGetVariable(It.IsAny<string>(), out value))
                .Returns(true)
                .Verifiable();

            var sut = new ListEnvironmentVariablesCommand(
                variablesServiceMock.Object,
                new NullLogger<ListEnvironmentVariablesCommand>());

            // Act
            int exitCode = await sut.ListEnvironmentVariablesAsync();

            // Assert
            Assert.Equal(Ok, exitCode);
            variablesServiceMock.Verify(service => service.GetVariableNames(), Times.Once);
            variablesServiceMock.Verify(service => service.TryGetVariable(It.IsAny<string>(), out value), Times.Exactly(variablesNames.Count()));
        }
    }
}