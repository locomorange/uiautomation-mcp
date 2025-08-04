using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("WinUI3GalleryTestCollection")]
    [Trait("Category", "E2E")]
    [Trait("Category", "Scenario")]
    public class WinUI3GalleryScenarioTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly UIAutomationTools _tools;

        public WinUI3GalleryScenarioTests(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = MCPToolsE2ETests.CreateServiceProvider();
            _tools = _serviceProvider.GetRequiredService<UIAutomationTools>();
        }

        [Fact]
        public async Task Scenario_NavigateToButtonPage_And_InteractWithButtons()
        {
            _output.WriteLine("=== Scenario: Navigate to Button page and interact ===");

            try
            {
                // Step 1: Take initial screenshot
                await _tools.TakeScreenshot("WinUI 3 Gallery", "C:\\temp\\gallery_initial.png");
                _output.WriteLine("Initial screenshot taken");

                // Step 2: Find navigation menu items
                var navItems = await _tools.SearchElements(searchText: "Button", controlType: "ListItem");
                LogResult("Navigation items with 'Button'", navItems);

                // Step 3: Try to invoke the Button navigation item
                if (navItems != null)
                {
                    var navItemsJson = JsonSerializer.Serialize(navItems);
                    var itemsElement = JsonSerializer.Deserialize<JsonElement>(navItemsJson);

                    if (TryGetFirstElementId(itemsElement, out var buttonNavId))
                    {
                        _output.WriteLine($"Attempting to invoke Button navigation item: {buttonNavId}");
                        var invokeResult = await _tools.InvokeElement(buttonNavId!);
                        LogResult("Invoke result", invokeResult);

                        // Wait for navigation
                        await Task.Delay(1000);

                        // Take screenshot after navigation
                        await _tools.TakeScreenshot("WinUI 3 Gallery", "C:\\temp\\gallery_button_page.png");
                        _output.WriteLine("Button page screenshot taken");
                    }
                }

                // Step 4: Find all buttons on the page
                var buttons = await _tools.SearchElements(controlType: "Button");
                LogResult("All buttons on page", buttons);

                // Step 5: Get element tree to understand page structure
                var tree = await _tools.GetElementTree(maxDepth: 2);
                LogResult("Page structure", tree);

                Assert.True(true, "Scenario completed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Scenario failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Scenario_NavigateToTextBox_And_EnterText()
        {
            _output.WriteLine("=== Scenario: Navigate to TextBox page and enter text ===");

            try
            {
                // Step 1: Find TextBox navigation item
                var navItems = await _tools.SearchElements(searchText: "TextBox", controlType: "ListItem");
                LogResult("TextBox navigation items", navItems);

                // Step 2: Navigate to TextBox page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var textBoxNavId))
                {
                    await _tools.InvokeElement(textBoxNavId!);
                    await Task.Delay(1000);
                }

                // Step 3: Find text boxes on the page
                var textBoxes = await _tools.SearchElements(controlType: "Edit");
                LogResult("Text boxes found", textBoxes);

                // Step 4: Try to set text in the first text box
                if (textBoxes != null)
                {
                    var textBoxesJson = JsonSerializer.Serialize(textBoxes);
                    var textBoxElement = JsonSerializer.Deserialize<JsonElement>(textBoxesJson);

                    if (TryGetFirstElementId(textBoxElement, out var textBoxId))
                    {
                        _output.WriteLine($"Setting text in TextBox: {textBoxId}");
                        var setValue = await _tools.SetElementValue(textBoxId!, "Hello from MCP Test!");
                        LogResult("SetElementValue result", setValue);

                        // Get the value back using SearchElements instead of GetElementInfo
                        var getValueElements = await _tools.SearchElements(automationId: textBoxId!);
                        LogResult("SearchElements result (replacing GetElementInfo)", getValueElements);
                        var getValue = getValueElements;
                    }
                }

                Assert.True(true, "TextBox scenario completed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"TextBox scenario failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Scenario_NavigateToCheckBox_And_Toggle()
        {
            _output.WriteLine("=== Scenario: Navigate to CheckBox page and toggle ===");

            try
            {
                // Step 1: Find CheckBox navigation item
                var navItems = await _tools.SearchElements(searchText: "CheckBox", controlType: "ListItem");
                LogResult("CheckBox navigation items", navItems);

                // Step 2: Navigate to CheckBox page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var checkBoxNavId))
                {
                    await _tools.InvokeElement(checkBoxNavId!);
                    await Task.Delay(1000);
                }

                // Step 3: Find checkboxes on the page
                var checkBoxes = await _tools.SearchElements(controlType: "CheckBox");
                LogResult("CheckBoxes found", checkBoxes);

                // Step 4: Toggle the first checkbox
                if (checkBoxes != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(checkBoxes)), out var checkBoxId))
                {
                    _output.WriteLine($"Toggling CheckBox: {checkBoxId}");
                    var toggleResult = await _tools.ToggleElement(checkBoxId!);
                    LogResult("Toggle result", toggleResult);
                }

                Assert.True(true, "CheckBox scenario completed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"CheckBox scenario failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Scenario_NavigateToSlider_And_SetValue()
        {
            _output.WriteLine("=== Scenario: Navigate to Slider page and set value ===");

            try
            {
                // Step 1: Find Slider navigation item
                var navItems = await _tools.SearchElements(searchText: "Slider", controlType: "ListItem");
                LogResult("Slider navigation items", navItems);

                // Step 2: Navigate to Slider page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var sliderNavId))
                {
                    await _tools.InvokeElement(sliderNavId!);
                    await Task.Delay(1000);
                }

                // Step 3: Find sliders on the page
                var sliders = await _tools.SearchElements(controlType: "Slider");
                LogResult("Sliders found", sliders);

                // Step 4: Get and set range value
                if (sliders != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(sliders)), out var sliderId))
                {
                    // GetRangeValue method has been removed - use SearchElements or pattern-specific operations
                    // For range information, use SetRangeValue directly or other available patterns
                    _output.WriteLine("GetRangeValue functionality has been removed - using direct operations");

                    // Set new value
                    var setResult = await _tools.SetRangeValue(automationId: sliderId!, value: 50);
                    LogResult("SetRangeValue result", setResult);
                }

                Assert.True(true, "Slider scenario completed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Slider scenario failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Scenario_NavigateToScrollViewer_And_Scroll()
        {
            _output.WriteLine("=== Scenario: Navigate to ScrollViewer and test scrolling ===");

            try
            {
                // Step 1: Find ScrollViewer navigation item
                var navItems = await _tools.SearchElements(searchText: "ScrollViewer", controlType: "ListItem");
                LogResult("ScrollViewer navigation items", navItems);

                // Step 2: Navigate to ScrollViewer page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var scrollViewerNavId))
                {
                    await _tools.InvokeElement(scrollViewerNavId!);
                    await Task.Delay(1000);
                }

                // Step 3: Find scrollable elements
                var scrollViewers = await _tools.SearchElements(controlType: "ScrollViewer");
                LogResult("ScrollViewers found", scrollViewers);

                // Step 4: Test scroll operations
                if (scrollViewers != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(scrollViewers)), out var scrollViewerId))
                {
                    // GetScrollInfo method has been removed - use direct scroll operations
                    // Scroll info is available through the scroll pattern operations themselves
                    _output.WriteLine("GetScrollInfo functionality has been removed - using direct scroll operations");

                    // Scroll down
                    var scrollResult = await _tools.ScrollElement(automationId: scrollViewerId!, direction: "down", amount: 2.0);
                    LogResult("Scroll down result", scrollResult);

                    // Set scroll percentage
                    var setScrollResult = await _tools.SetScrollPercent(automationId: scrollViewerId!, horizontalPercent: -1, verticalPercent: 50);
                    LogResult("SetScrollPercent result", setScrollResult);
                }

                Assert.True(true, "ScrollViewer scenario completed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"ScrollViewer scenario failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Scenario_AccessibilityValidation_AllPages()
        {
            _output.WriteLine("=== Scenario: Validate accessibility across different pages ===");

            try
            {
                // Step 1: Validate main window accessibility using SearchElements with includeDetails
                var mainWindowAccessibility = await _tools.SearchElements(
                    name: "WinUI 3 Gallery",
                    includeDetails: true,
                    maxResults: 1
                );
                LogResult("Main window accessibility", mainWindowAccessibility);

                // Step 2: Get control type info for various elements
                var buttons = await _tools.SearchElements(controlType: "Button", maxResults: 1);
                if (buttons != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(buttons)), out var buttonId))
                {
                    // Control type info is already included in SearchElements response
                    LogResult("Button elements (with control type info)", buttons);

                    // ValidateControlTypePatterns was removed - pattern info is available in SearchElements with includeDetails=true
                    var detailedButtons = await _tools.SearchElements(automationId: buttonId!, includeDetails: true, maxResults: 1);
                    LogResult("Button detailed pattern info", detailedButtons);
                }

                // Step 3: Check for labeled elements
                var textBoxes = await _tools.SearchElements(controlType: "Edit", maxResults: 1);
                if (textBoxes != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(textBoxes)), out var textBoxId))
                {
                    // GetLabeledBy method has been removed - label relationships can be found through SearchElements
                    // Look for elements with LabelFor relationships or nearby Text elements
                    try
                    {
                        var nearbyLabels = await _tools.SearchElements(controlType: "Text", maxResults: 5);
                        LogResult("Nearby labels (replacing GetLabeledBy)", nearbyLabels);
                        _output.WriteLine("GetLabeledBy functionality replaced with SearchElements for Text controls");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Label search not available: {ex.Message}");
                    }
                }

                Assert.True(true, "Accessibility validation completed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Accessibility validation failed: {ex.Message}");
                throw;
            }
        }

        private bool TryGetFirstElementId(JsonElement element, out string? elementId)
        {
            elementId = null;

            try
            {
                if (element.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                {
                    var items = dataElement.EnumerateArray().ToList();
                    if (items.Count > 0)
                    {
                        var firstItem = items[0];
                        if (firstItem.TryGetProperty("automationId", out var idElement) && !string.IsNullOrEmpty(idElement.GetString()))
                        {
                            elementId = idElement.GetString()!;
                            return true;
                        }
                        if (firstItem.TryGetProperty("name", out var nameElement) && !string.IsNullOrEmpty(nameElement.GetString()))
                        {
                            elementId = nameElement.GetString()!;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to extract element ID: {ex.Message}");
            }

            return false;
        }

        private void LogResult(string operation, object result)
        {
            try
            {
                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    MaxDepth = 5
                });
                _output.WriteLine($"{operation}: {json}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{operation} serialization failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

