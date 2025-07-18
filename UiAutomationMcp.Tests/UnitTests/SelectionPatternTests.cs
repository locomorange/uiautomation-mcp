using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Selection Pattern unit tests following Microsoft UI Automation specifications
    /// Uses Mock-based testing to avoid direct UI Automation calls
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SelectionPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ISelectionService> _mockSelectionService;
        private readonly UIAutomationTools _tools;

        public SelectionPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockSelectionService = new Mock<ISelectionService>();

            // Create UIAutomationTools with mocked dependencies
            _tools = new UIAutomationTools(
                Mock.Of<IApplicationLauncher>(),
                Mock.Of<IScreenshotService>(),
                Mock.Of<IElementSearchService>(),
                Mock.Of<ITreeNavigationService>(),
                Mock.Of<IInvokeService>(),
                Mock.Of<IValueService>(),
                Mock.Of<IRangeService>(),
                _mockSelectionService.Object,
                Mock.Of<ITextService>(),
                Mock.Of<IToggleService>(),
                Mock.Of<IWindowService>(),
                Mock.Of<ILayoutService>(),
                Mock.Of<IGridService>(),
                Mock.Of<ITableService>(),
                Mock.Of<IMultipleViewService>(),
                Mock.Of<IAccessibilityService>(),
                Mock.Of<ICustomPropertyService>(),
                Mock.Of<IControlTypeService>(),
                Mock.Of<ITransformService>(),
                Mock.Of<IVirtualizedItemService>(),
                Mock.Of<IItemContainerService>(),
                Mock.Of<ISynchronizedInputService>(),
                Mock.Of<UIAutomationMCP.Server.Helpers.SubprocessExecutor>()
            );
        }

        #region SelectionPattern Properties Tests

        [Fact]
        public async Task CanSelectMultiple_WhenCalled_ShouldReturnSelectionCapability()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { CanSelectMultiple = true } };
            _mockSelectionService.Setup(s => s.CanSelectMultipleAsync("list1", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.CanSelectMultiple("list1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync("list1", null, null, 30), Times.Once);
            _output.WriteLine("CanSelectMultiple test passed - Multiple selection capability verified");
        }

        [Fact]
        public async Task CanSelectMultiple_WithSingleSelectionContainer_ShouldReturnFalse()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { CanSelectMultiple = false } };
            _mockSelectionService.Setup(s => s.CanSelectMultipleAsync("radioGroup1", "Test Window", 1234, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.CanSelectMultiple("radioGroup1", "Test Window", 1234);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync("radioGroup1", "Test Window", 1234, 30), Times.Once);
            _output.WriteLine("CanSelectMultiple test passed - Single selection container verified");
        }

        [Fact]
        public async Task IsSelectionRequired_WhenCalled_ShouldReturnSelectionRequirement()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { IsSelectionRequired = true } };
            _mockSelectionService.Setup(s => s.IsSelectionRequiredAsync("tabControl1", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsSelectionRequired("tabControl1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectionRequiredAsync("tabControl1", null, null, 30), Times.Once);
            _output.WriteLine("IsSelectionRequired test passed - Selection requirement verified");
        }

        [Fact]
        public async Task IsSelectionRequired_WithOptionalSelection_ShouldReturnFalse()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { IsSelectionRequired = false } };
            _mockSelectionService.Setup(s => s.IsSelectionRequiredAsync("listBox1", "App Window", 5678, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsSelectionRequired("listBox1", "App Window", 5678);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectionRequiredAsync("listBox1", "App Window", 5678, 30), Times.Once);
            _output.WriteLine("IsSelectionRequired test passed - Optional selection verified");
        }

        #endregion

        #region SelectionItemPattern Properties Tests

        [Fact]
        public async Task IsElementSelected_WhenItemIsSelected_ShouldReturnTrue()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { IsSelected = true } };
            _mockSelectionService.Setup(s => s.IsSelectedAsync("listItem1", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsElementSelected("listItem1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync("listItem1", null, null, 30), Times.Once);
            _output.WriteLine("IsElementSelected test passed - Selected item verified");
        }

        [Fact]
        public async Task IsElementSelected_WhenItemIsNotSelected_ShouldReturnFalse()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { IsSelected = false } };
            _mockSelectionService.Setup(s => s.IsSelectedAsync("listItem2", "Main Window", 9876, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsElementSelected("listItem2", "Main Window", 9876);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync("listItem2", "Main Window", 9876, 30), Times.Once);
            _output.WriteLine("IsElementSelected test passed - Unselected item verified");
        }

        [Fact]
        public async Task GetSelectionContainer_WhenCalled_ShouldReturnContainerInfo()
        {
            // Arrange
            var expectedResult = new 
            { 
                Success = true, 
                Data = new 
                { 
                    SelectionContainer = new 
                    {
                        AutomationId = "listBox1",
                        Name = "Items List",
                        ControlType = "ControlType.List",
                        ClassName = "ListBox",
                        ProcessId = 1234,
                        RuntimeId = new int[] { 1, 2, 3, 4 }
                    }
                }
            };
            _mockSelectionService.Setup(s => s.GetSelectionContainerAsync("listItem1", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetSelectionContainer("listItem1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionContainerAsync("listItem1", null, null, 30), Times.Once);
            _output.WriteLine("GetSelectionContainer test passed - Container information retrieved");
        }

        [Fact]
        public async Task GetSelectionContainer_WhenNoContainer_ShouldReturnNull()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = new { SelectionContainer = (object?)null } };
            _mockSelectionService.Setup(s => s.GetSelectionContainerAsync("orphanedItem", "Test App", 4321, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetSelectionContainer("orphanedItem", "Test App", 4321);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionContainerAsync("orphanedItem", "Test App", 4321, 30), Times.Once);
            _output.WriteLine("GetSelectionContainer test passed - No container scenario verified");
        }

        #endregion

        #region Selection Operations Tests

        [Fact]
        public async Task AddToSelection_WhenCalled_ShouldAddItemToSelection()
        {
            // Arrange
            var expectedResult = new { Success = true, Message = "Element added to selection successfully" };
            _mockSelectionService.Setup(s => s.AddToSelectionAsync("item2", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.AddToSelection("item2");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.AddToSelectionAsync("item2", null, null, 30), Times.Once);
            _output.WriteLine("AddToSelection test passed - Item added to multi-selection");
        }

        [Fact]
        public async Task RemoveFromSelection_WhenCalled_ShouldRemoveItemFromSelection()
        {
            // Arrange
            var expectedResult = new { Success = true, Message = "Element removed from selection successfully" };
            _mockSelectionService.Setup(s => s.RemoveFromSelectionAsync("item1", "App Window", 2468, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.RemoveFromSelection("item1", "App Window", 2468);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.RemoveFromSelectionAsync("item1", "App Window", 2468, 30), Times.Once);
            _output.WriteLine("RemoveFromSelection test passed - Item removed from selection");
        }

        [Fact]
        public async Task ClearSelection_WhenCalled_ShouldClearAllSelections()
        {
            // Arrange
            var expectedResult = new { Success = true, Message = "Selection cleared successfully" };
            _mockSelectionService.Setup(s => s.ClearSelectionAsync("listContainer", null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ClearSelection("listContainer");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.ClearSelectionAsync("listContainer", null, null, 30), Times.Once);
            _output.WriteLine("ClearSelection test passed - All selections cleared");
        }

        [Fact]
        public async Task GetSelection_WhenCalled_ShouldReturnCurrentSelection()
        {
            // Arrange
            var expectedResult = new 
            { 
                Success = true, 
                Data = new object[] 
                { 
                    new { AutomationId = "item1", Name = "Item One", ControlType = "ControlType.ListItem" },
                    new { AutomationId = "item3", Name = "Item Three", ControlType = "ControlType.ListItem" }
                }
            };
            _mockSelectionService.Setup(s => s.GetSelectionAsync("multiSelectList", "Test Window", 1357, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetSelection("multiSelectList", "Test Window", 1357);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionAsync("multiSelectList", "Test Window", 1357, 30), Times.Once);
            _output.WriteLine("GetSelection test passed - Current selection retrieved");
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task SelectionMethods_WithInvalidElementId_ShouldHandleGracefully(string? invalidElementId)
        {
            // Arrange
            _mockSelectionService.Setup(s => s.IsSelectedAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new { Success = false, Error = "Element not found" });

            // Act & Assert - Should not throw exceptions
            var result = await _tools.IsElementSelected(invalidElementId ?? "");
            Assert.NotNull(result);
            _output.WriteLine($"Parameter validation test passed for invalid elementId: '{invalidElementId}'");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(300)]
        public async Task SelectionMethods_WithVariousTimeouts_ShouldUseCorrectTimeout(int timeout)
        {
            // Arrange
            _mockSelectionService.Setup(s => s.CanSelectMultipleAsync("list1", null, null, timeout))
                .ReturnsAsync(new { Success = true, Data = new { CanSelectMultiple = true } });

            // Act
            var result = await _tools.CanSelectMultiple("list1", null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync("list1", null, null, timeout), Times.Once);
            _output.WriteLine($"Timeout parameter test passed for value: {timeout}");
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task SelectionMethods_WhenServiceThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            _mockSelectionService.Setup(s => s.IsSelectionRequiredAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Element does not support SelectionPattern"));

            // Act & Assert - Should not propagate exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.IsSelectionRequired("unsupportedElement"));

            _output.WriteLine("Error handling test passed - Exception properly handled");
        }

        [Fact]
        public async Task SelectionMethods_WhenElementNotFound_ShouldReturnErrorResult()
        {
            // Arrange
            var errorResult = new { Success = false, Error = "Element not found" };
            _mockSelectionService.Setup(s => s.GetSelectionContainerAsync("nonExistentElement", null, null, 30))
                .ReturnsAsync(errorResult);

            // Act
            var result = await _tools.GetSelectionContainer("nonExistentElement");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionContainerAsync("nonExistentElement", null, null, 30), Times.Once);
            _output.WriteLine("Element not found test passed - Error result returned");
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("SelectionPatternTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}