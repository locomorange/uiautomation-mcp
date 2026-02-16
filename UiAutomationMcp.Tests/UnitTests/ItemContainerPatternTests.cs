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
    /// Mock-based tests for ItemContainerService (FindItemByProperty)
    /// Tests the service layer without direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ItemContainerPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<ItemContainerService>> _mockLogger;
        private readonly Mock<IProcessManager> _mockProcessManager;
        private readonly ItemContainerService _service;

        public ItemContainerPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<ItemContainerService>>();
            _mockProcessManager = new Mock<IProcessManager>();

            _service = new ItemContainerService(_mockProcessManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithAutomationId_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "dataGrid1";
            var expectedResult = new FindItemResult
            {
                Success = true,
                FoundElement = new UIAutomationMCP.Models.ElementInfo
                {
                    AutomationId = "row_5",
                    Name = "Found Item"
                },
                TotalMatches = 1
            };

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                automationId: containerId,
                propertyName: "Name",
                value: "Found Item",
                timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockProcessManager.Verify(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                It.Is<FindItemByPropertyRequest>(r =>
                    r.AutomationId == containerId &&
                    r.PropertyName == "Name" &&
                    r.Value == "Found Item"), 30), Times.Once);

            _output.WriteLine("FindItemByPropertyAsync with AutomationId test passed");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithName_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerName = "Employee List";
            var expectedResult = new FindItemResult
            {
                Success = true,
                FoundElement = new UIAutomationMCP.Models.ElementInfo
                {
                    AutomationId = "item_1",
                    Name = "John Doe"
                },
                TotalMatches = 1
            };

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                name: containerName,
                propertyName: "AutomationId",
                value: "item_1",
                timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockProcessManager.Verify(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                It.Is<FindItemByPropertyRequest>(r =>
                    r.Name == containerName &&
                    r.PropertyName == "AutomationId" &&
                    r.Value == "item_1"), 30), Times.Once);

            _output.WriteLine("FindItemByPropertyAsync with Name test passed");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithPagination_ShouldPassStartAfterId()
        {
            // Arrange
            var containerId = "listView1";
            var startAfterId = "item_10";
            var expectedResult = new FindItemResult
            {
                Success = true,
                FoundElement = new UIAutomationMCP.Models.ElementInfo
                {
                    AutomationId = "item_11",
                    Name = "Next Item"
                },
                TotalMatches = 1
            };

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                automationId: containerId,
                propertyName: "Name",
                value: "Next Item",
                startAfterId: startAfterId,
                timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockProcessManager.Verify(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                It.Is<FindItemByPropertyRequest>(r =>
                    r.AutomationId == containerId &&
                    r.StartAfterId == startAfterId), 30), Times.Once);

            _output.WriteLine("FindItemByPropertyAsync pagination test passed");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithEmptyPropertyName_ShouldSearchAllProperties()
        {
            // Arrange
            var containerId = "dataGrid1";
            var expectedResult = new FindItemResult
            {
                Success = true,
                FoundElement = new UIAutomationMCP.Models.ElementInfo
                {
                    AutomationId = "row_1",
                    Name = "First Row"
                },
                TotalMatches = 1
            };

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                automationId: containerId,
                propertyName: null,
                value: "First Row",
                timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockProcessManager.Verify(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                It.Is<FindItemByPropertyRequest>(r =>
                    r.PropertyName == ""), 30), Times.Once);

            _output.WriteLine("FindItemByPropertyAsync empty property name test passed");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WhenExecutorReturnsError_ShouldReturnError()
        {
            // Arrange
            var containerId = "grid1";
            var errorMessage = "ItemContainerPattern not supported";

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromError(errorMessage)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                automationId: containerId,
                propertyName: "Name",
                value: "Test");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(errorMessage, result.ErrorMessage);

            _output.WriteLine("FindItemByPropertyAsync error handling test passed");
        }

        [Fact]
        public async Task FindItemByPropertyAsync_WithWindowHandle_ShouldPassToRequest()
        {
            // Arrange
            long windowHandle = 0x12345;
            var expectedResult = new FindItemResult
            {
                Success = true,
                FoundElement = new UIAutomationMCP.Models.ElementInfo
                {
                    AutomationId = "item_1",
                    Name = "Item 1"
                },
                TotalMatches = 1
            };

            _mockProcessManager.Setup(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                    "FindItemByProperty", It.IsAny<FindItemByPropertyRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<FindItemResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _service.FindItemByPropertyAsync(
                automationId: "list1",
                propertyName: "Name",
                value: "Item 1",
                windowHandle: windowHandle,
                timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockProcessManager.Verify(e => e.ExecuteWorkerOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                It.Is<FindItemByPropertyRequest>(r =>
                    r.WindowHandle == windowHandle), 30), Times.Once);

            _output.WriteLine("FindItemByPropertyAsync with WindowHandle test passed");
        }

        public void Dispose()
        {
            // No cleanup needed for mocks
        }
    }
}
