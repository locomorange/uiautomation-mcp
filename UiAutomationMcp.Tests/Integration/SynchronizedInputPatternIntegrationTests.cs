using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using Xunit;
using UIAutomationMCP.Models.Abstractions;

namespace UiAutomationMcp.Tests.Integration;

[Trait("Category", "Integration")]
public class SynchronizedInputPatternIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ISynchronizedInputService _service;
    private readonly Mock<IProcessManager> _mockSubprocessExecutor;
    private readonly Mock<ILogger<SynchronizedInputService>> _mockLogger;

    public SynchronizedInputPatternIntegrationTests()
    {
        var services = new ServiceCollection();

        _mockSubprocessExecutor = new Mock<IProcessManager>();
        _mockLogger = new Mock<ILogger<SynchronizedInputService>>();

        services.AddSingleton<IProcessManager>(_mockSubprocessExecutor.Object);
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
            .Setup(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<StartSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("Element not found"));

        // Act
        var result = await _service.StartListeningAsync(
            "NonExistentElement",
            null,
            "MouseLeftButtonDown",
            timeoutSeconds: 1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task StartListeningAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<StartSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult())));

        // Act
        var result = await _service.StartListeningAsync(
            "button1",
            inputType: "MouseLeftButtonDown",
            timeoutSeconds: 30);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _mockSubprocessExecutor.Verify(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
            "StartSynchronizedInput",
            It.IsAny<StartSynchronizedInputRequest>(),
            30), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_AfterStartListening_ShouldCancelSuccessfully()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteWorkerOperationAsync<CancelSynchronizedInputRequest, ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<CancelSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult())));

        // Act
        var result = await _service.CancelAsync("testElement", timeoutSeconds: 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _mockSubprocessExecutor.Verify(x => x.ExecuteWorkerOperationAsync<CancelSynchronizedInputRequest, ElementSearchResult>(
            "CancelSynchronizedInput",
            It.IsAny<CancelSynchronizedInputRequest>(),
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
            .Setup(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<StartSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult())));

        // Act & Assert
        foreach (var inputType in inputTypes)
        {
            var result = await _service.StartListeningAsync(
                "testElement",
                inputType: inputType,
                timeoutSeconds: 10);

            Assert.NotNull(result);
        }

        // Verify the mock was called exactly 6 times (once per input type)
        _mockSubprocessExecutor.Verify(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
            "StartSynchronizedInput",
            It.IsAny<StartSynchronizedInputRequest>(),
            10), Times.Exactly(6));
    }

    [Fact]
    public async Task StartListeningAsync_PatternNotSupported_ShouldReturnError()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<StartSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("SynchronizedInputPattern is not supported by this element"));

        // Act
        var result = await _service.StartListeningAsync(
            "unsupportedElement",
            inputType: "MouseLeftButtonDown",
            timeoutSeconds: 15);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("not supported", result.ErrorMessage);
    }

    [Fact]
    public async Task WorkerProcessLifecycle_MultipleOperations_ShouldHandleCorrectly()
    {
        // Arrange
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                "StartSynchronizedInput",
                It.IsAny<StartSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult())));

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteWorkerOperationAsync<CancelSynchronizedInputRequest, ElementSearchResult>(
                "CancelSynchronizedInput",
                It.IsAny<CancelSynchronizedInputRequest>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult())));

        // Act - Simulate multiple start/cancel cycles
        for (int i = 0; i < 3; i++)
        {
            var startResult = await _service.StartListeningAsync(
                $"element_{i}",
                inputType: "KeyDown",
                timeoutSeconds: 5);

            Assert.NotNull(startResult);
            Assert.True(startResult.Success);

            var cancelResult = await _service.CancelAsync($"element_{i}", timeoutSeconds: 5);
            Assert.NotNull(cancelResult);
            Assert.True(cancelResult.Success);
        }

        // Assert
        _mockSubprocessExecutor.Verify(x => x.ExecuteWorkerOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
            "StartSynchronizedInput",
            It.IsAny<StartSynchronizedInputRequest>(),
            It.IsAny<int>()), Times.Exactly(3));
        _mockSubprocessExecutor.Verify(x => x.ExecuteWorkerOperationAsync<CancelSynchronizedInputRequest, ElementSearchResult>(
            "CancelSynchronizedInput",
            It.IsAny<CancelSynchronizedInputRequest>(),
            It.IsAny<int>()), Times.Exactly(3));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

