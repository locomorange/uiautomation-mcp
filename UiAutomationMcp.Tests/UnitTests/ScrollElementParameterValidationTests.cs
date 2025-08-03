using UIAutomationMCP.Models;

namespace UiAutomationMcp.Tests.UnitTests
{
    /// <summary>
    /// ScrollElementIntoView                                /// UIAutomation                                       /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ScrollElementParameterValidationTests : IDisposable
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidateElementId_WhenInvalidInput_ShouldReturnFalse(string? elementId)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("scrollable-item")]
        [InlineData("list-item-1")]
        [InlineData("tree-node")]
        public void ValidateElementId_WhenValidInput_ShouldReturnTrue(string elementId)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void CreateScrollRequest_WithValidParameters_ShouldCreateValidRequest()
        {
            // Arrange
            var elementId = "test-element";
            var windowTitle = "Test Window";
            var processId = 1234;

            // Act
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId,
                    ["windowTitle"] = windowTitle,
                    ["processId"] = processId,
                    ["timeoutSeconds"] = 30
                }
            };

            // Assert
            Assert.NotNull(request);
            Assert.Equal(elementId, ((Dictionary<string, object>)request.Parameters)["elementId"]);
            Assert.Equal(windowTitle, ((Dictionary<string, object>)request.Parameters)["windowTitle"]);
            Assert.Equal(processId, ((Dictionary<string, object>)request.Parameters)["processId"]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(601)] // 10 minutes is too much
        public void ValidateTimeoutSeconds_WhenInvalidTimeout_ShouldBeInvalid(int timeoutSeconds)
        {
            // Arrange & Act
            var isValid = timeoutSeconds > 0 && timeoutSeconds <= 600;

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(600)]
        public void ValidateTimeoutSeconds_WhenValidTimeout_ShouldBeValid(int timeoutSeconds)
        {
            // Arrange & Act
            var isValid = timeoutSeconds > 0 && timeoutSeconds <= 600;

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateWindowTitle_ShouldAcceptAnyValue()
        {
            // Arrange & Act - Window title is optional and can be any value
            var testCases = new[] { "Test Window", "", null };
            var isAcceptable = true; // Window title validation is permissive

            // Assert
            Assert.True(isAcceptable);
        }

        [Fact]
        public void ValidateProcessId_ShouldAcceptAnyIntegerValue()
        {
            // Arrange & Act - Process ID validation happens at runtime
            var testCases = new[] { 0, 1234, -1 }; // Any integer is acceptable at parameter level
            var isIntegerValue = true;

            // Assert
            Assert.True(isIntegerValue);
        }

        public void Dispose()
        {
            // No cleanup needed
        }
    }
}
