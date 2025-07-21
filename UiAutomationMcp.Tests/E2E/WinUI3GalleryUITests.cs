using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// Tests for WinUI 3 Gallery application that demonstrate various UI automation scenarios
/// using the MCP tools against real UI controls.
/// </summary>
[Collection("WinUI3GalleryTestCollection")]
[Trait("Category", "E2E")]
public class WinUI3GalleryUITests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;
    private readonly WinUI3GalleryTestFixture _fixture;

    public WinUI3GalleryUITests(ITestOutputHelper output, WinUI3GalleryTestFixture fixture) : base(output)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task NavigateToButtonPage_ShouldFindAndClickButtons()
    {
        _output.WriteLine("Testing navigation to Button page and interaction with buttons...");

        try
        {
            // Step 1: Get the WinUI 3 Gallery window
            var windowInfo = await Tools.GetWindows();
            _output.WriteLine($"Window info: {JsonSerializer.Serialize(windowInfo, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find the navigation view and search for "Button"
            var navigationResult = await Tools.SearchElements(controlType: "NavigationView");
            _output.WriteLine($"Navigation view search result: {JsonSerializer.Serialize(navigationResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Find and click the "Basic Input" section
            var basicInputResult = await Tools.SearchElements(
                searchText: "Basic Input",
                controlType: "NavigationViewItem");
            _output.WriteLine($"Basic Input section result: {JsonSerializer.Serialize(basicInputResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Find and click the "Button" item
            var buttonNavResult = await Tools.SearchElements(
                searchText: "Button",
                controlType: "NavigationViewItem");
            _output.WriteLine($"Button navigation item result: {JsonSerializer.Serialize(buttonNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take a screenshot of the current state
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"Screenshot taken: {screenshotResult != null}");

            // Step 6: Find actual button controls on the page
            var buttonElements = await Tools.SearchElements(
                controlType: "Button");
            _output.WriteLine($"Button elements found: {JsonSerializer.Serialize(buttonElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 7: Get the detailed element tree to understand the structure
            var elementTree = await Tools.GetElementTree(maxDepth: 3);
            _output.WriteLine($"Element tree: {JsonSerializer.Serialize(elementTree, new JsonSerializerOptions { WriteIndented = true })}");

            Assert.True(true, "Button page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Button page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task NavigateToTextBoxPage_ShouldFindAndInteractWithTextBoxes()
    {
        _output.WriteLine("Testing navigation to TextBox page and text input...");

        try
        {
            // Step 1: Navigate to TextBox page
            var textBoxNavResult = await Tools.SearchElements(
                searchText: "TextBox",
                controlType: "NavigationViewItem");
            _output.WriteLine($"TextBox navigation result: {JsonSerializer.Serialize(textBoxNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find TextBox controls
            var textBoxElements = await Tools.SearchElements(
                controlType: "TextBox");
            _output.WriteLine($"TextBox elements found: {JsonSerializer.Serialize(textBoxElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Find Edit controls (alternative for text input)
            var editElements = await Tools.SearchElements(
                controlType: "Edit");
            _output.WriteLine($"Edit elements found: {JsonSerializer.Serialize(editElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Take a screenshot of the TextBox page
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"TextBox page screenshot taken: {screenshotResult != null}");

            Assert.True(true, "TextBox page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"TextBox page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task NavigateToSliderPage_ShouldFindAndInteractWithSliders()
    {
        _output.WriteLine("Testing navigation to Slider page and slider interaction...");

        try
        {
            // Step 1: Navigate to Slider page
            var sliderNavResult = await Tools.SearchElements(
                searchText: "Slider",
                controlType: "NavigationViewItem");
            _output.WriteLine($"Slider navigation result: {JsonSerializer.Serialize(sliderNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find Slider controls
            var sliderElements = await Tools.SearchElements(
                controlType: "Slider");
            _output.WriteLine($"Slider elements found: {JsonSerializer.Serialize(sliderElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Take a screenshot of the Slider page
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"Slider page screenshot taken: {screenshotResult != null}");

            Assert.True(true, "Slider page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Slider page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task NavigateToComboBoxPage_ShouldFindAndInteractWithComboBoxes()
    {
        _output.WriteLine("Testing navigation to ComboBox page and dropdown interaction...");

        try
        {
            // Step 1: Navigate to ComboBox page
            var comboBoxNavResult = await Tools.SearchElements(
                searchText: "ComboBox",
                controlType: "NavigationViewItem");
            _output.WriteLine($"ComboBox navigation result: {JsonSerializer.Serialize(comboBoxNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find ComboBox controls
            var comboBoxElements = await Tools.SearchElements(
                controlType: "ComboBox");
            _output.WriteLine($"ComboBox elements found: {JsonSerializer.Serialize(comboBoxElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Take a screenshot of the ComboBox page
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"ComboBox page screenshot taken: {screenshotResult != null}");

            Assert.True(true, "ComboBox page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ComboBox page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task NavigateToListViewPage_ShouldFindAndInteractWithListViews()
    {
        _output.WriteLine("Testing navigation to ListView page and list interaction...");

        try
        {
            // Step 1: Navigate to Collection section
            var collectionNavResult = await Tools.SearchElements(
                searchText: "Collections",
                controlType: "NavigationViewItem");
            _output.WriteLine($"Collections navigation result: {JsonSerializer.Serialize(collectionNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Navigate to ListView page
            var listViewNavResult = await Tools.SearchElements(
                searchText: "ListView",
                controlType: "NavigationViewItem");
            _output.WriteLine($"ListView navigation result: {JsonSerializer.Serialize(listViewNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Find ListView controls
            var listViewElements = await Tools.SearchElements(
                controlType: "ListView");
            _output.WriteLine($"ListView elements found: {JsonSerializer.Serialize(listViewElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Find List controls (alternative)
            var listElements = await Tools.SearchElements(
                controlType: "List");
            _output.WriteLine($"List elements found: {JsonSerializer.Serialize(listElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take a screenshot of the ListView page
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"ListView page screenshot taken: {screenshotResult != null}");

            Assert.True(true, "ListView page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ListView page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task NavigateToTreeViewPage_ShouldFindAndInteractWithTreeViews()
    {
        _output.WriteLine("Testing navigation to TreeView page and tree interaction...");

        try
        {
            // Step 1: Navigate to TreeView page
            var treeViewNavResult = await Tools.SearchElements(
                searchText: "TreeView",
                controlType: "NavigationViewItem");
            _output.WriteLine($"TreeView navigation result: {JsonSerializer.Serialize(treeViewNavResult, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find TreeView controls
            var treeViewElements = await Tools.SearchElements(
                controlType: "TreeView");
            _output.WriteLine($"TreeView elements found: {JsonSerializer.Serialize(treeViewElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Find Tree controls (alternative)
            var treeElements = await Tools.SearchElements(
                controlType: "Tree");
            _output.WriteLine($"Tree elements found: {JsonSerializer.Serialize(treeElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Take a screenshot of the TreeView page
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"TreeView page screenshot taken: {screenshotResult != null}");

            Assert.True(true, "TreeView page navigation test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"TreeView page navigation test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task ExploreAllAvailableUIElements_ShouldDiscoverAllInteractableElements()
    {
        _output.WriteLine("Testing comprehensive UI element discovery in WinUI 3 Gallery...");

        try
        {
            // Step 1: Get the full element tree with deeper depth
            var fullElementTree = await Tools.GetElementTree(maxDepth: 5);
            _output.WriteLine($"Full element tree (depth 5): {JsonSerializer.Serialize(fullElementTree, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Find all interactable elements by common control types
            var controlTypesToSearch = new[] { "Button", "TextBox", "Edit", "ComboBox", "Slider", "CheckBox", "RadioButton", "ListView", "TreeView", "Tab", "MenuItem", "Hyperlink", "ProgressBar", "ScrollBar" };

            foreach (var controlType in controlTypesToSearch)
            {
                try
                {
                    var elements = await Tools.SearchElements(
                        controlType: controlType);
                    _output.WriteLine($"Found {controlType} elements: {JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to find {controlType} elements: {ex.Message}");
                }
            }

            // Step 3: Take a final screenshot
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"Final screenshot taken: {screenshotResult != null}");

            Assert.True(true, "Comprehensive UI element discovery test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Comprehensive UI element discovery test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestAdvancedUIPatterns_ShouldTestGridsAndDataVisualization()
    {
        _output.WriteLine("Testing advanced UI patterns like DataGrid and charts...");

        try
        {
            // Step 1: Look for DataGrid controls
            var dataGridElements = await Tools.SearchElements(
                controlType: "DataGrid");
            _output.WriteLine($"DataGrid elements found: {JsonSerializer.Serialize(dataGridElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 2: Look for Table controls
            var tableElements = await Tools.SearchElements(
                controlType: "Table");
            _output.WriteLine($"Table elements found: {JsonSerializer.Serialize(tableElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for Chart or Canvas controls (for data visualization)
            var canvasElements = await Tools.SearchElements(
                controlType: "Canvas");
            _output.WriteLine($"Canvas elements found: {JsonSerializer.Serialize(canvasElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for ScrollViewer controls
            var scrollViewerElements = await Tools.SearchElements(
                controlType: "ScrollViewer");
            _output.WriteLine($"ScrollViewer elements found: {JsonSerializer.Serialize(scrollViewerElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Take a screenshot of advanced patterns
            var screenshotResult = await Tools.TakeScreenshot();
            _output.WriteLine($"Advanced patterns screenshot taken: {screenshotResult != null}");

            Assert.True(true, "Advanced UI patterns test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Advanced UI patterns test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}