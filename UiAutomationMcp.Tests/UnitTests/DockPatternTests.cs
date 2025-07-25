using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Tests.UnitTests.Base;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// DockPatternの単体テスト
    /// Microsoft仕様に基づいたDockPatternの機能をモックベースでテストします
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class DockPatternTests : BasePatternTests<ILayoutService>
    {
        public DockPatternTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override Mock<ILayoutService> CreateServiceMock()
        {
            return new Mock<ILayoutService>();
        }

        #region Microsoft仕様準拠のDockPositionテスト

        [Theory]
        [MemberData(nameof(GetValidDockPositions))]
        public async Task DockElement_WithValidDockPositions_ShouldSucceed(string dockPosition)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = $"Docked to {dockPosition}", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", dockPosition } } }
            };
            _mockService.Setup(s => s.DockElementAsync("dockablePane", null, dockPosition, "TestWindow", null, 30))
                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement("dockablePane", "TestWindow", dockPosition);

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("dockablePane", null, dockPosition, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"✓ DockElement test passed for position: {dockPosition}");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData("center")]
        [InlineData("middle")]
        public async Task DockElement_WithInvalidDockPositions_ShouldHandleError(string invalidPosition)
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("dockablePane", null, invalidPosition, "TestWindow", null, 30))
                       .ThrowsAsync(new ArgumentException($"Unsupported dock position: {invalidPosition}"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _tools.DockElement("dockablePane", invalidPosition, "TestWindow"));

            _mockService.Verify(s => s.DockElementAsync("dockablePane", null, invalidPosition, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"✓ DockElement correctly rejected invalid position: {invalidPosition}");
        }

        public static IEnumerable<object[]> GetValidDockPositions()
        {
            return CommonTestData.DockPositions.Select(position => new object[] { position });
        }

        #endregion

        #region DockPattern状態変更テスト

        [Fact]
        public async Task DockElement_ChangingFromNoneToTop_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Top", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Top" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("toolbar", null, "top", "MainWindow", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement("toolbar", "top", "MainWindow");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("toolbar", null, "top", "MainWindow", null, 30), Times.Once);
            _output.WriteLine("Position change from None to Top test passed");
        }

        [Fact]
        public async Task DockElement_ChangingFromLeftToRight_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Right", Metadata = new Dictionary<string, object> { { "PreviousPosition", "Left" }, { "NewPosition", "Right" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("sidebar", null, "right", "IDE", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement("sidebar", "right", "IDE");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("sidebar", null, "right", "IDE", null, 30), Times.Once);
            _output.WriteLine("Position change from Left to Right test passed");
        }

        [Fact]
        public async Task DockElement_SettingToFill_ShouldExpandElement()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Fill", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Fill" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("contentArea", null, "fill", "App", null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement("contentArea", "fill", "App");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("contentArea", null, "fill", "App", null, 30), Times.Once);
            _output.WriteLine("Fill dock position test passed");
        }

        #endregion

        #region エラーハンドリングテスト

        [Fact]
        public async Task DockElement_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("nonExistentElement", null, "top", "TestWindow", null, 30))
                       .ThrowsAsync(new InvalidOperationException(CommonTestData.ErrorMessages.ElementNotFound));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement("nonExistentElement", "top", "TestWindow"));

            _mockService.Verify(s => s.DockElementAsync("nonExistentElement", null, "top", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("✓ Non-existent element error handling test passed");
        }

        [Fact]
        public async Task DockElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("staticText", null, "top", "TestWindow", null, 30))
                       .ThrowsAsync(new InvalidOperationException(CommonTestData.ErrorMessages.PatternNotSupported));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement("staticText", "top", "TestWindow"));

            _mockService.Verify(s => s.DockElementAsync("staticText", null, "top", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("✓ Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Fact]
        public void DockElement_ParameterValidation_ShouldHandleAllScenarios()
        {
            // Microsoft仕様準拠テスト
            PatternTestHelpers.VerifyMicrosoftSpecCompliance(
                _mockService, 
                "DockPattern", 
                new[] { "DockElementAsync" }, 
                _output);

            // 標準パラメータ検証
            PatternTestHelpers.VerifyStandardParameterValidation(
                _mockService, 
                "DockElementAsync", 
                _output, 
                "top");

            // タイムアウト処理検証
            PatternTestHelpers.VerifyTimeoutHandling(
                _mockService, 
                "DockElementAsync", 
                _output, 
                "top");

            _output.WriteLine("✓ All DockElement parameter validation tests completed");
        }

        [Theory]
        [MemberData(nameof(GetProcessIdTestData))]
        public async Task DockElement_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Top", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Top" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("element1", null, "top", "TestWindow", processId, 30))
                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "element1", dockPosition: "top", controlType: "TestWindow", processId: processId);

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("element1", null, "top", "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"✓ ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [MemberData(nameof(GetTimeoutTestData))]
        public async Task DockElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Bottom", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Bottom" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("element1", null, "bottom", "TestWindow", null, timeoutSeconds))
                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement("element1", "bottom", "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("element1", null, "bottom", "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"✓ Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        public static IEnumerable<object[]> GetProcessIdTestData()
        {
            return CommonTestData.ValidProcessIds.Select(id => new object[] { id });
        }

        public static IEnumerable<object[]> GetTimeoutTestData()
        {
            return CommonTestData.ValidTimeouts.Select(timeout => new object[] { timeout });
        }

        #endregion
    }
}