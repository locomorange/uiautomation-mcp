using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services.Elements;
using UiAutomationMcpServer.Services.Patterns;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Tools;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Tools
{
    /// <summary>
    /// Comprehensive tests for UIAutomationTools covering the complete tool API surface
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class UIAutomationToolsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<UIAutomationTools>> _logger;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IWindowService> _mockWindowService;
        private readonly Mock<IElementDiscoveryService> _mockElementDiscoveryService;
        private readonly Mock<IElementTreeService> _mockElementTreeService;
        private readonly Mock<IElementPropertiesService> _mockElementPropertiesService;
        private readonly Mock<ICorePatternService> _mockCorePatternService;
        private readonly Mock<ILayoutPatternService> _mockLayoutPatternService;
        private readonly Mock<IRangePatternService> _mockRangePatternService;
        private readonly Mock<IWindowPatternService> _mockWindowPatternService;
        private readonly Mock<ITextPatternService> _mockTextPatternService;
        private readonly Mock<IAdvancedPatternService> _mockAdvancedPatternService;
        private readonly Mock<IScreenshotService> _mockScreenshotService;

        public UIAutomationToolsTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<UIAutomationTools>>();
            
            // Create all service mocks
            _mockWindowService = new Mock<IWindowService>();
            _mockElementDiscoveryService = new Mock<IElementDiscoveryService>();
            _mockElementTreeService = new Mock<IElementTreeService>();
            _mockElementPropertiesService = new Mock<IElementPropertiesService>();
            _mockCorePatternService = new Mock<ICorePatternService>();
            _mockLayoutPatternService = new Mock<ILayoutPatternService>();
            _mockRangePatternService = new Mock<IRangePatternService>();
            _mockWindowPatternService = new Mock<IWindowPatternService>();
            _mockTextPatternService = new Mock<ITextPatternService>();
            _mockAdvancedPatternService = new Mock<IAdvancedPatternService>();
            _mockScreenshotService = new Mock<IScreenshotService>();

            _tools = new UIAutomationTools(
                _mockWindowService.Object,
                _mockElementDiscoveryService.Object,
                _mockElementTreeService.Object,
                _mockElementPropertiesService.Object,
                _mockCorePatternService.Object,
                _mockLayoutPatternService.Object,
                _mockRangePatternService.Object,
                _mockWindowPatternService.Object,
                _mockTextPatternService.Object,
                _mockAdvancedPatternService.Object,
                _mockScreenshotService.Object,
                _logger.Object
            );
        }

        #region Window and Element Discovery Tests

        [Fact]
        public async Task GetWindowInfo_Success_ReturnsWindowList()
        {
            // Arrange
            var expectedWindows = new List<WindowInfo>
            {
                new WindowInfo { Title = "Window1", ProcessName = "App1", ProcessId = 1234, Handle = (IntPtr)1001 },
                new WindowInfo { Title = "Window2", ProcessName = "App2", ProcessId = 5678, Handle = (IntPtr)1002 }
            };
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = expectedWindows
            };
            _mockWindowService.Setup(s => s.GetWindowInfoAsync())
                            .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetWindowInfo();

            // Assert
            Assert.NotNull(result);
            _mockWindowService.Verify(s => s.GetWindowInfoAsync(), Times.Once);
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
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = expectedElements
            };
            _mockElementDiscoveryService.Setup(s => s.FindElementsAsync("Button", null, "TestWindow", null))
                                       .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.FindElements("Button", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockElementDiscoveryService.Verify(s => s.FindElementsAsync("Button", null, "TestWindow", null), Times.Once);
            _output.WriteLine($"FindElements test passed: Found {expectedElements.Count} elements");
        }

        [Fact]
        public async Task GetElementTree_Success_ReturnsTree()
        {
            // Arrange
            var expectedTree = new ElementTreeNode
            {
                AutomationId = "root",
                Name = "Root",
                ControlType = "Window",
                Children = new List<ElementTreeNode>
                {
                    new ElementTreeNode { AutomationId = "child1", Name = "Child1", ControlType = "Button" }
                }
            };
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = expectedTree
            };
            _mockElementTreeService.Setup(s => s.GetElementTreeAsync("TestWindow", "control", 3, null))
                                 .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetElementTree("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockElementTreeService.Verify(s => s.GetElementTreeAsync("TestWindow", "control", 3, null), Times.Once);
            _output.WriteLine($"GetElementTree test passed: Tree root is {expectedTree.Name}");
        }

        #endregion

        #region Core Pattern Tests

        [Fact]
        public async Task InvokeElement_Success_InvokesElement()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element invoked successfully"
            };
            _mockCorePatternService.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.InvokeElement("testButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCorePatternService.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null), Times.Once);
            _output.WriteLine("InvokeElement test passed");
        }

        [Fact]
        public async Task SetElementValue_Success_SetsValue()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Value set successfully"
            };
            _mockCorePatternService.Setup(s => s.SetElementValueAsync("textBox", "Test Value", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetElementValue("textBox", "Test Value", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCorePatternService.Verify(s => s.SetElementValueAsync("textBox", "Test Value", "TestWindow", null), Times.Once);
            _output.WriteLine("SetElementValue test passed");
        }

        [Fact]
        public async Task GetElementValue_Success_GetsValue()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Current Value"
            };
            _mockCorePatternService.Setup(s => s.GetElementValueAsync("textBox", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetElementValue("textBox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCorePatternService.Verify(s => s.GetElementValueAsync("textBox", "TestWindow", null), Times.Once);
            _output.WriteLine("GetElementValue test passed");
        }

        [Fact]
        public async Task ToggleElement_Success_TogglesElement()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element toggled"
            };
            _mockCorePatternService.Setup(s => s.ToggleElementAsync("checkBox", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ToggleElement("checkBox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCorePatternService.Verify(s => s.ToggleElementAsync("checkBox", "TestWindow", null), Times.Once);
            _output.WriteLine("ToggleElement test passed");
        }

        [Fact]
        public async Task SelectElement_Success_SelectsElement()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element selected"
            };
            _mockCorePatternService.Setup(s => s.SelectElementAsync("listItem", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectElement("listItem", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockCorePatternService.Verify(s => s.SelectElementAsync("listItem", "TestWindow", null), Times.Once);
            _output.WriteLine("SelectElement test passed");
        }

        #endregion

        #region Layout Pattern Tests

        [Fact]
        public async Task ScrollElement_WithDirection_Success()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Scrolled down successfully"
            };
            _mockLayoutPatternService.Setup(s => s.ScrollElementAsync("scrollElement", "down", null, null, "TestWindow", null))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElement("scrollElement", "down", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutPatternService.Verify(s => s.ScrollElementAsync("scrollElement", "down", null, null, "TestWindow", null), Times.Once);
            _output.WriteLine("ScrollElement with direction test passed");
        }

        [Fact]
        public async Task ScrollElement_WithPercentages_Success()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Scrolled to position successfully"
            };
            _mockLayoutPatternService.Setup(s => s.ScrollElementAsync("scrollElement", null, 50.0, 75.0, "TestWindow", null))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElement("scrollElement", horizontal: 50.0, vertical: 75.0, windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutPatternService.Verify(s => s.ScrollElementAsync("scrollElement", null, 50.0, 75.0, "TestWindow", null), Times.Once);
            _output.WriteLine("ScrollElement with percentages test passed");
        }

        [Fact]
        public async Task ScrollElementIntoView_Success_ScrollsIntoView()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element scrolled into view"
            };
            _mockLayoutPatternService.Setup(s => s.ScrollElementIntoViewAsync("targetElement", "TestWindow", null))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ScrollElementIntoView("targetElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockLayoutPatternService.Verify(s => s.ScrollElementIntoViewAsync("targetElement", "TestWindow", null), Times.Once);
            _output.WriteLine("ScrollElementIntoView test passed");
        }

        [Fact]
        public async Task ExpandCollapseElement_Expand_Success()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element expanded"
            };
            _mockAdvancedPatternService.Setup(s => s.ExpandCollapseElementAsync("treeNode", true, "TestWindow", null))
                                     .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.ExpandCollapseElement("treeNode", true, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.ExpandCollapseElementAsync("treeNode", true, "TestWindow", null), Times.Once);
            _output.WriteLine("ExpandCollapseElement expand test passed");
        }

        #endregion

        #region Range Pattern Tests

        [Fact]
        public async Task SetRangeValue_Success_SetsRangeValue()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Range value set to 75"
            };
            _mockRangePatternService.Setup(s => s.SetRangeValueAsync("slider", 75.0, "TestWindow", null))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SetRangeValue("slider", 75.0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockRangePatternService.Verify(s => s.SetRangeValueAsync("slider", 75.0, "TestWindow", null), Times.Once);
            _output.WriteLine("SetRangeValue test passed");
        }

        [Fact]
        public async Task GetRangeValue_Success_GetsRangeValue()
        {
            // Arrange
            var rangeData = new Dictionary<string, object>
            {
                { "Value", 50.0 },
                { "Minimum", 0.0 },
                { "Maximum", 100.0 }
            };
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = rangeData
            };
            _mockRangePatternService.Setup(s => s.GetRangeValueAsync("slider", "TestWindow", null))
                                  .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetRangeValue("slider", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockRangePatternService.Verify(s => s.GetRangeValueAsync("slider", "TestWindow", null), Times.Once);
            _output.WriteLine("GetRangeValue test passed");
        }

        #endregion

        #region Window Pattern Tests

        [Fact]
        public async Task WindowAction_Close_Success()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Window closed successfully"
            };
            _mockWindowPatternService.Setup(s => s.WindowActionAsync("mainWindow", "close", "TestApp", null))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WindowAction("mainWindow", "close", "TestApp");

            // Assert
            Assert.NotNull(result);
            _mockWindowPatternService.Verify(s => s.WindowActionAsync("mainWindow", "close", "TestApp", null), Times.Once);
            _output.WriteLine("WindowAction close test passed");
        }

        [Theory]
        [InlineData("minimize")]
        [InlineData("maximize")]
        [InlineData("normal")]
        public async Task WindowAction_VariousActions_Success(string action)
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = $"Window {action} completed"
            };
            _mockWindowPatternService.Setup(s => s.WindowActionAsync("testWindow", action, null, null))
                                   .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.WindowAction("testWindow", action);

            // Assert
            Assert.NotNull(result);
            _mockWindowPatternService.Verify(s => s.WindowActionAsync("testWindow", action, null, null), Times.Once);
            _output.WriteLine($"WindowAction {action} test passed");
        }

        #endregion

        #region Text Pattern Tests

        [Fact]
        public async Task GetText_Success_ReturnsText()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Sample text content"
            };
            _mockTextPatternService.Setup(s => s.GetTextAsync("textElement", "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetText("textElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTextPatternService.Verify(s => s.GetTextAsync("textElement", "TestWindow", null), Times.Once);
            _output.WriteLine("GetText test passed");
        }

        [Fact]
        public async Task SelectText_Success_SelectsText()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Text selected successfully"
            };
            _mockTextPatternService.Setup(s => s.SelectTextAsync("textElement", 0, 10, "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.SelectText("textElement", 0, 10, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTextPatternService.Verify(s => s.SelectTextAsync("textElement", 0, 10, "TestWindow", null), Times.Once);
            _output.WriteLine("SelectText test passed");
        }

        [Fact]
        public async Task FindText_Success_FindsText()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Text found at position 5"
            };
            _mockTextPatternService.Setup(s => s.FindTextAsync("textElement", "search", false, true, "TestWindow", null))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.FindText("textElement", "search", false, true, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockTextPatternService.Verify(s => s.FindTextAsync("textElement", "search", false, true, "TestWindow", null), Times.Once);
            _output.WriteLine("FindText test passed");
        }

        #endregion

        #region Advanced Pattern Tests

        [Fact]
        public async Task TransformElement_Move_Success()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element moved successfully"
            };
            _mockAdvancedPatternService.Setup(s => s.TransformElementAsync("element", "move", 100, 200, null, null, null, "TestWindow", null))
                                     .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.TransformElement("element", "move", 100, 200, windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.TransformElementAsync("element", "move", 100, 200, null, null, null, "TestWindow", null), Times.Once);
            _output.WriteLine("TransformElement move test passed");
        }

        [Fact]
        public async Task DockElement_Success_DocksElement()
        {
            // Arrange
            var expectedResult = new OperationResult
            {
                Success = true,
                Data = "Element docked to top"
            };
            _mockAdvancedPatternService.Setup(s => s.DockElementAsync("dockableElement", "top", "TestWindow", null))
                                     .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.DockElement("dockableElement", "top", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockAdvancedPatternService.Verify(s => s.DockElementAsync("dockableElement", "top", "TestWindow", null), Times.Once);
            _output.WriteLine("DockElement test passed");
        }

        #endregion

        #region Screenshot Tests

        [Fact]
        public async Task TakeScreenshot_Success_TakesScreenshot()
        {
            // Arrange
            var expectedResult = new ScreenshotResult
            {
                Success = true,
                Data = "Screenshot taken successfully",
                OutputPath = "test_screenshot.png",
                Base64Image = "base64string",
                Width = 1920,
                Height = 1080
            };
            _mockScreenshotService.Setup(s => s.TakeScreenshotAsync("TestWindow", null, 0, null))
                                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.TakeScreenshot("TestWindow");

            // Assert
            _output.WriteLine($"TakeScreenshot result is null: {result == null}");
            _output.WriteLine($"TakeScreenshot result type: {result?.GetType()?.Name}");
            Assert.NotNull(result);
            _mockScreenshotService.Verify(s => s.TakeScreenshotAsync("TestWindow", null, 0, null), Times.Once);
            _output.WriteLine("TakeScreenshot test passed");
        }

        #endregion

        #region Element Properties Tests

        [Fact]
        public async Task GetElementProperties_Success_ReturnsProperties()
        {
            // Arrange
            var expectedProperties = new Dictionary<string, object>
            {
                { "AutomationId", "testButton" },
                { "Name", "Test Button" },
                { "ControlType", "Button" },
                { "IsEnabled", true }
            };
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = expectedProperties
            };
            _mockElementPropertiesService.Setup(s => s.GetElementPropertiesAsync("testButton", "TestWindow", null))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetElementProperties("testButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockElementPropertiesService.Verify(s => s.GetElementPropertiesAsync("testButton", "TestWindow", null), Times.Once);
            _output.WriteLine("GetElementProperties test passed");
        }

        [Fact]
        public async Task GetElementPatterns_Success_ReturnsPatterns()
        {
            // Arrange
            var expectedPatterns = new Dictionary<string, object>
            {
                { "patterns", new List<string> { "InvokePattern", "ValuePattern" } }
            };
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = expectedPatterns
            };
            _mockElementPropertiesService.Setup(s => s.GetElementPatternsAsync("testElement", "TestWindow", null))
                                        .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetElementPatterns("testElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockElementPropertiesService.Verify(s => s.GetElementPatternsAsync("testElement", "TestWindow", null), Times.Once);
            _output.WriteLine("GetElementPatterns test passed");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_FindAndInteractWithElement_Success()
        {
            // Arrange
            var findResult = new OperationResult
            {
                Success = true,
                Data = new List<ElementInfo>
                {
                    new ElementInfo { AutomationId = "foundButton", Name = "Found Button", ControlType = "Button" }
                }
            };
            var invokeResult = new OperationResult
            {
                Success = true,
                Data = "Button invoked successfully"
            };

            _mockElementDiscoveryService.Setup(s => s.FindElementsAsync("Button", "Button", "TestApp", null))
                                       .Returns(Task.FromResult(findResult));
            _mockCorePatternService.Setup(s => s.InvokeElementAsync("foundButton", "TestApp", null))
                                 .Returns(Task.FromResult(invokeResult));

            // Act
            var findElementsResult = await _tools.FindElements("Button", "Button", "TestApp");
            var invokeElementResult = await _tools.InvokeElement("foundButton", "TestApp");

            // Assert
            Assert.NotNull(findElementsResult);
            Assert.NotNull(invokeElementResult);
            _mockElementDiscoveryService.Verify(s => s.FindElementsAsync("Button", "Button", "TestApp", null), Times.Once);
            _mockCorePatternService.Verify(s => s.InvokeElementAsync("foundButton", "TestApp", null), Times.Once);
            _output.WriteLine("Complete workflow test passed");
        }

        #endregion

        public void Dispose()
        {
            // Dispose of any resources if needed
        }
    }
}
