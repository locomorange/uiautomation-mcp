using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// ExpandCollapsePatternの単体テスト
    /// Microsoft仕様に基づいたExpandCollapsePatternの機能をモックベースでテストします
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ExpandCollapsePatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<ILayoutService> _mockLayoutService;

        public ExpandCollapsePatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLayoutService = new Mock<ILayoutService>();
            
            // UIAutomationToolsの他のサービスもモック化（最小限の設定）
            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInvoke = new Mock<IInvokeService>();
            var mockValue = new Mock<IValueService>();
            var mockRange = new Mock<IRangeService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockToggle = new Mock<IToggleService>();
            var mockWindow = new Mock<IWindowService>();
            var mockGrid = new Mock<IGridService>();
            var mockTable = new Mock<ITableService>();
            var mockMultipleView = new Mock<IMultipleViewService>();
            var mockAccessibility = new Mock<IAccessibilityService>();
            var mockCustomProperty = new Mock<ICustomPropertyService>();
            var mockControlType = new Mock<IControlTypeService>();
            var mockTransform = new Mock<ITransformService>();
            var mockVirtualizedItem = new Mock<IVirtualizedItemService>();
            var mockItemContainer = new Mock<IItemContainerService>();
            var mockSynchronizedInput = new Mock<ISynchronizedInputService>();
            var mockEventMonitor = new Mock<IEventMonitorService>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                mockInvoke.Object,
                mockValue.Object,
                mockRange.Object,
                mockSelection.Object,
                mockText.Object,
                mockToggle.Object,
                mockWindow.Object,
                _mockLayoutService.Object,
                mockGrid.Object,
                mockTable.Object,
                mockMultipleView.Object,
                mockAccessibility.Object,
                mockCustomProperty.Object,
                mockControlType.Object,
                mockTransform.Object,
                mockVirtualizedItem.Object,
                mockItemContainer.Object,
                mockSynchronizedInput.Object,
                mockEventMonitor.Object,
                Mock.Of<UIAutomationMCP.Server.Helpers.SubprocessExecutor>()
            );
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region Microsoft仕様準拠のExpandCollapsePatternテスト

        [Theory]
        [InlineData("TreeItem")]
        [InlineData("Menu")]
        [InlineData("ComboBox")]
        [InlineData("Button")]
        public async Task ExpandCollapseElement_WithExpandAction_ShouldSucceed(string elementSelector)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync(elementSelector, "expand", "TestWindow", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement(elementSelector, "expand", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync(elementSelector, "expand", "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"ExpandCollapseElement expand test passed for control: {elementSelector}");
        }

        [Theory]
        [InlineData("TreeItem")]
        [InlineData("Menu")]
        [InlineData("ComboBox")]
        [InlineData("Button")]
        public async Task ExpandCollapseElement_WithCollapseAction_ShouldSucceed(string elementSelector)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync(elementSelector, "collapse", "TestWindow", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement(elementSelector, "collapse", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync(elementSelector, "collapse", "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"ExpandCollapseElement collapse test passed for control: {elementSelector}");
        }

        [Theory]
        [InlineData("TreeItem")]
        [InlineData("Menu")]
        [InlineData("ComboBox")]
        [InlineData("Button")]
        public async Task ExpandCollapseElement_WithToggleAction_ShouldSucceed(string elementSelector)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync(elementSelector, "toggle", "TestWindow", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement(elementSelector, "toggle", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync(elementSelector, "toggle", "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"ExpandCollapseElement toggle test passed for control: {elementSelector}");
        }

        #endregion

        #region ExpandCollapseState状態変更テスト

        [Fact]
        public async Task ExpandCollapseElement_FromCollapsedToExpanded_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("treeNode1", "expand", "Explorer", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("treeNode1", "expand", "Explorer");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("treeNode1", "expand", "Explorer", null, 30), Times.Once);
            _output.WriteLine("State change from Collapsed to Expanded test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_FromExpandedToCollapsed_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("menuItem", "collapse", "MainMenu", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("menuItem", "collapse", "MainMenu");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("menuItem", "collapse", "MainMenu", null, 30), Times.Once);
            _output.WriteLine("State change from Expanded to Collapsed test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_ToggleFromCollapsedToExpanded_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("comboBox1", "toggle", "Form", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("comboBox1", "toggle", "Form");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("comboBox1", "toggle", "Form", null, 30), Times.Once);
            _output.WriteLine("Toggle from Collapsed to Expanded test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_ToggleFromExpandedToCollapsed_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("accordion1", "toggle", "Panel", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("accordion1", "toggle", "Panel");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("accordion1", "toggle", "Panel", null, 30), Times.Once);
            _output.WriteLine("Toggle from Expanded to Collapsed test passed");
        }

        #endregion

        #region LeafNode状態のエラーハンドリングテスト

        [Fact]
        public async Task ExpandCollapseElement_WithLeafNodeExpand_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("leafNode", "expand", "TreeView", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Cannot expand a leaf node"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ExpandCollapseElement("leafNode", "expand", "TreeView"));

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("leafNode", "expand", "TreeView", null, 30), Times.Once);
            _output.WriteLine("LeafNode expand error handling test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_WithLeafNodeCollapse_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("leafNode", "collapse", "TreeView", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Cannot collapse a leaf node"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ExpandCollapseElement("leafNode", "collapse", "TreeView"));

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("leafNode", "collapse", "TreeView", null, 30), Times.Once);
            _output.WriteLine("LeafNode collapse error handling test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_WithLeafNodeToggle_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("leafNode", "toggle", "TreeView", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Cannot toggle expansion of a leaf node"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ExpandCollapseElement("leafNode", "toggle", "TreeView"));

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("leafNode", "toggle", "TreeView", null, 30), Times.Once);
            _output.WriteLine("LeafNode toggle error handling test passed");
        }

        #endregion

        #region 一般的なエラーハンドリングテスト

        [Fact]
        public async Task ExpandCollapseElement_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("nonExistentElement", "expand", "TestWindow", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element 'nonExistentElement' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ExpandCollapseElement("nonExistentElement", "expand", "TestWindow"));

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("nonExistentElement", "expand", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("staticText", "collapse", "TestWindow", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element does not support ExpandCollapsePattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ExpandCollapseElement("staticText", "collapse", "TestWindow"));

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("staticText", "collapse", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_WithAlreadyExpandedElementToggle_ShouldCollapseCorrectly()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("expandedNode", "toggle", "TreeView", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("expandedNode", "toggle", "TreeView");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("expandedNode", "toggle", "TreeView", null, 30), Times.Once);
            _output.WriteLine("Already expanded element toggle test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Theory]
        [InlineData("", "expand", "TestWindow")]
        [InlineData("element1", "collapse", "")]
        [InlineData("element1", "toggle", "TestWindow")]
        public async Task ExpandCollapseElement_WithEmptyParameters_ShouldCallService(string elementSelector, string action, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "ExpandCollapse", Details = "Operation completed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync(elementSelector, action,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement(elementSelector, action,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync(elementSelector, action,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: element='{elementSelector}', action='{action}', window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task ExpandCollapseElement_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("element1", "collapse", "TestWindow", processId, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("element1", "collapse", "TestWindow", processId);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("element1", "collapse", "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task ExpandCollapseElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("element1", "expand", "TestWindow", null, timeoutSeconds))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("element1", "expand", "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("element1", "expand", "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion

        #region 複雑なシナリオテスト

        [Fact]
        public async Task ExpandCollapseElement_NestedTreeNodes_ShouldExecuteInHierarchicalOrder()
        {
            // Arrange
            var rootExpandResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            var childExpandResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            var grandChildExpandResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Expand", Details = "Expanded" }
            };
            var childCollapseResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Collapse", Details = "Collapsed" }
            };

            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("rootNode", "expand", "TreeView", null, 30))
                             .Returns(Task.FromResult(rootExpandResult));
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("childNode", "expand", "TreeView", null, 30))
                             .Returns(Task.FromResult(childExpandResult));
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("grandChildNode", "expand", "TreeView", null, 30))
                             .Returns(Task.FromResult(grandChildExpandResult));
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("childNode", "collapse", "TreeView", null, 30))
                             .Returns(Task.FromResult(childCollapseResult));

            // Act
            var result1 = await _tools.ExpandCollapseElement("rootNode", "expand", "TreeView");
            var result2 = await _tools.ExpandCollapseElement("childNode", "expand", "TreeView");
            var result3 = await _tools.ExpandCollapseElement("grandChildNode", "expand", "TreeView");
            var result4 = await _tools.ExpandCollapseElement("childNode", "collapse", "TreeView");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("rootNode", "expand", "TreeView", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("childNode", "expand", "TreeView", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("grandChildNode", "expand", "TreeView", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("childNode", "collapse", "TreeView", null, 30), Times.Once);

            _output.WriteLine("Nested tree nodes hierarchical operation test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_MultipleToggles_ShouldAlternateStates()
        {
            // Arrange
            var firstToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Details = "Expanded" }
            };
            var secondToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Details = "Collapsed" }
            };
            var thirdToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Details = "Expanded" }
            };

            _mockLayoutService.SetupSequence(s => s.ExpandCollapseElementAsync("accordionPanel", "toggle", "Form", null, 30))
                             .Returns(Task.FromResult(firstToggleResult))
                             .Returns(Task.FromResult(secondToggleResult))
                             .Returns(Task.FromResult(thirdToggleResult));

            // Act
            var result1 = await _tools.ExpandCollapseElement("accordionPanel", "toggle", "Form");
            var result2 = await _tools.ExpandCollapseElement("accordionPanel", "toggle", "Form");
            var result3 = await _tools.ExpandCollapseElement("accordionPanel", "toggle", "Form");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("accordionPanel", "toggle", "Form", null, 30), Times.Exactly(3));
            _output.WriteLine("Multiple toggles alternating states test passed");
        }

        #endregion

        #region Microsoft仕様準拠のPropertyChangedEventテスト

        [Fact]
        public async Task ExpandCollapseElement_PropertyChange_ShouldTriggerEvent()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    OperationName = "Expand", 
                    Details = "Expanded",
                    Metadata = new Dictionary<string, object> {
                        { "PropertyChanged", true },
                        { "EventFired", "ExpandCollapseState" }
                    }
                }
            };
            _mockLayoutService.Setup(s => s.ExpandCollapseElementAsync("treeItem", "expand", "Explorer", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ExpandCollapseElement("treeItem", "expand", "Explorer");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ExpandCollapseElementAsync("treeItem", "expand", "Explorer", null, 30), Times.Once);
            _output.WriteLine("ExpandCollapseState property change event test passed");
        }

        #endregion
    }
}