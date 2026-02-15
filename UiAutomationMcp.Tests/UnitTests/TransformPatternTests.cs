using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Models.Results;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Models.Serialization;

namespace UIAutomationMCP.Tests.UnitTests
{
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

            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInteraction = new Mock<IInteractionService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockLayout = new Mock<ILayoutService>();
            var mockGridTable = new Mock<IGridTableService>();
            var mockAdvancedPattern = new Mock<IAdvancedPatternService>();
            var mockWindow = new Mock<IWindowService>();
            var mockEventMonitor = new Mock<IEventMonitorService>();
            var mockFocus = new Mock<IFocusService>();
            var mockLog = new Mock<IMcpLogService>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                mockInteraction.Object,
                mockSelection.Object,
                mockText.Object,
                mockLayout.Object,
                mockGridTable.Object,
                mockAdvancedPattern.Object,
                mockWindow.Object,
                _mockTransformService.Object,
                mockEventMonitor.Object,
                mockFocus.Object,
                mockLog.Object);
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task TransformElement_Move_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Move" }
            };
            _mockTransformService.Setup(s => s.MoveElementAsync("element1", null, 100, 200, null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.TransformElement(action: "move", automationId: "element1", value1: 100, value2: 200);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.MoveElementAsync("element1", null, 100, 200, null, null, null, 30), Times.Once);
        }

        [Fact]
        public async Task TransformElement_Resize_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Resize" }
            };
            _mockTransformService.Setup(s => s.ResizeElementAsync("element1", null, 800, 600, null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.TransformElement(action: "resize", automationId: "element1", value1: 800, value2: 600);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.ResizeElementAsync("element1", null, 800, 600, null, null, null, 30), Times.Once);
        }

        [Fact]
        public async Task TransformElement_Rotate_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Rotate" }
            };
            _mockTransformService.Setup(s => s.RotateElementAsync("element1", null, 45, null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.TransformElement(action: "rotate", automationId: "element1", value1: 45);

            // Assert
            Assert.NotNull(result);
            _mockTransformService.Verify(s => s.RotateElementAsync("element1", null, 45, null, null, null, 30), Times.Once);
        }
    }
}
