using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;
using System.Threading;

namespace UIAutomationMCP.Tests.Tools
{
    /// <summary>
    /// Tests for UIAutomationTools covering the essential functionality
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class UIAutomationToolsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IApplicationLauncher> _mockApplicationLauncher;
        private readonly Mock<IScreenshotService> _mockScreenshotService;
        private readonly Mock<IElementSearchService> _mockElementSearchService;
        private readonly Mock<ITreeNavigationService> _mockTreeNavigationService;
        private readonly Mock<IInvokeService> _mockInvokeService;
        private readonly Mock<IValueService> _mockValueService;
        private readonly Mock<IRangeService> _mockRangeService;
        private readonly Mock<ISelectionService> _mockSelectionService;
        private readonly Mock<ITextService> _mockTextService;
        private readonly Mock<IToggleService> _mockToggleService;
        private readonly Mock<IWindowService> _mockWindowService;
        private readonly Mock<ILayoutService> _mockLayoutService;
        private readonly Mock<IGridService> _mockGridService;
        private readonly Mock<ITableService> _mockTableService;
        private readonly Mock<IMultipleViewService> _mockMultipleViewService;
        private readonly Mock<IAccessibilityService> _mockAccessibilityService;
        private readonly Mock<ICustomPropertyService> _mockCustomPropertyService;
        private readonly Mock<IControlTypeService> _mockControlTypeService;
        private readonly Mock<ITransformService> _mockTransformService;

        public UIAutomationToolsTests(ITestOutputHelper output)
        {
            _output = output;
            
            // Create all service mocks
            _mockApplicationLauncher = new Mock<IApplicationLauncher>();
            _mockScreenshotService = new Mock<IScreenshotService>();
            _mockElementSearchService = new Mock<IElementSearchService>();
            _mockTreeNavigationService = new Mock<ITreeNavigationService>();
            _mockInvokeService = new Mock<IInvokeService>();
            _mockValueService = new Mock<IValueService>();
            _mockRangeService = new Mock<IRangeService>();
            _mockSelectionService = new Mock<ISelectionService>();
            _mockTextService = new Mock<ITextService>();
            _mockToggleService = new Mock<IToggleService>();
            _mockWindowService = new Mock<IWindowService>();
            _mockLayoutService = new Mock<ILayoutService>();
            _mockGridService = new Mock<IGridService>();
            _mockTableService = new Mock<ITableService>();
            _mockMultipleViewService = new Mock<IMultipleViewService>();
            _mockAccessibilityService = new Mock<IAccessibilityService>();
            _mockCustomPropertyService = new Mock<ICustomPropertyService>();
            _mockControlTypeService = new Mock<IControlTypeService>();
            _mockTransformService = new Mock<ITransformService>();
            
            _tools = new UIAutomationTools(
                _mockApplicationLauncher.Object,
                _mockScreenshotService.Object,
                _mockElementSearchService.Object,
                _mockTreeNavigationService.Object,
                _mockInvokeService.Object,
                _mockValueService.Object,
                _mockRangeService.Object,
                _mockSelectionService.Object,
                _mockTextService.Object,
                _mockToggleService.Object,
                _mockWindowService.Object,
                _mockLayoutService.Object,
                _mockGridService.Object,
                _mockTableService.Object,
                _mockMultipleViewService.Object,
                _mockAccessibilityService.Object,
                _mockCustomPropertyService.Object,
                _mockControlTypeService.Object,
                _mockTransformService.Object
            );
        }

        public void Dispose()
        {
            // No cleanup needed for mocks
        }

        #region Window and Element Discovery Tests

        [Fact]
        public async Task GetWindowInfo_Success_ReturnsWindowList()
        {
            // Arrange
            var expectedWindows = new List<WindowInfo>
            {
                new WindowInfo { Title = "Window1", ProcessName = "App1", ProcessId = 1234, Handle = 1001 },
                new WindowInfo { Title = "Window2", ProcessName = "App2", ProcessId = 5678, Handle = 1002 }
            };
            _mockElementSearchService.Setup(s => s.GetWindowsAsync(60))
                            .Returns(Task.FromResult((object)expectedWindows));

            // Act
            var result = await _tools.GetWindowInfo();

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.GetWindowsAsync(60), Times.Once);
            _output.WriteLine($"GetWindowInfo test passed: Found {expectedWindows.Count} windows");
        }

        [Fact]
        public async Task FindElements_WithSearchText_Success()
        {
            // Arrange
            var expectedElements = new List<ElementInfo>
            {
                new ElementInfo { AutomationId = "btn1", Name = "Button1", ControlType = "Button" },
                new ElementInfo { AutomationId = "btn2", Name = "Button2", ControlType = "Button" }
            };
            _mockElementSearchService.Setup(s => s.FindElementsAsync("TestWindow", "Button", null, null, 60))
                                   .Returns(Task.FromResult((object)expectedElements));

            // Act
            var result = await _tools.FindElements("Button", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.FindElementsAsync("TestWindow", "Button", null, null, 60), Times.Once);
            _output.WriteLine($"FindElements test passed: Found {expectedElements.Count} elements");
        }

        #endregion

        #region Core Pattern Tests

        [Fact]
        public async Task InvokeElement_Success_InvokesElement()
        {
            // Arrange
            var expectedResult = "Element invoked successfully";
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.InvokeElement("testButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInvokeService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("InvokeElement test passed");
        }

        [Fact]
        public async Task InvokeElement_WithProcessId_Success()
        {
            // Arrange
            var expectedResult = "Element invoked successfully";
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", null, 1234, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.InvokeElement("testButton", null, 1234);

            // Assert
            Assert.NotNull(result);
            _mockInvokeService.Verify(s => s.InvokeElementAsync("testButton", null, 1234, 30), Times.Once);
            _output.WriteLine("InvokeElement with ProcessId test passed");
        }

        [Fact]
        public async Task InvokeElement_WithCustomTimeout_Success()
        {
            // Arrange
            var expectedResult = "Element invoked successfully";
            var customTimeout = 60;
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, customTimeout))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.InvokeElement("testButton", "TestWindow", null, customTimeout);

            // Assert
            Assert.NotNull(result);
            _mockInvokeService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, customTimeout), Times.Once);
            _output.WriteLine("InvokeElement with custom timeout test passed");
        }

        [Fact]
        public async Task InvokeElement_ElementNotFound_ReturnsError()
        {
            // Arrange
            var errorResult = "Element not found";
            _mockInvokeService.Setup(s => s.InvokeElementAsync("nonExistentButton", "TestWindow", null, 30))
                                 .ReturnsAsync(errorResult);

            // Act
            var result = await _tools.InvokeElement("nonExistentButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInvokeService.Verify(s => s.InvokeElementAsync("nonExistentButton", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("InvokeElement element not found test passed");
        }

        [Fact]
        public async Task InvokeElement_ServiceException_PropagatesError()
        {
            // Arrange
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, 30))
                                 .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.InvokeElement("testButton", "TestWindow"));
            
            _mockInvokeService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("InvokeElement service exception test passed");
        }

        [Fact]
        public async Task InvokeElement_EmptyElementId_CallsService()
        {
            // Arrange
            var expectedResult = "Invoked with empty ID";
            _mockInvokeService.Setup(s => s.InvokeElementAsync("", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.InvokeElement("", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInvokeService.Verify(s => s.InvokeElementAsync("", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("InvokeElement empty element ID test passed");
        }

        [Fact]
        public async Task SetElementValue_Success_SetsValue()
        {
            // Arrange
            var expectedResult = "Value set successfully";
            _mockValueService.Setup(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetElementValue("textBox", "Test Value", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetElementValue test passed");
        }

        [Fact]
        public async Task GetElementValue_Success_GetsValue()
        {
            // Arrange
            var expectedResult = "Current Value";
            _mockValueService.Setup(s => s.GetValueAsync("textBox", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetElementValue("textBox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.GetValueAsync("textBox", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetElementValue test passed");
        }

        [Fact]
        public async Task ToggleElement_Success_TogglesElement()
        {
            // Arrange
            var expectedResult = "Element toggled successfully";
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkbox", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ToggleElement("checkbox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkbox", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("ToggleElement test passed");
        }

        [Fact]
        public async Task SelectElement_Success_SelectsElement()
        {
            // Arrange
            var expectedResult = "Element selected successfully";
            _mockSelectionService.Setup(s => s.SelectItemAsync("listItem", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectElement("listItem", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.SelectItemAsync("listItem", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SelectElement test passed");
        }

        #endregion

        #region Layout Pattern Tests

        [Fact]
        public async Task ScrollElement_Success_ScrollsElement()
        {
            // Arrange
            var expectedResult = "Element scrolled successfully";
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollableList", "down", 1.0, "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElement("scrollableList", "down", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementAsync("scrollableList", "down", 1.0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("ScrollElement test passed");
        }

        #endregion

        #region Range Value Tests

        [Fact]
        public async Task SetRangeValue_Success_SetsRangeValue()
        {
            // Arrange
            var expectedResult = "Range value set successfully";
            _mockRangeService.Setup(s => s.SetRangeValueAsync("slider", 50.0, "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetRangeValue("slider", 50.0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("slider", 50.0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetRangeValue test passed");
        }

        [Fact]
        public async Task GetRangeValue_Success_GetsRangeValue()
        {
            // Arrange
            var rangeInfo = new Dictionary<string, object>
            {
                ["Value"] = 25.0,
                ["Minimum"] = 0.0,
                ["Maximum"] = 100.0
            };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("slider", "TestWindow", null, 30))
                                 .ReturnsAsync(rangeInfo);

            // Act
            var result = await _tools.GetRangeValue("slider", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.GetRangeValueAsync("slider", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetRangeValue test passed");
        }

        #endregion

        #region Text Pattern Tests

        [Fact]
        public async Task GetText_Success_GetsText()
        {
            // Arrange
            var expectedResult = "Sample text content";
            _mockTextService.Setup(s => s.GetTextAsync("textElement", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetText("textElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTextService.Verify(s => s.GetTextAsync("textElement", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetText test passed");
        }

        [Fact]
        public async Task SelectText_Success_SelectsText()
        {
            // Arrange
            var expectedResult = "Text selected successfully";
            _mockTextService.Setup(s => s.SelectTextAsync("textElement", 5, 10, "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectText("textElement", 5, 10, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTextService.Verify(s => s.SelectTextAsync("textElement", 5, 10, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SelectText test passed");
        }

        #endregion

        #region Screenshot Tests

        [Fact]
        public async Task TakeScreenshot_Success_TakesScreenshot()
        {
            // Arrange
            var screenshotResult = new ScreenshotResult
            {
                Success = true,
                OutputPath = "screenshot.png",
                Base64Image = "base64data",
                Width = 1920,
                Height = 1080
            };
            _mockScreenshotService.Setup(s => s.TakeScreenshotAsync("TestWindow", null, 0, null, 60, It.IsAny<CancellationToken>()))
                                .Returns(Task.FromResult(screenshotResult));

            // Act
            var result = await _tools.TakeScreenshot("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockScreenshotService.Verify(s => s.TakeScreenshotAsync("TestWindow", null, 0, null, 60, It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("TakeScreenshot test passed");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task GetElementTree_Success_ReturnsElementTree()
        {
            // Arrange
            var expectedTree = new Dictionary<string, object>
            {
                ["WindowTitle"] = "TestWindow",
                ["Children"] = new List<object>()
            };
            _mockTreeNavigationService.Setup(s => s.GetElementTreeAsync("TestWindow", null, 3, 60))
                                   .ReturnsAsync(expectedTree);

            // Act
            var result = await _tools.GetElementTree("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTreeNavigationService.Verify(s => s.GetElementTreeAsync("TestWindow", null, 3, 60), Times.Once);
            _output.WriteLine("GetElementTree test passed");
        }

        [Fact]
        public async Task GetElementInfo_WithControlType_CallsCorrectService()
        {
            // Arrange
            var expectedElements = new List<ElementInfo>
            {
                new ElementInfo { AutomationId = "textBox1", Name = "TextBox1", ControlType = "Edit" }
            };
            _mockElementSearchService.Setup(s => s.FindElementsAsync("TestWindow", null, "Edit", null, 60))
                                   .ReturnsAsync(expectedElements);

            // Act
            var result = await _tools.GetElementInfo("TestWindow", "Edit");

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.FindElementsAsync("TestWindow", null, "Edit", null, 60), Times.Once);
            _output.WriteLine("GetElementInfo with control type test passed");
        }

        [Fact]
        public async Task LaunchWin32Application_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = new ProcessResult { ProcessId = 1234, Success = true };
            _mockApplicationLauncher.Setup(s => s.LaunchWin32ApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.LaunchWin32Application("notepad.exe");

            // Assert
            Assert.NotNull(result);
            _mockApplicationLauncher.Verify(s => s.LaunchWin32ApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchWin32Application test passed");
        }

        [Fact]
        public async Task LaunchApplicationByName_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = new ProcessResult { ProcessId = 5678, Success = true };
            _mockApplicationLauncher.Setup(s => s.LaunchApplicationByNameAsync("Calculator", It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.LaunchApplicationByName("Calculator");

            // Assert
            Assert.NotNull(result);
            _mockApplicationLauncher.Verify(s => s.LaunchApplicationByNameAsync("Calculator", It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchApplicationByName test passed");
        }

        [Fact]
        public async Task GetSelection_Success_ReturnsSelection()
        {
            // Arrange
            var expectedSelection = new List<string> { "item1", "item2" };
            _mockSelectionService.Setup(s => s.GetSelectionAsync("listContainer", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedSelection);

            // Act
            var result = await _tools.GetSelection("listContainer", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.GetSelectionAsync("listContainer", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetSelection test passed");
        }

        [Fact]
        public async Task WindowAction_Success_PerformsWindowAction()
        {
            // Arrange
            var expectedResult = "Window minimized successfully";
            _mockWindowService.Setup(s => s.WindowOperationAsync("minimize", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WindowAction("minimize", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.WindowOperationAsync("minimize", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("WindowAction test passed");
        }

        #endregion


        #region Advanced Pattern Tests

        [Fact]
        public async Task GetGridInfo_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new { RowCount = 10, ColumnCount = 5 };
            _mockGridService.Setup(s => s.GetGridInfoAsync("grid1", "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridInfo("grid1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("grid1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetGridInfo test passed");
        }

        [Fact]
        public async Task GetGridItem_WithValidCoordinates_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new ElementInfo { AutomationId = "cell_1_2", Name = "Cell Content" };
            _mockGridService.Setup(s => s.GetGridItemAsync("grid1", 1, 2, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("grid1", 1, 2, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("grid1", 1, 2, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetGridItem test passed");
        }

        [Fact]
        public async Task GetTableInfo_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new { RowCount = 5, ColumnCount = 3, Headers = new[] { "Col1", "Col2", "Col3" } };
            _mockTableService.Setup(s => s.GetTableInfoAsync("table1", "TestWindow", null, 30))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetTableInfo("table1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetTableInfoAsync("table1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetTableInfo test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetAvailableViews_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["ViewId"] = 1, ["ViewName"] = "View1" },
                new Dictionary<string, object> { ["ViewId"] = 2, ["ViewName"] = "View2" },
                new Dictionary<string, object> { ["ViewId"] = 3, ["ViewName"] = "View3" }
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetAvailableViews("viewContainer1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetAvailableViews test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetAvailableViews_WithProcessId_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["ViewId"] = 1, ["ViewName"] = "List View" },
                new Dictionary<string, object> { ["ViewId"] = 2, ["ViewName"] = "Details View" }
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", null, 1234, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetAvailableViews("viewContainer1", null, 1234);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetAvailableViewsAsync("viewContainer1", null, 1234, 30), Times.Once);
            _output.WriteLine("GetAvailableViews with ProcessId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetAvailableViews_WithCustomTimeout_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["ViewId"] = 1, ["ViewName"] = "Thumbnail View" }
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 60))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetAvailableViews("viewContainer1", "TestWindow", null, 60);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 60), Times.Once);
            _output.WriteLine("GetAvailableViews with custom timeout test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetAvailableViews_EmptyElementId_CallsService()
        {
            // Arrange
            var expectedResult = new List<Dictionary<string, object>>();
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("", "TestWindow", null, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetAvailableViews("", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetAvailableViewsAsync("", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetAvailableViews with empty elementId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetAvailableViews_ServiceException_PropagatesError()
        {
            // Arrange
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 30))
                                  .ThrowsAsync(new InvalidOperationException("MultipleViewPattern not supported"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.GetAvailableViews("viewContainer1", "TestWindow"));
            
            _mockMultipleViewService.Verify(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetAvailableViews service exception test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetCurrentView_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new Dictionary<string, object>
            {
                ["ViewId"] = 2,
                ["ViewName"] = "Details View"
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetCurrentView("viewContainer1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetCurrentView test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetCurrentView_WithProcessId_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new Dictionary<string, object>
            {
                ["ViewId"] = 1,
                ["ViewName"] = "List View"
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", null, 1234, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetCurrentView("viewContainer1", null, 1234);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetCurrentViewAsync("viewContainer1", null, 1234, 30), Times.Once);
            _output.WriteLine("GetCurrentView with ProcessId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetCurrentView_WithCustomTimeout_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new Dictionary<string, object>
            {
                ["ViewId"] = 3,
                ["ViewName"] = "Thumbnail View"
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 60))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetCurrentView("viewContainer1", "TestWindow", null, 60);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 60), Times.Once);
            _output.WriteLine("GetCurrentView with custom timeout test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetCurrentView_ServiceException_PropagatesError()
        {
            // Arrange
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 30))
                                  .ThrowsAsync(new ArgumentException("Element not found"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _tools.GetCurrentView("viewContainer1", "TestWindow"));
            
            _mockMultipleViewService.Verify(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetCurrentView service exception test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = "View set successfully";
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 2, "TestWindow", null, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetView("viewContainer1", 2, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", 2, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetView test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithProcessId_CallsCorrectService()
        {
            // Arrange
            var expectedResult = "View set successfully";
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 1, null, 1234, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetView("viewContainer1", 1, null, 1234);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", 1, null, 1234, 30), Times.Once);
            _output.WriteLine("SetView with ProcessId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithCustomTimeout_CallsCorrectService()
        {
            // Arrange
            var expectedResult = "View set successfully";
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 3, "TestWindow", null, 60))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetView("viewContainer1", 3, "TestWindow", null, 60);

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", 3, "TestWindow", null, 60), Times.Once);
            _output.WriteLine("SetView with custom timeout test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithInvalidViewId_ServiceException_PropagatesError()
        {
            // Arrange
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 999, "TestWindow", null, 30))
                                  .ThrowsAsync(new ArgumentException("Unsupported view ID: 999"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _tools.SetView("viewContainer1", 999, "TestWindow"));
            
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", 999, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetView with invalid viewId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithZeroViewId_CallsService()
        {
            // Arrange
            var expectedResult = "View set to default";
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 0, "TestWindow", null, 30))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetView("viewContainer1", 0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetView with zero viewId test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithNegativeViewId_CallsService()
        {
            // Arrange
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", -1, "TestWindow", null, 30))
                                  .ThrowsAsync(new ArgumentException("Invalid view ID: -1"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _tools.SetView("viewContainer1", -1, "TestWindow"));
            
            _mockMultipleViewService.Verify(s => s.SetViewAsync("viewContainer1", -1, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetView with negative viewId test passed");
        }

        [Fact]
        public async Task GetAccessibilityInfo_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new { Role = "Button", Name = "Submit", Description = "Submit the form" };
            _mockAccessibilityService.Setup(s => s.GetAccessibilityInfoAsync("button1", "TestWindow", null, 30))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetAccessibilityInfo("button1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockAccessibilityService.Verify(s => s.GetAccessibilityInfoAsync("button1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetAccessibilityInfo test passed");
        }

        [Fact]
        public async Task GetCustomProperties_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var expectedResult = new Dictionary<string, object> { ["CustomProp1"] = "Value1", ["CustomProp2"] = "Value2" };
            _mockCustomPropertyService.Setup(s => s.GetCustomPropertiesAsync("element1", new[] { "CustomProp1", "CustomProp2" }, "TestWindow", null, 30))
                                     .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetCustomProperties("element1", new[] { "CustomProp1", "CustomProp2" }, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCustomPropertyService.Verify(s => s.GetCustomPropertiesAsync("element1", new[] { "CustomProp1", "CustomProp2" }, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetCustomProperties test passed");
        }

        #endregion

        #region Parameter Validation Tests

        [Fact]
        public async Task SetElementValue_WithEmptyElementId_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Value set successfully";
            _mockValueService.Setup(s => s.SetValueAsync("", "testValue", null, null, 30))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetElementValue("", "testValue");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.SetValueAsync("", "testValue", null, null, 30), Times.Once);
            _output.WriteLine("SetElementValue with empty elementId test passed");
        }

        [Fact]
        public async Task SetRangeValue_WithBoundaryValues_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Range value set successfully";
            _mockRangeService.Setup(s => s.SetRangeValueAsync("slider1", 0.0, null, null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetRangeValue("slider1", 0.0);

            // Assert
            Assert.NotNull(result);
            _mockRangeService.Verify(s => s.SetRangeValueAsync("slider1", 0.0, null, null, 30), Times.Once);
            _output.WriteLine("SetRangeValue with boundary values test passed");
        }

        [Fact]
        public async Task ScrollElement_WithCustomAmount_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Element scrolled successfully";
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollable1", "down", 2.5, null, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElement("scrollable1", "down", 2.5);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementAsync("scrollable1", "down", 2.5, null, null, 30), Times.Once);
            _output.WriteLine("ScrollElement with custom amount test passed");
        }

        #endregion
    }
}
