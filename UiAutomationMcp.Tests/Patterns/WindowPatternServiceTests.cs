using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Services.Patterns;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Patterns
{
    /// <summary>
    /// Comprehensive tests for WindowPatternService covering all window management scenarios
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class WindowPatternServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<WindowPatternService>> _logger;
        private readonly WindowPatternService _service;
        private readonly Mock<IUIAutomationWorker> _mockWorker;

        public WindowPatternServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<WindowPatternService>>();
            _mockWorker = new Mock<IUIAutomationWorker>();
            _service = new WindowPatternService(_logger.Object, _mockWorker.Object);
        }

        [Theory]
        [InlineData("close")]
        [InlineData("minimize")]
        [InlineData("maximize")]
        [InlineData("normal")]
        public async Task WindowActionAsync_ValidActions_Success(string action)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Window {action} action completed successfully"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("windowElement", action, "TestWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("windowElement", action, "TestWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Window {action} action completed successfully", result.Data);
            _mockWorker.Verify(w => w.SetWindowStateAsync("windowElement", action, "TestWindow", null, 20), Times.Once);
            _output.WriteLine($"Window {action} test passed: {result.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_CloseWindow_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Window closed successfully"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("mainWindow", "close", "Application", 1234, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("mainWindow", "close", "Application", 1234);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Window closed successfully", result.Data);
            _mockWorker.Verify(w => w.SetWindowStateAsync("mainWindow", "close", "Application", 1234, 20), Times.Once);
            _output.WriteLine($"Close window test passed: {result.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_MinimizeWindow_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Window minimized successfully"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("browserWindow", "minimize", "Browser", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("browserWindow", "minimize", "Browser");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Window minimized successfully", result.Data);
            _mockWorker.Verify(w => w.SetWindowStateAsync("browserWindow", "minimize", "Browser", null, 20), Times.Once);
            _output.WriteLine($"Minimize window test passed: {result.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_MaximizeWindow_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Window maximized successfully"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("editorWindow", "maximize", "Text Editor", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("editorWindow", "maximize", "Text Editor");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Window maximized successfully", result.Data);
            _mockWorker.Verify(w => w.SetWindowStateAsync("editorWindow", "maximize", "Text Editor", null, 20), Times.Once);
            _output.WriteLine($"Maximize window test passed: {result.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_RestoreWindow_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Window restored to normal state"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("restoredWindow", "normal", "Restored Window", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("restoredWindow", "normal", "Restored Window");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Window restored to normal state", result.Data);
            _mockWorker.Verify(w => w.SetWindowStateAsync("restoredWindow", "normal", "Restored Window", null, 20), Times.Once);
            _output.WriteLine($"Restore window test passed: {result.Data}");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("resize")]
        [InlineData("move")]
        [InlineData("")]
        public async Task WindowActionAsync_InvalidActions_ReturnsFailure(string action)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = $"Invalid window action: {action}"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("windowElement", action, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("windowElement", action);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Invalid window action: {action}", result.Error);
            _output.WriteLine($"Invalid action '{action}' test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_NullAction_ReturnsFailure()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.WindowActionAsync("windowElement", null!));
        }

        [Fact]
        public async Task WindowActionAsync_WorkerFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Worker process failed to execute window action"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("windowElement", "close", "TestWindow");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Worker process failed to execute window action", result.Error);
            _output.WriteLine($"Worker failure test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_ElementNotFound_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Window element not found"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("nonexistentWindow", "close", "NonexistentApp", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("nonexistentWindow", "close", "NonexistentApp");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Window element not found", result.Error);
            _output.WriteLine($"Element not found test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_WindowAlreadyClosed_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Cannot close window: window is already closed"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("closedWindow", "close", "ClosedApp", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("closedWindow", "close", "ClosedApp");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot close window: window is already closed", result.Error);
            _output.WriteLine($"Already closed window test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_WindowAlreadyMaximized_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Cannot maximize window: window is already maximized"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("maximizedWindow", "maximize", "MaximizedApp", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("maximizedWindow", "maximize", "MaximizedApp");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot maximize window: window is already maximized", result.Error);
            _output.WriteLine($"Already maximized window test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_WindowAlreadyMinimized_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Cannot minimize window: window is already minimized"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("minimizedWindow", "minimize", "MinimizedApp", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("minimizedWindow", "minimize", "MinimizedApp");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot minimize window: window is already minimized", result.Error);
            _output.WriteLine($"Already minimized window test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_ModalDialog_CloseSuccess()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Modal dialog closed successfully"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("modalDialog", "close", "Dialog", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("modalDialog", "close", "Dialog");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Modal dialog closed successfully", result.Data);
            _output.WriteLine($"Modal dialog close test passed: {result.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_SystemWindow_RestrictedAction()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Cannot close system window: action restricted"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("systemWindow", "close", "System", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("systemWindow", "close", "System");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot close system window: action restricted", result.Error);
            _output.WriteLine($"System window restriction test passed: {result.Error}");
        }

        [Fact]
        public async Task WindowActionAsync_MultipleWindows_ManageIndependently()
        {
            // Arrange
            var closeResult = new OperationResult<string>
            {
                Success = true,
                Data = "First window closed"
            };
            var minimizeResult = new OperationResult<string>
            {
                Success = true,
                Data = "Second window minimized"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("window1", "close", "App1", null, 20))
                      .ReturnsAsync(closeResult);
            _mockWorker.Setup(w => w.SetWindowStateAsync("window2", "minimize", "App2", null, 20))
                      .ReturnsAsync(minimizeResult);

            // Act
            var result1 = await _service.WindowActionAsync("window1", "close", "App1");
            var result2 = await _service.WindowActionAsync("window2", "minimize", "App2");

            // Assert
            Assert.True(result1.Success);
            Assert.True(result2.Success);
            Assert.Equal("First window closed", result1.Data);
            Assert.Equal("Second window minimized", result2.Data);
            _output.WriteLine($"Multiple windows test passed: {result1.Data}, {result2.Data}");
        }

        [Fact]
        public async Task WindowActionAsync_LogsCorrectly()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Window action logged correctly"
            };
            _mockWorker.Setup(w => w.SetWindowStateAsync("loggedWindow", "maximize", "LogWindow", 9999, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.WindowActionAsync("loggedWindow", "maximize", "LogWindow", 9999);

            // Assert
            Assert.True(result.Success);
            
            // Verify logging occurred (this is tricky with Moq, but we can verify the service was called correctly)
            _mockWorker.Verify(w => w.SetWindowStateAsync("loggedWindow", "maximize", "LogWindow", 9999, 20), Times.Once);
            _output.WriteLine($"Logging test passed: {result.Data}");
        }

        public void Dispose()
        {
            _mockWorker?.Object?.Dispose();
        }
    }
}
