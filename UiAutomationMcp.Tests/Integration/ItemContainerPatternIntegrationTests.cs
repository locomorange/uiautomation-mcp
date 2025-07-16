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
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.False((bool)resultDict["Success"]);
        Assert.Contains("not found", (string)resultDict["ErrorMessage"]);
    }

    [Fact]
    public async Task FindItemByPropertyAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
    {
        // Arrange
        var successResponse = new Dictionary<string, object>
        {
            { "Success", true },
            { "ElementInfo", new Dictionary<string, object>
                {
                    { "AutomationId", "item_1" },
                    { "Name", "Item1" },
                    { "ClassName", "ListItem" },
                    { "ControlType", "ListItem" }
                }
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
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.True((bool)resultDict["Success"]);
        Assert.NotNull(resultDict["ElementInfo"]);
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

        foreach (var (propertyName, value) in propertyTypes)
        {
            _mockSubprocessExecutor
                .Setup(x => x.ExecuteAsync<object>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<string, object> { { "Success", true } });

            // Act
            var result = await _service.FindItemByPropertyAsync(
                "FormContainer",
                propertyName,
                value,
                timeoutSeconds: 10);

            // Assert
            Assert.NotNull(result);
            _mockSubprocessExecutor.Verify(x => x.ExecuteAsync<object>(
                "FindItemByProperty",
                It.IsAny<object>(),
                10), Times.Once);
        }
    }

    [Fact]
    public async Task FindItemByPropertyAsync_WithNullPropertyValue_ShouldFindItemsWithNullProperty()
    {
        // Arrange
        var response = new Dictionary<string, object>
        {
            { "Success", true },
            { "ElementInfo", new Dictionary<string, object>
                {
                    { "AutomationId", "unnamed_node" },
                    { "Name", null! },
                    { "ClassName", "TreeNode" },
                    { "ControlType", "TreeItem" }
                }
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
        var resultDict = result as Dictionary<string, object>;
        Assert.NotNull(resultDict);
        Assert.True((bool)resultDict["Success"]);
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
                return new Dictionary<string, object>
                {
                    { "Success", true },
                    { "ElementInfo", new Dictionary<string, object> { { "AutomationId", $"item_{callCount}" } } }
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