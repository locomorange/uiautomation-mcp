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

namespace UIAutomationMCP.Tests.UnitTests
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GridItemPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IGridTableService> _mockGridTableService;

        public GridItemPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockGridTableService = new Mock<IGridTableService>();

            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInteraction = new Mock<IInteractionService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockLayout = new Mock<ILayoutService>();
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
                mockSelection.Object,
                mockText.Object,
                mockLayout.Object,
                _mockGridTableService.Object,
                mockAdvancedPattern.Object,
                mockWindow.Object,
                mockTransform.Object,
                mockEventMonitor.Object,
                mockFocus.Object,
                Mock.Of<IItemContainerService>(),
                mockLog.Object);
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task GetGridInfo_GetItem_ShouldCallService()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult { Elements = new System.Collections.Generic.List<ElementInfo> { new ElementInfo { AutomationId = "cell1" } } }
            };
            _mockGridTableService.Setup(s => s.GetGridItemAsync("grid1", null, 0, 0, null, null, 30))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridInfo(action: "getitem", automationId: "grid1", row: 0, column: 0);

            // Assert
            Assert.NotNull(result);
            _mockGridTableService.Verify(s => s.GetGridItemAsync("grid1", null, 0, 0, null, null, 30), Times.Once);
        }
    }
}
