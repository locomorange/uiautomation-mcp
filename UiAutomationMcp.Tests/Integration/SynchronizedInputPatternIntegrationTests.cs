using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Requests;
using Xunit;

namespace UiAutomationMcp.Tests.Integration;

public class SynchronizedInputPatternIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ISynchronizedInputService _service;
    private readonly Mock<ISubprocessExecutor> _mockSubprocessExecutor;
    private readonly Mock<ILogger<SynchronizedInputService>> _mockLogger;

    public SynchronizedInputPatternIntegrationTests()
    {
        var services = new ServiceCollection();

        _mockSubprocessExecutor = new Mock<ISubprocessExecutor>();
        _mockLogger = new Mock<ILogger<SynchronizedInputService>>();

        services.AddSingleton(_mockSubprocessExecutor.Object);
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton<ISynchronizedInputService, SynchronizedInputService>();

        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<ISynchronizedInputService>();
    }

    [Fact]
    public async Task StartListeningAsync_WithNonExistentElement_ShouldHandleGracefully()
    {
        // Arrange
        var errorResponse = new { Success = false, ErrorMessage = "Element not found", Result = false };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _service.StartListeningAsync(
            "NonExistentElement",
            "MouseLeftButtonDown",
            windowTitle: "TestWindow",
            processId: 1234,
            timeoutSeconds: 1);

        // Assert
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.False((bool)resultDict["Success"]);
        Assert.Contains("not found", (string)resultDict["ErrorMessage"]);
    }

    [Fact]
    public async Task StartListeningAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        var successResponse = new Dictionary<string, object>
        {
            { "Success", true },
            { "Result", true }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _service.StartListeningAsync(
            "button1",
            "MouseLeftButtonDown",
            windowTitle: "MyApp",
            processId: 5678,
            timeoutSeconds: 30);

        // Assert
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.True((bool)resultDict["Success"]);
        Assert.True((bool)resultDict["Result"]);
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "StartSynchronizedInput",
            It.IsAny<object>(),
            30), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_AfterStartListening_ShouldCancelSuccessfully()
    {
        // Arrange
        var cancelResponse = new Dictionary<string, object>
        {
            { "Success", true },
            { "Result", true }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(cancelResponse);

        // Act
        var result = await _service.CancelAsync("testElement", timeoutSeconds: 10);

        // Assert
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.True((bool)resultDict["Success"]);
        Assert.True((bool)resultDict["Result"]);
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "CancelSynchronizedInput",
            It.IsAny<object>(),
            10), Times.Once);
    }

    [Fact]
    public async Task StartListeningAsync_WithDifferentInputTypes_ShouldPassCorrectly()
    {
        // Arrange
        var inputTypes = new[]
        {
            "KeyUp",
            "KeyDown",
            "MouseLeftButtonDown",
            "MouseLeftButtonUp",
            "MouseRightButtonDown",
            "MouseRightButtonUp"
        };

        foreach (var inputType in inputTypes)
        {
            _mockSubprocessExecutor
                .Setup(x => x.ExecuteAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<string, object> { { "Success", true }, { "Result", true } });

            // Act
            var result = await _service.StartListeningAsync(
                "testElement",
                inputType,
                timeoutSeconds: 10);

            // Assert
            Assert.NotNull(result);
            _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
                "StartSynchronizedInput",
                It.IsAny<object>(),
                10), Times.Once);
        }
    }

    [Fact]
    public async Task StartListeningAsync_PatternNotSupported_ShouldReturnError()
    {
        // Arrange
        var errorResponse = new Dictionary<string, object>
        {
            { "Success", false },
            { "ErrorMessage", "SynchronizedInputPattern is not supported by this element" },
            { "Result", false }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _service.StartListeningAsync(
            "unsupportedElement",
            "MouseLeftButtonDown",
            timeoutSeconds: 15);

        // Assert
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.False((bool)resultDict["Success"]);
        Assert.Contains("not supported", (string)resultDict["ErrorMessage"]);
    }

    [Fact]
    public async Task WorkerProcessLifecycle_MultipleOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var startCallCount = 0;
        var cancelCallCount = 0;

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                "StartSynchronizedInput",
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                startCallCount++;
                return new Dictionary<string, object> { { "Success", true }, { "Result", true } };
            });

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                "CancelSynchronizedInput",
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                cancelCallCount++;
                return new Dictionary<string, object> { { "Success", true }, { "Result", true } };
            });

        // Act - Simulate multiple start/cancel cycles
        for (int i = 0; i < 3; i++)
        {
            var startResult = await _service.StartListeningAsync(
                $"element_{i}",
                "KeyDown",
                timeoutSeconds: 5);
            
            var startDict = startResult as Dictionary<string, object>;
            Assert.NotNull(startDict);
            Assert.True((bool)startDict["Success"]);

            var cancelResult = await _service.CancelAsync($"element_{i}", timeoutSeconds: 5);
            var cancelDict = cancelResult as Dictionary<string, object>;
            Assert.NotNull(cancelDict);
            Assert.True((bool)cancelDict["Success"]);
        }

        // Assert
        Assert.Equal(3, startCallCount);
        Assert.Equal(3, cancelCallCount);
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "StartSynchronizedInput",
            It.IsAny<object>(),
            It.IsAny<int>()), Times.Exactly(3));
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "CancelSynchronizedInput",
            It.IsAny<object>(),
            It.IsAny<int>()), Times.Exactly(3));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}