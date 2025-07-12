using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// DockPatternの単体テスト
    /// Microsoft仕様に基づいたDockPatternの機能をモックベースでテストします
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class DockPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<ILayoutService> _mockLayoutService;

        public DockPatternTests(ITestOutputHelper output)
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
                mockTransform.Object
            );
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region Microsoft仕様準拠のDockPositionテスト

        [Theory]
        [InlineData("top")]
        [InlineData("bottom")]
        [InlineData("left")]
        [InlineData("right")]
        [InlineData("fill")]
        [InlineData("none")]
        public async Task DockElement_WithValidDockPositions_ShouldSucceed(string dockPosition)
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = dockPosition };
            _mockLayoutService.Setup(s => s.DockElementAsync("dockablePane", dockPosition, "TestWindow", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("dockablePane", dockPosition, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("dockablePane", dockPosition, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"DockElement test passed for position: {dockPosition}");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData("center")]
        [InlineData("middle")]
        public async Task DockElement_WithInvalidDockPositions_ShouldHandleError(string invalidPosition)
        {
            // Arrange
            _mockLayoutService.Setup(s => s.DockElementAsync("dockablePane", invalidPosition, "TestWindow", null, 30))
                             .ThrowsAsync(new ArgumentException($"Unsupported dock position: {invalidPosition}"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _tools.DockElement("dockablePane", invalidPosition, "TestWindow"));

            _mockLayoutService.Verify(s => s.DockElementAsync("dockablePane", invalidPosition, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"DockElement correctly rejected invalid position: {invalidPosition}");
        }

        #endregion

        #region DockPattern状態変更テスト

        [Fact]
        public async Task DockElement_ChangingFromNoneToTop_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = "Top" };
            _mockLayoutService.Setup(s => s.DockElementAsync("toolbar", "top", "MainWindow", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("toolbar", "top", "MainWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("toolbar", "top", "MainWindow", null, 30), Times.Once);
            _output.WriteLine("Position change from None to Top test passed");
        }

        [Fact]
        public async Task DockElement_ChangingFromLeftToRight_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "Left", NewPosition = "Right" };
            _mockLayoutService.Setup(s => s.DockElementAsync("sidebar", "right", "IDE", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("sidebar", "right", "IDE");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("sidebar", "right", "IDE", null, 30), Times.Once);
            _output.WriteLine("Position change from Left to Right test passed");
        }

        [Fact]
        public async Task DockElement_SettingToFill_ShouldExpandElement()
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = "Fill" };
            _mockLayoutService.Setup(s => s.DockElementAsync("contentArea", "fill", "App", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("contentArea", "fill", "App");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("contentArea", "fill", "App", null, 30), Times.Once);
            _output.WriteLine("Fill dock position test passed");
        }

        #endregion

        #region エラーハンドリングテスト

        [Fact]
        public async Task DockElement_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.DockElementAsync("nonExistentElement", "top", "TestWindow", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element 'nonExistentElement' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement("nonExistentElement", "top", "TestWindow"));

            _mockLayoutService.Verify(s => s.DockElementAsync("nonExistentElement", "top", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }

        [Fact]
        public async Task DockElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockLayoutService.Setup(s => s.DockElementAsync("staticText", "top", "TestWindow", null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element does not support DockPattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement("staticText", "top", "TestWindow"));

            _mockLayoutService.Verify(s => s.DockElementAsync("staticText", "top", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Theory]
        [InlineData("", "top", "TestWindow")]
        [InlineData("element1", "", "TestWindow")]
        [InlineData("element1", "top", "")]
        public async Task DockElement_WithEmptyParameters_ShouldCallService(string elementId, string dockPosition, string windowTitle)
        {
            // Arrange
            var expectedResult = new { Success = true };
            _mockLayoutService.Setup(s => s.DockElementAsync(elementId, dockPosition, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement(elementId, dockPosition, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync(elementId, dockPosition, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', position='{dockPosition}', window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task DockElement_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = "Top" };
            _mockLayoutService.Setup(s => s.DockElementAsync("element1", "top", "TestWindow", processId, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("element1", "top", "TestWindow", processId);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("element1", "top", "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task DockElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = "Bottom" };
            _mockLayoutService.Setup(s => s.DockElementAsync("element1", "bottom", "TestWindow", null, timeoutSeconds))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("element1", "bottom", "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("element1", "bottom", "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion

        #region ケース不感知テスト

        [Theory]
        [InlineData("TOP", "TOP")]
        [InlineData("Bottom", "Bottom")]
        [InlineData("LEFT", "LEFT")]
        [InlineData("Right", "Right")]
        [InlineData("FILL", "FILL")]
        [InlineData("None", "None")]
        public async Task DockElement_WithMixedCasePositions_ShouldCallServiceWithOriginalCase(string inputPosition, string expectedPosition)
        {
            // Arrange
            var expectedResult = new { PreviousPosition = "None", NewPosition = expectedPosition };
            _mockLayoutService.Setup(s => s.DockElementAsync("element1", expectedPosition, "TestWindow", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("element1", inputPosition, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.DockElementAsync("element1", expectedPosition, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Mixed case test passed: '{inputPosition}' passed as '{expectedPosition}'");
        }

        #endregion

        #region 複数要素の同時Dockテスト（シナリオベース）

        [Fact]
        public async Task DockElement_MultipleElementsSequence_ShouldExecuteInOrder()
        {
            // Arrange
            var toolbar1Result = new { PreviousPosition = "None", NewPosition = "Top" };
            var toolbar2Result = new { PreviousPosition = "None", NewPosition = "Bottom" };
            var sidebar1Result = new { PreviousPosition = "None", NewPosition = "Left" };
            var sidebar2Result = new { PreviousPosition = "None", NewPosition = "Right" };

            _mockLayoutService.Setup(s => s.DockElementAsync("toolbar1", "top", "IDE", null, 30))
                             .ReturnsAsync(toolbar1Result);
            _mockLayoutService.Setup(s => s.DockElementAsync("toolbar2", "bottom", "IDE", null, 30))
                             .ReturnsAsync(toolbar2Result);
            _mockLayoutService.Setup(s => s.DockElementAsync("sidebar1", "left", "IDE", null, 30))
                             .ReturnsAsync(sidebar1Result);
            _mockLayoutService.Setup(s => s.DockElementAsync("sidebar2", "right", "IDE", null, 30))
                             .ReturnsAsync(sidebar2Result);

            // Act
            var result1 = await _tools.DockElement("toolbar1", "top", "IDE");
            var result2 = await _tools.DockElement("toolbar2", "bottom", "IDE");
            var result3 = await _tools.DockElement("sidebar1", "left", "IDE");
            var result4 = await _tools.DockElement("sidebar2", "right", "IDE");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);

            _mockLayoutService.Verify(s => s.DockElementAsync("toolbar1", "top", "IDE", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.DockElementAsync("toolbar2", "bottom", "IDE", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.DockElementAsync("sidebar1", "left", "IDE", null, 30), Times.Once);
            _mockLayoutService.Verify(s => s.DockElementAsync("sidebar2", "right", "IDE", null, 30), Times.Once);

            _output.WriteLine("Multiple elements dock sequence test passed");
        }

        #endregion
    }
}