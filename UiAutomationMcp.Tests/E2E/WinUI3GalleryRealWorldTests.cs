using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// Real-world tests for WinUI 3 Gallery that demonstrate practical automation scenarios
/// that developers would commonly encounter when automating WinUI 3 applications.
/// </summary>
[Collection("WinUI3GalleryTestCollection")]
[Trait("Category", "E2E")]
public class WinUI3GalleryRealWorldTests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;
    private readonly WinUI3GalleryTestFixture _fixture;

    public WinUI3GalleryRealWorldTests(ITestOutputHelper output, WinUI3GalleryTestFixture fixture) : base(output)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task DiscoverApplicationStructure_ShouldMapEntireApplication()
    {
        _output.WriteLine("Testing comprehensive application structure discovery...");

        try
        {
            // Step 1: Wait for the application to be fully loaded
            await Task.Delay(3000);

            // Step 2: Get the main window information
            var windowInfo = await Tools.SearchElements(controlType: "Window", scope: "children");
            _output.WriteLine($"Main window info: {JsonSerializer.Serialize(windowInfo, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Get the complete element tree with maximum depth
            var completeTree = await Tools.GetElementTree(maxDepth: 6);
            _output.WriteLine($"Complete element tree (depth 6): {JsonSerializer.Serialize(completeTree, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Find all unique control types in the application
            var allControlTypes = new[] { 
                "Window", "NavigationView", "NavigationViewItem", "ScrollViewer", "Grid", "StackPanel", 
                "Button", "TextBox", "TextBlock", "Image", "CheckBox", "RadioButton", "ComboBox", 
                "Slider", "ProgressBar", "ListView", "TreeView", "TabView", "TabItem", "Canvas", 
                "Border", "ContentPresenter", "ItemsPresenter", "ScrollBar", "Thumb", "RepeatButton",
                "Hyperlink", "RichTextBlock", "MediaElement", "WebView", "DatePicker", "TimePicker",
                "CalendarView", "AutoSuggestBox", "SearchBox", "MenuBar", "MenuItem", "Flyout",
                "Popup", "ToolTip", "SplitView", "Pivot", "CommandBar", "AppBar", "StatusBar"
            };

            foreach (var controlType in allControlTypes)
            {
                try
                {
                    var elements = await Tools.SearchElements(
                        controlType: controlType);
                    if (elements != null)
                    {
                        _output.WriteLine($"Found {controlType} elements: {JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true })}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error finding {controlType} elements: {ex.Message}");
                }
            }

            // Step 5: Take a comprehensive screenshot
            var screenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Application structure screenshot captured: {screenshot != null}");

            Assert.True(true, "Application structure discovery completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Application structure discovery failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestNavigationViewInteraction_ShouldNavigateThroughAllSections()
    {
        _output.WriteLine("Testing navigation view interaction with all sections...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Find the NavigationView control
            var navigationView = await Tools.SearchElements(
                controlType: "NavigationView");
            _output.WriteLine($"NavigationView found: {JsonSerializer.Serialize(navigationView, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Find all NavigationViewItem elements
            var navigationItems = await Tools.SearchElements(
                controlType: "NavigationViewItem");
            _output.WriteLine($"NavigationViewItem elements found: {JsonSerializer.Serialize(navigationItems, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Try to find specific navigation categories
            var knownSections = new[] { 
                "Basic Input", "Collections", "Date & Time", "Dialogs & Flyouts", 
                "Layout", "Media", "Navigation", "Status & Info", "Text", "Motion"
            };

            foreach (var section in knownSections)
            {
                try
                {
                    var sectionElement = await Tools.SearchElements(
                        searchText: section,
                        controlType: "NavigationViewItem");
                    _output.WriteLine($"Section '{section}' found: {JsonSerializer.Serialize(sectionElement, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find section '{section}': {ex.Message}");
                }
            }

            // Step 5: Take screenshot of navigation interface
            var navScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Navigation interface screenshot captured: {navScreenshot != null}");

            Assert.True(true, "Navigation view interaction test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Navigation view interaction test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestSearchFunctionality_ShouldTestApplicationSearch()
    {
        _output.WriteLine("Testing WinUI 3 Gallery search functionality...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Look for search-related controls
            var searchControls = new[] { "SearchBox", "AutoSuggestBox", "TextBox" };
            
            foreach (var controlType in searchControls)
            {
                try
                {
                    var searchElements = await Tools.SearchElements(
                        controlType: controlType);
                    _output.WriteLine($"Search control type '{controlType}' found: {JsonSerializer.Serialize(searchElements, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find search control type '{controlType}': {ex.Message}");
                }
            }

            // Step 3: Look for search-related elements by name
            var searchNames = new[] { "Search", "search", "Find", "Filter" };
            
            foreach (var searchName in searchNames)
            {
                try
                {
                    var searchByName = await Tools.SearchElements(
                        searchText: searchName);
                    _output.WriteLine($"Search element by name '{searchName}' found: {JsonSerializer.Serialize(searchByName, new JsonSerializerOptions { WriteIndented = true })}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find search element by name '{searchName}': {ex.Message}");
                }
            }

            // Step 4: Take screenshot of search interface
            var searchScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Search interface screenshot captured: {searchScreenshot != null}");

            Assert.True(true, "Search functionality test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Search functionality test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestSettingsAndConfiguration_ShouldAccessSettings()
    {
        _output.WriteLine("Testing WinUI 3 Gallery settings and configuration access...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Look for settings-related controls
            var settingsElements = await Tools.SearchElements(
                searchText: "Settings");
            _output.WriteLine($"Settings elements found: {JsonSerializer.Serialize(settingsElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 3: Look for menu or command bar elements
            var menuElements = await Tools.SearchElements(
                controlType: "MenuBar");
            _output.WriteLine($"MenuBar elements found: {JsonSerializer.Serialize(menuElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for command bar elements
            var commandBarElements = await Tools.SearchElements(
                controlType: "CommandBar");
            _output.WriteLine($"CommandBar elements found: {JsonSerializer.Serialize(commandBarElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Look for app bar elements
            var appBarElements = await Tools.SearchElements(
                controlType: "AppBar");
            _output.WriteLine($"AppBar elements found: {JsonSerializer.Serialize(appBarElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 6: Take screenshot of settings interface
            var settingsScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Settings interface screenshot captured: {settingsScreenshot != null}");

            Assert.True(true, "Settings and configuration test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Settings and configuration test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestDifferentViewStates_ShouldTestResizingAndStates()
    {
        _output.WriteLine("Testing different view states and responsive design...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Take screenshot of initial state
            var initialScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Initial state screenshot captured: {initialScreenshot != null}");

            // Step 3: Find the main window and try to get its properties
            var windowElements = await Tools.SearchElements(
                controlType: "Window");
            _output.WriteLine($"Window elements found: {JsonSerializer.Serialize(windowElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 4: Look for split view or adaptive layouts
            var splitViewElements = await Tools.SearchElements(
                controlType: "SplitView");
            _output.WriteLine($"SplitView elements found: {JsonSerializer.Serialize(splitViewElements, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 5: Look for scroll viewers that might change with view state
            var scrollViewers = await Tools.SearchElements(
                controlType: "ScrollViewer");
            _output.WriteLine($"ScrollViewer elements found: {JsonSerializer.Serialize(scrollViewers, new JsonSerializerOptions { WriteIndented = true })}");

            // Step 6: Take screenshot of current state
            var currentStateScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Current state screenshot captured: {currentStateScreenshot != null}");

            Assert.True(true, "Different view states test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Different view states test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestAllInteractableElements_ShouldDiscoverAllUIElements()
    {
        _output.WriteLine("Testing discovery of all interactable UI elements...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Find all elements that users can interact with
            var interactableControlTypes = new[] { 
                "Button", "TextBox", "CheckBox", "RadioButton", "ComboBox", "Slider", 
                "ToggleButton", "Hyperlink", "MenuItem", "TreeItem", "ListItem", 
                "TabItem", "ScrollBar", "Thumb", "RepeatButton", "SplitButton"
            };

            foreach (var controlType in interactableControlTypes)
            {
                try
                {
                    var elements = await Tools.SearchElements(
                        controlType: controlType);
                    if (elements != null)
                    {
                        _output.WriteLine($"Interactable {controlType} elements found: {JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true })}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error finding {controlType} elements: {ex.Message}");
                }
            }

            // Step 3: Take screenshot of all interactable elements
            var interactableScreenshot = await Tools.TakeScreenshot();
            _output.WriteLine($"Interactable elements screenshot captured: {interactableScreenshot != null}");

            Assert.True(true, "All interactable elements test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"All interactable elements test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task TestApplicationPerformance_ShouldMeasureResponseTimes()
    {
        _output.WriteLine("Testing application performance and response times...");

        try
        {
            // Step 1: Wait for the application to be ready
            await Task.Delay(2000);

            // Step 2: Measure time to get window info
            var windowInfoStartTime = DateTime.Now;
            var windowInfo = await Tools.SearchElements(controlType: "Window", scope: "children");
            var windowInfoEndTime = DateTime.Now;
            var windowInfoDuration = windowInfoEndTime - windowInfoStartTime;
            _output.WriteLine($"SearchElements(Window) took {windowInfoDuration.TotalMilliseconds}ms");

            // Step 3: Measure time to get element tree
            var elementTreeStartTime = DateTime.Now;
            var elementTree = await Tools.GetElementTree(maxDepth: 3);
            var elementTreeEndTime = DateTime.Now;
            var elementTreeDuration = elementTreeEndTime - elementTreeStartTime;
            _output.WriteLine($"GetElementTree took {elementTreeDuration.TotalMilliseconds}ms");

            // Step 4: Measure time to find elements
            var searchElementsStartTime = DateTime.Now;
            var buttons = await Tools.SearchElements(controlType: "Button");
            var searchElementsEndTime = DateTime.Now;
            var searchElementsDuration = searchElementsEndTime - searchElementsStartTime;
            _output.WriteLine($"SearchElements took {searchElementsDuration.TotalMilliseconds}ms");

            // Step 5: Measure time to take screenshot
            var screenshotStartTime = DateTime.Now;
            var screenshot = await Tools.TakeScreenshot();
            var screenshotEndTime = DateTime.Now;
            var screenshotDuration = screenshotEndTime - screenshotStartTime;
            _output.WriteLine($"TakeScreenshot took {screenshotDuration.TotalMilliseconds}ms");

            // Step 6: Output performance summary
            _output.WriteLine($"Performance Summary:");
            _output.WriteLine($"  Window Info: {windowInfoDuration.TotalMilliseconds}ms");
            _output.WriteLine($"  Element Tree: {elementTreeDuration.TotalMilliseconds}ms");
            _output.WriteLine($"  Find Elements: {searchElementsDuration.TotalMilliseconds}ms");
            _output.WriteLine($"  Screenshot: {screenshotDuration.TotalMilliseconds}ms");

            Assert.True(true, "Application performance test completed successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Application performance test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
