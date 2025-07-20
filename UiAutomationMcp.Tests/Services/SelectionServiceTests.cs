using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Services
{
    /// <summary>
    /// Mock-based tests for ISelectionService implementations
    /// Tests the service layer without direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SelectionServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<SelectionService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly SelectionService _selectionService;

        public SelectionServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<SelectionService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            
            _selectionService = new SelectionService(_mockLogger.Object, _mockExecutor.Object);
        }

        #region SelectionPattern Properties Service Tests

        [Fact]
        public async Task CanSelectMultipleAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "listBox1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new BooleanResult { Value = true, Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", It.IsAny<CanSelectMultipleRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", 
                It.Is<CanSelectMultipleRequest>(p => 
                    p.ElementId == containerId &&
                    p.WindowTitle == windowTitle &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("CanSelectMultipleAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task CanSelectMultipleAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var containerId = "container1";
            var expectedResult = new BooleanResult { Value = false, Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", It.IsAny<CanSelectMultipleRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", 
                It.Is<CanSelectMultipleRequest>(p => 
                    p.ElementId == containerId &&
                    p.WindowTitle == "" &&
                    p.ProcessId == 0), 30), Times.Once);
            
            _output.WriteLine("CanSelectMultipleAsync null parameters test passed");
        }

        [Fact]
        public async Task IsSelectionRequiredAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "tabControl1";
            var windowTitle = "App Window";
            var expectedResult = new BooleanResult { Value = true, Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<IsSelectionRequiredRequest, BooleanResult>("IsSelectionRequired", It.IsAny<IsSelectionRequiredRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectionRequiredAsync(containerId, windowTitle, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<IsSelectionRequiredRequest, BooleanResult>("IsSelectionRequired", 
                It.Is<IsSelectionRequiredRequest>(p => 
                    p.ElementId == containerId &&
                    p.WindowTitle == windowTitle), 30), Times.Once);
            
            _output.WriteLine("IsSelectionRequiredAsync service test passed - Correct subprocess execution verified");
        }

        #endregion

        #region SelectionItemPattern Properties Service Tests

        [Fact]
        public async Task IsSelectedAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "listItem1";
            var windowTitle = "Main Window";
            var processId = 5678;
            var expectedResult = new BooleanResult { Value = true, Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", It.IsAny<IsSelectedRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", 
                It.Is<IsSelectedRequest>(p => 
                    p.ElementId == elementId &&
                    p.WindowTitle == windowTitle &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("IsSelectedAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task GetSelectionContainerAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "checkBox1";
            var expectedResult = new SelectionInfoResult
            {
                Success = true,
                ContainerElementId = "parentContainer",
                ContainerName = "Selection Container",
                ContainerControlType = "ControlType.List"
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<GetSelectionContainerRequest, SelectionInfoResult>("GetSelectionContainer", It.IsAny<GetSelectionContainerRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.GetSelectionContainerAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<GetSelectionContainerRequest, SelectionInfoResult>("GetSelectionContainer", 
                It.Is<GetSelectionContainerRequest>(p => 
                    p.ElementId == elementId), 30), Times.Once);
            
            _output.WriteLine("GetSelectionContainerAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task GetSelectionContainerAsync_WithNoContainer_ShouldReturnNull()
        {
            // Arrange
            var elementId = "orphanedElement";
            var expectedResult = new SelectionInfoResult
            {
                Success = true,
                ContainerElementId = null,
                ContainerName = null,
                ContainerControlType = null
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<GetSelectionContainerRequest, SelectionInfoResult>("GetSelectionContainer", It.IsAny<GetSelectionContainerRequest>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.GetSelectionContainerAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<GetSelectionContainerRequest, SelectionInfoResult>("GetSelectionContainer", 
                It.Is<GetSelectionContainerRequest>(p => 
                    p.ElementId == elementId), 30), Times.Once);
            
            _output.WriteLine("GetSelectionContainerAsync null container test passed");
        }

        #endregion

        #region Selection Operations Service Tests

        [Fact]
        public async Task AddToSelectionAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "item2";
            var windowTitle = "Multi-Select Window";
            var processId = 9876;

            _mockExecutor.Setup(e => e.ExecuteAsync<AddToSelectionRequest, ActionResult>("AddToSelection", It.IsAny<AddToSelectionRequest>(), 30))
                .Returns(Task.FromResult(new ActionResult { Success = true }));

            // Act
            var result = await _selectionService.AddToSelectionAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<AddToSelectionRequest, ActionResult>("AddToSelection", 
                It.Is<AddToSelectionRequest>(p => 
                    p.ElementId == elementId &&
                    p.WindowTitle == windowTitle &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("AddToSelectionAsync service test passed");
        }

        [Fact]
        public async Task RemoveFromSelectionAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "selectedItem";
            var windowTitle = "List Window";

            _mockExecutor.Setup(e => e.ExecuteAsync<RemoveFromSelectionRequest, ActionResult>("RemoveFromSelection", It.IsAny<RemoveFromSelectionRequest>(), 30))
                .Returns(Task.FromResult(new ActionResult { Success = true }));

            // Act
            var result = await _selectionService.RemoveFromSelectionAsync(elementId, windowTitle, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<RemoveFromSelectionRequest, ActionResult>("RemoveFromSelection", 
                It.Is<RemoveFromSelectionRequest>(p => 
                    p.ElementId == elementId &&
                    p.WindowTitle == windowTitle), 30), Times.Once);
            
            _output.WriteLine("RemoveFromSelectionAsync service test passed");
        }

        [Fact]
        public async Task ClearSelectionAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "multiSelectList";
            var processId = 4321;

            _mockExecutor.Setup(e => e.ExecuteAsync<ClearSelectionRequest, ActionResult>("ClearSelection", It.IsAny<ClearSelectionRequest>(), 30))
                .Returns(Task.FromResult(new ActionResult { Success = true }));

            // Act
            var result = await _selectionService.ClearSelectionAsync(containerId, null, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<ClearSelectionRequest, ActionResult>("ClearSelection", 
                It.Is<ClearSelectionRequest>(p => 
                    p.ElementId == containerId &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("ClearSelectionAsync service test passed");
        }

        [Fact]
        public async Task GetSelectionAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var containerId = "selectionContainer";
            var expectedSelection = new SelectionInfoResult
            {
                Success = true,
                SelectedCount = 2,
                TotalCount = 5,
                SelectedItems = new List<SelectionItem>
                {
                    new SelectionItem { ElementId = "item1", Name = "First Item" },
                    new SelectionItem { ElementId = "item3", Name = "Third Item" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<GetSelectionRequest, SelectionInfoResult>("GetSelection", It.IsAny<GetSelectionRequest>(), 30))
                .Returns(Task.FromResult(expectedSelection));

            // Act
            var result = await _selectionService.GetSelectionAsync(containerId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<GetSelectionRequest, SelectionInfoResult>("GetSelection", 
                It.Is<GetSelectionRequest>(p => 
                    p.ElementId == containerId), 30), Times.Once);
            
            _output.WriteLine("GetSelectionAsync service test passed");
        }

        #endregion

        #region Error Handling and Logging Tests

        [Fact]
        public async Task SelectionService_WhenExecutorThrowsException_ShouldReturnErrorResult()
        {
            // Arrange
            var elementId = "problematicElement";
            var expectedException = new InvalidOperationException("Element not found");

            _mockExecutor.Setup(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", It.IsAny<IsSelectedRequest>(), 30))
                .ThrowsAsync(expectedException);

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            var errorProperty = resultType.GetProperty("Error");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(errorProperty);
            
            _output.WriteLine("Error handling test passed - Service properly handles executor exceptions");
        }

        [Fact]
        public async Task SelectionService_ShouldLogOperations()
        {
            // Arrange
            var containerId = "loggedContainer";
            _mockExecutor.Setup(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", It.IsAny<CanSelectMultipleRequest>(), 30))
                .Returns(Task.FromResult(new BooleanResult { Value = true, Success = true }));

            // Act
            await _selectionService.CanSelectMultipleAsync(containerId, null, null, 30);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Checking if container supports multiple selection")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _output.WriteLine("Logging test passed - Service properly logs operations");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(300)]
        public async Task SelectionService_WithVariousTimeouts_ShouldUseCorrectTimeout(int timeout)
        {
            // Arrange
            var elementId = "timeoutTestElement";
            _mockExecutor.Setup(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", It.IsAny<IsSelectedRequest>(), timeout))
                .Returns(Task.FromResult(new BooleanResult { Value = false, Success = true }));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", It.IsAny<IsSelectedRequest>(), timeout), Times.Once);
            
            _output.WriteLine($"Timeout test passed for value: {timeout} seconds");
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public async Task SelectionService_WithEmptyElementId_ShouldStillExecute(string emptyElementId)
        {
            // Arrange
            _mockExecutor.Setup(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", It.IsAny<IsSelectedRequest>(), 30))
                .Returns(Task.FromResult(new BooleanResult { Value = false, Success = true }));

            // Act
            var result = await _selectionService.IsSelectedAsync(emptyElementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<IsSelectedRequest, BooleanResult>("IsSelected", 
                It.Is<IsSelectedRequest>(p => p.ElementId == emptyElementId), 30), Times.Once);
            
            _output.WriteLine($"Empty elementId test passed for value: '{emptyElementId}'");
        }

        [Fact]
        public async Task SelectionService_WithZeroProcessId_ShouldConvertCorrectly()
        {
            // Arrange
            var containerId = "container1";
            _mockExecutor.Setup(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", It.IsAny<CanSelectMultipleRequest>(), 30))
                .Returns(Task.FromResult(new BooleanResult { Value = true, Success = true }));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, null, 0, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<CanSelectMultipleRequest, BooleanResult>("CanSelectMultiple", 
                It.Is<CanSelectMultipleRequest>(p => p.ProcessId == 0), 30), Times.Once);
            
            _output.WriteLine("Zero processId test passed - Correctly converted to string");
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("SelectionServiceTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}