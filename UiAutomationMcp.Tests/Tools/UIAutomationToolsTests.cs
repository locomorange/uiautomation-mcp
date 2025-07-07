using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Models;
// using UIAutomationMCP.Server.Services.Windows;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Tools
{
    /// <summary>
    /// Tests for UIAutomationTools covering the essential functionality
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class UIAutomationToolsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<UIAutomationTools>> _logger;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IApplicationLauncher> _mockApplicationLauncher;
        private readonly Mock<IUIAutomationWorker> _mockUIAutomationWorker;
        private readonly Mock<IScreenshotService> _mockScreenshotService;

        public UIAutomationToolsTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<UIAutomationTools>>();
            
            // Create all service mocks
            _mockApplicationLauncher = new Mock<IApplicationLauncher>();
            _mockUIAutomationWorker = new Mock<IUIAutomationWorker>();
            _mockScreenshotService = new Mock<IScreenshotService>();

            var mockUIAutomationService = new Mock<IUIAutomationService>();
            mockUIAutomationService.Setup(x => x.Worker).Returns(_mockUIAutomationWorker.Object);
            
            _tools = new UIAutomationTools(
                _mockApplicationLauncher.Object,
                mockUIAutomationService.Object,
                _mockScreenshotService.Object
            );
        }

        public void Dispose()
        {
            _mockUIAutomationWorker?.Object?.Dispose();
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
            var expectedResult = new OperationResult<List<WindowInfo>>
            {
                Success = true,
                Data = expectedWindows
            };
            _mockUIAutomationWorker.Setup(s => s.GetWindowInfoAsync(It.IsAny<object>(), It.IsAny<int>()))
                            .Returns(Task.FromResult(new OperationResult<object> { Success = true, Data = expectedWindows }));

            // Act
            var result = await _tools.GetWindowInfo();

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.GetWindowInfoAsync(It.IsAny<object>(), It.IsAny<int>()), Times.Once);
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
            var expectedResult = new OperationResult<List<ElementInfo>>
            {
                Success = true,
                Data = expectedElements
            };
            _mockUIAutomationWorker.Setup(s => s.FindAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                                   .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.FindElements("Button", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.FindAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()), Times.Once);
            _output.WriteLine($"FindElements test passed: Found {expectedElements.Count} elements");
        }

        #endregion

        #region Core Pattern Tests - Using UIAutomationWorker

        [Fact]
        public async Task InvokeElement_Success_InvokesElement()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Element invoked successfully" };
            _mockUIAutomationWorker.Setup(s => s.InvokeElementAsync("testButton", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.InvokeElement("testButton", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.InvokeElementAsync("testButton", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("InvokeElement test passed");
        }

        [Fact]
        public async Task SetElementValue_Success_SetsValue()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Value set successfully" };
            _mockUIAutomationWorker.Setup(s => s.SetElementValueAsync("textBox", "Test Value", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.SetElementValue("textBox", "Test Value", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.SetElementValueAsync("textBox", "Test Value", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("SetElementValue test passed");
        }

        [Fact]
        public async Task GetElementValue_Success_GetsValue()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Current Value" };
            _mockUIAutomationWorker.Setup(s => s.GetElementValueAsync("textBox", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.GetElementValue("textBox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.GetElementValueAsync("textBox", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("GetElementValue test passed");
        }

        [Fact]
        public async Task ToggleElement_Success_TogglesElement()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Element toggled successfully" };
            _mockUIAutomationWorker.Setup(s => s.ToggleElementAsync("checkbox", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.ToggleElement("checkbox", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.ToggleElementAsync("checkbox", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("ToggleElement test passed");
        }

        [Fact]
        public async Task SelectElement_Success_SelectsElement()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Element selected successfully" };
            _mockUIAutomationWorker.Setup(s => s.SelectElementAsync("listItem", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.SelectElement("listItem", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.SelectElementAsync("listItem", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("SelectElement test passed");
        }

        #endregion

        #region Layout Pattern Tests

        [Fact]
        public async Task ScrollElement_Success_ScrollsElement()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Element scrolled successfully" };
            _mockUIAutomationWorker.Setup(s => s.ScrollElementAsync("scrollableList", "down", null, null, "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.ScrollElement("scrollableList", "down", windowTitle: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.ScrollElementAsync("scrollableList", "down", null, null, "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("ScrollElement test passed");
        }

        #endregion

        #region Range Value Tests

        [Fact]
        public async Task SetRangeValue_Success_SetsRangeValue()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Range value set successfully" };
            _mockUIAutomationWorker.Setup(s => s.SetRangeValueAsync("slider", 50.0, "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.SetRangeValue("slider", 50.0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.SetRangeValueAsync("slider", 50.0, "TestWindow", null, It.IsAny<int>()), Times.Once);
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
            var workerResult = new OperationResult<Dictionary<string, object>> { Success = true, Data = rangeInfo };
            _mockUIAutomationWorker.Setup(s => s.GetRangeValueAsync("slider", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.GetRangeValue("slider", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.GetRangeValueAsync("slider", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("GetRangeValue test passed");
        }

        #endregion

        #region Text Pattern Tests

        [Fact]
        public async Task GetText_Success_GetsText()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Sample text content" };
            _mockUIAutomationWorker.Setup(s => s.GetTextAsync("textElement", "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.GetText("textElement", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.GetTextAsync("textElement", "TestWindow", null, It.IsAny<int>()), Times.Once);
            _output.WriteLine("GetText test passed");
        }

        [Fact]
        public async Task SelectText_Success_SelectsText()
        {
            // Arrange
            var workerResult = new OperationResult<string> { Success = true, Data = "Text selected successfully" };
            _mockUIAutomationWorker.Setup(s => s.SelectTextAsync("textElement", 5, 10, "TestWindow", null, It.IsAny<int>()))
                                 .ReturnsAsync(workerResult);

            // Act
            var result = await _tools.SelectText("textElement", 5, 10, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockUIAutomationWorker.Verify(s => s.SelectTextAsync("textElement", 5, 10, "TestWindow", null, It.IsAny<int>()), Times.Once);
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
            _mockScreenshotService.Setup(s => s.TakeScreenshotAsync("TestWindow", null, 0, null))
                                .Returns(Task.FromResult(screenshotResult));

            // Act
            var result = await _tools.TakeScreenshot("TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockScreenshotService.Verify(s => s.TakeScreenshotAsync("TestWindow", null, 0, null), Times.Once);
            _output.WriteLine("TakeScreenshot test passed");
        }

        #endregion
    }
}