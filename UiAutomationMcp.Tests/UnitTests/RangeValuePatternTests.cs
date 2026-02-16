using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Models.Results;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;

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
        private readonly Mock<IInteractionService> _mockInteractionService;
        private readonly Mock<IRangeService> _mockRangeService;

        public RangeValuePatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockInteractionService = new Mock<IInteractionService>();
            _mockRangeService = new Mock<IRangeService>(); // Kept for legacy if needed, but we use Interaction for Tools test

            // UIAutomationToolsの他のサービスもモック化（最小限の設定）
            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockWindow = new Mock<IWindowService>();
            var mockTransform = new Mock<ITransformService>();
            var mockFocus = new Mock<IFocusService>();
            var mockLog = new Mock<IMcpLogService>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                _mockInteractionService.Object,
                mockSelection.Object,
                mockText.Object,
                Mock.Of<ILayoutService>(),
                Mock.Of<IGridTableService>(),
                Mock.Of<IAdvancedPatternService>(),
                mockWindow.Object,
                mockTransform.Object,
                Mock.Of<IEventMonitorService>(),
                mockFocus.Object,
                mockLog.Object);
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region Microsoft仕様準拠の必須プロパティテスト
        // GetRangeValue tests removed - method deleted, functionality moved to SearchElements includeDetails
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
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    Action = "SetValue",
                    ReturnValue = value,
                    Details = $"Set value from 10.0 to {value}"
                }
            };
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("volumeSlider", "MediaPlayer", value, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(automationId: "volumeSlider", name: "MediaPlayer", value: value);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("volumeSlider", "MediaPlayer", value, null, null, 30), Times.Once);
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
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("rangeControl", "TestWindow", invalidValue, null, null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException("value",
                               $"Value {invalidValue} is out of range. Valid range: 0 - 100"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.SetRangeValue(automationId: "rangeControl", name: "TestWindow", value: invalidValue));

            _mockInteractionService.Verify(s => s.SetRangeValueAsync("rangeControl", "TestWindow", invalidValue, null, null, 30), Times.Once);
            _output.WriteLine($"ArgumentOutOfRangeException correctly thrown for value: {invalidValue}");
        }

        #endregion

        #region Microsoft仕様準拠のIsReadOnlyプロパティテスト
        [Fact]
        public async Task SetRangeValue_OnReadOnlyElement_ShouldThrowInvalidOperationException()
        {
            // Arrange - 読み取り専用要素
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("readOnlyProgressBar", "StatusWindow", 50.0, null, null, 30))
                           .ThrowsAsync(new InvalidOperationException("Range element is read-only"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.SetRangeValue(automationId: "readOnlyProgressBar", name: "StatusWindow", value: 50.0));

            _mockInteractionService.Verify(s => s.SetRangeValueAsync("readOnlyProgressBar", "StatusWindow", 50.0, null, null, 30), Times.Once);
            _output.WriteLine("ReadOnly element error handling test passed");
        }

        // GetRangeValue_ReadOnlyElement test removed - method deleted, functionality moved to SearchElements includeDetails

        #endregion

        #region 境界値テスト
        [Fact]
        public async Task SetRangeValue_AtMinimumBoundary_ShouldSucceed()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    Action = "SetValue",
                    ReturnValue = 0.0,
                    Details = "Set value from 50.0 to 0.0"
                }
            };
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("brightnessSlider", "Settings", 0.0, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(automationId: "brightnessSlider", name: "Settings", value: 0.0);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("brightnessSlider", "Settings", 0.0, null, null, 30), Times.Once);
            _output.WriteLine("Minimum boundary value test passed");
        }

        [Fact]
        public async Task SetRangeValue_AtMaximumBoundary_ShouldSucceed()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    Action = "SetValue",
                    ReturnValue = 100.0,
                    Details = "Set value from 50.0 to 100.0"
                }
            };
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("temperatureControl", "Thermostat", 100.0, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(automationId: "temperatureControl", name: "Thermostat", value: 100.0);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("temperatureControl", "Thermostat", 100.0, null, null, 30), Times.Once);
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
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    Action = "SetValue",
                    ReturnValue = value,
                    Details = $"Set value from {min} to {value}"
                }
            };
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("customRange", "AdvancedApp", value, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(automationId: "customRange", name: "AdvancedApp", value: value);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("customRange", "AdvancedApp", value, null, null, 30), Times.Once);
            _output.WriteLine($"Custom range test passed: [{min}, {max}], value={value}");
        }

        #endregion

        #region エラーハンドリングテスト
        // GetRangeValue_WithNonExistentElement test removed - method deleted, functionality moved to SearchElements includeDetails

        [Fact]
        public async Task SetRangeValue_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("textBox", "TestWindow", 50.0, null, null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element does not support RangeValuePattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.SetRangeValue(automationId: "textBox", name: "TestWindow", value: 50.0));

            _mockInteractionService.Verify(s => s.SetRangeValueAsync("textBox", "TestWindow", 50.0, null, null, 30), Times.Once);
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
            var expectedResult = new ServerEnhancedResponse<ActionResult> { Success = true, Data = new ActionResult { Success = true, Action = "SetValue", ReturnValue = value, Details = $"Set value to {value}" } };
            _mockInteractionService.Setup(s => s.SetRangeValueAsync(elementId, string.IsNullOrEmpty(windowTitle) ? null : windowTitle, value, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetRangeValue(automationId: elementId, value: value,
                name: string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync(elementId, string.IsNullOrEmpty(windowTitle) ? null : windowTitle, value, null, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', value={value}, window='{windowTitle}'");
        }

        // GetRangeValue_WithCustomTimeout test removed - method deleted, functionality moved to SearchElements includeDetails

        #endregion

        #region 値変更シーケンステスト
        [Fact]
        public async Task SetRangeValue_MultipleValueChanges_ShouldExecuteInSequence()
        {
            // Arrange
            var result1 = new ServerEnhancedResponse<ActionResult> { Success = true, Data = new ActionResult { Success = true, Action = "SetValue", ReturnValue = 25.0, Details = "Set value from 0.0 to 25.0" } };
            var result2 = new ServerEnhancedResponse<ActionResult> { Success = true, Data = new ActionResult { Success = true, Action = "SetValue", ReturnValue = 50.0, Details = "Set value from 25.0 to 50.0" } };
            var result3 = new ServerEnhancedResponse<ActionResult> { Success = true, Data = new ActionResult { Success = true, Action = "SetValue", ReturnValue = 75.0, Details = "Set value from 50.0 to 75.0" } };
            var result4 = new ServerEnhancedResponse<ActionResult> { Success = true, Data = new ActionResult { Success = true, Action = "SetValue", ReturnValue = 100.0, Details = "Set value from 75.0 to 100.0" } };

            _mockInteractionService.Setup(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 25.0, null, null, 30))
                           .Returns(Task.FromResult(result1));
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 50.0, null, null, 30))
                           .Returns(Task.FromResult(result2));
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 75.0, null, null, 30))
                           .Returns(Task.FromResult(result3));
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 100.0, null, null, 30))
                           .Returns(Task.FromResult(result4));

            // Act
            var r1 = await _tools.SetRangeValue(automationId: "animatedSlider", name: "Presentation", value: 25.0);
            var r2 = await _tools.SetRangeValue(automationId: "animatedSlider", name: "Presentation", value: 50.0);
            var r3 = await _tools.SetRangeValue(automationId: "animatedSlider", name: "Presentation", value: 75.0);
            var r4 = await _tools.SetRangeValue(automationId: "animatedSlider", name: "Presentation", value: 100.0);

            // Assert
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.NotNull(r3);
            Assert.NotNull(r4);

            _mockInteractionService.Verify(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 25.0, null, null, 30), Times.Once);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 50.0, null, null, 30), Times.Once);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 75.0, null, null, 30), Times.Once);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("animatedSlider", "Presentation", 100.0, null, null, 30), Times.Once);

            _output.WriteLine("Multiple value changes sequence test passed");
        }

        #endregion
    }
}
