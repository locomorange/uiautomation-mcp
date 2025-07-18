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
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("Element not found"));

        // Act
        var result = await _service.StartListeningAsync(
            "NonExistentElement",
            "MouseLeftButtonDown",
            windowTitle: "TestWindow",
            processId: 1234,
            timeoutSeconds: 1);

        // Assert
        Assert.NotNull(result);
        dynamic resultObj = result;
        Assert.False(resultObj.Success);
        Assert.Contains("not found", resultObj.Error);
    }

    [Fact]
    public async Task StartListeningAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(new object()));

        // Act
        var result = await _service.StartListeningAsync(
            "button1",
            "MouseLeftButtonDown",
            windowTitle: "MyApp",
            processId: 5678,
            timeoutSeconds: 30);

        // Assert
        Assert.NotNull(result);
        dynamic resultObj = result;
        Assert.True(resultObj.Success);
        Assert.Equal("Synchronized input listening started", resultObj.Message);
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "StartSynchronizedInput",
            It.IsAny<object>(),
            30), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_AfterStartListening_ShouldCancelSuccessfully()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(new object()));

        // Act
        var result = await _service.CancelAsync("testElement", timeoutSeconds: 10);

        // Assert
        Assert.NotNull(result);
        dynamic resultObj = result;
        Assert.True(resultObj.Success);
        Assert.Equal("Synchronized input canceled", resultObj.Message);
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

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(new object()));

        // Act & Assert
        foreach (var inputType in inputTypes)
        {
            var result = await _service.StartListeningAsync(
                "testElement",
                inputType,
                timeoutSeconds: 10);

            Assert.NotNull(result);
        }

        // Verify the mock was called exactly 6 times (once per input type)
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "StartSynchronizedInput",
            It.IsAny<object>(),
            10), Times.Exactly(6));
    }

    [Fact]
    public async Task StartListeningAsync_PatternNotSupported_ShouldReturnError()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("SynchronizedInputPattern is not supported by this element"));

        // Act
        var result = await _service.StartListeningAsync(
            "unsupportedElement",
            "MouseLeftButtonDown",
            timeoutSeconds: 15);

        // Assert
        Assert.NotNull(result);
        dynamic resultObj = result;
        Assert.False(resultObj.Success);
        Assert.Contains("not supported", resultObj.Error);
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
            .Returns(Task.FromResult<object>(new object()));

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                "CancelSynchronizedInput",
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult<object>(new object()));

        // Act - Simulate multiple start/cancel cycles
        for (int i = 0; i < 3; i++)
        {
            var startResult = await _service.StartListeningAsync(
                $"element_{i}",
                "KeyDown",
                timeoutSeconds: 5);
            
            Assert.NotNull(startResult);
            dynamic startObj = startResult;
            Assert.True(startObj.Success);

            var cancelResult = await _service.CancelAsync($"element_{i}", timeoutSeconds: 5);
            Assert.NotNull(cancelResult);
            dynamic cancelObj = cancelResult;
            Assert.True(cancelObj.Success);
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