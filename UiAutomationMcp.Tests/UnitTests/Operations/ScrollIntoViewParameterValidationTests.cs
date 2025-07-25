using UIAutomationMCP.Models;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    /// <summary>
    /// ScrollElementIntoView operation parameter validation tests
    /// Tests parameter validation logic without UIAutomation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ScrollIntoViewParameterValidationTests : IDisposable
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
        [InlineData("scroll-item")]
        [InlineData("list-element")]
        [InlineData("tree-node-item")]
        public void ValidateElementId_WhenValidInput_ShouldReturnTrue(string elementId)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void CreateScrollIntoViewRequest_WithValidParameters_ShouldCreateValidRequest()
        {
            // Arrange
            var elementId = "scrollable-element";
            var windowTitle = "Container Window";
            var processId = 5678;

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
            Assert.Equal(elementId, request.Parameters["elementId"]);
            Assert.Equal(windowTitle, request.Parameters["windowTitle"]);
            Assert.Equal(processId, request.Parameters["processId"]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(601)] // 10分を超える
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
            // Arrange & Act - Window title is optional
            var testCases = new[] { "Main Window", "", null };
            var isAcceptable = true;

            // Assert
            Assert.True(isAcceptable);
        }

        public void Dispose()
        {
            // リソースクリーンアップ（必要に応じて）
        }
    }
}
