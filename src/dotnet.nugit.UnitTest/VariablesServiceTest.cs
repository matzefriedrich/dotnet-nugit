namespace dotnet.nugit.UnitTest
{
    using Abstractions;
    using Moq;
    using Services;

    public class VariablesServiceTest
    {
        [Fact]
        public void VariablesService_GetVariablesNames_returns_empty_enumeration_Test()
        {
            // Arrange
            var sut = new VariablesService(Array.Empty<VariableAccessor>());

            // Act
            IEnumerable<string> actual = sut.GetVariableNames().ToList();

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void VariablesService_GetVariablesNames_returns_expected_names_Test()
        {
            // Arrange
            const string expectedVariableName = "variable1";

            var variableAccessorMock = new Mock<VariableAccessor>(expectedVariableName);
            variableAccessorMock
                .Setup(accessor => accessor.Value())
                .Returns("some-string-value").Verifiable();

            var sut = new VariablesService(new[] { variableAccessorMock.Object });

            // Act
            IEnumerable<string> actual = sut.GetVariableNames().ToList();

            // Assert
            Assert.Single(actual, expectedVariableName);
        }

        [Fact]
        public void VariablesService_TryGetVariable_outputs_expected_value_Test()
        {
            // Arrange
            const string expectedVariableName = "variable1";
            const string expectedVariableValue = "some-string-value";

            var variableAccessorMock = new Mock<VariableAccessor>(expectedVariableName);
            variableAccessorMock
                .Setup(accessor => accessor.Value())
                .Returns(expectedVariableValue).Verifiable();

            var sut = new VariablesService(new[] { variableAccessorMock.Object });

            // Act
            bool actual = sut.TryGetVariable(expectedVariableName, out string? actualValue);

            // Assert
            Assert.True(actual);
            Assert.Equal(expectedVariableValue, actualValue);
        }
    }
}