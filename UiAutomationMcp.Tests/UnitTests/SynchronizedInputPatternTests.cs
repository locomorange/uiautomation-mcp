using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
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
            var expectedResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = elementId,
                        Name = $"Listening for {inputType}"
                    }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<StartSynchronizedInputRequest, ElementSearchResult>("StartSynchronizedInput", It.IsAny<StartSynchronizedInputRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.StartListeningAsync(automationId: elementId, inputType: inputType, name: windowTitle, processId: processId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<StartSynchronizedInputRequest, ElementSearchResult>("StartSynchronizedInput",
                It.Is<StartSynchronizedInputRequest>(r => 
                    r.AutomationId == elementId &&
                    r.InputType == inputType &&
                    r.WindowTitle == windowTitle &&
                    r.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine($"StartListeningAsync service test passed for {inputType}");
        }

        [Fact]
        public async Task CancelAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "element1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = elementId,
                        Name = "Listening canceled"
                    }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<CancelSynchronizedInputRequest, ElementSearchResult>("CancelSynchronizedInput", It.IsAny<CancelSynchronizedInputRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.CancelAsync(automationId: elementId, name: windowTitle, processId: processId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<CancelSynchronizedInputRequest, ElementSearchResult>("CancelSynchronizedInput",
                It.Is<CancelSynchronizedInputRequest>(r => 
                    r.AutomationId == elementId &&
                    r.WindowTitle == windowTitle &&
                    r.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("CancelAsync service test passed");
        }

        [Fact]
        public async Task StartListeningAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "element1";
            var inputType = "KeyUp";

            _mockExecutor.Setup(e => e.ExecuteAsync<StartSynchronizedInputRequest, ElementSearchResult>("StartSynchronizedInput", It.IsAny<StartSynchronizedInputRequest>(), 30))
                .Returns(Task.FromResult(new ElementSearchResult { Success = true }));

            // Act
            var result = await _service.StartListeningAsync(elementId, null, inputType, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<StartSynchronizedInputRequest, ElementSearchResult>("StartSynchronizedInput",
                It.Is<StartSynchronizedInputRequest>(r => 
                    r.AutomationId == elementId &&
                    r.InputType == inputType &&
                    r.WindowTitle == "" &&
                    r.ProcessId == 0), 30), Times.Once);
        }

        [Fact]
        public async Task StartListeningAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "element1";
            var inputType = "KeyDown";
            var exceptionMessage = "Failed to start listening";

            _mockExecutor.Setup(e => e.ExecuteAsync<StartSynchronizedInputRequest, ElementSearchResult>("StartSynchronizedInput", It.IsAny<StartSynchronizedInputRequest>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.StartListeningAsync(elementId, inputType);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to start synchronized input listening")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "element1";
            var exceptionMessage = "Failed to cancel";

            _mockExecutor.Setup(e => e.ExecuteAsync<CancelSynchronizedInputRequest, ElementSearchResult>("CancelSynchronizedInput", It.IsAny<CancelSynchronizedInputRequest>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.CancelAsync(elementId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to cancel synchronized input")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}