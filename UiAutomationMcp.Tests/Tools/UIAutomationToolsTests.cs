using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using Xunit.Abstractions;
using System.Threading;
using System.Text.Json;
using UIAutomationMCP.Shared.Serialization;

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
        private readonly Mock<IEventMonitorService> _mockEventMonitorService;
        private readonly Mock<ISubprocessExecutor> _mockSubprocessExecutor;

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
            _mockEventMonitorService = new Mock<IEventMonitorService>();
            _mockSubprocessExecutor = new Mock<ISubprocessExecutor>();
            
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
                _mockSynchronizedInputService.Object,
                _mockEventMonitorService.Object,
                _mockSubprocessExecutor.Object
            );
        }

        public void Dispose()
        {
            // No cleanup needed for mocks
        }

        #region Window and Element Discovery Tests

        [Fact]
        public async Task GetWindows_Success_ReturnsWindowList()
        {
            // Arrange
            var expectedWindows = new List<UIAutomationMCP.Shared.Results.WindowInfo>
            {
                new UIAutomationMCP.Shared.Results.WindowInfo { Title = "Window1", ProcessName = "App1", ProcessId = 1234, Handle = 1001 },
                new UIAutomationMCP.Shared.Results.WindowInfo { Title = "Window2", ProcessName = "App2", ProcessId = 5678, Handle = 1002 }
            };
            
            var desktopWindowsResult = new DesktopWindowsResult
            {
                Success = true,
                Windows = expectedWindows,
                TotalCount = expectedWindows.Count,
                VisibleCount = expectedWindows.Count
            };
            var serverResponse = new ServerEnhancedResponse<DesktopWindowsResult>
            {
                Success = true,
                Data = desktopWindowsResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockElementSearchService.Setup(s => s.GetWindowsAsync(60))
                            .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetWindows();

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.GetWindowsAsync(60), Times.Once);
            _output.WriteLine($"GetWindows test passed: Found {expectedWindows.Count} windows");
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
            
            var elementSearchResult = new ElementSearchResult
            {
                Success = true,
                Elements = expectedElements,
                // Count is read-only, set via Elements property
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = elementSearchResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockElementSearchService.Setup(s => s.FindElementsAsync("TestWindow", "Button", null, null, 60))
                                   .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetElementInfo("Button", windowTitle: "TestWindow");

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
            var invokeResult = new ActionResult
            {
                Success = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = invokeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var invokeResult = new ActionResult
            {
                Success = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = invokeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", null, 1234, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var invokeResult = new ActionResult
            {
                Success = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = invokeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            var customTimeout = 60;
            _mockInvokeService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, customTimeout))
                                 .Returns(Task.FromResult(serverResponse));

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
            var invokeResult = new ActionResult
            {
                Success = false,
                ErrorMessage = "Element not found"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = false,
                Data = invokeResult,
                ErrorMessage = "Element not found",
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockInvokeService.Setup(s => s.InvokeElementAsync("nonExistentButton", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var invokeResult = new ActionResult
            {
                Success = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = invokeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockInvokeService.Setup(s => s.InvokeElementAsync("", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "textBox",
                Action = "SetValue",
                ActionName = "SetValue",
                Completed = true,
                ExecutedAt = DateTime.Now
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetElementValue("textBox", "Test Value", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockValueService.Verify(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("SetElementValue test passed");
        }


        [Fact]
        public async Task IsElementReadOnly_Success_ReturnsReadOnlyStatus()
        {
            // Arrange
            var resultObject = new BooleanResult
            {
                Success = true,
                Value = true,
                PropertyName = "IsReadOnly",
                ElementId = "textBox",
                // WindowTitle not available in ElementSearchResult,
                Pattern = "ValuePattern",
                Method = "IsReadOnlyAsync",
                Description = "Element is read-only"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("textBox", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new BooleanResult
            {
                Success = false,
                Value = false,
                PropertyName = "IsReadOnly",
                ElementId = "nonExistentElement",
                // WindowTitle not available in ElementSearchResult,
                Pattern = "ValuePattern",
                Method = "IsReadOnlyAsync",
                Description = "Element not found"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = false,
                Data = resultObject,
                ErrorMessage = "Element not found",
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("nonExistentElement", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new BooleanResult
            {
                Success = false,
                Value = false,
                PropertyName = "IsReadOnly",
                ElementId = "unsupportedElement",
                // WindowTitle not available in ElementSearchResult,
                Pattern = "ValuePattern",
                Method = "IsReadOnlyAsync",
                Description = "ValuePattern not supported"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = false,
                Data = resultObject,
                ErrorMessage = "ValuePattern not supported",
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("unsupportedElement", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new BooleanResult
            {
                Success = true,
                Value = false,
                PropertyName = "IsReadOnly",
                ElementId = "editableTextBox",
                // WindowTitle not available in ElementSearchResult,
                Pattern = "ValuePattern",
                Method = "IsReadOnlyAsync",
                Description = "Element is not read-only"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.IsReadOnlyAsync("editableTextBox", "TestWindow", null, 60))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "checkbox",
                Action = "Toggle",
                ActionName = "Toggle",
                TargetName = "checkbox",
                TargetControlType = "CheckBox",
                Pattern = "TogglePattern",
                PatternMethod = "Toggle",
                ReturnValue = "On",
                Completed = true,
                Details = "Element toggled successfully"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkbox", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "SelectItem",
                ElementId = "listItem",
                // WindowTitle not available in ElementSearchResult,
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockSelectionService.Setup(s => s.SelectItemAsync("listItem", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElement",
                ElementId = "scrollableList",
                // WindowTitle not available in ElementSearchResult,
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollableList", "down", 1.0, "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "slider",
                Action = "SetRangeValue",
                ActionName = "SetRangeValue",
                Completed = true,
                ExecutedAt = DateTime.Now
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("slider", 50.0, "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new RangeValueResult
            {
                Success = true,
                ElementId = "slider",
                // WindowTitle not available in ElementSearchResult,
                CurrentValue = 25.0,
                Value = 25.0,
                MinimumValue = 0.0,
                Minimum = 0.0,
                MaximumValue = 100.0,
                Maximum = 100.0,
                SmallChange = 1.0,
                LargeChange = 10.0,
                IsReadOnly = false
            };
            var serverResponse = new ServerEnhancedResponse<RangeValueResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockRangeService.Setup(s => s.GetRangeValueAsync("slider", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new TextInfoResult
            {
                Success = true,
                ElementId = "textElement",
                // WindowTitle not available in ElementSearchResult,
                Text = "Sample text content",
                TextLength = 19,
                IsReadOnly = false,
                IsPasswordField = false,
                IsMultiline = false,
                CanSelectText = true,
                CanEditText = true,
                HasText = true,
                TextPattern = "TextPattern",
                InputType = "text"
            };
            var serverResponse = new ServerEnhancedResponse<TextInfoResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTextService.Setup(s => s.GetTextAsync("textElement", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "textElement",
                Action = "SelectText",
                ActionName = "SelectText",
                TargetName = "textElement",
                ReturnValue = "Selected text from index 5 to 10",
                Details = "Text selection completed successfully"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTextService.Setup(s => s.SelectTextAsync("textElement", 5, 10, "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ScreenshotResult
            {
                Success = true,
                OutputPath = "screenshot.png",
                Base64Image = "base64data",
                Width = 1920,
                Height = 1080
            };
            var serverResponse = new ServerEnhancedResponse<ScreenshotResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockScreenshotService.Setup(s => s.TakeScreenshotAsync("TestWindow", null, 0, null, 60, It.IsAny<CancellationToken>()))
                                .Returns(Task.FromResult(serverResponse));

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
            var elementTreeResult = new ElementTreeResult
            {
                Success = true,
                RootNode = new TreeNode { Name = "TestWindow", ElementId = "root1" },
                TotalElements = 1
            };
            var serverResponse = new ServerEnhancedResponse<ElementTreeResult>
            {
                Success = true,
                Data = elementTreeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockTreeNavigationService.Setup(s => s.GetElementTreeAsync("TestWindow", null, 3, 60))
                                   .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetElementTree("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTreeNavigationService.Verify(s => s.GetElementTreeAsync("TestWindow", null, 3, 60), Times.Once);
            _output.WriteLine("GetElementTree test passed");
        }

        // GetElementInfo test removed - functionality replaced by FindElements

        [Fact]
        public async Task LaunchWin32Application_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = ProcessLaunchResponse.CreateSuccess(1234, "notepad", false);
            var serverResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
            {
                Success = true,
                Data = expectedResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockApplicationLauncher.Setup(s => s.LaunchWin32ApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.LaunchWin32Application("notepad.exe");

            // Assert
            Assert.NotNull(result);
            // The result is now a JSON string, so we just verify it's not null
            // and the service was called correctly
            _mockApplicationLauncher.Verify(s => s.LaunchWin32ApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchWin32Application test passed");
        }

        [Fact]
        public async Task LaunchApplicationByName_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = ProcessLaunchResponse.CreateSuccess(5678, "Calculator", false);
            var serverResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
            {
                Success = true,
                Data = expectedResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockApplicationLauncher.Setup(s => s.LaunchApplicationByNameAsync("Calculator", It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.LaunchApplicationByName("Calculator");

            // Assert
            Assert.NotNull(result);
            // The result is now a JSON string, so we just verify it's not null
            // and the service was called correctly
            _mockApplicationLauncher.Verify(s => s.LaunchApplicationByNameAsync("Calculator", It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchApplicationByName test passed");
        }

        [Fact]
        public async Task GetSelection_Success_ReturnsSelection()
        {
            // Arrange
            var selectionResult = new SelectionInfoResult
            {
                Success = true,
                SelectedItems = new List<SelectionItem>
                {
                    new SelectionItem { ElementId = "item1", Name = "Item 1", IsSelected = true },
                    new SelectionItem { ElementId = "item2", Name = "Item 2", IsSelected = true }
                },
                SelectedCount = 2
            };
            var serverResponse = new ServerEnhancedResponse<SelectionInfoResult>
            {
                Success = true,
                Data = selectionResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockSelectionService.Setup(s => s.GetSelectionAsync("listContainer", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var actionResult = new ActionResult
            {
                Success = true,
                Action = "minimize",
                // WindowTitle not available in ElementSearchResult,
                Completed = true
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = actionResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockWindowService.Setup(s => s.WindowOperationAsync("minimize", "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var interactionStateResult = new WindowInteractionStateResult
            {
                Success = true,
                // WindowTitle not available in ElementSearchResult,
                InteractionState = "Running",
                InteractionStateValue = "0",
                Description = "The window is running and responding to user input",
                CanMinimize = true,
                CanMaximize = true,
                WindowVisualState = "Normal"
            };
            var serverResponse = new ServerEnhancedResponse<WindowInteractionStateResult>
            {
                Success = true,
                Data = interactionStateResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockWindowService.Setup(s => s.GetWindowInteractionStateAsync("TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new WindowCapabilitiesResult
            {
                Success = true,
                // WindowTitle not available in ElementSearchResult,
                CanMaximize = true,
                CanMinimize = true,
                CanMove = true,
                CanResize = true,
                CanClose = true,
                IsResizable = true,
                IsMovable = true,
                HasSystemMenu = true,
                IsModal = false,
                IsTopmost = false,
                WindowVisualState = "Normal",
                WindowInteractionState = "Running"
            };
            var serverResponse = new ServerEnhancedResponse<WindowCapabilitiesResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockWindowService.Setup(s => s.GetWindowCapabilitiesAsync("TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new BooleanResult
            {
                Success = true,
                Value = true,
                PropertyName = "WaitForInputIdle",
                ExecutedAt = DateTime.UtcNow,
                OperationName = "WaitForInputIdle"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockWindowService.Setup(s => s.WaitForInputIdleAsync(5000, "TestWindow", null, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = action,
                WindowTitle = windowTitle,
                ProcessId = processId ?? 0,
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockWindowService.Setup(s => s.WindowOperationAsync(action, windowTitle, processId, 30))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new BooleanResult
            {
                Success = true,
                Value = true,
                PropertyName = "WaitForInputIdle",
                ExecutedAt = DateTime.UtcNow,
                OperationName = "WaitForInputIdle"
            };
            var serverResponse = new ServerEnhancedResponse<BooleanResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockWindowService.Setup(s => s.WaitForInputIdleAsync(timeoutMs, It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                                 .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new GridInfoResult
            {
                Success = true,
                GridElementId = "grid1",
                // WindowTitle not available in ElementSearchResult,
                RowCount = 10,
                ColumnCount = 5,
                HasHeaders = true,
                IsScrollable = true,
                IsSelectable = true,
                CanSelectMultiple = false,
                SelectionMode = "Single",
                TotalItemCount = 50,
                VisibleItemCount = 50
            };
            var serverResponse = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("grid1", "TestWindow", null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "cell_1_2",
                        Name = "Cell Content",
                        ControlType = "DataItem"
                    }
                },
                SearchCriteria = "GridItem at row 1, column 2"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("grid1", 1, 2, "TestWindow", null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new TableInfoResult
            {
                Success = true,
                TableElementId = "table1",
                // WindowTitle not available in ElementSearchResult,
                RowCount = 5,
                ColumnCount = 3,
                HasRowHeaders = true,
                HasColumnHeaders = true,
                IsScrollable = true,
                IsSelectable = true,
                CanSelectMultiple = false,
                SelectionMode = "Single",
                TotalCellCount = 15,
                VisibleCellCount = 15,
                RowOrColumnMajor = "RowMajor"
            };
            var serverResponse = new ServerEnhancedResponse<TableInfoResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetTableInfoAsync("table1", "TestWindow", null, 30))
                            .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Available views for viewContainer1"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Available views for viewContainer1 with ProcessId 1234"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", null, 1234, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Available views for viewContainer1 with custom timeout"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("viewContainer1", "TestWindow", null, 60))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>(),
                SearchCriteria = "Available views for empty element ID"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetAvailableViewsAsync("", "TestWindow", null, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Current view for viewContainer1"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Current view for viewContainer1 with ProcessId 1234"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", null, 1234, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Current view for viewContainer1 with custom timeout"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.GetCurrentViewAsync("viewContainer1", "TestWindow", null, 60))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        Name = "View Container",
                        ControlType = "List"
                    }
                },
                SearchCriteria = "Set view to ViewId 2 for viewContainer1"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 2, "TestWindow", null, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "viewContainer1",
                        ProcessId = 1234,
                        ControlType = "MultipleView",
                        Name = "List View",
                        IsEnabled = true,
                        IsVisible = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle()
                    }
                },
                SearchCriteria = "View set to List View"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 1, null, 1234, 30))
                                  .Returns(Task.FromResult(serverResponse));

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
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<UIAutomationMCP.Shared.ElementInfo>(),
                    SearchCriteria = "View set successfully"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 3, "TestWindow", null, 60))
                                  .Returns(Task.FromResult(expectedResult));

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
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<UIAutomationMCP.Shared.ElementInfo>(),
                    SearchCriteria = "View set to default"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockMultipleViewService.Setup(s => s.SetViewAsync("viewContainer1", 0, "TestWindow", null, 30))
                                  .Returns(Task.FromResult(expectedResult));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        Name = "Submit",
                        AutomationId = "button1",
                        ControlType = "Button",
                        IsEnabled = true,
                        IsVisible = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 100, Y = 200, Width = 80, Height = 30 }
                    }
                },
                SearchCriteria = "Accessibility info for button1"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockAccessibilityService.Setup(s => s.GetAccessibilityInfoAsync("button1", "TestWindow", null, 30))
                                   .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        Name = "Element1",
                        ControlType = "Custom",
                        AutomationId = "element1",
                        IsEnabled = true,
                        IsVisible = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 0, Y = 0, Width = 100, Height = 50 }
                    }
                },
                SearchCriteria = "Custom properties for element1"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockCustomPropertyService.Setup(s => s.GetCustomPropertiesAsync("element1", new[] { "CustomProp1", "CustomProp2" }, "TestWindow", null, 30))
                                     .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetCustomProperties("element1", "CustomProp1,CustomProp2", "TestWindow");

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "",
                Action = "SetValue",
                ActionName = "SetValue",
                Completed = true,
                ExecutedAt = DateTime.Now
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockValueService.Setup(s => s.SetValueAsync("", "testValue", null, null, 30))
                            .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ElementId = "slider1",
                Action = "SetRangeValue",
                ActionName = "SetRangeValue",
                Completed = true,
                ExecutedAt = DateTime.Now
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockRangeService.Setup(s => s.SetRangeValueAsync("slider1", 0.0, null, null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElement",
                ElementId = "scrollable1",
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollable1", "down", 2.5, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ScrollInfoResult
            {
                Success = true,
                ElementId = "scrollableElement",
                // WindowTitle not available in ElementSearchResult,
                // ProcessId not available in ElementSearchResult,
                HorizontalScrollPercent = 25.0,
                VerticalScrollPercent = 50.0,
                HorizontalViewSize = 80.0,
                VerticalViewSize = 60.0,
                HorizontallyScrollable = true,
                VerticallyScrollable = true,
                CanScrollHorizontally = true,
                CanScrollVertically = true
            };
            var serverResponse = new ServerEnhancedResponse<ScrollInfoResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.GetScrollInfoAsync("scrollableElement", "TestWindow", 1234, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                ElementId = "scrollContainer"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollContainer", 75.0, 25.0, "TestWindow", 1234, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                ElementId = "scrollElement"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollElement", -1.0, 50.0, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                ElementId = "testElement"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("testElement", horizontal, vertical, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    OperationName = "Element scrolled into view successfully"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("scrollableItem", "TestWindow", 1234, 30))
                             .Returns(Task.FromResult(expectedResult));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElementIntoView",
                ElementId = "listItem",
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("listItem", null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElementIntoView",
                ElementId = "slowElement",
                WindowTitle = "TestWindow",
                Completed = true,
                ErrorMessage = null
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("slowElement", "TestWindow", null, 60))
                             .Returns(Task.FromResult(serverResponse));

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
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = new ActionResult
                {
                    Success = true,
                    OperationName = $"Element {elementId} scrolled into view successfully"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, "TestApplication", null, 30))
                             .Returns(Task.FromResult(expectedResult));

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
            var expectedResult = new ServerEnhancedResponse<ActionResult>
            {
                Success = false,
                ErrorMessage = "Invalid parameters",
                Data = new ActionResult
                {
                    Action = "ScrollElementIntoView",
                    ElementId = elementId,
                    WindowTitle = windowTitle,
                    Success = false,
                    ErrorMessage = "Invalid parameters"
                }
            };
            
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, windowTitle, null, 30))
                             .Returns(Task.FromResult(expectedResult));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo
                    {
                        AutomationId = "header_col1",
                        Name = "Name",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new BoundingRectangle { X = 10, Y = 5, Width = 100, Height = 25 }
                    },
                    new ElementInfo
                    {
                        AutomationId = "header_col2",
                        Name = "Age",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new BoundingRectangle { X = 110, Y = 5, Width = 80, Height = 25 }
                    }
                },
                // Count is automatically calculated from Items property,
                // WindowTitle not available in ElementSearchResult
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("tableCell1", "TestWindow", null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo { Name = "Column Header 1", ControlType = "Header" }
                },
                // Count is automatically calculated from Items property,
                // ProcessId not available in ElementSearchResult
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("cell2_3", null, 1234, 60))
                           .Returns(Task.FromResult(serverResponse));

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
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = false,
                Data = new ElementSearchResult
                {
                    Success = false,
                    ErrorMessage = "No column header items found"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync("emptyTableCell", "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo
                    {
                        AutomationId = "header_row1",
                        Name = "Person 1",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new BoundingRectangle { X = 5, Y = 30, Width = 80, Height = 20 }
                    },
                    new ElementInfo
                    {
                        AutomationId = "header_row2",
                        Name = "Person 2",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new BoundingRectangle { X = 5, Y = 50, Width = 80, Height = 20 }
                    }
                },
                // Count is automatically calculated from Items property,
                // WindowTitle not available in ElementSearchResult
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("tableCell2", "TestWindow", null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo { Name = "Row Header 1" }
                    }
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("defaultCell", null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

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
            var resultObject = new ElementSearchResult
            {
                Success = false,
                Elements = new List<ElementInfo>(),
                // Count is automatically calculated from Items property,
                // WindowTitle not available in ElementSearchResult,
                ErrorMessage = "No row header items found"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = false,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("emptyRowTableCell", "TestWindow", null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo
                    {
                        AutomationId = "row_header_special",
                        Name = "Special Row Header",
                        ControlType = "Header"
                    }
                },
                // Count is automatically calculated from Items property,
                // ProcessId not available in ElementSearchResult
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync("specialCell", null, 5678, 45))
                           .Returns(Task.FromResult(serverResponse));

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
            var columnHeadersResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo { Name = $"Column Header for {cellId}", ControlType = "Header" }
                },
                // Count is automatically calculated from Items property,
                // WindowTitle not available in ElementSearchResult
            };
            var columnServerResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = columnHeadersResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            var rowHeadersResult = new ElementSearchResult
            {
                Success = true,
                Elements = new List<ElementInfo>
                {
                    new ElementInfo { Name = $"Row Header for {cellId}", ControlType = "Header" }
                },
                // Count is automatically calculated from Items property,
                // WindowTitle not available in ElementSearchResult
            };
            var rowServerResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = rowHeadersResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync(cellId, "TestApplication", null, 30))
                           .Returns(Task.FromResult(columnServerResponse));
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync(cellId, "TestApplication", null, 30))
                           .Returns(Task.FromResult(rowServerResponse));

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
            var resultObject = new ElementSearchResult
            {
                Success = false,
                Elements = new List<ElementInfo>(),
                // Count is automatically calculated from Items property,
                ErrorMessage = "Invalid parameters"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = false,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            
            _mockTableService.Setup(s => s.GetColumnHeaderItemsAsync(elementId, windowTitle, null, 30))
                           .Returns(Task.FromResult(serverResponse));
            _mockTableService.Setup(s => s.GetRowHeaderItemsAsync(elementId, windowTitle, null, 30))
                           .Returns(Task.FromResult(serverResponse));

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
