using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Operations.Value;
using Microsoft.Extensions.Logging;
using Moq;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    [Trait("Category", "Unit")]
    public class SetElementValueOperationTests
    {
        private readonly SetElementValueOperation _operation;

        public SetElementValueOperationTests()
        {
            var mockLogger = new Mock<ILogger<ElementFinderService>>();
            var elementFinderService = new ElementFinderService(mockLogger.Object);
            _operation = new SetElementValueOperation(elementFinderService);
        }

        [Fact]
        public async Task ExecuteAsync_WhenElementNotFound_ShouldReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element-12345",
                    ["windowTitle"] = "NonexistentWindow",
                    ["processId"] = "99999",
                    ["value"] = "test value"
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
        public async Task ExecuteAsync_WhenValueIsNullOrEmpty_ShouldReturnFailure(string? value)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234",
                    ["value"] = value ?? ""
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Theory]
        [InlineData("simple text")]
        [InlineData("text with spaces")]
        [InlineData("12345")]
        [InlineData("special!@#$%^&*()characters")]
        public async Task ExecuteAsync_WhenValueIsValid_ShouldReturnFailure(string value)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234",
                    ["value"] = value
                }
            };

            // When
            var result = await _operation.ExecuteAsync(request);

            // Then
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WhenValueParameterMissing_ShouldReturnFailure()
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234"
                    // value parameter missing
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
        public async Task ExecuteAsync_WhenProcessIdIsValid_ShouldReturnFailure(string processId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = processId,
                    ["value"] = "test value"
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
        [InlineData("abc")]
        public async Task ExecuteAsync_WhenProcessIdIsInvalid_ShouldReturnFailure(string processId)
        {
            // Given
            var request = new WorkerRequest
            {
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = processId,
                    ["value"] = "test value"
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
                Operation = "SetElementValue",
                Parameters = null
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
                Operation = "SetElementValue",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "nonexistent-element",
                    ["windowTitle"] = "TestWindow",
                    ["processId"] = "1234",
                    ["value"] = "test value",
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