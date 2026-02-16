using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Tests.UnitTests.Base;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// DockPattern               /// Microsoft            DockPattern                                /// </summary>
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

        #region Microsoft         DockPosition     
        [Theory]
        [MemberData(nameof(GetValidDockPositions))]
        public async Task DockElement_WithValidDockPositions_ShouldSucceed(string dockPosition)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = $"Docked to {dockPosition}", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", dockPosition } } }
            };
            _mockService.Setup(s => s.DockElementAsync("dockablePane", "TestWindow", dockPosition, null, null, 30))
                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "dockablePane", name: "TestWindow", dockPosition: dockPosition);

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("dockablePane", "TestWindow", dockPosition, null, null, 30), Times.Once);
            _output.WriteLine($"  DockElement test passed for position: {dockPosition}");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData("center")]
        [InlineData("middle")]
        public async Task DockElement_WithInvalidDockPositions_ShouldHandleError(string invalidPosition)
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("dockablePane", "TestWindow", invalidPosition, null, null, 30))
                       .ThrowsAsync(new ArgumentException($"Unsupported dock position: {invalidPosition}"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _tools.DockElement(automationId: "dockablePane", name: "TestWindow", dockPosition: invalidPosition));

            _mockService.Verify(s => s.DockElementAsync("dockablePane", "TestWindow", invalidPosition, null, null, 30), Times.Once);
            _output.WriteLine($"  DockElement correctly rejected invalid position: {invalidPosition}");
        }

        public static IEnumerable<object[]> GetValidDockPositions()
        {
            return CommonTestData.DockPositions.Select(position => new object[] { position });
        }

        #endregion

        #region DockPattern            
        [Fact]
        public async Task DockElement_ChangingFromNoneToTop_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Top", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Top" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("toolbar", "MainWindow", "top", null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "toolbar", name: "MainWindow", dockPosition: "top");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("toolbar", "MainWindow", "top", null, null, 30), Times.Once);
            _output.WriteLine("Position change from None to Top test passed");
        }

        [Fact]
        public async Task DockElement_ChangingFromLeftToRight_ShouldReturnCorrectPositions()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Right", Metadata = new Dictionary<string, object> { { "PreviousPosition", "Left" }, { "NewPosition", "Right" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("sidebar", "IDE", "right", null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "sidebar", name: "IDE", dockPosition: "right");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("sidebar", "IDE", "right", null, null, 30), Times.Once);
            _output.WriteLine("Position change from Left to Right test passed");
        }

        [Fact]
        public async Task DockElement_SettingToFill_ShouldExpandElement()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Fill", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Fill" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("contentArea", "App", "fill", null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "contentArea", name: "App", dockPosition: "fill");

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("contentArea", "App", "fill", null, null, 30), Times.Once);
            _output.WriteLine("Fill dock position test passed");
        }

        #endregion

        #region                      
        [Fact]
        public async Task DockElement_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("nonExistentElement", "TestWindow", "top", null, null, 30))
                       .ThrowsAsync(new InvalidOperationException(CommonTestData.ErrorMessages.ElementNotFound));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement(automationId: "nonExistentElement", name: "TestWindow", dockPosition: "top"));

            _mockService.Verify(s => s.DockElementAsync("nonExistentElement", "TestWindow", "top", null, null, 30), Times.Once);
            _output.WriteLine("  Non-existent element error handling test passed");
        }

        [Fact]
        public async Task DockElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockService.Setup(s => s.DockElementAsync("staticText", "TestWindow", "top", null, null, 30))
                       .ThrowsAsync(new InvalidOperationException(CommonTestData.ErrorMessages.PatternNotSupported));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.DockElement(automationId: "staticText", name: "TestWindow", dockPosition: "top"));

            _mockService.Verify(s => s.DockElementAsync("staticText", "TestWindow", "top", null, null, 30), Times.Once);
            _output.WriteLine("  Unsupported element error handling test passed");
        }

        #endregion

        #region                   
        [Fact]
        public void DockElement_ParameterValidation_ShouldHandleAllScenarios()
        {
            // Microsoft specification compliance test
            PatternTestHelpers.VerifyMicrosoftSpecCompliance(
                _mockService,
                "DockPattern",
                new[] { "DockElementAsync" },
                _output);

            //                  
            PatternTestHelpers.VerifyStandardParameterValidation(
                _mockService,
                "DockElementAsync",
                _output,
                "top");

            //                    
            PatternTestHelpers.VerifyTimeoutHandling(
                _mockService,
                "DockElementAsync",
                _output,
                "top");

            _output.WriteLine("  All DockElement parameter validation tests completed");
        }



        [Theory]
        [MemberData(nameof(GetTimeoutTestData))]
        public async Task DockElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Dock", Details = "Docked to Bottom", Metadata = new Dictionary<string, object> { { "PreviousPosition", "None" }, { "NewPosition", "Bottom" } } }
            };
            _mockService.Setup(s => s.DockElementAsync("element1", "TestWindow", "bottom", null, null, timeoutSeconds))
                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.DockElement(automationId: "element1", name: "TestWindow", dockPosition: "bottom", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockService.Verify(s => s.DockElementAsync("element1", "TestWindow", "bottom", null, null, timeoutSeconds), Times.Once);
            _output.WriteLine($"  Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        public static IEnumerable<object[]> GetTimeoutTestData()
        {
            return CommonTestData.ValidTimeouts.Select(timeout => new object[] { timeout });
        }

        #endregion
    }
}

