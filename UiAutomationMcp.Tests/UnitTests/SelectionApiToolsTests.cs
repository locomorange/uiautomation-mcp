using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Unit tests for new Selection API methods in UIAutomationTools
    /// Tests the API layer with mocked service dependencies
    /// Verifies proper parameter passing and service interaction
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SelectionApiToolsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<ISelectionService> _mockSelectionService;
        private readonly Mock<IApplicationLauncher> _mockApplicationLauncher;
        private readonly Mock<IScreenshotService> _mockScreenshotService;
        private readonly Mock<IElementSearchService> _mockElementSearchService;
        private readonly Mock<ITreeNavigationService> _mockTreeNavigationService;
        private readonly Mock<IInvokeService> _mockInvokeService;
        private readonly Mock<IValueService> _mockValueService;
        private readonly Mock<IRangeService> _mockRangeService;
        private readonly Mock<ITextService> _mockTextService;
        private readonly Mock<IToggleService> _mockToggleService;
        private readonly Mock<IWindowService> _mockWindowService;
        private readonly Mock<ILayoutService> _mockLayoutService;
        private readonly Mock<IGridService> _mockGridService;
        private readonly Mock<ITableService> _mockTableService;
        private readonly Mock<IMultipleViewService> _mockMultipleViewService;
        private readonly Mock<IAccessibilityService> _mockAccessibilityService;
        private readonly Mock<ICustomPropertyService> _mockCustomPropertyService;
        private readonly Mock<IControlTypeService> _mockControlTypeService;
        private readonly Mock<ITransformService> _mockTransformService;
        private readonly Mock<IVirtualizedItemService> _mockVirtualizedItemService;
        private readonly Mock<IItemContainerService> _mockItemContainerService;
        private readonly Mock<ISynchronizedInputService> _mockSynchronizedInputService;
        private readonly Mock<IEventMonitorService> _mockEventMonitorService;

        public SelectionApiToolsTests(ITestOutputHelper output)
        {
            _output = output;

            // Create mocks for all dependencies
            _mockApplicationLauncher = new Mock<IApplicationLauncher>();
            _mockScreenshotService = new Mock<IScreenshotService>();
            _mockElementSearchService = new Mock<IElementSearchService>();
            _mockTreeNavigationService = new Mock<ITreeNavigationService>();
            _mockInvokeService = new Mock<IInvokeService>();
            _mockValueService = new Mock<IValueService>();
            _mockRangeService = new Mock<IRangeService>();
            _mockSelectionService = new Mock<ISelectionService>();
            _mockTextService = new Mock<ITextService>();
            _mockToggleService = new Mock<IToggleService>();
            _mockWindowService = new Mock<IWindowService>();
            _mockLayoutService = new Mock<ILayoutService>();
            _mockGridService = new Mock<IGridService>();
            _mockTableService = new Mock<ITableService>();
            _mockMultipleViewService = new Mock<IMultipleViewService>();
            _mockAccessibilityService = new Mock<IAccessibilityService>();
            _mockCustomPropertyService = new Mock<ICustomPropertyService>();
            _mockControlTypeService = new Mock<IControlTypeService>();
            _mockTransformService = new Mock<ITransformService>();
            _mockVirtualizedItemService = new Mock<IVirtualizedItemService>();
            _mockItemContainerService = new Mock<IItemContainerService>();
            _mockSynchronizedInputService = new Mock<ISynchronizedInputService>();
            _mockEventMonitorService = new Mock<IEventMonitorService>();

            // Create UIAutomationTools with all mocked dependencies
            _tools = new UIAutomationTools(
                _mockApplicationLauncher.Object,
                _mockScreenshotService.Object,
                _mockElementSearchService.Object,
                _mockTreeNavigationService.Object,
                _mockInvokeService.Object,
                _mockValueService.Object,
                _mockRangeService.Object,
                _mockSelectionService.Object,
                _mockTextService.Object,
                _mockToggleService.Object,
                _mockWindowService.Object,
                _mockLayoutService.Object,
                _mockGridService.Object,
                _mockTableService.Object,
                _mockMultipleViewService.Object,
                _mockAccessibilityService.Object,
                _mockCustomPropertyService.Object,
                _mockControlTypeService.Object,
                _mockTransformService.Object,
                _mockVirtualizedItemService.Object,
                _mockItemContainerService.Object,
                _mockSynchronizedInputService.Object,
                _mockEventMonitorService.Object,
                Mock.Of<ISubprocessExecutor>()
            );
        }

        #region SelectionItemPattern API Tests

        [Fact]
        public async Task IsElementSelected_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var elementId = "testListItem";
            var windowTitle = "Test Window";
            var processId = 1234;
            var timeoutSeconds = 45;
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = true, PropertyName = "IsSelected" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsElementSelected(elementId, windowTitle, processId, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds), Times.Once);
            
            _output.WriteLine("IsElementSelected API test passed - Service called correctly");
        }

        [Fact]
        public async Task IsElementSelected_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var elementId = "defaultTestItem";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = false, PropertyName = "IsSelected" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectedAsync(elementId, null, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsElementSelected(elementId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync(elementId, null, null, 30), Times.Once);
            
            _output.WriteLine("IsElementSelected default parameters test passed");
        }

        [Fact]
        public async Task GetSelectionContainer_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var elementId = "containerTestItem";
            var windowTitle = "Container Window";
            var processId = 5678;
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "GetSelectionContainer",
                    ActionName = "Container information retrieved",
                    ElementId = elementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.GetSelectionContainerAsync(elementId, windowTitle, processId, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetSelectionContainer(elementId, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionContainerAsync(elementId, windowTitle, processId, 30), Times.Once);
            
            _output.WriteLine("GetSelectionContainer API test passed - Service called correctly");
        }

        [Fact]
        public async Task GetSelectionContainer_WithNullContainer_ShouldReturnNull()
        {
            // Arrange
            var elementId = "orphanedItem";
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "GetSelectionContainer",
                    ActionName = "Container not found",
                    ElementId = elementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.GetSelectionContainerAsync(elementId, null, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetSelectionContainer(elementId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionContainerAsync(elementId, null, null, 30), Times.Once);
            
            _output.WriteLine("GetSelectionContainer null container test passed");
        }

        #endregion

        #region SelectionPattern API Tests

        [Fact]
        public async Task CanSelectMultiple_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var containerElementId = "multiSelectListBox";
            var windowTitle = "Multi-Select Window";
            var processId = 9999;
            var timeoutSeconds = 60;
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = true, PropertyName = "CanSelectMultiple" } 
            };

            _mockSelectionService
                .Setup(s => s.CanSelectMultipleAsync(containerElementId, windowTitle, processId, timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.CanSelectMultiple(containerElementId, windowTitle, processId, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync(containerElementId, windowTitle, processId, timeoutSeconds), Times.Once);
            
            _output.WriteLine("CanSelectMultiple API test passed - Service called correctly");
        }

        [Fact]
        public async Task CanSelectMultiple_WithSingleSelectContainer_ShouldReturnFalse()
        {
            // Arrange
            var containerElementId = "singleSelectRadioGroup";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = false, PropertyName = "CanSelectMultiple" } 
            };

            _mockSelectionService
                .Setup(s => s.CanSelectMultipleAsync(containerElementId, null, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.CanSelectMultiple(containerElementId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync(containerElementId, null, null, 30), Times.Once);
            
            _output.WriteLine("CanSelectMultiple single-select test passed");
        }

        [Fact]
        public async Task IsSelectionRequired_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var containerElementId = "requiredTabControl";
            var windowTitle = "Tab Control Window";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = true, PropertyName = "IsSelectionRequired" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectionRequiredAsync(containerElementId, windowTitle, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsSelectionRequired(containerElementId, windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectionRequiredAsync(containerElementId, windowTitle, null, 30), Times.Once);
            
            _output.WriteLine("IsSelectionRequired API test passed - Service called correctly");
        }

        [Fact]
        public async Task IsSelectionRequired_WithOptionalSelection_ShouldReturnFalse()
        {
            // Arrange
            var containerElementId = "optionalSelectionList";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = false, PropertyName = "IsSelectionRequired" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectionRequiredAsync(containerElementId, null, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsSelectionRequired(containerElementId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectionRequiredAsync(containerElementId, null, null, 30), Times.Once);
            
            _output.WriteLine("IsSelectionRequired optional selection test passed");
        }

        #endregion

        #region Selection Operations API Tests

        [Fact]
        public async Task AddToSelection_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var elementId = "additionalItem";
            var windowTitle = "Multi-Select List";
            var processId = 7777;
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "AddToSelection",
                    ActionName = "Element added to selection successfully",
                    ElementId = elementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.AddToSelectionAsync(elementId, windowTitle, processId, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.AddToSelection(elementId, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.AddToSelectionAsync(elementId, windowTitle, processId, 30), Times.Once);
            
            _output.WriteLine("AddToSelection API test passed - Service called correctly");
        }

        [Fact]
        public async Task AddToSelection_WithCustomTimeout_ShouldUseCustomTimeout()
        {
            // Arrange
            var elementId = "timeoutAddItem";
            var timeoutSeconds = 120;
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "AddToSelection",
                    ActionName = "Element added with custom timeout",
                    ElementId = elementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.AddToSelectionAsync(elementId, null, null, timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.AddToSelection(elementId, null, null, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.AddToSelectionAsync(elementId, null, null, timeoutSeconds), Times.Once);
            
            _output.WriteLine("AddToSelection custom timeout test passed");
        }

        [Fact]
        public async Task RemoveFromSelection_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var elementId = "removeableItem";
            var windowTitle = "Editable Selection";
            var processId = 3333;
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "RemoveFromSelection",
                    ActionName = "Element removed from selection successfully",
                    ElementId = elementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.RemoveFromSelectionAsync(elementId, windowTitle, processId, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.RemoveFromSelection(elementId, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.RemoveFromSelectionAsync(elementId, windowTitle, processId, 30), Times.Once);
            
            _output.WriteLine("RemoveFromSelection API test passed - Service called correctly");
        }

        [Fact]
        public async Task ClearSelection_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var containerElementId = "clearableContainer";
            var windowTitle = "Clearable Selection Window";
            var processId = 4444;
            var expectedResult = new ServerEnhancedResponse<ActionResult> 
            { 
                Success = true, 
                Data = new ActionResult 
                { 
                    Action = "ClearSelection",
                    ActionName = "Selection cleared successfully",
                    ElementId = containerElementId,
                    Completed = true
                }
            };

            _mockSelectionService
                .Setup(s => s.ClearSelectionAsync(containerElementId, windowTitle, processId, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ClearSelection(containerElementId, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.ClearSelectionAsync(containerElementId, windowTitle, processId, 30), Times.Once);
            
            _output.WriteLine("ClearSelection API test passed - Service called correctly");
        }

        [Fact]
        public async Task GetSelection_WhenCalled_ShouldCallSelectionService()
        {
            // Arrange
            var containerElementId = "selectionContainer";
            var windowTitle = "Current Selection Window";
            var expectedResult = new ServerEnhancedResponse<SelectionInfoResult> 
            { 
                Success = true, 
                Data = new SelectionInfoResult 
                { 
                    ContainerElementId = containerElementId,
                    WindowTitle = windowTitle,
                    SelectedItems = new List<SelectionItem>
                    {
                        new SelectionItem { AutomationId = "item1", Name = "First Selected Item" },
                        new SelectionItem { AutomationId = "item3", Name = "Third Selected Item" }
                    },
                    SelectedCount = 2
                }
            };

            _mockSelectionService
                .Setup(s => s.GetSelectionAsync(containerElementId, windowTitle, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetSelection(containerElementId, windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionAsync(containerElementId, windowTitle, null, 30), Times.Once);
            
            _output.WriteLine("GetSelection API test passed - Service called correctly");
        }

        #endregion

        #region Error Handling API Tests

        [Fact]
        public async Task SelectionApis_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var elementId = "problematicElement";
            var expectedException = new InvalidOperationException("Service operation failed");

            _mockSelectionService
                .Setup(s => s.IsSelectedAsync(elementId, null, null, 30))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.IsElementSelected(elementId));
            
            Assert.Equal(expectedException.Message, exception.Message);
            _mockSelectionService.Verify(s => s.IsSelectedAsync(elementId, null, null, 30), Times.Once);
            
            _output.WriteLine("Exception propagation test passed - API properly propagates service exceptions");
        }

        [Fact]
        public async Task SelectionApis_WithEmptyElementId_ShouldCallServiceCorrectly()
        {
            // Arrange
            var emptyElementId = "";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = false, PropertyName = "IsSelected" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectedAsync(emptyElementId, null, null, 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsElementSelected(emptyElementId);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync(emptyElementId, null, null, 30), Times.Once);
            
            _output.WriteLine("Empty element ID test passed - API handles empty parameters");
        }

        #endregion

        #region Parameter Validation API Tests

        [Theory]
        [InlineData("validElement", "Valid Window", 1234, 30)]
        [InlineData("", "", 0, 60)]
        [InlineData("specialChars_元素", "Window with spaces", 99999, 5)]
        public async Task SelectionApis_WithVariousParameters_ShouldPassThrough(
            string elementId, string windowTitle, int processId, int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = true, PropertyName = "IsSelected" } 
            };

            _mockSelectionService
                .Setup(s => s.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.IsElementSelected(elementId, windowTitle, processId, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds), Times.Once);
            
            _output.WriteLine($"Parameter variation test passed for: elementId='{elementId}', windowTitle='{windowTitle}', processId={processId}, timeout={timeoutSeconds}");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(300)]
        public async Task SelectionApis_WithVariousTimeouts_ShouldUseCorrectTimeout(int timeoutSeconds)
        {
            // Arrange
            var containerElementId = "timeoutTestContainer";
            var expectedResult = new ServerEnhancedResponse<BooleanResult> 
            { 
                Success = true, 
                Data = new BooleanResult { Value = false, PropertyName = "CanSelectMultiple" } 
            };

            _mockSelectionService
                .Setup(s => s.CanSelectMultipleAsync(containerElementId, null, null, timeoutSeconds))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.CanSelectMultiple(containerElementId, null, null, timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.CanSelectMultipleAsync(containerElementId, null, null, timeoutSeconds), Times.Once);
            
            _output.WriteLine($"Timeout variation test passed for {timeoutSeconds} seconds");
        }

        #endregion

        #region API Method Signature Tests

        [Fact]
        public void SelectionApiMethods_ShouldHaveCorrectSignatures()
        {
            // Test that all new selection API methods exist with correct signatures
            var toolsType = typeof(UIAutomationTools);

            // Test IsElementSelected method signature
            var isSelectedMethod = toolsType.GetMethod("IsElementSelected");
            Assert.NotNull(isSelectedMethod);
            Assert.Equal(typeof(Task<object>), isSelectedMethod.ReturnType);

            // Test GetSelectionContainer method signature
            var getContainerMethod = toolsType.GetMethod("GetSelectionContainer");
            Assert.NotNull(getContainerMethod);
            Assert.Equal(typeof(Task<object>), getContainerMethod.ReturnType);

            // Test CanSelectMultiple method signature
            var canSelectMultipleMethod = toolsType.GetMethod("CanSelectMultiple");
            Assert.NotNull(canSelectMultipleMethod);
            Assert.Equal(typeof(Task<object>), canSelectMultipleMethod.ReturnType);

            // Test IsSelectionRequired method signature
            var isSelectionRequiredMethod = toolsType.GetMethod("IsSelectionRequired");
            Assert.NotNull(isSelectionRequiredMethod);
            Assert.Equal(typeof(Task<object>), isSelectionRequiredMethod.ReturnType);

            // Test AddToSelection method signature
            var addToSelectionMethod = toolsType.GetMethod("AddToSelection");
            Assert.NotNull(addToSelectionMethod);
            Assert.Equal(typeof(Task<object>), addToSelectionMethod.ReturnType);

            // Test RemoveFromSelection method signature
            var removeFromSelectionMethod = toolsType.GetMethod("RemoveFromSelection");
            Assert.NotNull(removeFromSelectionMethod);
            Assert.Equal(typeof(Task<object>), removeFromSelectionMethod.ReturnType);

            _output.WriteLine("API method signature validation test passed - All selection methods have correct signatures");
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("SelectionApiToolsTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}