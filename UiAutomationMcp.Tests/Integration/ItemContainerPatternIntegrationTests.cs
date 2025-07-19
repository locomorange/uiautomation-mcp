using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using Xunit;

namespace UiAutomationMcp.Tests.Integration;

public class ItemContainerPatternIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IItemContainerService _service;
    private readonly Mock<ISubprocessExecutor> _mockSubprocessExecutor;
    private readonly Mock<ILogger<ItemContainerService>> _mockLogger;

    public ItemContainerPatternIntegrationTests()
    {
        var services = new ServiceCollection();

        _mockSubprocessExecutor = new Mock<ISubprocessExecutor>();
        _mockLogger = new Mock<ILogger<ItemContainerService>>();

        services.AddSingleton(_mockSubprocessExecutor.Object);
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton<IItemContainerService, ItemContainerService>();

        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IItemContainerService>();
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
    {
        // Arrange
        var errorResponse = new ElementSearchResult
        {
            Success = false,
            ErrorMessage = "Container element not found"
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(errorResponse));

        // Act
        var result = await _service.FindItemByPropertyAsync(
            "NonExistentContainer",
            "Name",
            "TestItem",
            windowTitle: "TestWindow",
            processId: 1234,
            timeoutSeconds: 1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        var successResponse = new ElementSearchResult
        {
            Success = true,
            Elements = new List<UIAutomationMCP.Shared.ElementInfo>
            {
                new UIAutomationMCP.Shared.ElementInfo
                {
                    AutomationId = "item_1",
                    Name = "Item1",
                    ClassName = "ListItem",
                    ControlType = "ListItem"
                }
            }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(successResponse));

        // Act
        var result = await _service.FindItemByPropertyAsync(
            "ListContainer",
            "Name",
            "Item1",
            windowTitle: "MyApp",
            processId: 5678,
            timeoutSeconds: 30);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Elements);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WithDifferentPropertyTypes_ShouldPassCorrectly()
    {
        // Arrange
        var propertyTypes = new[]
        {
            ("AutomationId", "btn_submit"),
            ("ClassName", "Button"),
            ("ControlType", "Button")
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(new ElementSearchResult { Success = true }));

        // Act & Assert
        foreach (var (propertyName, value) in propertyTypes)
        {
            var result = await _service.FindItemByPropertyAsync(
                "FormContainer",
                propertyName,
                value,
                timeoutSeconds: 10);

            Assert.NotNull(result);
        }

        // Verify the mock was called exactly 3 times (once per property type)
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<ElementSearchResult>(
            "FindItemByProperty",
            It.IsAny<object>(),
            10), Times.Exactly(3));
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WithNullPropertyValue_ShouldFindItemsWithNullProperty()
    {
        // Arrange
        var response = new ElementSearchResult
        {
            Success = true,
            Elements = new List<UIAutomationMCP.Shared.ElementInfo>
            {
                new UIAutomationMCP.Shared.ElementInfo
                {
                    AutomationId = "unnamed_node",
                    Name = null!,
                    ClassName = "TreeNode",
                    ControlType = "TreeItem"
                }
            }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(Task.FromResult(response));

        // Act
        var result = await _service.FindItemByPropertyAsync(
            "TreeContainer",
            "Name",
            null,
            timeoutSeconds: 15);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WorkerProcessLifecycle_ShouldStartAndStop()
    {
        // Arrange
        var callCount = 0;
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<ElementSearchResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .Returns(() => Task.FromResult(new ElementSearchResult
            {
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { AutomationId = $"item_{++callCount}" }
                }
            }));

        // Act - Multiple sequential calls
        for (int i = 0; i < 3; i++)
        {
            var result = await _service.FindItemByPropertyAsync(
                "Container",
                "AutomationId",
                $"item_{i}",
                timeoutSeconds: 5);

            Assert.NotNull(result);
        }

        // Assert
        Assert.Equal(3, callCount);
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<ElementSearchResult>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<int>()), Times.Exactly(3));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}