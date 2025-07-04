using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Services.Patterns;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Patterns
{
    /// <summary>
    /// Comprehensive tests for TextPatternService covering all text operations
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class TextPatternServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<TextPatternService>> _logger;
        private readonly TextPatternService _service;
        private readonly Mock<IUIAutomationWorker> _mockWorker;

        public TextPatternServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<TextPatternService>>();
            _mockWorker = new Mock<IUIAutomationWorker>();
            _service = new TextPatternService(_logger.Object, _mockWorker.Object);
        }

        [Fact]
        public async Task GetTextAsync_Success_ReturnsText()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Sample text content"
            };
            _mockWorker.Setup(w => w.GetTextAsync("textElement", "Text Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetTextAsync("textElement", "Text Window");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Sample text content", result.Data);
            _mockWorker.Verify(w => w.GetTextAsync("textElement", "Text Window", null, 20), Times.Once);
            _output.WriteLine($"GetText test passed: {result.Data}");
        }

        [Fact]
        public async Task GetTextAsync_Failed_ReturnsError()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Element not found"
            };
            _mockWorker.Setup(w => w.GetTextAsync("nonExistentElement", "Text Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetTextAsync("nonExistentElement", "Text Window");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
            _output.WriteLine($"GetText error test passed: {result.Error}");
        }

        [Fact]
        public async Task SelectTextAsync_Success_SelectsText()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Text selected successfully"
            };
            _mockWorker.Setup(w => w.SelectTextAsync("textElement", 5, 10, "Text Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SelectTextAsync("textElement", 5, 10, "Text Window");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Text selected successfully", result.Data);
            _mockWorker.Verify(w => w.SelectTextAsync("textElement", 5, 10, "Text Window", null, 20), Times.Once);
            _output.WriteLine($"SelectText test passed: {result.Data}");
        }

        [Fact]
        public async Task SelectTextAsync_InvalidRange_ReturnsError()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Invalid text range"
            };
            _mockWorker.Setup(w => w.SelectTextAsync("textElement", -1, 10, "Text Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SelectTextAsync("textElement", -1, 10, "Text Window");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid text range", result.Error);
            _output.WriteLine($"SelectText error test passed: {result.Error}");
        }

        [Fact]
        public async Task FindTextAsync_NotImplemented_ReturnsError()
        {
            // Act
            var result = await _service.FindTextAsync("textElement", "search text", false, false, "Text Window");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not yet implemented", result.Error);
            _output.WriteLine($"FindText not implemented test passed: {result.Error}");
        }

        [Fact]
        public async Task GetTextSelectionAsync_NotImplemented_ReturnsError()
        {
            // Act
            var result = await _service.GetTextSelectionAsync("textElement", "Text Window");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not yet implemented", result.Error);
            _output.WriteLine($"GetTextSelection not implemented test passed: {result.Error}");
        }

        [Theory]
        [InlineData("", "Text Window", null)]
        [InlineData("textElement", "", null)]
        [InlineData("textElement", "Text Window", 0)]
        public async Task GetTextAsync_InvalidParameters_HandlesGracefully(
            string elementId, string windowTitle, int? processId)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Invalid parameters"
            };
            _mockWorker.Setup(w => w.GetTextAsync(elementId, windowTitle, processId, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetTextAsync(elementId, windowTitle, processId);

            // Assert
            Assert.False(result.Success);
            _output.WriteLine($"GetText invalid params test passed for {elementId}");
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(10, 0)]
        [InlineData(-1, 5)]
        [InlineData(5, -1)]
        public async Task SelectTextAsync_EdgeCases_HandlesCorrectly(int startIndex, int length)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = startIndex >= 0 && length >= 0,
                Data = startIndex >= 0 && length >= 0 ? "Success" : null,
                Error = startIndex < 0 || length < 0 ? "Invalid range" : null
            };
            _mockWorker.Setup(w => w.SelectTextAsync("textElement", startIndex, length, "Text Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SelectTextAsync("textElement", startIndex, length, "Text Window");

            // Assert
            Assert.Equal(expectedResult.Success, result.Success);
            if (result.Success)
            {
                Assert.Equal("Success", result.Data);
            }
            else
            {
                Assert.Equal("Invalid range", result.Error);
            }
            _output.WriteLine($"SelectText edge case test passed for {startIndex}, {length}");
        }

        [Fact]
        public async Task TextPatternService_MultipleOperations_WorksCorrectly()
        {
            // Arrange
            var getTextResult = new OperationResult<string>
            {
                Success = true,
                Data = "Some text content"
            };
            var selectTextResult = new OperationResult<string>
            {
                Success = true,
                Data = "Text selected"
            };

            _mockWorker.Setup(w => w.GetTextAsync("textElement", "Text Window", null, 20))
                      .ReturnsAsync(getTextResult);
            _mockWorker.Setup(w => w.SelectTextAsync("textElement", 0, 5, "Text Window", null, 20))
                      .ReturnsAsync(selectTextResult);

            // Act
            var getText = await _service.GetTextAsync("textElement", "Text Window");
            var selectText = await _service.SelectTextAsync("textElement", 0, 5, "Text Window");

            // Assert
            Assert.True(getText.Success);
            Assert.True(selectText.Success);
            Assert.Equal("Some text content", getText.Data);
            Assert.Equal("Text selected", selectText.Data);
            _output.WriteLine("Multiple operations test passed");
        }
    }
}
