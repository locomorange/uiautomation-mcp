using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Shared.Models;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// TransformPatternの単体テスト
    /// Microsoft仕様に基づいたTransformPatternの機能をモックベースでテストします
    /// https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-transform-control-pattern
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TransformPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<ITransformService> _mockTransformService;

        public TransformPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockTransformService = new Mock<ITransformService>();
            
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
            var mockLayout = new Mock<ILayoutService>();
            var mockGrid = new Mock<IGridService>();
            var mockTable = new Mock<ITableService>();
            var mockMultipleView = new Mock<IMultipleViewService>();
            var mockAccessibility = new Mock<IAccessibilityService>();
            var mockCustomProperty = new Mock<ICustomPropertyService>();
            var mockControlType = new Mock<IControlTypeService>();
            var mockVirtualizedItem = new Mock<IVirtualizedItemService>();
            var mockItemContainer = new Mock<IItemContainerService>();
            var mockSynchronizedInput = new Mock<ISynchronizedInputService>();

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
                mockLayout.Object,
                mockGrid.Object,
                mockTable.Object,
                mockMultipleView.Object,
                mockAccessibility.Object,
                mockCustomProperty.Object,
                mockControlType.Object,
                _mockTransformService.Object,
                mockVirtualizedItem.Object,
                mockItemContainer.Object,
                mockSynchronizedInput.Object,
                Mock.Of<IEventMonitorService>(),
                Mock.Of<ISubprocessExecutor>()
            );
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region Microsoft仕様準拠の必須プロパティテスト

        [Fact]
        public async Task GetTransformCapabilities_ShouldReturnAllRequiredProperties()
        {
            // Arrange - Microsoft仕様の必須プロパティ
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    CanMove = true,
                    CanResize = true,
                    CanRotate = false
                }
            };
            _mockTransformService.Setup(s => s.GetTransformCapabilitiesAsync("window1", "TestWindow", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetTransformCapabilities("window1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.GetTransformCapabilitiesAsync("window1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("All required Transform capabilities test passed");
        }

        [Theory]
        [InlineData(true, true, true)]   // すべて可能
        [InlineData(true, false, false)] // 移動のみ可能
        [InlineData(false, true, false)] // リサイズのみ可能
        [InlineData(false, false, true)]  // 回転のみ可能
        [InlineData(false, false, false)] // すべて不可
        public async Task GetTransformCapabilities_ShouldReturnCorrectCapabilities(bool canMove, bool canResize, bool canRotate)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    CanMove = canMove,
                    CanResize = canResize,
                    CanRotate = canRotate
                }
            };
            _mockTransformService.Setup(s => s.GetTransformCapabilitiesAsync("element1", "App", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetTransformCapabilities("element1", "App");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.GetTransformCapabilitiesAsync("element1", "App", null, 30), Times.Once);
            _output.WriteLine($"Transform capabilities test passed: Move={canMove}, Resize={canResize}, Rotate={canRotate}");
        }

        #endregion

        #region Microsoft仕様準拠のMoveメソッドテスト

        [Theory]
        [InlineData(100.0, 200.0)]  // 正の座標
        [InlineData(-50.0, -100.0)] // 負の座標
        [InlineData(0.0, 0.0)]      // 原点
        [InlineData(1920.0, 1080.0)] // 大きな座標
        public async Task MoveElement_WithValidCoordinates_ShouldSucceed(double x, double y)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new { ElementId = "movableWindow", X = x, Y = y, Operation = "Move" }
            };
            _mockTransformService.Setup(s => s.MoveElementAsync("movableWindow", x, y, "MainApp", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.MoveElement("movableWindow", x, y, "MainApp");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("movableWindow", x, y, "MainApp", null, 30), Times.Once);
            _output.WriteLine($"Move element test passed for coordinates: ({x}, {y})");
        }

        [Fact]
        public async Task MoveElement_OnNonMovableElement_ShouldThrowInvalidOperationException()
        {
            // Arrange - Microsoft仕様：CanMove=falseの場合にInvalidOperationExceptionをスロー
            _mockTransformService.Setup(s => s.MoveElementAsync("fixedElement", 100.0, 200.0, "App", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "Element cannot be moved (CanMove = false)"
                               }));

            // Act
            var result = await _tools.MoveElement("fixedElement", 100.0, 200.0, "App");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("fixedElement", 100.0, 200.0, "App", null, 30), Times.Once);
            _output.WriteLine("Non-movable element error handling test passed");
        }

        #endregion

        #region Microsoft仕様準拠のResizeメソッドテスト

        [Theory]
        [InlineData(800.0, 600.0)]   // 標準的なサイズ
        [InlineData(1920.0, 1080.0)] // 大きなサイズ
        [InlineData(320.0, 240.0)]   // 小さなサイズ
        [InlineData(100.5, 200.75)]  // 小数点サイズ
        public async Task ResizeElement_WithValidDimensions_ShouldSucceed(double width, double height)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new { ElementId = "resizableWindow", Width = width, Height = height, Operation = "Resize" }
            };
            _mockTransformService.Setup(s => s.ResizeElementAsync("resizableWindow", width, height, "Designer", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ResizeElement("resizableWindow", width, height, "Designer");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.ResizeElementAsync("resizableWindow", width, height, "Designer", null, 30), Times.Once);
            _output.WriteLine($"Resize element test passed for dimensions: {width}x{height}");
        }

        [Theory]
        [InlineData(0.0, 100.0)]   // 幅がゼロ
        [InlineData(100.0, 0.0)]   // 高さがゼロ
        [InlineData(-100.0, 200.0)] // 負の幅
        [InlineData(200.0, -100.0)] // 負の高さ
        public async Task ResizeElement_WithInvalidDimensions_ShouldHandleError(double width, double height)
        {
            // Arrange
            _mockTransformService.Setup(s => s.ResizeElementAsync("window1", width, height, "App", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "Width and height must be greater than 0"
                               }));

            // Act
            var result = await _tools.ResizeElement("window1", width, height, "App");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.ResizeElementAsync("window1", width, height, "App", null, 30), Times.Once);
            _output.WriteLine($"Invalid dimensions error handling test passed: {width}x{height}");
        }

        [Fact]
        public async Task ResizeElement_OnNonResizableElement_ShouldThrowInvalidOperationException()
        {
            // Arrange - Microsoft仕様：CanResize=falseの場合にInvalidOperationExceptionをスロー
            _mockTransformService.Setup(s => s.ResizeElementAsync("fixedSizeDialog", 800.0, 600.0, "App", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "Element cannot be resized (CanResize = false)"
                               }));

            // Act
            var result = await _tools.ResizeElement("fixedSizeDialog", 800.0, 600.0, "App");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.ResizeElementAsync("fixedSizeDialog", 800.0, 600.0, "App", null, 30), Times.Once);
            _output.WriteLine("Non-resizable element error handling test passed");
        }

        #endregion

        #region Microsoft仕様準拠のRotateメソッドテスト

        [Theory]
        [InlineData(90.0)]    // 90度回転
        [InlineData(180.0)]   // 180度回転
        [InlineData(270.0)]   // 270度回転
        [InlineData(360.0)]   // 360度回転
        [InlineData(45.5)]    // 小数点角度
        [InlineData(-90.0)]   // 負の角度
        public async Task RotateElement_WithValidDegrees_ShouldSucceed(double degrees)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new { ElementId = "rotatableImage", Degrees = degrees, Operation = "Rotate" }
            };
            _mockTransformService.Setup(s => s.RotateElementAsync("rotatableImage", degrees, "GraphicsApp", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.RotateElement("rotatableImage", degrees, "GraphicsApp");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.RotateElementAsync("rotatableImage", degrees, "GraphicsApp", null, 30), Times.Once);
            _output.WriteLine($"Rotate element test passed for degrees: {degrees}");
        }

        [Fact]
        public async Task RotateElement_OnNonRotatableElement_ShouldThrowInvalidOperationException()
        {
            // Arrange - Microsoft仕様：CanRotate=falseの場合にInvalidOperationExceptionをスロー
            _mockTransformService.Setup(s => s.RotateElementAsync("textBox", 45.0, "Editor", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "Element cannot be rotated (CanRotate = false)"
                               }));

            // Act
            var result = await _tools.RotateElement("textBox", 45.0, "Editor");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.RotateElementAsync("textBox", 45.0, "Editor", null, 30), Times.Once);
            _output.WriteLine("Non-rotatable element error handling test passed");
        }

        #endregion

        #region エラーハンドリングテスト

        [Fact]
        public async Task GetTransformCapabilities_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockTransformService.Setup(s => s.GetTransformCapabilitiesAsync("nonExistentElement", "TestWindow", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "Element 'nonExistentElement' not found"
                               }));

            // Act
            var result = await _tools.GetTransformCapabilities("nonExistentElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.GetTransformCapabilitiesAsync("nonExistentElement", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }

        [Fact]
        public async Task MoveElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockTransformService.Setup(s => s.MoveElementAsync("unsupportedElement", 100.0, 200.0, "App", null, 30))
                               .Returns(Task.FromResult(new ServerEnhancedResponse<object>
                               {
                                   Success = false,
                                   Error = "TransformPattern not supported"
                               }));

            // Act
            var result = await _tools.MoveElement("unsupportedElement", 100.0, 200.0, "App");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("unsupportedElement", 100.0, 200.0, "App", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Theory]
        [InlineData("", "TestWindow")]
        [InlineData("element1", "")]
        public async Task GetTransformCapabilities_WithEmptyParameters_ShouldCallService(string elementId, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { CanMove = true, CanResize = true, CanRotate = false } };
            _mockTransformService.Setup(s => s.GetTransformCapabilitiesAsync(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetTransformCapabilities(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.GetTransformCapabilitiesAsync(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task MoveElement_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { ElementId = "window1", X = 100.0, Y = 200.0 } };
            _mockTransformService.Setup(s => s.MoveElementAsync("window1", 100.0, 200.0, "App", processId, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.MoveElement("window1", 100.0, 200.0, "App", processId);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("window1", 100.0, 200.0, "App", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task ResizeElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { ElementId = "window1", Width = 800.0, Height = 600.0 } };
            _mockTransformService.Setup(s => s.ResizeElementAsync("window1", 800.0, 600.0, "App", null, timeoutSeconds))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ResizeElement("window1", 800.0, 600.0, "App", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.ResizeElementAsync("window1", 800.0, 600.0, "App", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion

        #region 変換シーケンステスト

        [Fact]
        public async Task TransformElement_MultipleTransformations_ShouldExecuteInSequence()
        {
            // Arrange
            var moveResult = new ServerEnhancedResponse<object> { Success = true, Data = new { ElementId = "transformableWindow", X = 200.0, Y = 300.0, Operation = "Move" } };
            var resizeResult = new ServerEnhancedResponse<object> { Success = true, Data = new { ElementId = "transformableWindow", Width = 1024.0, Height = 768.0, Operation = "Resize" } };
            var rotateResult = new ServerEnhancedResponse<object> { Success = true, Data = new { ElementId = "transformableWindow", Degrees = 45.0, Operation = "Rotate" } };

            _mockTransformService.Setup(s => s.MoveElementAsync("transformableWindow", 200.0, 300.0, "CADApp", null, 30))
                               .Returns(Task.FromResult(moveResult));
            _mockTransformService.Setup(s => s.ResizeElementAsync("transformableWindow", 1024.0, 768.0, "CADApp", null, 30))
                               .Returns(Task.FromResult(resizeResult));
            _mockTransformService.Setup(s => s.RotateElementAsync("transformableWindow", 45.0, "CADApp", null, 30))
                               .Returns(Task.FromResult(rotateResult));

            // Act
            var r1 = await _tools.MoveElement("transformableWindow", 200.0, 300.0, "CADApp");
            var r2 = await _tools.ResizeElement("transformableWindow", 1024.0, 768.0, "CADApp");
            var r3 = await _tools.RotateElement("transformableWindow", 45.0, "CADApp");

            // Assert
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.NotNull(r3);

            _mockTransformService.Verify(s => s.MoveElementAsync("transformableWindow", 200.0, 300.0, "CADApp", null, 30), Times.Once);
            _mockTransformService.Verify(s => s.ResizeElementAsync("transformableWindow", 1024.0, 768.0, "CADApp", null, 30), Times.Once);
            _mockTransformService.Verify(s => s.RotateElementAsync("transformableWindow", 45.0, "CADApp", null, 30), Times.Once);

            _output.WriteLine("Multiple transformations sequence test passed");
        }

        #endregion

        #region 境界値テスト

        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0.000001, 0.000001)]
        public async Task MoveElement_WithExtremeBoundaryValues_ShouldHandleCorrectly(double x, double y)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new { ElementId = "element1", X = x, Y = y, Operation = "Move" }
            };
            _mockTransformService.Setup(s => s.MoveElementAsync("element1", x, y, "TestApp", null, 30))
                               .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.MoveElement("element1", x, y, "TestApp");

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("element1", x, y, "TestApp", null, 30), Times.Once);
            _output.WriteLine($"Extreme boundary values test passed: ({x}, {y})");
        }

        #endregion
    }
}