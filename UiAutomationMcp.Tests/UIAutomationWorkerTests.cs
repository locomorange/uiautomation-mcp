using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcp.Tests.Mocks;
using UiAutomationMcpServer.Services;
using Xunit;

namespace UiAutomationMcp.Tests.Services
{
    /// <summary>
    /// UIAutomationWorkerのユニットテスト
    /// モック実装を使用してプロセス実行を行わず、インターフェースのテストのみを行う
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class UIAutomationWorkerTests : IDisposable
    {
        private readonly Mock<ILogger<MockUIAutomationWorker>> _mockLogger;
        private readonly MockUIAutomationWorker _worker;

        public UIAutomationWorkerTests()
        {
            _mockLogger = new Mock<ILogger<MockUIAutomationWorker>>();
            _worker = new MockUIAutomationWorker(_mockLogger.Object);
        }

        public void Dispose()
        {
            _worker?.Dispose();
        }

        [Fact]
        public void Constructor_ShouldInitializeWorkerExecutablePath()
        {
            // Act & Assert - Constructor should not throw
            var mockLogger = new Mock<ILogger<MockUIAutomationWorker>>();
            var worker = new MockUIAutomationWorker(mockLogger.Object);
            Assert.NotNull(worker);
        }

        [Fact]
        public async Task FindAllElementsAsync_ShouldReturnSuccessWithMockData()
        {
            // Arrange
            var searchParams = new ElementSearchParameters
            {
                WindowTitle = "TestWindow",
                SearchText = "Button",
                ControlType = "Button",
                ProcessId = 1234
            };

            // Act
            var result = await _worker.FindAllElementsAsync(searchParams, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task FindFirstElementAsync_ShouldReturnSuccessWithMockElement()
        {
            // Arrange
            var searchParams = new ElementSearchParameters
            {
                WindowTitle = "TestWindow",
                SearchText = "TextBox",
                AutomationId = "txtInput"
            };

            // Act
            var result = await _worker.FindFirstElementAsync(searchParams, 15);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("MockElement", result.Data?.Name);
        }

        [Fact]
        public async Task ExecuteAdvancedOperationAsync_ShouldReturnSuccess()
        {
            // Arrange
            var operationParams = new AdvancedOperationParameters
            {
                Operation = "invoke",
                ElementId = "btnTest",
                WindowTitle = "TestWindow",
                ProcessId = 1234,
                TimeoutSeconds = 20,
                Parameters = new Dictionary<string, object>
                {
                    ["Action"] = "click"
                }
            };

            // Act
            var result = await _worker.ExecuteAdvancedOperationAsync(operationParams);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Mock advanced operation success", result.Data["result"]);
        }

        [Theory]
        [InlineData("btn1", "TestWindow", 1234)]
        [InlineData("btn2", null, null)]
        [InlineData("btn3", "", 0)]
        public async Task InvokeElementAsync_ShouldReturnSuccess(string elementId, string? windowTitle, int? processId)
        {
            // Act
            var result = await _worker.InvokeElementAsync(elementId, windowTitle, processId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Mock invoke success", result.Data);
        }

        [Theory]
        [InlineData("input1", "test value")]
        [InlineData("input2", "")]
        [InlineData("input3", "special chars: !@#$%")]
        public async Task SetElementValueAsync_ShouldReturnSuccess(string elementId, string value)
        {
            // Act
            var result = await _worker.SetElementValueAsync(elementId, value);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Mock set value success", result.Data);
        }

        [Theory]
        [InlineData("slider1", 50.0)]
        [InlineData("slider2", 0.0)]
        [InlineData("slider3", 100.0)]
        [InlineData("slider4", 75.5)]
        public async Task SetRangeValueAsync_ShouldReturnSuccess(string elementId, double value)
        {
            // Act
            var result = await _worker.SetRangeValueAsync(elementId, value);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Mock range value set", result.Data);
        }

        [Theory]
        [InlineData("text1", 0, 5)]
        [InlineData("text2", 10, 20)]
        [InlineData("text3", -1, 0)] // Edge case
        public async Task SelectTextAsync_ShouldReturnSuccess(string elementId, int startIndex, int length)
        {
            // Act
            var result = await _worker.SelectTextAsync(elementId, startIndex, length);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Mock text selected", result.Data);
        }

        [Theory]
        [InlineData("TestWindow", null, 3)]
        [InlineData(null, 1234, 5)]
        [InlineData("", null, 1)]
        public async Task GetElementTreeAsync_ShouldReturnSuccess(string? windowTitle, int? processId, int maxDepth)
        {
            // Act
            var result = await _worker.GetElementTreeAsync(windowTitle, processId, maxDepth);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("MockWindow", result.Data["Name"]);
        }

        [Theory]
        [InlineData("window1", "minimize")]
        [InlineData("window2", "maximize")]
        [InlineData("window3", "normal")]
        [InlineData("window4", "close")]
        public async Task SetWindowStateAsync_ShouldReturnSuccess(string elementId, string state)
        {
            // Act
            var result = await _worker.SetWindowStateAsync(elementId, state);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Mock window state set", result.Data);
        }
    }
}
