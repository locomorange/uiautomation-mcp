using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Requests;
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
        var errorResponse = new { Success = false, ErrorMessage = "Container element not found" };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(errorResponse);

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
        dynamic resultObj = result;
        Assert.False(resultObj.Success);
        Assert.Contains("not found", resultObj.ErrorMessage);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        var successResponse = new {
            Success = true,
            ElementInfo = new {
                AutomationId = "item_1",
                Name = "Item1",
                ClassName = "ListItem",
                ControlType = "ListItem"
            }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(successResponse);

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
        dynamic resultObj = result;
        Assert.True(resultObj.Success);
        Assert.NotNull(resultObj.ElementInfo);
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
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(new { Success = true });

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
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            "FindItemByProperty",
            It.IsAny<object>(),
            10), Times.Exactly(3));
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WithNullPropertyValue_ShouldFindItemsWithNullProperty()
    {
        // Arrange
        var response = new {
            Success = true,
            ElementInfo = new {
                AutomationId = "unnamed_node",
                Name = (string?)null,
                ClassName = "TreeNode",
                ControlType = "TreeItem"
            }
        };

        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.FindItemByPropertyAsync(
            "TreeContainer",
            "Name",
            null,
            timeoutSeconds: 15);

        // Assert
        Assert.NotNull(result);
        dynamic resultObj = result;
        Assert.True(resultObj.Success);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WorkerProcessLifecycle_ShouldStartAndStop()
    {
        // Arrange
        var callCount = 0;
        _mockSubprocessExecutor
            .Setup(x => x.ExecuteAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new {
                    Success = true,
                    ElementInfo = new { AutomationId = $"item_{callCount}" }
                };
            });

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
        _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<int>()), Times.Exactly(3));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}