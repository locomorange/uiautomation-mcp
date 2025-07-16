using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Mock-based tests for SynchronizedInputService
    /// Tests the service layer without direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SynchronizedInputPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<SynchronizedInputService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly SynchronizedInputService _service;

        public SynchronizedInputPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<SynchronizedInputService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            
            _service = new SynchronizedInputService(_mockLogger.Object, _mockExecutor.Object);
        }

        [Theory]
        [InlineData("KeyUp")]
        [InlineData("KeyDown")]
        [InlineData("LeftMouseUp")]
        [InlineData("LeftMouseDown")]
        [InlineData("RightMouseUp")]
        [InlineData("RightMouseDown")]
        public async Task StartListeningAsync_WhenCalled_ShouldExecuteCorrectOperation(string inputType)
        {
            // Arrange
            var elementId = "element1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new { Success = true, Message = $"Listening for {inputType}" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("StartSynchronizedInput", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.StartListeningAsync(elementId, inputType, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("StartSynchronizedInput", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["inputType"].ToString() == inputType &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine($"StartListeningAsync service test passed for {inputType}");
        }

        [Fact]
        public async Task CancelAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "element1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new { Success = true, Message = "Listening canceled" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CancelSynchronizedInput", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.CancelAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("CancelSynchronizedInput", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine("CancelAsync service test passed");
        }

        [Fact]
        public async Task StartListeningAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "element1";
            var inputType = "KeyUp";

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("StartSynchronizedInput", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(new object());

            // Act
            var result = await _service.StartListeningAsync(elementId, inputType, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("StartSynchronizedInput", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["inputType"].ToString() == inputType &&
                    p["windowTitle"].ToString() == "" &&
                    p["processId"].ToString() == "0"), 30), Times.Once);
        }

        [Fact]
        public async Task StartListeningAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "element1";
            var inputType = "KeyDown";
            var exceptionMessage = "Failed to start listening";

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("StartSynchronizedInput", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.StartListeningAsync(elementId, inputType);

            // Assert
            Assert.NotNull(result);
            dynamic response = result;
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Error);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to start synchronized input listening")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "element1";
            var exceptionMessage = "Failed to cancel";

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CancelSynchronizedInput", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.CancelAsync(elementId);

            // Assert
            Assert.NotNull(result);
            dynamic response = result;
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Error);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to cancel synchronized input")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}