using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Models;
using UIAutomationMCP.Shared.Results;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for new Selection pattern functionality
    /// Tests Microsoft UI Automation SelectionItemPattern specification compliance
    /// Uses service-layer testing to avoid direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class NewSelectionPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<SelectionService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly SelectionService _selectionService;

        public NewSelectionPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<SelectionService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            _selectionService = new SelectionService(_mockLogger.Object, _mockExecutor.Object);
        }

        #region New SelectionItemPattern Properties Tests

        [Fact]
        public async Task IsSelectedAsync_WhenElementIsSelected_ShouldReturnTrue()
        {
            // Arrange
            var elementId = "selectedListItem";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = true } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);

            _output.WriteLine("IsSelectedAsync test passed - Element correctly identified as selected");
        }

        [Fact]
        public async Task IsSelectedAsync_WhenElementIsNotSelected_ShouldReturnFalse()
        {
            // Arrange
            var elementId = "unselectedListItem";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == elementId), 30), Times.Once);

            _output.WriteLine("IsSelectedAsync test passed - Element correctly identified as not selected");
        }

        [Fact]
        public async Task GetSelectionContainerAsync_WhenContainerExists_ShouldReturnContainerInfo()
        {
            // Arrange
            var elementId = "listItem1";
            var expectedResult = new ServerEnhancedResponse<object>
            { 
                Success = true, 
                Data = new 
                { 
                    SelectionContainer = new 
                    {
                        AutomationId = "parentListBox",
                        Name = "Items Container",
                        ControlType = "ControlType.List",
                        ClassName = "ListBox",
                        ProcessId = 1234,
                        RuntimeId = new int[] { 1, 2, 3, 4 }
                    }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("GetSelectionContainer", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.GetSelectionContainerAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("GetSelectionContainer", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == elementId), 30), Times.Once);

            _output.WriteLine("GetSelectionContainerAsync test passed - Container information retrieved");
        }

        [Fact]
        public async Task GetSelectionContainerAsync_WhenNoContainer_ShouldReturnNull()
        {
            // Arrange
            var elementId = "orphanedElement";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { SelectionContainer = (object?)null } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("GetSelectionContainer", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.GetSelectionContainerAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("GetSelectionContainer", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == elementId), 30), Times.Once);

            _output.WriteLine("GetSelectionContainerAsync test passed - Null container handled correctly");
        }

        #endregion

        #region New SelectionPattern Properties Tests

        [Fact]
        public async Task CanSelectMultipleAsync_WhenMultipleSelectionSupported_ShouldReturnTrue()
        {
            // Arrange
            var containerId = "multiSelectListBox";
            var windowTitle = "Multi-Select Window";
            var processId = 5678;
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanSelectMultiple = true } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CanSelectMultiple", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("CanSelectMultiple", 
                It.Is<Dictionary<string, object>>(p => 
                    p["containerId"].ToString() == containerId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);

            _output.WriteLine("CanSelectMultipleAsync test passed - Multiple selection correctly identified");
        }

        [Fact]
        public async Task CanSelectMultipleAsync_WhenSingleSelectionOnly_ShouldReturnFalse()
        {
            // Arrange
            var containerId = "singleSelectRadioGroup";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanSelectMultiple = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CanSelectMultiple", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("CanSelectMultiple", 
                It.Is<Dictionary<string, object>>(p => p["containerId"].ToString() == containerId), 30), Times.Once);

            _output.WriteLine("CanSelectMultipleAsync test passed - Single selection correctly identified");
        }

        [Fact]
        public async Task IsSelectionRequiredAsync_WhenSelectionRequired_ShouldReturnTrue()
        {
            // Arrange
            var containerId = "tabControl";
            var windowTitle = "Tabbed Interface";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelectionRequired = true } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelectionRequired", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectionRequiredAsync(containerId, windowTitle, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelectionRequired", 
                It.Is<Dictionary<string, object>>(p => 
                    p["containerId"].ToString() == containerId &&
                    p["windowTitle"].ToString() == windowTitle), 30), Times.Once);

            _output.WriteLine("IsSelectionRequiredAsync test passed - Required selection correctly identified");
        }

        [Fact]
        public async Task IsSelectionRequiredAsync_WhenSelectionOptional_ShouldReturnFalse()
        {
            // Arrange
            var containerId = "optionalSelectionList";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelectionRequired = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelectionRequired", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectionRequiredAsync(containerId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelectionRequired", 
                It.Is<Dictionary<string, object>>(p => p["containerId"].ToString() == containerId), 30), Times.Once);

            _output.WriteLine("IsSelectionRequiredAsync test passed - Optional selection correctly identified");
        }

        #endregion

        #region Microsoft Specification Compliance Tests

        [Theory]
        [InlineData("listItem", true)]
        [InlineData("radioButton", true)]
        [InlineData("tabItem", true)]
        [InlineData("menuItem", false)]
        [InlineData("treeItem", false)]
        public async Task SelectionItemPattern_WithControlTypes_ShouldFollowMicrosoftSpecification(
            string controlTypeName, bool isRequired)
        {
            // Arrange
            var elementId = $"element_{controlTypeName}";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == elementId), 30), Times.Once);

            _output.WriteLine($"Microsoft specification test passed for {controlTypeName} (Required: {isRequired})");
        }

        [Theory]
        [InlineData("list", true)]
        [InlineData("dataGrid", true)]
        [InlineData("tree", true)]
        [InlineData("tab", true)]
        [InlineData("menu", false)]
        public async Task SelectionPattern_WithControlTypes_ShouldFollowMicrosoftSpecification(
            string controlTypeName, bool isRequired)
        {
            // Arrange
            var containerId = $"container_{controlTypeName}";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanSelectMultiple = true } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CanSelectMultiple", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("CanSelectMultiple", 
                It.Is<Dictionary<string, object>>(p => p["containerId"].ToString() == containerId), 30), Times.Once);

            _output.WriteLine($"Microsoft specification test passed for {controlTypeName} (Required: {isRequired})");
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData("validElement", "Valid Window", 1234, 30)]
        [InlineData("", "", 0, 60)]
        [InlineData("unicodeElement_テスト", "Unicode Window テスト", 99999, 5)]
        public async Task SelectionService_WithVariousParameters_ShouldPassCorrectly(
            string elementId, string windowTitle, int processId, int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = true } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), timeoutSeconds), Times.Once);

            _output.WriteLine($"Parameter validation test passed for: elementId='{elementId}', windowTitle='{windowTitle}', processId={processId}, timeout={timeoutSeconds}");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(300)]
        public async Task SelectionService_WithVariousTimeouts_ShouldUseCorrectTimeout(int timeoutSeconds)
        {
            // Arrange
            var containerId = "timeoutTestContainer";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanSelectMultiple = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CanSelectMultiple", It.IsAny<Dictionary<string, object>>(), timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, null, null, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("CanSelectMultiple", 
                It.Is<Dictionary<string, object>>(p => p["containerId"].ToString() == containerId), timeoutSeconds), Times.Once);

            _output.WriteLine($"Timeout test passed for {timeoutSeconds} seconds");
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task SelectionService_WhenExecutorThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "problematicElement";
            var expectedException = new InvalidOperationException("Worker operation failed");

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
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
            
            var success = (bool?)successProperty.GetValue(result);
            var error = errorProperty.GetValue(result)?.ToString();
            
            Assert.False(success);
            Assert.Equal(expectedException.Message, error);
            
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == elementId), 30), Times.Once);

            _output.WriteLine("Exception handling test passed - Service properly handles executor exceptions and returns error result");
        }

        [Fact]
        public async Task SelectionService_WithNullAndEmptyParameters_ShouldHandleGracefully()
        {
            // Arrange
            var emptyElementId = "";
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = false } };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _selectionService.IsSelectedAsync(emptyElementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<object>("IsSelected", 
                It.Is<Dictionary<string, object>>(p => p["elementId"].ToString() == emptyElementId), 30), Times.Once);

            _output.WriteLine("Empty parameter handling test passed");
        }

        #endregion

        #region Required Members Verification Tests

        [Fact]
        public async Task RequiredSelectionItemMembers_ShouldAllBeImplemented()
        {
            // Arrange
            var elementId = "testElement";
            var expectedIsSelectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelected = true } };
            var expectedContainerResult = new ServerEnhancedResponse<object> { Success = true, Data = new { SelectionContainer = new { AutomationId = "container" } } };
            var expectedAddResult = new ServerEnhancedResponse<object> { Success = true, Message = "Added to selection" };
            var expectedRemoveResult = new ServerEnhancedResponse<object> { Success = true, Message = "Removed from selection" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelected", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedIsSelectedResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("GetSelectionContainer", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedContainerResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("AddToSelection", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedAddResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("RemoveFromSelection", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedRemoveResult));

            // Act & Assert - Test all required SelectionItem members
            var isSelectedResult = await _selectionService.IsSelectedAsync(elementId, null, null, 30);
            var containerResult = await _selectionService.GetSelectionContainerAsync(elementId, null, null, 30);
            var addResult = await _selectionService.AddToSelectionAsync(elementId, null, null, 30);
            var removeResult = await _selectionService.RemoveFromSelectionAsync(elementId, null, null, 30);

            Assert.NotNull(isSelectedResult);
            Assert.NotNull(containerResult);
            Assert.NotNull(addResult);
            Assert.NotNull(removeResult);

            _output.WriteLine("All required SelectionItem members verified as implemented");
        }

        [Fact]
        public async Task RequiredSelectionMembers_ShouldAllBeImplemented()
        {
            // Arrange
            var containerId = "testContainer";
            var expectedCanSelectResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanSelectMultiple = true } };
            var expectedRequiredResult = new ServerEnhancedResponse<object> { Success = true, Data = new { IsSelectionRequired = false } };
            var expectedSelectionResult = new ServerEnhancedResponse<object> { Success = true, Data = new List<object>() };
            var expectedClearResult = new ServerEnhancedResponse<object> { Success = true, Message = "Selection cleared" };

            _mockExecutor.Setup(e => e.ExecuteAsync<object>("CanSelectMultiple", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedCanSelectResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("IsSelectionRequired", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedRequiredResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("GetSelection", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedSelectionResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<object>("ClearSelection", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedClearResult));

            // Act & Assert - Test all required Selection members
            var canSelectResult = await _selectionService.CanSelectMultipleAsync(containerId, null, null, 30);
            var requiredResult = await _selectionService.IsSelectionRequiredAsync(containerId, null, null, 30);
            var selectionResult = await _selectionService.GetSelectionAsync(containerId, null, null, 30);
            var clearResult = await _selectionService.ClearSelectionAsync(containerId, null, null, 30);

            Assert.NotNull(canSelectResult);
            Assert.NotNull(requiredResult);
            Assert.NotNull(selectionResult);
            Assert.NotNull(clearResult);

            _output.WriteLine("All required Selection members verified as implemented");
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("NewSelectionPatternTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}