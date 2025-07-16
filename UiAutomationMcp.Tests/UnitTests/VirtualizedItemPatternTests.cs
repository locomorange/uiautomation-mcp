using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Mock-based tests for VirtualizedItemService
    /// Tests the service layer without direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class VirtualizedItemPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<VirtualizedItemService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly VirtualizedItemService _service;

        public VirtualizedItemPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<VirtualizedItemService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            
            _service = new VirtualizedItemService(_mockLogger.Object, _mockExecutor.Object);
        }

        [Fact]
        public async Task RealizeItemAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "virtualizedItem1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new { Success = true, Message = "Item realized successfully" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("RealizeVirtualizedItem", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.RealizeItemAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("RealizeVirtualizedItem", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine("RealizeItemAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task RealizeItemAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "item1";
            var expectedResult = new { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("RealizeVirtualizedItem", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.RealizeItemAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("RealizeVirtualizedItem", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == "" &&
                    p["processId"].ToString() == "0"), 30), Times.Once);
        }

        [Fact]
        public async Task RealizeItemAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "item1";
            var exceptionMessage = "Failed to realize item";

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("RealizeVirtualizedItem", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.RealizeItemAsync(elementId);

            // Assert
            Assert.NotNull(result);
            dynamic response = result;
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Error);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to realize virtualized item")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}