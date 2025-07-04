using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Services.Patterns;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Patterns
{
    /// <summary>
    /// Comprehensive tests for LayoutPatternService covering scrolling and layout management scenarios
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class LayoutPatternServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<LayoutPatternService>> _logger;
        private readonly LayoutPatternService _service;
        private readonly Mock<IUIAutomationWorker> _mockWorker;

        public LayoutPatternServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<LayoutPatternService>>();
            _mockWorker = new Mock<IUIAutomationWorker>();
            _service = new LayoutPatternService(_logger.Object, _mockWorker.Object);
        }

        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("left")]
        [InlineData("right")]
        public async Task ScrollElementAsync_DirectionalScroll_Success(string direction)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Scrolled {direction} successfully"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("scrollElement", direction, null, null, "ScrollWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", direction, windowTitle: "ScrollWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Scrolled {direction} successfully", result.Data);
            _mockWorker.Verify(w => w.ScrollElementAsync("scrollElement", direction, null, null, "ScrollWindow", null, 20), Times.Once);
            _output.WriteLine($"Scroll {direction} test passed: {result.Data}");
        }

        [Theory]
        [InlineData(0.0, 50.0)]
        [InlineData(25.5, 75.0)]
        [InlineData(100.0, 0.0)]
        [InlineData(50.0, 50.0)]
        public async Task ScrollElementAsync_PercentageScroll_Success(double horizontal, double vertical)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Scrolled to position ({horizontal}%, {vertical}%)"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("scrollElement", null, horizontal, vertical, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", horizontal: horizontal, vertical: vertical);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Scrolled to position ({horizontal}%, {vertical}%)", result.Data);
            _mockWorker.Verify(w => w.ScrollElementAsync("scrollElement", null, horizontal, vertical, null, null, 20), Times.Once);
            _output.WriteLine($"Scroll to ({horizontal}%, {vertical}%) test passed: {result.Data}");
        }

        [Fact]
        public async Task ScrollElementAsync_CombinedDirectionAndPercentage_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Scrolled with combined parameters successfully"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("scrollElement", "down", 25.0, 75.0, "CombinedWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", "down", 25.0, 75.0, "CombinedWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Scrolled with combined parameters successfully", result.Data);
            _mockWorker.Verify(w => w.ScrollElementAsync("scrollElement", "down", 25.0, 75.0, "CombinedWindow", null, 20), Times.Once);
            _output.WriteLine($"Combined scroll test passed: {result.Data}");
        }

        [Fact]
        public async Task ScrollElementIntoViewAsync_Success_ScrollsIntoView()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Element scrolled into view successfully"
            };
            _mockWorker.Setup(w => w.ScrollElementIntoViewAsync("targetElement", "ListView", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementIntoViewAsync("targetElement", "ListView");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Element scrolled into view successfully", result.Data);
            _mockWorker.Verify(w => w.ScrollElementIntoViewAsync("targetElement", "ListView", null, 20), Times.Once);
            _output.WriteLine($"Scroll into view test passed: {result.Data}");
        }

        [Theory]
        [InlineData(-10.0, 50.0)]
        [InlineData(50.0, -10.0)]
        [InlineData(110.0, 50.0)]
        [InlineData(50.0, 110.0)]
        [InlineData(-5.0, 105.0)]
        public async Task ScrollElementAsync_InvalidPercentages_HandlesGracefully(double horizontal, double vertical)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = $"Invalid scroll percentages: horizontal={horizontal}, vertical={vertical}"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("scrollElement", null, horizontal, vertical, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", horizontal: horizontal, vertical: vertical);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Invalid scroll percentages: horizontal={horizontal}, vertical={vertical}", result.Error);
            _output.WriteLine($"Invalid percentages ({horizontal}%, {vertical}%) test passed: {result.Error}");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("diagonal")]
        [InlineData("")]
        [InlineData("UP")]  // Case sensitivity
        public async Task ScrollElementAsync_InvalidDirections_HandlesGracefully(string direction)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = $"Invalid scroll direction: {direction}"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("scrollElement", direction, null, null, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", direction);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Invalid scroll direction: {direction}", result.Error);
            _output.WriteLine($"Invalid direction '{direction}' test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementAsync_WorkerFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Worker process failed to execute scroll operation"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("scrollElement", "down", windowTitle: "TestWindow");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Worker process failed to execute scroll operation", result.Error);
            _output.WriteLine($"Worker failure test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementIntoViewAsync_WorkerFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Worker process failed to scroll element into view"
            };
            _mockWorker.Setup(w => w.ScrollElementIntoViewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementIntoViewAsync("targetElement", "ListView");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Worker process failed to scroll element into view", result.Error);
            _output.WriteLine($"ScrollIntoView worker failure test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementAsync_ElementNotFound_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Scroll element not found"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("nonexistentElement", "down", null, null, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("nonexistentElement", "down");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Scroll element not found", result.Error);
            _output.WriteLine($"Element not found test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementIntoViewAsync_TargetNotFound_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Target element not found"
            };
            _mockWorker.Setup(w => w.ScrollElementIntoViewAsync("nonexistentTarget", "ListView", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementIntoViewAsync("nonexistentTarget", "ListView");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Target element not found", result.Error);
            _output.WriteLine($"Target not found test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementAsync_NoScrollableArea_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Element does not support scrolling"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("nonScrollableElement", "down", null, null, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("nonScrollableElement", "down");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element does not support scrolling", result.Error);
            _output.WriteLine($"Non-scrollable element test passed: {result.Error}");
        }

        [Fact]
        public async Task ScrollElementAsync_LargeDocument_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Large document scrolled successfully"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("largeDocument", "down", null, null, "DocumentViewer", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("largeDocument", "down", windowTitle: "DocumentViewer");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Large document scrolled successfully", result.Data);
            _output.WriteLine($"Large document scroll test passed: {result.Data}");
        }

        [Fact]
        public async Task ScrollElementIntoViewAsync_ListViewItem_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "List item scrolled into view"
            };
            _mockWorker.Setup(w => w.ScrollElementIntoViewAsync("listItem_42", "ListView", 5678, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementIntoViewAsync("listItem_42", "ListView", 5678);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("List item scrolled into view", result.Data);
            _mockWorker.Verify(w => w.ScrollElementIntoViewAsync("listItem_42", "ListView", 5678, 20), Times.Once);
            _output.WriteLine($"ListView item scroll test passed: {result.Data}");
        }

        [Fact]
        public async Task ScrollElementAsync_NestedScrollContainers_Success()
        {
            // Arrange
            var outerResult = new OperationResult<string>
            {
                Success = true,
                Data = "Outer container scrolled"
            };
            var innerResult = new OperationResult<string>
            {
                Success = true,
                Data = "Inner container scrolled"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("outerContainer", "down", null, null, null, null, 20))
                      .ReturnsAsync(outerResult);
            _mockWorker.Setup(w => w.ScrollElementAsync("innerContainer", "right", null, null, null, null, 20))
                      .ReturnsAsync(innerResult);

            // Act
            var outerScrollResult = await _service.ScrollElementAsync("outerContainer", "down");
            var innerScrollResult = await _service.ScrollElementAsync("innerContainer", "right");

            // Assert
            Assert.True(outerScrollResult.Success);
            Assert.True(innerScrollResult.Success);
            Assert.Equal("Outer container scrolled", outerScrollResult.Data);
            Assert.Equal("Inner container scrolled", innerScrollResult.Data);
            _output.WriteLine($"Nested containers test passed: {outerScrollResult.Data}, {innerScrollResult.Data}");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(25.0)]
        [InlineData(50.0)]
        [InlineData(75.0)]
        [InlineData(100.0)]
        public async Task ScrollElementAsync_PreciseHorizontalScroll_Success(double horizontalPercent)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Horizontally scrolled to {horizontalPercent}%"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("horizontalScroller", null, horizontalPercent, null, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("horizontalScroller", horizontal: horizontalPercent);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Horizontally scrolled to {horizontalPercent}%", result.Data);
            _output.WriteLine($"Horizontal scroll {horizontalPercent}% test passed: {result.Data}");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(25.0)]
        [InlineData(50.0)]
        [InlineData(75.0)]
        [InlineData(100.0)]
        public async Task ScrollElementAsync_PreciseVerticalScroll_Success(double verticalPercent)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Vertically scrolled to {verticalPercent}%"
            };
            _mockWorker.Setup(w => w.ScrollElementAsync("verticalScroller", null, null, verticalPercent, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ScrollElementAsync("verticalScroller", vertical: verticalPercent);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Vertically scrolled to {verticalPercent}%", result.Data);
            _output.WriteLine($"Vertical scroll {verticalPercent}% test passed: {result.Data}");
        }

        public void Dispose()
        {
            _mockWorker?.Object?.Dispose();
        }
    }
}
