using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Services.Patterns;

namespace UiAutomationMcp.Tests.Patterns
{
    /// <summary>
    /// CorePatternServiceのユニットテスト
    /// UIAutomationWorkerをモックして依存関係を排除
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class CorePatternServiceTests : IDisposable
    {
        private readonly Mock<ILogger<CorePatternService>> _mockLogger;
        private readonly Mock<IUIAutomationWorker> _mockWorker;
        private readonly CorePatternService _service;

        public CorePatternServiceTests()
        {
            _mockLogger = new Mock<ILogger<CorePatternService>>();
            _mockWorker = new Mock<IUIAutomationWorker>();
            _service = new CorePatternService(_mockLogger.Object, _mockWorker.Object);
        }

        public void Dispose()
        {
            _mockWorker?.Object?.Dispose();
        }

        [Fact]
        public async Task InvokeElementAsync_ShouldReturnSuccess_WhenWorkerReturnsSuccess()
        {
            // Arrange
            var elementId = "TestElement";
            var windowTitle = "TestWindow";
            var processId = 12345;
            var workerResult = new OperationResult<string> { Success = true, Data = "Invoked successfully" };

            _mockWorker.Setup(w => w.InvokeElementAsync(elementId, windowTitle, processId, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.InvokeElementAsync(elementId, windowTitle, processId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Invoked successfully", result.Data);
            Assert.Null(result.Error);

            _mockWorker.Verify(w => w.InvokeElementAsync(elementId, windowTitle, processId, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task InvokeElementAsync_ShouldReturnFailure_WhenWorkerReturnsFailure()
        {
            // Arrange
            var elementId = "TestElement";
            var errorMessage = "Element not found";
            var workerResult = new OperationResult<string> { Success = false, Error = errorMessage };

            _mockWorker.Setup(w => w.InvokeElementAsync(elementId, null, null, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.InvokeElementAsync(elementId);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.Error);
        }

        [Fact]
        public async Task SetElementValueAsync_ShouldReturnSuccess_WhenWorkerReturnsSuccess()
        {
            // Arrange
            var elementId = "TestInput";
            var value = "Test Value";
            var workerResult = new OperationResult<string> { Success = true, Data = "Value set successfully" };

            _mockWorker.Setup(w => w.SetElementValueAsync(elementId, value, null, null, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.SetElementValueAsync(elementId, value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Value set successfully", result.Data);

            _mockWorker.Verify(w => w.SetElementValueAsync(elementId, value, null, null, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetElementValueAsync_ShouldReturnValue_WhenWorkerReturnsValue()
        {
            // Arrange
            var elementId = "TestInput";
            var expectedValue = "Current Value";
            var workerResult = new OperationResult<string> { Success = true, Data = expectedValue };

            _mockWorker.Setup(w => w.GetElementValueAsync(elementId, null, null, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.GetElementValueAsync(elementId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedValue, result.Data);
        }

        [Fact]
        public async Task ToggleElementAsync_ShouldReturnSuccess_WhenWorkerReturnsSuccess()
        {
            // Arrange
            var elementId = "TestCheckbox";
            var workerResult = new OperationResult<string> { Success = true, Data = "Toggled successfully" };

            _mockWorker.Setup(w => w.ToggleElementAsync(elementId, null, null, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.ToggleElementAsync(elementId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Toggled successfully", result.Data);
        }

        [Fact]
        public async Task SelectElementAsync_ShouldReturnSuccess_WhenWorkerReturnsSuccess()
        {
            // Arrange
            var elementId = "TestListItem";
            var workerResult = new OperationResult<string> { Success = true, Data = "Selected successfully" };

            _mockWorker.Setup(w => w.SelectElementAsync(elementId, null, null, It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.SelectElementAsync(elementId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Selected successfully", result.Data);
        }

        [Theory]
        [InlineData("btn1", "MainWindow", 1234)]
        [InlineData("btn2", null, null)]
        [InlineData("btn3", "", 0)]
        public async Task InvokeElementAsync_ShouldHandleDifferentParameters(string elementId, string? windowTitle, int? processId)
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Success" };
            _mockWorker.Setup(w => w.InvokeElementAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(workerResult);

            // Act
            var result = await _service.InvokeElementAsync(elementId, windowTitle, processId);

            // Assert
            Assert.True(result.Success);
            _mockWorker.Verify(w => w.InvokeElementAsync(elementId, windowTitle, processId, It.IsAny<int>()), Times.Once);
        }
    }
}
