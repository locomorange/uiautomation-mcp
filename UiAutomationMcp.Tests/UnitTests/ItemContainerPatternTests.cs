using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using Xunit;
using Xunit.Abstractions;

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
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly ItemContainerService _service;

        public ItemContainerPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<ItemContainerService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            
            _service = new ItemContainerService(_mockLogger.Object, _mockExecutor.Object);
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
            var expectedResult = new { AutomationId = "item1", Name = "TestItem" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("FindItemByProperty", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.FindItemByPropertyAsync(containerId, propertyName, value, startAfterId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("FindItemByProperty", 
                It.Is<Dictionary<string, object>>(p => 
                    p["containerId"].ToString() == containerId &&
                    p["propertyName"].ToString() == propertyName &&
                    p["value"].ToString() == value &&
                    p["startAfterId"].ToString() == startAfterId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine("FindItemByPropertyAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var containerId = "container1";
            var expectedResult = new { AutomationId = "anyItem" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("FindItemByProperty", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.FindItemByPropertyAsync(containerId, null, null, null, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("FindItemByProperty", 
                It.Is<Dictionary<string, object>>(p => 
                    p["containerId"].ToString() == containerId &&
                    p["propertyName"].ToString() == "" &&
                    p["value"].ToString() == "" &&
                    p["startAfterId"].ToString() == "" &&
                    p["windowTitle"].ToString() == "" &&
                    p["processId"].ToString() == "0"), 30), Times.Once);
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WhenExecutorThrows_ShouldReturnError()
        {
            // Arrange
            var containerId = "container1";
            var exceptionMessage = "Failed to find item";

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("FindItemByProperty", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _service.FindItemByPropertyAsync(containerId);

            // Assert
            Assert.NotNull(result);
            dynamic response = result;
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Error);
            
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
            var expectedResult = new { Found = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("FindItemByProperty", It.IsAny<Dictionary<string, object>>(), 30))
                .ReturnsAsync(expectedResult);

            // Act
            await _service.FindItemByPropertyAsync(containerId, propertyName, value);

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