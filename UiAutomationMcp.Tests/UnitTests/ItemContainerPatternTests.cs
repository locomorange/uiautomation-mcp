using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Mock-based tests for ItemContainerService
    /// Tests the service layer without direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ItemContainerPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<ItemContainerService>> _mockLogger;
        private readonly Mock<IOperationExecutor> _mockProcessManager;
        private readonly ItemContainerService _service;

        public ItemContainerPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<ItemContainerService>>();
            _mockProcessManager = new Mock<IOperationExecutor>();
            
            _service = new ItemContainerService(_mockLogger.Object, _mockProcessManager.Object);
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "container1";
            var propertyName = "Name";
            var value = "TestItem";
            var startAfterId = "item0";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new ElementSearchResult {
                Success = true,
                OperationName = "FindItem",
                Metadata = new Dictionary<string, object> { { "AutomationId", "item1" }, { "Name", "TestItem" } }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(automationId: containerId, propertyName: propertyName, value: value, startAfterId: startAfterId, controlType: windowTitle, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", 
                It.Is<FindItemByPropertyRequest>(p => 
                    p.ContainerId == containerId &&
                    p.PropertyName == propertyName &&
                    p.Value == value &&
                    p.StartAfterId == startAfterId &&
                    p.WindowTitle == windowTitle &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("FindItemByPropertyAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var containerId = "container1";
            var expectedResult = new ElementSearchResult {
                Success = true,
                OperationName = "FindItem",
                Metadata = new Dictionary<string, object> { { "AutomationId", "anyItem" } }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(automationId: containerId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", 
                It.Is<FindItemByPropertyRequest>(p => 
                    p.ContainerId == containerId &&
                    p.PropertyName == "" &&
                    p.Value == "" &&
                    p.StartAfterId == "" &&
                    p.WindowTitle == "" &&
                    p.ProcessId == 0), 30), Times.Once);
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var containerId = "container1";
            var exceptionMessage = "Failed to find item";

            _mockProcessManager.Setup(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.FindItemByPropertyAsync(automationId: containerId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to find item in container")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task FindItemByPropertyAsync_ShouldLogInformationOnSuccess()
        {
            // Arrange
            var containerId = "container1";
            var propertyName = "Name";
            var value = "TestItem";
            var expectedResult = new ElementSearchResult {
                Success = true,
                OperationName = "FindItem",
                Metadata = new Dictionary<string, object> { { "Found", true } }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            await _service.FindItemByPropertyAsync(automationId: containerId, propertyName: propertyName, value: value);

            // Assert
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Finding item in container: {containerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Item search completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
