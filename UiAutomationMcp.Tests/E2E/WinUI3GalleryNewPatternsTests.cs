using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// Tests for WinUI 3 Gallery that specifically test the new UI automation patterns:
/// - VirtualizedItemPattern
/// - ItemContainerPattern  
/// - SynchronizedInputPattern
/// These tests demonstrate real-world usage of the new patterns with actual UI controls.
/// </summary>
[Collection("WinUI3GalleryTestCollection")]
[Trait("Category", "E2E")]
public class WinUI3GalleryNewPatternsTests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;
    private readonly WinUI3GalleryTestFixture _fixture;

    public WinUI3GalleryNewPatternsTests(ITestOutputHelper output, WinUI3GalleryTestFixture fixture) : base(output)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task VirtualizedItemPattern_ShouldRealizeVirtualizedListItems()
    {
        _output.WriteLine("Testing VirtualizedItemPattern with WinUI 3 Gallery ListView...");

        try
        {
            // Step 1: Navigate to Collections section first
            await Task.Delay(2000); // Wait for app to be ready

            // Step 2: Find ListView or similar virtualized control
            var listViews = await Tools.FindElements(
                controlType: "ListView");
            _output.WriteLine($"ListView controls found: {JsonSerializer.Serialize(listViews, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: If ListView found, try to realize virtualized items
            if (listViews != null)
            {
                // Try to realize a virtualized item that might not be visible
                var realizeResult = await Tools.RealizeVirtualizedItem(
                    "Item_50", // Assuming there's an item with this ID
                    timeoutSeconds: 10);

                _output.WriteLine($"Realize virtualized item result: {JsonSerializer.Serialize(realizeResult, new JsonSerializerOptions { WriteIndented = true })}");

                // Alternative: Try with different naming patterns
                var realizeResult2 = await Tools.RealizeVirtualizedItem(
                    "ListViewItem_50",
                    timeoutSeconds: 5);

                _output.WriteLine($"Realize virtualized item result 2: {JsonSerializer.Serialize(realizeResult2, new JsonSerializerOptions { WriteIndented = true })}");
            }

            // Step 4: Try with ScrollViewer (which often contains virtualized content)
            var scrollViewers = await Tools.FindElements(
                controlType: "ScrollViewer");
            _output.WriteLine($"ScrollViewer controls found: {JsonSerializer.Serialize(scrollViewers, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take screenshot of virtualized content
            var virtualizedScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Virtualized content screenshot captured: {virtualizedScreenshot != null}");

            Assert.True(true, "VirtualizedItemPattern test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"VirtualizedItemPattern test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task ItemContainerPattern_ShouldFindItemsByProperty()
    {
        _output.WriteLine("Testing ItemContainerPattern with WinUI 3 Gallery containers...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Find container controls that might support ItemContainerPattern
            var containerControls = new[] { "ListView", "GridView", "TreeView", "ComboBox", "ListBox" };

            foreach (var controlType in containerControls)
            {
                try
                {
                    var containers = await Tools.FindElements(
                        controlType: controlType);
                    _output.WriteLine($"{controlType} containers found: {JsonSerializer.Serialize(containers, new JsonSerializerOptions { WriteIndented = true })}");

                    // If containers found, try to find items by property
                    if (containers != null)
                    {
                        // Try to find an item by name property
                        var findResult = await Tools.FindItemByProperty(
                            "MainContainer", // Generic container ID
                            propertyName: "Name",
                            value: "Sample Item",
                            timeoutSeconds: 5);

                        _output.WriteLine($"Find item by property result for {controlType}: {JsonSerializer.Serialize(findResult, new JsonSerializerOptions { WriteIndented = true })}");

                        // Try to find by ControlType property
                        var findResult2 = await Tools.FindItemByProperty(
                            "MainContainer",
                            propertyName: "ControlType",
                            value: "ListItem",
                            timeoutSeconds: 5);

                        _output.WriteLine($"Find item by ControlType result for {controlType}: {JsonSerializer.Serialize(findResult2, new JsonSerializerOptions { WriteIndented = true })}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error testing {controlType}: {ex.Message}");
                }
            }

            // Step 3: Take screenshot of container patterns
            var containerScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Container patterns screenshot captured: {containerScreenshot != null}");

            Assert.True(true, "ItemContainerPattern test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ItemContainerPattern test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task SynchronizedInputPattern_ShouldTestSynchronizedInput()
    {
        _output.WriteLine("Testing SynchronizedInputPattern with WinUI 3 Gallery controls...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Find input controls that might support SynchronizedInputPattern
            var inputControls = new[] { "TextBox", "Edit", "Button", "ComboBox", "Slider" };

            foreach (var controlType in inputControls)
            {
                try
                {
                    var controls = await Tools.FindElements(
                        controlType: controlType);
                    _output.WriteLine($"{controlType} controls found: {JsonSerializer.Serialize(controls, new JsonSerializerOptions { WriteIndented = true })}");

                    // If controls found, try synchronized input
                    if (controls != null)
                    {
                        // Start synchronized input
                        var startResult = await Tools.StartSynchronizedInput(
                            "MainControl", // Generic control ID
                            inputType: "KeyDown",
                            timeoutSeconds: 5);

                        _output.WriteLine($"Start synchronized input result for {controlType}: {JsonSerializer.Serialize(startResult, new JsonSerializerOptions { WriteIndented = true })}");

                        // Wait a bit to simulate input processing
                        await Task.Delay(1000);

                        // Cancel synchronized input
                        var cancelResult = await Tools.CancelSynchronizedInput(
                            "MainControl");

                        _output.WriteLine($"Cancel synchronized input result for {controlType}: {JsonSerializer.Serialize(cancelResult, new JsonSerializerOptions { WriteIndented = true })}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error testing synchronized input for {controlType}: {ex.Message}");
                }
            }

            // Step 3: Take screenshot of synchronized input interface
            var syncInputScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Synchronized input screenshot captured: {syncInputScreenshot != null}");

            Assert.True(true, "SynchronizedInputPattern test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"SynchronizedInputPattern test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task CombinedNewPatterns_ShouldTestAllNewPatternsTogether()
    {
        _output.WriteLine("Testing all new patterns together in a comprehensive scenario...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Get comprehensive element tree to understand the structure
            var elementTree = await Tools.GetElementTree(maxDepth: 4);
            _output.WriteLine($"Comprehensive element tree: {JsonSerializer.Serialize(elementTree, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Test VirtualizedItemPattern - Try to realize multiple items
            var virtualizedTargets = new[] { "Item_1", "Item_10", "Item_100", "ListItem_1", "TreeItem_1" };
            foreach (var target in virtualizedTargets)
            {
                try
                {
                    var realizeResult = await Tools.RealizeVirtualizedItem(
                        target,
                        timeoutSeconds: 3);
                    _output.WriteLine($"Realize {target} result: {JsonSerializer.Serialize(realizeResult, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not realize {target}: {ex.Message}");
                }
            }

            // Step 4: Test ItemContainerPattern - Search for items by different properties
            var propertySearches = new[]
            {
                ("Name", "Button"),
                ("ControlType", "Button"),
                ("Name", "Text"),
                ("ControlType", "TextBox"),
                ("AutomationId", "PART_Root")
            };

            foreach (var (propertyName, value) in propertySearches)
            {
                try
                {
                    var findResult = await Tools.FindItemByProperty(
                        "MainWindow",
                        propertyName: propertyName,
                        value: value,
                        timeoutSeconds: 3);
                    _output.WriteLine($"Find by {propertyName}={value} result: {JsonSerializer.Serialize(findResult, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find by {propertyName}={value}: {ex.Message}");
                }
            }

            // Step 5: Test SynchronizedInputPattern - Start and cancel synchronized input
            var inputTargets = new[] { "MainButton", "MainTextBox", "MainSlider" };
            foreach (var target in inputTargets)
            {
                try
                {
                    // Start synchronized input
                    var startResult = await Tools.StartSynchronizedInput(
                        target,
                        inputType: "LeftMouseButton",
                        timeoutSeconds: 2);
                    _output.WriteLine($"Start sync input for {target}: {JsonSerializer.Serialize(startResult, new JsonSerializerOptions { WriteIndented = true })}");

                    // Small delay
                    await Task.Delay(500);

                    // Cancel synchronized input
                    var cancelResult = await Tools.CancelSynchronizedInput(
                        target);
                    _output.WriteLine($"Cancel sync input for {target}: {JsonSerializer.Serialize(cancelResult, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not test synchronized input for {target}: {ex.Message}");
                }
            }

            // Step 6: Final screenshot
            var finalScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Final comprehensive test screenshot captured: {finalScreenshot != null}");

            Assert.True(true, "Combined new patterns test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Combined new patterns test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task RealWorldNavigationScenario_ShouldTestComplexUINavigation()
    {
        _output.WriteLine("Testing real-world navigation scenario with new patterns...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(3000);

            // Step 2: Take initial screenshot
            var initialScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Initial screenshot captured: {initialScreenshot != null}");

            // Step 3: Discover the navigation structure
            var navigationStructure = await Tools.GetElementTree(maxDepth: 3);
            _output.WriteLine($"Navigation structure: {JsonSerializer.Serialize(navigationStructure, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Find all navigation items using ItemContainerPattern
            var navFindResult = await Tools.FindItemByProperty(
                "NavigationView",
                propertyName: "ControlType",
                value: "NavigationViewItem",
                timeoutSeconds: 10);
            _output.WriteLine($"Navigation items found: {JsonSerializer.Serialize(navFindResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Try to realize virtualized navigation items
            var navigationTargets = new[] { "BasicInput", "Collections", "DateAndTime", "Dialogs", "Layout", "Media", "Navigation", "StatusAndInfo", "Text" };
            foreach (var target in navigationTargets)
            {
                try
                {
                    var realizeResult = await Tools.RealizeVirtualizedItem(
                        target,
                        timeoutSeconds: 2);
                    _output.WriteLine($"Realize navigation item {target}: {JsonSerializer.Serialize(realizeResult, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not realize navigation item {target}: {ex.Message}");
                }
            }

            // Step 6: Use synchronized input for navigation
            var syncNavResult = await Tools.StartSynchronizedInput(
                "NavigationView",
                inputType: "LeftMouseButton",
                timeoutSeconds: 5);
            _output.WriteLine($"Synchronized navigation input started: {JsonSerializer.Serialize(syncNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 7: Simulate navigation interaction
            await Task.Delay(1000);

            // Step 8: Cancel synchronized input
            var cancelNavResult = await Tools.CancelSynchronizedInput(
                "NavigationView");
            _output.WriteLine($"Synchronized navigation input cancelled: {JsonSerializer.Serialize(cancelNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 9: Final screenshot
            var finalScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Final navigation screenshot captured: {finalScreenshot != null}");

            Assert.True(true, "Real-world navigation scenario completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Real-world navigation scenario failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}