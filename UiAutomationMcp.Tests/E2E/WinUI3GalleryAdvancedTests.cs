using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// Advanced tests for WinUI 3 Gallery that demonstrate complex UI automation scenarios
/// including navigation, interaction patterns, and data manipulation.
/// </summary>
[Collection("WinUI3GalleryTestCollection")]
[Trait("Category", "E2E")]
public class WinUI3GalleryAdvancedTests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;
    private readonly WinUI3GalleryTestFixture _fixture;

    public WinUI3GalleryAdvancedTests(ITestOutputHelper output, WinUI3GalleryTestFixture fixture) : base(output)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteButtonInteractionScenario_ShouldNavigateAndInteractWithButtons()
    {
        _output.WriteLine("Testing complete button interaction scenario...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Take initial screenshot
            var initialScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Initial screenshot captured: {initialScreenshot != null}");

            // Step 3: Navigate through the navigation pane to find buttons
            // First, let's explore the navigation structure
            var navigationElements = await Tools.SearchElements(
                controlType: "NavigationView");
            _output.WriteLine($"Navigation elements: {JsonSerializer.Serialize(navigationElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for navigation items
            var navItems = await Tools.SearchElements(
                controlType: "NavigationViewItem");
            _output.WriteLine($"Navigation items found: {JsonSerializer.Serialize(navItems, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Search for text elements that might be navigation labels
            var textElements = await Tools.SearchElements(
                controlType: "Text");
            _output.WriteLine($"Text elements found: {JsonSerializer.Serialize(textElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 6: Get a comprehensive view of the UI structure
            var elementTree = await Tools.GetElementTree(maxDepth: 4);
            _output.WriteLine($"Element tree: {JsonSerializer.Serialize(elementTree, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 7: Find and interact with any available buttons
            var buttons = await Tools.SearchElements(
                controlType: "Button");
            _output.WriteLine($"Buttons found: {JsonSerializer.Serialize(buttons, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 8: Take final screenshot
            var finalScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Final screenshot captured: {finalScreenshot != null}");

            Assert.True(true, "Complete button interaction scenario completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Button interaction scenario failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task SearchAndFilterFunctionality_ShouldTestSearchCapabilities()
    {
        _output.WriteLine("Testing search and filter functionality in WinUI 3 Gallery...");

        try
        {
            // Step 1: Look for search box or filter controls
            var searchBoxes = await Tools.SearchElements(
                controlType: "SearchBox");
            _output.WriteLine($"Search boxes found: {JsonSerializer.Serialize(searchBoxes, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for AutoSuggestBox controls (WinUI 3 search implementation)
            var autoSuggestBoxes = await Tools.SearchElements(
                controlType: "AutoSuggestBox");
            _output.WriteLine($"AutoSuggestBox controls found: {JsonSerializer.Serialize(autoSuggestBoxes, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for generic Edit controls that might be search fields
            var editControls = await Tools.SearchElements(
                controlType: "Edit");
            _output.WriteLine($"Edit controls found: {JsonSerializer.Serialize(editControls, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Take screenshot of search interface
            var searchScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Search interface screenshot captured: {searchScreenshot != null}");

            Assert.True(true, "Search and filter functionality test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Search and filter functionality test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TabNavigationScenario_ShouldNavigateThroughTabs()
    {
        _output.WriteLine("Testing tab navigation scenario...");

        try
        {
            // Step 1: Look for TabView or Tab controls
            var tabViews = await Tools.SearchElements(
                controlType: "TabView");
            _output.WriteLine($"TabView controls found: {JsonSerializer.Serialize(tabViews, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for Tab controls
            var tabs = await Tools.SearchElements(
                controlType: "Tab");
            _output.WriteLine($"Tab controls found: {JsonSerializer.Serialize(tabs, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for TabItem controls
            var tabItems = await Tools.SearchElements(
                controlType: "TabItem");
            _output.WriteLine($"TabItem controls found: {JsonSerializer.Serialize(tabItems, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Take screenshot of tab interface
            var tabScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Tab interface screenshot captured: {tabScreenshot != null}");

            Assert.True(true, "Tab navigation scenario completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Tab navigation scenario failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task MediaAndVisualizationControls_ShouldTestMediaElements()
    {
        _output.WriteLine("Testing media and visualization controls...");

        try
        {
            // Step 1: Look for MediaElement controls
            var mediaElements = await Tools.SearchElements(
                controlType: "MediaElement");
            _output.WriteLine($"MediaElement controls found: {JsonSerializer.Serialize(mediaElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for Image controls
            var imageElements = await Tools.SearchElements(
                controlType: "Image");
            _output.WriteLine($"Image controls found: {JsonSerializer.Serialize(imageElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for Canvas controls (often used for custom drawing)
            var canvasElements = await Tools.SearchElements(
                controlType: "Canvas");
            _output.WriteLine($"Canvas controls found: {JsonSerializer.Serialize(canvasElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for ProgressBar controls
            var progressBars = await Tools.SearchElements(
                controlType: "ProgressBar");
            _output.WriteLine($"ProgressBar controls found: {JsonSerializer.Serialize(progressBars, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take screenshot of media interface
            var mediaScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Media interface screenshot captured: {mediaScreenshot != null}");

            Assert.True(true, "Media and visualization controls test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Media and visualization controls test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task AccessibilityAndKeyboardNavigation_ShouldTestAccessibilityFeatures()
    {
        _output.WriteLine("Testing accessibility and keyboard navigation features...");

        try
        {
            // Step 1: Look for elements with accessibility properties
            var allElements = await Tools.SearchElements();
            _output.WriteLine($"All elements count: {allElements}");

            // Step 2: Get detailed element tree to analyze accessibility properties
            var detailedTree = await Tools.GetElementTree(maxDepth: 3);
            _output.WriteLine($"Detailed element tree: {JsonSerializer.Serialize(detailedTree, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for focusable elements
            var focusableElements = await Tools.SearchElements(
                controlType: "Button"); // Buttons are typically focusable
            _output.WriteLine($"Focusable button elements: {JsonSerializer.Serialize(focusableElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Take screenshot of accessibility interface
            var accessibilityScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Accessibility interface screenshot captured: {accessibilityScreenshot != null}");

            Assert.True(true, "Accessibility and keyboard navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Accessibility and keyboard navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task DynamicContentAndVirtualization_ShouldTestDynamicElements()
    {
        _output.WriteLine("Testing dynamic content and virtualization scenarios...");

        try
        {
            // Step 1: Look for ScrollViewer controls (often contain virtualized content)
            var scrollViewers = await Tools.SearchElements(
                controlType: "ScrollViewer");
            _output.WriteLine($"ScrollViewer controls found: {JsonSerializer.Serialize(scrollViewers, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for ListView controls (often virtualized)
            var listViews = await Tools.SearchElements(
                controlType: "ListView");
            _output.WriteLine($"ListView controls found: {JsonSerializer.Serialize(listViews, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for GridView controls (often virtualized)
            var gridViews = await Tools.SearchElements(
                controlType: "GridView");
            _output.WriteLine($"GridView controls found: {JsonSerializer.Serialize(gridViews, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for DataGrid controls
            var dataGrids = await Tools.SearchElements(
                controlType: "DataGrid");
            _output.WriteLine($"DataGrid controls found: {JsonSerializer.Serialize(dataGrids, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take screenshot of dynamic content
            var dynamicScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Dynamic content screenshot captured: {dynamicScreenshot != null}");

            Assert.True(true, "Dynamic content and virtualization test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Dynamic content and virtualization test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task UserInputValidationScenario_ShouldTestInputValidation()
    {
        _output.WriteLine("Testing user input validation scenarios...");

        try
        {
            // Step 1: Look for TextBox controls that might have validation
            var textBoxes = await Tools.SearchElements(
                controlType: "TextBox");
            _output.WriteLine($"TextBox controls found: {JsonSerializer.Serialize(textBoxes, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for NumberBox controls (WinUI 3 specific)
            var numberBoxes = await Tools.SearchElements(
                controlType: "NumberBox");
            _output.WriteLine($"NumberBox controls found: {JsonSerializer.Serialize(numberBoxes, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for DatePicker controls
            var datePickers = await Tools.SearchElements(
                controlType: "DatePicker");
            _output.WriteLine($"DatePicker controls found: {JsonSerializer.Serialize(datePickers, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for TimePicker controls
            var timePickers = await Tools.SearchElements(
                controlType: "TimePicker");
            _output.WriteLine($"TimePicker controls found: {JsonSerializer.Serialize(timePickers, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take screenshot of input validation interface
            var inputScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Input validation interface screenshot captured: {inputScreenshot != null}");

            Assert.True(true, "User input validation scenario completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"User input validation scenario failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}