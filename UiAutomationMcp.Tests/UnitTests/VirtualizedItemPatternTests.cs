using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Abstractions;

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
        private readonly Mock<IProcessManager> _mockProcessManager;
        private readonly VirtualizedItemService _service;

        public VirtualizedItemPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<VirtualizedItemService>>();
            _mockProcessManager = new Mock<IProcessManager>();
            
            _service = new VirtualizedItemService(_mockProcessManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RealizeItemAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "virtualizedItem1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = elementId,
                        Name = "Realized Item"
                    }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", It.IsAny<RealizeVirtualizedItemRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.RealizeItemAsync(automationId: elementId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", 
                It.Is<RealizeVirtualizedItemRequest>(r => 
                    r.AutomationId == elementId), 30), Times.Once);
            
            _output.WriteLine("RealizeItemAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task RealizeItemAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "item1";
            var expectedResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = elementId,
                        Name = "Realized Item"
                    }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", It.IsAny<RealizeVirtualizedItemRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.RealizeItemAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", 
                It.Is<RealizeVirtualizedItemRequest>(r => 
                    r.AutomationId == elementId &&
                    r.WindowTitle == "" &&
                    r.ProcessId == 0), 30), Times.Once);
        }

        [Fact]
        public async Task RealizeItemAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var elementId = "item1";
            var exceptionMessage = "Failed to realize item";

            _mockProcessManager.Setup(e => e.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", It.IsAny<RealizeVirtualizedItemRequest>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.RealizeItemAsync(elementId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to realize virtualized item")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
