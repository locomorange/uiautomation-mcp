using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Server.Helpers;
using Xunit.Abstractions;
using System.Threading;
using System.Text.Json;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;

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
        private readonly Mock<IInteractionService> _mockInteractionService;
        private readonly Mock<ISelectionService> _mockSelectionService;
        private readonly Mock<ITextService> _mockTextService;
        private readonly Mock<ILayoutService> _mockLayoutService;
        private readonly Mock<IGridTableService> _mockGridTableService;
        private readonly Mock<IAdvancedPatternService> _mockAdvancedPatternService;
        private readonly Mock<IWindowService> _mockWindowService;
        private readonly Mock<ITransformService> _mockTransformService;
        private readonly Mock<IEventMonitorService> _mockEventMonitorService;
        private readonly Mock<IFocusService> _mockFocusService;
        private readonly Mock<IMcpLogService> _mockMcpLogService;

        public UIAutomationToolsTests(ITestOutputHelper output)
        {
            _output = output;

            // Create all service mocks
            _mockApplicationLauncher = new Mock<IApplicationLauncher>();
            _mockScreenshotService = new Mock<IScreenshotService>();
            _mockElementSearchService = new Mock<IElementSearchService>();
            _mockTreeNavigationService = new Mock<ITreeNavigationService>();
            _mockInteractionService = new Mock<IInteractionService>();
            _mockSelectionService = new Mock<ISelectionService>();
            _mockTextService = new Mock<ITextService>();
            _mockLayoutService = new Mock<ILayoutService>();
            _mockGridTableService = new Mock<IGridTableService>();
            _mockAdvancedPatternService = new Mock<IAdvancedPatternService>();
            _mockWindowService = new Mock<IWindowService>();
            _mockTransformService = new Mock<ITransformService>();
            _mockEventMonitorService = new Mock<IEventMonitorService>();
            _mockFocusService = new Mock<IFocusService>();
            _mockMcpLogService = new Mock<IMcpLogService>();

            _tools = new UIAutomationTools(
                _mockApplicationLauncher.Object,
                _mockScreenshotService.Object,
                _mockElementSearchService.Object,
                _mockTreeNavigationService.Object,
                _mockInteractionService.Object,
                _mockSelectionService.Object,
                _mockTextService.Object,
                _mockLayoutService.Object,
                _mockGridTableService.Object,
                _mockAdvancedPatternService.Object,
                _mockWindowService.Object,
                _mockTransformService.Object,
                _mockEventMonitorService.Object,
                _mockFocusService.Object,
                Mock.Of<IItemContainerService>(),
                _mockMcpLogService.Object
            );
        }

        public void Dispose()
        {
            // No cleanup needed for mocks
        }

        #region Window and Element Discovery Tests

        [Fact]
        public async Task SearchElements_Windows_ReturnsWindowList()
        {
            // Arrange
            var expectedElements = new List<ElementInfo>
            {
                new ElementInfo { Name = "Window1", ProcessId = 1234, ControlType = "Window", IsVisible = true },
                new ElementInfo { Name = "Window2", ProcessId = 5678, ControlType = "Window", IsVisible = true }
            };

            var searchElementsResult = new SearchElementsResult
            {
                Success = true,
                Elements = expectedElements.ToArray(),
                Metadata = new SearchMetadata { TotalFound = expectedElements.Count }
            };
            var serverResponse = new ServerEnhancedResponse<SearchElementsResult>
            {
                Success = true,
                Data = searchElementsResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };

            _mockElementSearchService.Setup(s => s.SearchElementsAsync(It.IsAny<UIAutomationMCP.Models.Requests.SearchElementsRequest>(), 30))
                            .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SearchElements(controlType: "Window", scope: "children");

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.SearchElementsAsync(It.IsAny<UIAutomationMCP.Models.Requests.SearchElementsRequest>(), 30), Times.Once);
            _output.WriteLine($"SearchElements Windows test passed: Found {expectedElements.Count} windows");
        }

        [Fact]
        public async Task SearchElements_WithSearchText_Success()
        {
            // Arrange
            var expectedElements = new List<ElementInfo>
            {
                new ElementInfo { AutomationId = "btn1", Name = "Button1", ControlType = "Button" },
                new ElementInfo { AutomationId = "btn2", Name = "Button2", ControlType = "Button" }
            };

            var searchElementsResult = new SearchElementsResult
            {
                Success = true,
                Elements = expectedElements.ToArray(),
            };
            var serverResponse = new ServerEnhancedResponse<SearchElementsResult>
            {
                Success = true,
                Data = searchElementsResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };

            _mockElementSearchService.Setup(s => s.SearchElementsAsync(It.IsAny<UIAutomationMCP.Models.Requests.SearchElementsRequest>(), 30))
                                   .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SearchElements("Button");

            // Assert
            Assert.NotNull(result);
            _mockElementSearchService.Verify(s => s.SearchElementsAsync(It.IsAny<UIAutomationMCP.Models.Requests.SearchElementsRequest>(), 30), Times.Once);
            _output.WriteLine("SearchElements with search text test passed");
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
            
            _mockInteractionService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.InvokeElement(automationId: "testButton", name: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("InvokeElement test passed");
        }

        [Fact]
        public async Task InvokeElement_WithDefaultParameters_Success()
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

            _mockInteractionService.Setup(s => s.InvokeElementAsync("testButton", null, null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.InvokeElement("testButton");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.InvokeElementAsync("testButton", null, null, null, 30), Times.Once);
            _output.WriteLine("InvokeElement with default parameters test passed");
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
            _mockInteractionService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, customTimeout))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.InvokeElement(automationId: "testButton", name: "TestWindow", timeoutSeconds: customTimeout);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, customTimeout), Times.Once);
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

            _mockInteractionService.Setup(s => s.InvokeElementAsync("nonExistentButton", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.InvokeElement("nonExistentButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.InvokeElementAsync("nonExistentButton", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("InvokeElement element not found test passed");
        }

        [Fact]
        public async Task InvokeElement_ServiceException_PropagatesError()
        {
            // Arrange
            _mockInteractionService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, 30))
                                 .ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _tools.InvokeElement("testButton", "TestWindow"));

            _mockInteractionService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, null, 30), Times.Once);
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

            _mockInteractionService.Setup(s => s.InvokeElementAsync("", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.InvokeElement("", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.InvokeElementAsync("", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("InvokeElement empty element ID test passed");
        }

        [Fact]
        public async Task SetElementValue_Success_SetsValue()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                AutomationId = "textBox",
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
            _mockInteractionService.Setup(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetElementValue("textBox", "Test Value", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetValueAsync("textBox", "Test Value", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("SetElementValue test passed");
        }



        [Fact]
        public async Task ToggleElement_Success_TogglesElement()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                AutomationId = "checkbox",
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
            _mockInteractionService.Setup(s => s.ToggleElementAsync("checkbox", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.ToggleElement("checkbox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.ToggleElementAsync("checkbox", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("ToggleElement test passed");
        }

        [Fact]
        public async Task SelectionAction_Select_Success()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "SelectItem",
                AutomationId = "listItem",
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
            _mockSelectionService.Setup(s => s.SelectItemAsync("listItem", "TestWindow", null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SelectionAction("select", "listItem", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockSelectionService.Verify(s => s.SelectItemAsync("listItem", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("SelectionAction select test passed");
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
                AutomationId = "scrollableList",
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
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollableList", null, "down", 1.0, null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.ScrollElement(automationId: "scrollableList", direction: "down");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementAsync("scrollableList", null, "down", 1.0, null, null, 30), Times.Once);
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
                AutomationId = "slider",
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
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("slider", null, 50.0, null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetRangeValue(automationId: "slider", value: 50.0);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("slider", null, 50.0, null, null, 30), Times.Once);
            _output.WriteLine("SetRangeValue test passed");
        }

        #endregion

        #region Text Pattern Tests



        [Fact]
        public async Task SelectText_Success_SelectsText()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                AutomationId = "textElement",
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
            _mockTextService.Setup(s => s.SelectTextAsync("textElement", null, 5, 10, null, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SelectText(automationId: "textElement", startIndex: 5, length: 10);

            // Assert
            Assert.NotNull(result);
            _mockTextService.Verify(s => s.SelectTextAsync("textElement", null, 5, 10, null, null, 30), Times.Once);
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
                RootNode = new TreeNode { Name = "TestWindow", AutomationId = "root1" },
                TotalElements = 1
            };
            var serverResponse = new ServerEnhancedResponse<ElementTreeResult>
            {
                Success = true,
                Data = elementTreeResult,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };

            _mockTreeNavigationService.Setup(s => s.GetElementTreeAsync(null, 3, 60))
                                   .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetElementTree();

            // Assert
            Assert.NotNull(result);
            _mockTreeNavigationService.Verify(s => s.GetElementTreeAsync(null, 3, 60), Times.Once);
            _output.WriteLine("GetElementTree test passed");
        }

        // GetElementInfo test removed - functionality replaced by SearchElements

        [Fact]
        public async Task LaunchWin32Application_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = ProcessLaunchResponse.CreateSuccess(1234, "notepad", false);
            _mockApplicationLauncher.Setup(s => s.LaunchApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.LaunchApplication("notepad.exe");

            // Assert
            Assert.NotNull(result);
            // The result is now a JSON string, so we just verify it's not null
            // and the service was called correctly
            _mockApplicationLauncher.Verify(s => s.LaunchApplicationAsync("notepad.exe", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchApplication test passed");
        }

        [Fact]
        public async Task LaunchApplicationByName_Success_CallsApplicationLauncher()
        {
            // Arrange
            var expectedResult = ProcessLaunchResponse.CreateSuccess(5678, "Calculator", false);
            _mockApplicationLauncher.Setup(s => s.LaunchApplicationAsync("Calculator", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.LaunchApplication("Calculator");

            // Assert
            Assert.NotNull(result);
            // The result is now a JSON string, so we just verify it's not null
            // and the service was called correctly
            _mockApplicationLauncher.Verify(s => s.LaunchApplicationAsync("Calculator", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _output.WriteLine("LaunchApplicationByName test passed");
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
        [InlineData("minimize", "TestWindow")]
        [InlineData("maximize", "AnotherWindow")]
        [InlineData("close", null)]
        public async Task WindowAction_WithVariousParameters_CallsCorrectService(string action, string windowTitle)
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                Action = action,
                WindowTitle = windowTitle,
                ProcessId = 0,
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
            _mockWindowService.Setup(s => s.WindowOperationAsync(action, windowTitle, null, 30))
                                 .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.WindowAction(action, windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.WindowOperationAsync(action, windowTitle, null, 30), Times.Once);
            _output.WriteLine($"WindowAction with parameters ({action}, {windowTitle}) test passed");
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
            _mockWindowService.Setup(s => s.WaitForInputIdleAsync(timeoutMs, It.IsAny<string>(), null, It.IsAny<int>()))
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
        public async Task GetGridInfo_GetItem_Success()
        {
            // Arrange
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
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
            _mockGridTableService.Setup(s => s.GetGridItemAsync("grid1", null, 1, 2, null, null, 30))
                           .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetGridInfo("get-item", automationId: "grid1", row: 1, column: 2);

            // Assert
            Assert.NotNull(result);
            _mockGridTableService.Verify(s => s.GetGridItemAsync("grid1", null, 1, 2, null, null, 30), Times.Once);
            _output.WriteLine("GetGridInfo get-item test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithValidParameters_CallsCorrectService()
        {
            // Arrange
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
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
            _mockAdvancedPatternService.Setup(s => s.SetViewAsync("viewContainer1", null, 2, null, null, 30))
                                  .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetView(automationId: "viewContainer1", viewId: 2, controlType: null);

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.SetViewAsync("viewContainer1", null, 2, null, null, 30), Times.Once);
            _output.WriteLine("SetView test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithDefaultParameters_CallsCorrectService()
        {
            // Arrange
            var resultObject = new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Models.ElementInfo>(),
                SearchCriteria = "View set successfully"
            };
            var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockAdvancedPatternService.Setup(s => s.SetViewAsync("viewContainer1", null, 1, null, null, 30))
                                  .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetView(automationId: "viewContainer1", viewId: 1);

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.SetViewAsync("viewContainer1", null, 1, null, null, 30), Times.Once);
            _output.WriteLine("SetView with default parameters test passed");
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
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>(),
                    SearchCriteria = "View set successfully"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockAdvancedPatternService.Setup(s => s.SetViewAsync("viewContainer1", null, 3, null, null, 60))
                                  .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetView(automationId: "viewContainer1", viewId: 3, timeoutSeconds: 60);

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.SetViewAsync("viewContainer1", null, 3, null, null, 60), Times.Once);
            _output.WriteLine("SetView with custom timeout test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetView_WithInvalidViewId_ServiceException_PropagatesError()
        {
            // Arrange
            _mockAdvancedPatternService.Setup(s => s.SetViewAsync("viewContainer1", null, 999, null, null, 30))
                                  .ThrowsAsync(new ArgumentException("Unsupported view ID: 999"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _tools.SetView(automationId: "viewContainer1", viewId: 999));

            _mockAdvancedPatternService.Verify(s => s.SetViewAsync("viewContainer1", null, 999, null, null, 30), Times.Once);
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
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>(),
                    SearchCriteria = "View set to default"
                },
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockAdvancedPatternService.Setup(s => s.SetViewAsync("viewContainer1", null, 0, null, null, 30))
                                  .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.SetView(automationId: "viewContainer1", viewId: 0);

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.SetViewAsync("viewContainer1", null, 0, null, null, 30), Times.Once);
            _output.WriteLine("SetView with zero viewId test passed");
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
                AutomationId = "",
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
            _mockInteractionService.Setup(s => s.SetValueAsync("", null, "testValue", null, null, 30))
                            .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetElementValue(automationId: "", value: "testValue");

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetValueAsync("", null, "testValue", null, null, 30), Times.Once);
            _output.WriteLine("SetElementValue with empty elementId test passed");
        }

        [Fact]
        public async Task SetRangeValue_WithBoundaryValues_ShouldCallService()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                AutomationId = "slider1",
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
            _mockInteractionService.Setup(s => s.SetRangeValueAsync("slider1", null, 0.0, null, null, 30))
                           .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetRangeValue(automationId: "slider1", value: 0.0);

            // Assert
            Assert.NotNull(result);
            _mockInteractionService.Verify(s => s.SetRangeValueAsync("slider1", null, 0.0, null, null, 30), Times.Once);
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
                AutomationId = "scrollable1",
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
            _mockLayoutService.Setup(s => s.ScrollElementAsync("scrollable1", null, "down", 2.5, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.ScrollElement(automationId: "scrollable1", direction: "down", amount: 2.5);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementAsync("scrollable1", null, "down", 2.5, null, null, 30), Times.Once);
            _output.WriteLine("ScrollElement with custom amount test passed");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetScrollPercent_Should_Call_LayoutService_With_Valid_Percentages()
        {
            // Arrange - Microsoft ScrollPattern SetScrollPercent
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                AutomationId = "scrollContainer"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollContainer", null, 75.0, 25.0, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetScrollPercent(automationId: "scrollContainer", horizontalPercent: 75.0, verticalPercent: 25.0, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("scrollContainer", null, 75.0, 25.0, null, null, 30), Times.Once);
            _output.WriteLine("SetScrollPercent test passed - percentage-based scrolling verified");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetScrollPercent_Should_Handle_NoScroll_Values()
        {
            // Arrange - Microsoft -1 NoScroll values
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                AutomationId = "scrollElement"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("scrollElement", null, -1.0, 50.0, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

            // Act - NoScroll(-1) for horizontal, 50% for vertical
            var result = await _tools.SetScrollPercent(automationId: "scrollElement", horizontalPercent: -1.0, verticalPercent: 50.0);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("scrollElement", null, -1.0, 50.0, null, null, 30), Times.Once);
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
            // Arrange - Microsoft valid range values -100 to 100
            var resultObject = new ActionResult
            {
                Success = true,
                ActionName = "SetScrollPercent",
                AutomationId = "testElement"
            };
            var serverResponse = new ServerEnhancedResponse<ActionResult>
            {
                Success = true,
                Data = resultObject,
                ExecutionInfo = new ServerExecutionInfo(),
                RequestMetadata = new RequestMetadata()
            };
            _mockLayoutService.Setup(s => s.SetScrollPercentAsync("testElement", null, horizontal, vertical, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.SetScrollPercent(automationId: "testElement", horizontalPercent: horizontal, verticalPercent: vertical);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.SetScrollPercentAsync("testElement", null, horizontal, vertical, null, null, 30), Times.Once);
            _output.WriteLine($"SetScrollPercent valid range test passed for H:{horizontal}, V:{vertical}");
        }

        #endregion


        #region ScrollElementIntoView Integration Tests

        /// <summary>
        /// ScrollElementIntoView -  icrosoft ScrollItemPattern         /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Call_LayoutService_With_Correct_Parameters()
        {
            // Arrange - Microsoft ScrollItemPattern.ScrollIntoView()
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

            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("scrollableItem", "TestWindow", null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ScrollElementIntoView(automationId: "scrollableItem", name: "TestWindow", windowHandle: null);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("scrollableItem", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView test passed - ScrollItemPattern.ScrollIntoView() verified");
        }

        /// <summary>
        /// ScrollElementIntoView -          /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Use_Default_Parameters()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElementIntoView",
                AutomationId = "listItem",
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
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("listItem", null, null, null, 30))
                             .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.ScrollElementIntoView(automationId: "listItem");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("listItem", null, null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView with default parameters test passed");
        }

        /// <summary>
        /// ScrollElementIntoView -  crollItemPattern         /// Microsoft  InvalidOperationException handling
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Pattern_Not_Supported_Exception()
        {
            // Arrange
            var expectedException = new InvalidOperationException("ScrollItemPattern is not supported by this element");

            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("nonScrollableElement", null, null, null, 30))
                             .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _tools.ScrollElementIntoView(automationId: "nonScrollableElement"));

            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("nonScrollableElement", null, null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView pattern not supported exception test passed");
        }

        /// <summary>
        /// ScrollElementIntoView -          /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Element_Not_Found_Exception()
        {
            // Arrange
            var expectedException = new ArgumentException("Element not found: nonExistentElement");

            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("nonExistentElement", "TestWindow", null, null, 30))
                             .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _tools.ScrollElementIntoView(automationId: "nonExistentElement", name: "TestWindow"));

            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("nonExistentElement", "TestWindow", null, null, 30), Times.Once);
            _output.WriteLine("ScrollElementIntoView element not found exception test passed");
        }

        /// <summary>
        /// ScrollElementIntoView -          /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ScrollElementIntoView_Should_Handle_Custom_Timeout()
        {
            // Arrange
            var resultObject = new ActionResult
            {
                Success = true,
                Action = "ScrollElementIntoView",
                AutomationId = "slowElement",
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
            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync("slowElement", "TestWindow", null, null, 60))
                             .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.ScrollElementIntoView(automationId: "slowElement", name: "TestWindow", timeoutSeconds: 60);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync("slowElement", "TestWindow", null, null, 60), Times.Once);
            _output.WriteLine("ScrollElementIntoView with custom timeout test passed");
        }

        /// <summary>
        /// ScrollElementIntoView - Microsoft istItem TreeItem         /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("list-item-1")]
        [InlineData("tree-item-node-2")]
        [InlineData("dataitem-3")]
        public async Task ScrollElementIntoView_Should_Work_With_Expected_Control_Types(string elementId)
        {
            // Arrange - ListItem TreeItem DataItem ScrollItemPattern
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

            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, "TestApplication", null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ScrollElementIntoView(automationId: elementId, name: "TestApplication");

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync(elementId, "TestApplication", null, null, 30), Times.Once);
            _output.WriteLine($"ScrollElementIntoView worked correctly for control type element: {elementId}");
        }

        /// <summary>
        /// ScrollElementIntoView -          /// </summary>
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
                    AutomationId = elementId,
                    WindowTitle = windowTitle,
                    Success = false,
                    ErrorMessage = "Invalid parameters"
                }
            };

            _mockLayoutService.Setup(s => s.ScrollElementIntoViewAsync(elementId, windowTitle, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ScrollElementIntoView(automationId: elementId, name: windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockLayoutService.Verify(s => s.ScrollElementIntoViewAsync(elementId, windowTitle, null, null, 30), Times.Once);
            _output.WriteLine($"ScrollElementIntoView empty string parameters test passed for elementId:'{elementId}'");
        }

        #endregion


        #region TableItem Pattern Tests - Microsoft UI Automation 

        /// <summary>
        /// GetColumnHeader -  icrosoft TableItemPattern.GetColumnHeader()         /// Required Members: GetColumnHeader() -          /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetGridInfo_GetColumnHeader_Success()
        {
            // Arrange - Microsoft TableItemPattern GetColumnHeader()
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
            _mockGridTableService.Setup(s => s.GetColumnHeaderAsync("tableCell1", "TestWindow", 0, null, null, 30))
                           .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetGridInfo("get-column-header", automationId: "tableCell1", name: "TestWindow", column: 0);

            // Assert
            Assert.NotNull(result);
            _mockGridTableService.Verify(s => s.GetColumnHeaderAsync("tableCell1", "TestWindow", 0, null, null, 30), Times.Once);
            _output.WriteLine("GetGridInfo get-column-header test passed");
        }

        /// <summary>
        /// GetRowHeader -  icrosoft TableItemPattern.GetRowHeader()         /// Required Members: GetRowHeader() -          /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetGridInfo_GetRowHeader_Success()
        {
            // Arrange - Microsoft TableItemPattern GetRowHeader()
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
            _mockGridTableService.Setup(s => s.GetRowHeaderAsync("tableCell2", "TestWindow", 0, null, null, 30))
                           .Returns(Task.FromResult(serverResponse));

            // Act
            var result = await _tools.GetGridInfo("get-row-header", automationId: "tableCell2", name: "TestWindow", row: 0);

            // Assert
            Assert.NotNull(result);
            _mockGridTableService.Verify(s => s.GetRowHeaderAsync("tableCell2", "TestWindow", 0, null, null, 30), Times.Once);
            _output.WriteLine("GetGridInfo get-row-header test passed");
        }

        #endregion
    }
}

