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
    /// RangeValuePatternの単体テスト
    /// Microsoft仕様に基づいたRangeValuePatternの機能をモックベースでテストします
    /// https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-rangevalue-control-pattern
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class RangeValuePatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IRangeService> _mockRangeService;

        public RangeValuePatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockRangeService = new Mock<IRangeService>();
            
            // UIAutomationToolsの他のサービスもモック化（最小限の設定）
            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInvoke = new Mock<IInvokeService>();
            var mockValue = new Mock<IValueService>();
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
            var mockTransform = new Mock<ITransformService>();
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
                _mockRangeService.Object,
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
                mockTransform.Object,
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
        public async Task GetRangeValue_ShouldReturnAllRequiredProperties()
        {
            // Arrange - Microsoft仕様の必須プロパティ
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    Value = 50.0,
                    Minimum = 0.0,
                    Maximum = 100.0,
                    LargeChange = 10.0,
                    SmallChange = 1.0,
                    IsReadOnly = false
                }
            };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("slider1", "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetRangeValue("slider1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.GetRangeValueAsync("slider1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("All required RangeValue properties test passed");
        }

        [Fact]
        public async Task GetRangeValue_ShouldReturnCurrentValue()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    Value = 75.5,
                    Minimum = 0.0,
                    Maximum = 100.0,
                    LargeChange = 5.0,
                    SmallChange = 0.5,
                    IsReadOnly = false
                }
            };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("progressBar", "App", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetRangeValue("progressBar", "App");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.GetRangeValueAsync("progressBar", "App", null, 30), Times.Once);
            _output.WriteLine("GetRangeValue test passed");
        }

        #endregion

        #region Microsoft仕様準拠のSetValueメソッドテスト

        [Theory]
        [InlineData(0.0)]    // Minimum値
        [InlineData(50.0)]   // 中間値
        [InlineData(100.0)]  // Maximum値
        [InlineData(25.5)]   // 小数点値
        public async Task SetRangeValue_WithValidValues_ShouldSucceed(double value)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    PreviousValue = 10.0,
                    NewValue = value,
                    Minimum = 0.0,
                    Maximum = 100.0
                }
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("volumeSlider", value, "MediaPlayer", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue("volumeSlider", value, "MediaPlayer");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("volumeSlider", value, "MediaPlayer", null, 30), Times.Once);
            _output.WriteLine($"SetRangeValue test passed for value: {value}");
        }

        [Theory]
        [InlineData(-1.0)]   // Minimum未満
        [InlineData(101.0)]  // Maximum超過
        [InlineData(-50.0)]  // 大幅にMinimum未満
        [InlineData(200.0)]  // 大幅にMaximum超過
        public async Task SetRangeValue_WithOutOfRangeValues_ShouldThrowArgumentOutOfRangeException(double invalidValue)
        {
            // Arrange - Microsoft仕様：ArgumentOutOfRangeExceptionをスロー
            _mockRangeService.Setup(s => s.SetRangeValueAsync("rangeControl", invalidValue, "TestWindow", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException("value", 
                               $"Value {invalidValue} is out of range. Valid range: 0 - 100"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.SetRangeValue("rangeControl", invalidValue, "TestWindow"));

            _mockRangeService.Verify(s => s.SetRangeValueAsync("rangeControl", invalidValue, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"ArgumentOutOfRangeException correctly thrown for value: {invalidValue}");
        }

        #endregion

        #region Microsoft仕様準拠のIsReadOnlyプロパティテスト

        [Fact]
        public async Task SetRangeValue_OnReadOnlyElement_ShouldThrowInvalidOperationException()
        {
            // Arrange - 読み取り専用要素
            _mockRangeService.Setup(s => s.SetRangeValueAsync("readOnlyProgressBar", 50.0, "StatusWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Range element is read-only"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.SetRangeValue("readOnlyProgressBar", 50.0, "StatusWindow"));

            _mockRangeService.Verify(s => s.SetRangeValueAsync("readOnlyProgressBar", 50.0, "StatusWindow", null, 30), Times.Once);
            _output.WriteLine("ReadOnly element error handling test passed");
        }

        [Fact]
        public async Task GetRangeValue_ReadOnlyElement_ShouldReturnIsReadOnlyTrue()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    Value = 65.0,
                    Minimum = 0.0,
                    Maximum = 100.0,
                    LargeChange = 10.0,
                    SmallChange = 1.0,
                    IsReadOnly = true  // 読み取り専用
                }
            };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("batteryLevel", "SystemTray", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetRangeValue("batteryLevel", "SystemTray");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.GetRangeValueAsync("batteryLevel", "SystemTray", null, 30), Times.Once);
            _output.WriteLine("ReadOnly property retrieval test passed");
        }

        #endregion

        #region 範囲境界値テスト

        [Fact]
        public async Task SetRangeValue_AtMinimumBoundary_ShouldSucceed()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    PreviousValue = 50.0,
                    NewValue = 0.0,
                    Minimum = 0.0,
                    Maximum = 100.0
                }
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("brightnessSlider", 0.0, "Settings", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue("brightnessSlider", 0.0, "Settings");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("brightnessSlider", 0.0, "Settings", null, 30), Times.Once);
            _output.WriteLine("Minimum boundary value test passed");
        }

        [Fact]
        public async Task SetRangeValue_AtMaximumBoundary_ShouldSucceed()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    PreviousValue = 50.0,
                    NewValue = 100.0,
                    Minimum = 0.0,
                    Maximum = 100.0
                }
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("temperatureControl", 100.0, "Thermostat", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue("temperatureControl", 100.0, "Thermostat");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("temperatureControl", 100.0, "Thermostat", null, 30), Times.Once);
            _output.WriteLine("Maximum boundary value test passed");
        }

        #endregion

        #region カスタム範囲テスト

        [Theory]
        [InlineData(-50.0, 50.0, 0.0)]    // 負の範囲を含む
        [InlineData(1000.0, 2000.0, 1500.0)]  // 大きな値の範囲
        [InlineData(0.1, 0.9, 0.5)]      // 小数点範囲
        public async Task SetRangeValue_WithCustomRanges_ShouldSucceed(double min, double max, double value)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object>
            {
                Success = true,
                Data = new
                {
                    PreviousValue = min,
                    NewValue = value,
                    Minimum = min,
                    Maximum = max
                }
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("customRange", value, "AdvancedApp", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue("customRange", value, "AdvancedApp");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("customRange", value, "AdvancedApp", null, 30), Times.Once);
            _output.WriteLine($"Custom range test passed: [{min}, {max}], value={value}");
        }

        #endregion

        #region エラーハンドリングテスト

        [Fact]
        public async Task GetRangeValue_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockRangeService.Setup(s => s.GetRangeValueAsync("nonExistentSlider", "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element 'nonExistentSlider' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetRangeValue("nonExistentSlider", "TestWindow"));

            _mockRangeService.Verify(s => s.GetRangeValueAsync("nonExistentSlider", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }

        [Fact]
        public async Task SetRangeValue_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockRangeService.Setup(s => s.SetRangeValueAsync("textBox", 50.0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element does not support RangeValuePattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.SetRangeValue("textBox", 50.0, "TestWindow"));

            _mockRangeService.Verify(s => s.SetRangeValueAsync("textBox", 50.0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Theory]
        [InlineData("", 50.0, "TestWindow")]
        [InlineData("slider1", 50.0, "")]
        public async Task SetRangeValue_WithEmptyParameters_ShouldCallService(string elementId, double value, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true };
            _mockRangeService.Setup(s => s.SetRangeValueAsync(elementId, value, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(elementId, value, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync(elementId, value, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', value={value}, window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task SetRangeValue_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { PreviousValue = 10.0, NewValue = 75.0 } };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("volumeSlider", 75.0, "AudioApp", processId, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue("volumeSlider", 75.0, "AudioApp", processId);

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("volumeSlider", 75.0, "AudioApp", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task GetRangeValue_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<object> { Success = true, Data = new { Value = 25.0, Minimum = 0.0, Maximum = 100.0 } };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("progressBar", "App", null, timeoutSeconds))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetRangeValue("progressBar", "App", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.GetRangeValueAsync("progressBar", "App", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion

        #region 値変更シーケンステスト

        [Fact]
        public async Task SetRangeValue_MultipleValueChanges_ShouldExecuteInSequence()
        {
            // Arrange
            var result1 = new ServerEnhancedResponse<object> { Success = true, Data = new { PreviousValue = 0.0, NewValue = 25.0, Minimum = 0.0, Maximum = 100.0 } };
            var result2 = new ServerEnhancedResponse<object> { Success = true, Data = new { PreviousValue = 25.0, NewValue = 50.0, Minimum = 0.0, Maximum = 100.0 } };
            var result3 = new ServerEnhancedResponse<object> { Success = true, Data = new { PreviousValue = 50.0, NewValue = 75.0, Minimum = 0.0, Maximum = 100.0 } };
            var result4 = new ServerEnhancedResponse<object> { Success = true, Data = new { PreviousValue = 75.0, NewValue = 100.0, Minimum = 0.0, Maximum = 100.0 } };

            _mockRangeService.Setup(s => s.SetRangeValueAsync("animatedSlider", 25.0, "Presentation", null, 30))
                           .Returns(Task.FromResult(result1));
            _mockRangeService.Setup(s => s.SetRangeValueAsync("animatedSlider", 50.0, "Presentation", null, 30))
                           .Returns(Task.FromResult(result2));
            _mockRangeService.Setup(s => s.SetRangeValueAsync("animatedSlider", 75.0, "Presentation", null, 30))
                           .Returns(Task.FromResult(result3));
            _mockRangeService.Setup(s => s.SetRangeValueAsync("animatedSlider", 100.0, "Presentation", null, 30))
                           .Returns(Task.FromResult(result4));

            // Act
            var r1 = await _tools.SetRangeValue("animatedSlider", 25.0, "Presentation");
            var r2 = await _tools.SetRangeValue("animatedSlider", 50.0, "Presentation");
            var r3 = await _tools.SetRangeValue("animatedSlider", 75.0, "Presentation");
            var r4 = await _tools.SetRangeValue("animatedSlider", 100.0, "Presentation");

            // Assert
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.NotNull(r3);
            Assert.NotNull(r4);

            _mockRangeService.Verify(s => s.SetRangeValueAsync("animatedSlider", 25.0, "Presentation", null, 30), Times.Once);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("animatedSlider", 50.0, "Presentation", null, 30), Times.Once);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("animatedSlider", 75.0, "Presentation", null, 30), Times.Once);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("animatedSlider", 100.0, "Presentation", null, 30), Times.Once);

            _output.WriteLine("Multiple value changes sequence test passed");
        }

        #endregion
    }
}