using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Models.Serialization;

namespace UIAutomationMCP.Tests.UnitTests
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SelectionPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<ISelectionService> _mockSelectionService;

        public SelectionPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockSelectionService = new Mock<ISelectionService>();

            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInteraction = new Mock<IInteractionService>();
            var mockText = new Mock<ITextService>();
            var mockLayout = new Mock<ILayoutService>();
            var mockGridTable = new Mock<IGridTableService>();
            var mockAdvancedPattern = new Mock<IAdvancedPatternService>();
            var mockWindow = new Mock<IWindowService>();
            var mockTransform = new Mock<ITransformService>();
            var mockEventMonitor = new Mock<IEventMonitorService>();
            var mockFocus = new Mock<IFocusService>();
            var mockLog = new Mock<IMcpLogService>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                mockInteraction.Object,
                _mockSelectionService.Object,
                mockText.Object,
                mockLayout.Object,
                mockGridTable.Object,
                mockAdvancedPattern.Object,
                mockWindow.Object,
                mockTransform.Object,
                mockEventMonitor.Object,
                mockFocus.Object,
                mockLog.Object);
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task SelectionAction_Select_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Select" }
            };
            _mockSelectionService.Setup(s => s.SelectItemAsync("item1", null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectionAction(action: "select", automationId: "item1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.SelectItemAsync("item1", null, null, null, 30), Times.Once);
        }

        [Fact]
        public async Task SelectionAction_AddToSelection_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "AddToSelection" }
            };
            _mockSelectionService.Setup(s => s.AddToSelectionAsync("item1", null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectionAction(action: "add", automationId: "item1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.AddToSelectionAsync("item1", null, null, null, 30), Times.Once);
        }

        [Fact]
        public async Task SelectionAction_RemoveFromSelection_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "RemoveFromSelection" }
            };
            _mockSelectionService.Setup(s => s.RemoveFromSelectionAsync("item1", null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectionAction(action: "remove", automationId: "item1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.RemoveFromSelectionAsync("item1", null, null, null, 30), Times.Once);
        }

        [Fact]
        public async Task SelectionAction_ClearSelection_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult { Success = true, Action = "ClearSelection" }
            };
            _mockSelectionService.Setup(s => s.ClearSelectionAsync("container1", null, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectionAction(action: "clear", automationId: "container1");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.ClearSelectionAsync("container1", null, null, null, 30), Times.Once);
        }
    }
}
