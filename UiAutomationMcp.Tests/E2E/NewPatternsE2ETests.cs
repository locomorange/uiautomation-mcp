using System.Diagnostics;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// End-to-end tests for the newly implemented UI Automation patterns.
/// These tests require real Windows applications to be available.
/// </summary>
[Collection("UIAutomation")]
[Trait("Category", "E2E")]
public class NewPatternsE2ETests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;

    public NewPatternsE2ETests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    #region VirtualizedItemPattern Tests

    [Fact(Skip = "Requires Windows Explorer or a virtualized list application")]
    public async Task VirtualizedItemPattern_RealizeItem_InFileExplorer()
    {
        // This test would require:
        // 1. Opening Windows Explorer
        // 2. Navigating to a folder with many files (to trigger virtualization)
        // 3. Finding a virtualized item
        // 4. Realizing it
        // 5. Verifying the item is now accessible

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            // Arrange
            var explorerProcess = Process.Start("explorer.exe");
            await Task.Delay(2000); // Wait for Explorer to open

            // Act
            var result = await Tools.RealizeVirtualizedItem(
                automationId: "VirtualizedListItem",
                timeoutSeconds: 10);

            // Assert
            _output.WriteLine($"Realize result: {System.Text.Json.JsonSerializer.Serialize(result)}");
            // In a real scenario, we would verify the item is now visible

            return explorerProcess;
        }, _output, "File Explorer", 6000);
    }

    [Fact(Skip = "Requires a WPF application with virtualized DataGrid")]
    public async Task VirtualizedItemPattern_RealizeItem_InDataGrid()
    {
        // This test scenario would:
        // 1. Launch a test WPF application with a virtualized DataGrid
        // 2. Find a virtualized row/cell
        // 3. Realize it
        // 4. Verify it's now in the UI tree

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            // Example implementation:
            var testAppPath = @"C:\TestApps\VirtualizedDataGridApp.exe";
            var testApp = Process.Start(testAppPath);
            await Task.Delay(2000);

            // Find a virtualized item in the DataGrid
            var result = await Tools.RealizeVirtualizedItem(
                "DataGridRow_100", // A row that's likely virtualized
                timeoutSeconds: 15);

            // Verify the item is now accessible
            var elementInfo = await Tools.SearchElements();

            Assert.NotNull(elementInfo);
            // In a real scenario, we would verify the specific element is now in the tree

            return testApp;
        }, _output, "DataGrid Test App", 8000);
    }

    #endregion

    #region Performance and Stress Tests

    [Fact(Skip = "Requires a large dataset application")]
    public async Task VirtualizedItemPattern_Performance_LargeDataset()
    {
        // Test performance with large virtualized lists
        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var testApp = Process.Start(@"C:\TestApps\LargeDatasetApp.exe");
            await Task.Delay(3000);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Realize multiple items in sequence
            for (int i = 0; i < 10; i++)
            {
                var result = await Tools.RealizeVirtualizedItem(
                    $"Row_{i * 100}",
                    timeoutSeconds: 5);

                var resultDict = result as Dictionary<string, object>;
                Assert.NotNull(resultDict);
                Assert.True((bool)resultDict["Success"]);
            }

            stopwatch.Stop();
            _output.WriteLine($"Realized 10 items in {stopwatch.ElapsedMilliseconds}ms");

            // Performance should be reasonable even with virtualization
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "Realization took too long");

            return testApp;
        }, _output, "Large Dataset App", 12000);
    }

    #endregion
}

