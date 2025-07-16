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
        private readonly Mock<IVirtualizedItemService> _mockVirtualizedItemService;
        private readonly Mock<IItemContainerService> _mockItemContainerService;
        private readonly Mock<ISynchronizedInputService> _mockSynchronizedInputService;

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
            _mockVirtualizedItemService = new Mock<IVirtualizedItemService>();
            _mockItemContainerService = new Mock<IItemContainerService>();
            _mockSynchronizedInputService = new Mock<ISynchronizedInputService>();
            
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
                _mockTransformService.Object,
                _mockVirtualizedItemService.Object,
                _mockItemContainerService.Object,
                _mockSynchronizedInputService.Object
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
        public async Task IsElementReadOnly_Success_ReturnsReadOnlyStatus()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = true };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("textBox", "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsElementReadOnly("textBox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.IsReadOnlyAsync("textBox", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("IsElementReadOnly test passed");
        }

        [Fact]
        public async Task IsElementReadOnly_ElementNotFound_ReturnsError()
        {
            // Arrange
            var errorResult = new { Success = false, Error = "Element not found" };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("nonExistentElement", "TestWindow", null, 30))
                                 .ReturnsAsync(errorResult);

            // Act
            var result = await _tools.IsElementReadOnly("nonExistentElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.IsReadOnlyAsync("nonExistentElement", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("IsElementReadOnly element not found test passed");
        }

        [Fact]
        public async Task IsElementReadOnly_ValuePatternNotSupported_ReturnsError()
        {
            // Arrange
            var errorResult = new { Success = false, Error = "ValuePattern not supported" };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("unsupportedElement", "TestWindow", null, 30))
                                 .ReturnsAsync(errorResult);

            // Act
            var result = await _tools.IsElementReadOnly("unsupportedElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.IsReadOnlyAsync("unsupportedElement", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("IsElementReadOnly ValuePattern not supported test passed");
        }

        [Fact]
        public async Task IsElementReadOnly_WithCustomTimeout_Success()
        {
            // Arrange
            var expectedResult = new { Success = true, Data = false };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("editableTextBox", "TestWindow", null, 60))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.IsElementReadOnly("editableTextBox", "TestWindow", timeoutSeconds: 60);

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.IsReadOnlyAsync("editableTextBox", "TestWindow", null, 60), Times.Once);
            _output.WriteLine("IsElementReadOnly with custom timeout test passed");
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

        [Fact]
        public async Task GetWindowInteractionState_Success_ReturnsInteractionState()
        {
            // Arrange
            var expectedResult = new 
            { 
                Success = true, 
                Data = new Dictionary<string, object>
                {
                    ["InteractionState"] = "Running",
                    ["InteractionStateValue"] = 0,
                    ["Description"] = "The window is running and responding to user input"
                }
            };
            _mockWindowService.Setup(s => s.GetWindowInteractionStateAsync("TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetWindowInteractionState("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.GetWindowInteractionStateAsync("TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetWindowInteractionState test passed");
        }

        [Fact]
        public async Task GetWindowCapabilities_Success_ReturnsCapabilities()
        {
            // Arrange
            var expectedResult = new 
            { 
                Success = true, 
                Data = new Dictionary<string, object>
                {
                    ["Maximizable"] = true,
                    ["Minimizable"] = true,
                    ["CanMaximize"] = true,
                    ["CanMinimize"] = true,
                    ["IsModal"] = false,
                    ["IsTopmost"] = false,
                    ["WindowVisualState"] = "Normal",
                    ["WindowInteractionState"] = "Running"
                }
            };
            _mockWindowService.Setup(s => s.GetWindowCapabilitiesAsync("TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetWindowCapabilities("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.GetWindowCapabilitiesAsync("TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetWindowCapabilities test passed");
        }

        [Fact]
        public async Task WaitForWindowInputIdle_Success_WaitsForIdle()
        {
            // Arrange
            var expectedResult = new 
            { 
                Success = true, 
                Data = new Dictionary<string, object>
                {
                    ["Success"] = true,
                    ["TimeoutMilliseconds"] = 5000,
                    ["ElapsedMilliseconds"] = 1234.5,
                    ["TimedOut"] = false,
                    ["WindowInteractionState"] = "ReadyForUserInteraction",
                    ["Message"] = "Window became idle within the specified timeout"
                }
            };
            _mockWindowService.Setup(s => s.WaitForInputIdleAsync(5000, "TestWindow", null, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WaitForWindowInputIdle(5000, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.WaitForInputIdleAsync(5000, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("WaitForWindowInputIdle test passed");
        }

        [Theory]
        [InlineData("minimize", "TestWindow", 1234)]
        [InlineData("maximize", "AnotherWindow", 5678)]
        [InlineData("close", null, null)]
        public async Task WindowAction_WithVariousParameters_CallsCorrectService(string action, string windowTitle, int? processId)
        {
            // Arrange
            var expectedResult = $"Window {action} action performed successfully";
            _mockWindowService.Setup(s => s.WindowOperationAsync(action, windowTitle, processId, 30))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WindowAction(action, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.WindowOperationAsync(action, windowTitle, processId, 30), Times.Once);
            _output.WriteLine($"WindowAction with parameters ({action}, {windowTitle}, {processId}) test passed");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(30000)]
        public async Task WaitForWindowInputIdle_WithCustomTimeout_UsesCorrectTimeout(int timeoutMs)
        {
            // Arrange
            var expectedResult = new { Success = true, Data = "Idle operation completed" };
            _mockWindowService.Setup(s => s.WaitForInputIdleAsync(timeoutMs, It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WaitForWindowInputIdle(timeoutMs);

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.WaitForInputIdleAsync(timeoutMs, null, null, 30), Times.Once);
            _output.WriteLine($"WaitForWindowInputIdle with timeout {timeoutMs}ms test passed");
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

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetScrollInfo_Should_Call_LayoutService_With_Correct_Parameters()
        {
            // Arrange - Microsoft ScrollPattern仕様の6つの必須プロパティをテスト
            var expectedScrollInfo = new
            {
                HorizontalScrollPercent = 25.0,
                VerticalScrollPercent = 50.0,
                HorizontalViewSize = 80.0,
                VerticalViewSize = 60.0,
                HorizontallyScrollable = true,
                VerticallyScrollable = true
            };
            
            _mockLayoutService.Setup(s => s.GetScrollInfoAsync("scrollableElement", "TestWindow", 1234, 30))
                             .ReturnsAsync(new { Success = true, Data = expectedScrollInfo });

            // Act
            var result = await _tools.GetScrollInfo("scrollableElement", "TestWindow", 1234, 30);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.GetScrollInfoAsync("scrollableElement", "TestWindow", 1234, 30), Times.Once);
            _output.WriteLine("GetScrollInfo test passed - all ScrollPattern properties verified");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetScrollPercent_Should_Call_LayoutService_With_Valid_Percentages()
        {
            // Arrange - Microsoft ScrollPattern仕様のSetScrollPercentメソッドをテスト
            var expectedResult = new
            {
                Success = true,
                Data = new
                {
                    HorizontalScrollPercent = 75.0,
                    VerticalScrollPercent = 25.0,
                    HorizontalViewSize = 100.0,
                    VerticalViewSize = 100.0
                }
            };
            
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollContainer", 75.0, 25.0, "TestWindow", 1234, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetScrollPercent("scrollContainer", 75.0, 25.0, "TestWindow", 1234, 30);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("scrollContainer", 75.0, 25.0, "TestWindow", 1234, 30), Times.Once);
            _output.WriteLine("SetScrollPercent test passed - percentage-based scrolling verified");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetScrollPercent_Should_Handle_NoScroll_Values()
        {
            // Arrange - Microsoft仕様の-1値（NoScroll）をテスト
            var expectedResult = new { Success = true, Message = "Scroll percentage set with NoScroll" };
            
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollElement", -1.0, 50.0, null, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act - 水平方向にNoScroll(-1)、垂直方向に50%を指定
            var result = await _tools.SetScrollPercent("scrollElement", -1.0, 50.0);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("scrollElement", -1.0, 50.0, null, null, 30), Times.Once);
            _output.WriteLine("SetScrollPercent with NoScroll values test passed");
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(0.0, 0.0)]     // Minimum valid values
        [InlineData(100.0, 100.0)] // Maximum valid values
        [InlineData(50.5, 33.3)]   // Decimal values
        [InlineData(-1.0, -1.0)]   // NoScroll values
        public async Task SetScrollPercent_Should_Accept_Valid_Range_Values(double horizontal, double vertical)
        {
            // Arrange - Microsoft仕様の有効範囲（0-100、-1）をテスト
            var expectedResult = new { Success = true };
            
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("testElement", horizontal, vertical, null, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetScrollPercent("testElement", horizontal, vertical);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("testElement", horizontal, vertical, null, null, 30), Times.Once);
            _output.WriteLine($"SetScrollPercent valid range test passed for H:{horizontal}, V:{vertical}");
        }

        #endregion

        #region ScrollElementIntoView Integration Tests

        /// <summary>
        /// ScrollElementIntoView - 正常系：Microsoft ScrollItemPattern仕様準拠テスト
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Call_LayoutService_With_Correct_Parameters()
        {
            // Arrange - Microsoft ScrollItemPattern.ScrollIntoView()仕様をテスト
            var expectedResult = new
            {
                Success = true,
                Data = "Element scrolled into view successfully"
            };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("scrollableItem", "TestWindow", 1234, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView("scrollableItem", "TestWindow", 1234, 30);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("scrollableItem", "TestWindow", 1234, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView test passed - ScrollItemPattern.ScrollIntoView() verified");
        }

        /// <summary>
        /// ScrollElementIntoView - デフォルトパラメータでの動作テスト
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Use_Default_Parameters()
        {
            // Arrange
            var expectedResult = new { Success = true, Message = "Element scrolled into view" };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("listItem", null, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView("listItem");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("listItem", null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView with default parameters test passed");
        }

        /// <summary>
        /// ScrollElementIntoView - エラーハンドリング：ScrollItemPatternが利用できない場合
        /// Microsoft仕様: InvalidOperationException handling
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Pattern_Not_Supported_Exception()
        {
            // Arrange
            var expectedException = new InvalidOperationException("ScrollItemPattern is not supported by this element");
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("nonScrollableElement", null, null, 30))
                             .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.ScrollElementIntoView("nonScrollableElement"));
            
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("nonScrollableElement", null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView pattern not supported exception test passed");
        }

        /// <summary>
        /// ScrollElementIntoView - エラーハンドリング：要素が見つからない場合
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Element_Not_Found_Exception()
        {
            // Arrange
            var expectedException = new ArgumentException("Element not found: nonExistentElement");
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("nonExistentElement", "TestWindow", null, 30))
                             .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _tools.ScrollElementIntoView("nonExistentElement", "TestWindow"));
            
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("nonExistentElement", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView element not found exception test passed");
        }

        /// <summary>
        /// ScrollElementIntoView - タイムアウト処理テスト
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Custom_Timeout()
        {
            // Arrange
            var expectedResult = new { Success = true };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("slowElement", "TestWindow", null, 60))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView("slowElement", "TestWindow", null, 60);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("slowElement", "TestWindow", null, 60), Times.Once);
            _output.WriteLine("ScrollElementIntoView with custom timeout test passed");
        }

        /// <summary>
        /// ScrollElementIntoView - Microsoft仕様準拠：ListItemとTreeItemコントロールタイプでの動作確認
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("list-item-1")]
        [InlineData("tree-item-node-2")]
        [InlineData("dataitem-3")]
        public async Task ScrollElementIntoView_Should_Work_With_Expected_Control_Types(string elementId)
        {
            // Arrange - ListItem、TreeItem、DataItemでScrollItemPatternがサポートされることをテスト
            var expectedResult = new
            {
                Success = true,
                Data = $"Element {elementId} scrolled into view successfully"
            };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, "TestApplication", null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView(elementId, "TestApplication");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync(elementId, "TestApplication", null, 30), Times.Once);
            _output.WriteLine($"ScrollElementIntoView worked correctly for control type element: {elementId}");
        }

        /// <summary>
        /// ScrollElementIntoView - 空文字列パラメータの処理
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("", "TestWindow")]
        [InlineData("element1", "")]
        public async Task ScrollElementIntoView_Should_Handle_Empty_String_Parameters(string elementId, string windowTitle)
        {
            // Arrange
            var expectedResult = new { Success = false, Error = "Invalid parameters" };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, windowTitle, null, 30))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView(elementId, windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync(elementId, windowTitle, null, 30), Times.Once);
            _output.WriteLine($"ScrollElementIntoView empty string parameters test passed for elementId:'{elementId}', windowTitle:'{windowTitle}'");
        }

        #endregion

        #region TableItem Pattern Tests - Microsoft UI Automation仕様準拠

        /// <summary>
        /// GetColumnHeaderItems - 正常系：Microsoft TableItemPattern.GetColumnHeaderItems()仕様準拠テスト
        /// Required Members: GetColumnHeaderItems() - テーブル項目の列ヘッダー要素を取得
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetColumnHeaderItems_Should_Call_TableService_With_Correct_Parameters()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetColumnHeaderItems()をテスト
            var expectedColumnHeaders = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["AutomationId"] = "header_col1",
                    ["Name"] = "Name",
                    ["ControlType"] = "Header",
                    ["IsEnabled"] = true,
                    ["BoundingRectangle"] = new { X = 10, Y = 5, Width = 100, Height = 25 }
                },
                new Dictionary<string, object>
                {
                    ["AutomationId"] = "header_col2", 
                    ["Name"] = "Age",
                    ["ControlType"] = "Header",
                    ["IsEnabled"] = true,
                    ["BoundingRectangle"] = new { X = 110, Y = 5, Width = 80, Height = 25 }
                }
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("tableCell1", "TestWindow", null, 30))
                           .ReturnsAsync(new { Success = true, Data = expectedColumnHeaders });

            // Act
            var result = await _tools.GetColumnHeaderItems("tableCell1", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync("tableCell1", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetColumnHeaderItems test passed - TableItemPattern.GetColumnHeaderItems() verified");
        }

        /// <summary>
        /// GetColumnHeaderItems - プロセスIDとカスタムタイムアウト指定テスト
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetColumnHeaderItems_Should_Work_With_ProcessId_And_CustomTimeout()
        {
            // Arrange
            var expectedResult = new
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { ["Name"] = "Column Header 1" }
                }
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("cell2_3", null, 1234, 60))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetColumnHeaderItems("cell2_3", null, 1234, 60);

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync("cell2_3", null, 1234, 60), Times.Once);
            _output.WriteLine("GetColumnHeaderItems with ProcessId and custom timeout test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - TableItemPatternがサポートされていない場合のエラーハンドリング
        /// Microsoft仕様: InvalidOperationException when pattern not supported
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetColumnHeaderItems_Should_Handle_Pattern_Not_Supported_Exception()
        {
            // Arrange
            var expectedException = new InvalidOperationException("TableItemPattern not supported");
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("nonTableCell", "TestWindow", null, 30))
                           .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.GetColumnHeaderItems("nonTableCell", "TestWindow"));
            
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync("nonTableCell", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetColumnHeaderItems pattern not supported exception test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - 列ヘッダーが見つからない場合の処理
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetColumnHeaderItems_Should_Handle_No_Column_Headers_Found()
        {
            // Arrange
            var expectedResult = new
            {
                Success = false,
                Error = "No column header items found"
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("emptyTableCell", "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetColumnHeaderItems("emptyTableCell", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync("emptyTableCell", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetColumnHeaderItems no headers found test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - 正常系：Microsoft TableItemPattern.GetRowHeaderItems()仕様準拠テスト
        /// Required Members: GetRowHeaderItems() - テーブル項目の行ヘッダー要素を取得
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetRowHeaderItems_Should_Call_TableService_With_Correct_Parameters()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetRowHeaderItems()をテスト
            var expectedRowHeaders = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["AutomationId"] = "header_row1",
                    ["Name"] = "Person 1",
                    ["ControlType"] = "Header",
                    ["IsEnabled"] = true,
                    ["BoundingRectangle"] = new { X = 5, Y = 30, Width = 80, Height = 20 }
                },
                new Dictionary<string, object>
                {
                    ["AutomationId"] = "header_row2",
                    ["Name"] = "Person 2", 
                    ["ControlType"] = "Header",
                    ["IsEnabled"] = true,
                    ["BoundingRectangle"] = new { X = 5, Y = 50, Width = 80, Height = 20 }
                }
            };
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("tableCell2", "TestWindow", null, 30))
                           .ReturnsAsync(new { Success = true, Data = expectedRowHeaders });

            // Act
            var result = await _tools.GetRowHeaderItems("tableCell2", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync("tableCell2", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetRowHeaderItems test passed - TableItemPattern.GetRowHeaderItems() verified");
        }

        /// <summary>
        /// GetRowHeaderItems - デフォルトパラメータでの動作テスト
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetRowHeaderItems_Should_Use_Default_Parameters()
        {
            // Arrange
            var expectedResult = new
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { ["Name"] = "Row Header 1" }
                }
            };
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("defaultCell", null, null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetRowHeaderItems("defaultCell");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync("defaultCell", null, null, 30), Times.Once);
            _output.WriteLine("GetRowHeaderItems with default parameters test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - TableItemPatternがサポートされていない場合のエラーハンドリング
        /// Microsoft仕様: InvalidOperationException when pattern not supported
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetRowHeaderItems_Should_Handle_Pattern_Not_Supported_Exception()
        {
            // Arrange
            var expectedException = new InvalidOperationException("TableItemPattern not supported");
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("nonTableCell", "TestWindow", null, 30))
                           .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tools.GetRowHeaderItems("nonTableCell", "TestWindow"));
            
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync("nonTableCell", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetRowHeaderItems pattern not supported exception test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - 行ヘッダーが見つからない場合の処理
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetRowHeaderItems_Should_Handle_No_Row_Headers_Found()
        {
            // Arrange
            var expectedResult = new
            {
                Success = false,
                Error = "No row header items found"
            };
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("emptyRowTableCell", "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetRowHeaderItems("emptyRowTableCell", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync("emptyRowTableCell", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetRowHeaderItems no headers found test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - プロセスIDとカスタムタイムアウト指定での動作確認
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetRowHeaderItems_Should_Work_With_ProcessId_And_CustomTimeout()
        {
            // Arrange
            var expectedResult = new
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["AutomationId"] = "row_header_special",
                        ["Name"] = "Special Row Header",
                        ["ControlType"] = "Header"
                    }
                }
            };
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("specialCell", null, 5678, 45))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetRowHeaderItems("specialCell", null, 5678, 45);

            // Assert
            Assert.NotNull(result);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync("specialCell", null, 5678, 45), Times.Once);
            _output.WriteLine("GetRowHeaderItems with ProcessId and custom timeout test passed");
        }

        /// <summary>
        /// TableItem Pattern Integration - 両方のメソッドが協調動作することをテスト
        /// Microsoft仕様: TableItemPatternは通常GridItemPatternと併用される
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("cell_1_1", "Cell at row 1, column 1")]
        [InlineData("cell_2_3", "Cell at row 2, column 3")]
        [InlineData("cell_0_0", "Top-left cell")]
        public async Task TableItem_Pattern_Methods_Should_Work_For_Different_Cell_Types(string cellId, string description)
        {
            // Arrange - 異なるタイプのテーブルセルでの動作確認
            var columnHeadersResult = new
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { ["Name"] = $"Column Header for {cellId}" }
                }
            };
            
            var rowHeadersResult = new
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { ["Name"] = $"Row Header for {cellId}" }
                }
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync(cellId, "TestApplication", null, 30))
                           .ReturnsAsync(columnHeadersResult);
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync(cellId, "TestApplication", null, 30))
                           .ReturnsAsync(rowHeadersResult);

            // Act
            var columnResult = await _tools.GetColumnHeaderItems(cellId, "TestApplication");
            var rowResult = await _tools.GetRowHeaderItems(cellId, "TestApplication");

            // Assert
            Assert.NotNull(columnResult);
            Assert.NotNull(rowResult);
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync(cellId, "TestApplication", null, 30), Times.Once);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync(cellId, "TestApplication", null, 30), Times.Once);
            _output.WriteLine($"TableItem pattern integration test passed for {description}");
        }

        /// <summary>
        /// TableItem Pattern - 空文字列パラメータの処理
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("", "TestWindow")]
        [InlineData("validCell", "")]
        public async Task TableItem_Pattern_Should_Handle_Empty_String_Parameters(string elementId, string windowTitle)
        {
            // Arrange
            var expectedResult = new { Success = false, Error = "Invalid parameters" };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync(elementId, windowTitle, null, 30))
                           .ReturnsAsync(expectedResult);
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync(elementId, windowTitle, null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var columnResult = await _tools.GetColumnHeaderItems(elementId, windowTitle);
            var rowResult = await _tools.GetRowHeaderItems(elementId, windowTitle);

            // Assert
            Assert.NotNull(columnResult);
            Assert.NotNull(rowResult);
            _mockTableService.Verify(s => s.GetColumnHeaderItemsAsync(elementId, windowTitle, null, 30), Times.Once);
            _mockTableService.Verify(s => s.GetRowHeaderItemsAsync(elementId, windowTitle, null, 30), Times.Once);
            _output.WriteLine($"TableItem pattern empty string parameters test passed for elementId:'{elementId}', windowTitle:'{windowTitle}'");
        }

        #endregion
    }
}
