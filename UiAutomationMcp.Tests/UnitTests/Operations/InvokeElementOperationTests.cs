using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Operations.Invoke;
using Microsoft.Extensions.Logging;
using Moq;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    [Trait("Category", "Unit")]
    public class InvokeElementOperationTests
    {
        private readonly InvokeElementOperation _operation;

        public InvokeElementOperationTests()
        {
            var mockLogger = new Mock<ILogger<ElementFinderService>>();
            var elementFinderService = new ElementFinderService(mockLogger.Object);
            _operation = new InvokeElementOperation(elementFinderService);
        }

        [Fact]
        public async Task ExecuteAsync_WhenElementNotFound_ShouldReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element-12345",
                    ["windowTitle"] = "NonexistentWindow",
                    ["processId"] = "99999"
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ExecuteAsync_WhenElementIdIsNullOrEmpty_ShouldReturnFailure(string? elementId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId ?? "",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234"
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("not-a-number")]
        public async Task ExecuteAsync_WhenProcessIdIsInvalid_ShouldDefaultToZero(string processId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = processId
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WhenParametersAreNull_ShouldReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = null
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WhenRequiredParametersMissing_ShouldReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element"
                    // windowTitle and processId missing
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1234")]
        [InlineData("99999")]
        public async Task ExecuteAsync_WhenProcessIdIsValid_ShouldParseCorrectly(string processId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = processId
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Theory]
        [InlineData("btn1", "Calculator", "1234")]
        [InlineData("menu", "Notepad", "5678")]
        [InlineData("textbox", "", "0")]
        public async Task ExecuteAsync_WhenValidParameters_ShouldReturnElementNotFound(string elementId, string windowTitle, string processId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId,
                    ["windowTitle"] = windowTitle,
                    ["processId"] = processId
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WhenParametersContainExtraKeys_ShouldIgnoreAndReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234",
                    ["extraKey"] = "extraValue",
                    ["anotherKey"] = 42
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }
    }
}