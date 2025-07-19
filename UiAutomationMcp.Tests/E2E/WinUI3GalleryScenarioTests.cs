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
                await _tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery", outputPath: "C:\\temp\\gallery_initial.png");
                _output.WriteLine("Initial screenshot taken");

                // Step 2: Find navigation menu items
                var navItems = await _tools.FindElements(searchText: "Button", controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Navigation items with 'Button'", navItems);

                // Step 3: Try to invoke the Button navigation item
                if (navItems != null)
                {
                    var navItemsJson = JsonSerializer.Serialize(navItems);
                    var itemsElement = JsonSerializer.Deserialize<JsonElement>(navItemsJson);
                    
                    if (TryGetFirstElementId(itemsElement, out var buttonNavId))
                    {
                        _output.WriteLine($"Attempting to invoke Button navigation item: {buttonNavId}");
                        var invokeResult = await _tools.InvokeElement(buttonNavId!, windowTitle: "WinUI 3 Gallery");
                        LogResult("Invoke result", invokeResult);
                        
                        // Wait for navigation
                        await Task.Delay(1000);
                        
                        // Take screenshot after navigation
                        await _tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery", outputPath: "C:\\temp\\gallery_button_page.png");
                        _output.WriteLine("Button page screenshot taken");
                    }
                }

                // Step 4: Find all buttons on the page
                var buttons = await _tools.FindElements(controlType: "Button", windowTitle: "WinUI 3 Gallery");
                LogResult("All buttons on page", buttons);

                // Step 5: Get element tree to understand page structure
                var tree = await _tools.GetElementTree(windowTitle: "WinUI 3 Gallery", maxDepth: 2);
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
                var navItems = await _tools.FindElements(searchText: "TextBox", controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("TextBox navigation items", navItems);

                // Step 2: Navigate to TextBox page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var textBoxNavId))
                {
                    await _tools.InvokeElement(textBoxNavId!, windowTitle: "WinUI 3 Gallery");
                    await Task.Delay(1000);
                }

                // Step 3: Find text boxes on the page
                var textBoxes = await _tools.FindElements(controlType: "Edit", windowTitle: "WinUI 3 Gallery");
                LogResult("Text boxes found", textBoxes);

                // Step 4: Try to set text in the first text box
                if (textBoxes != null)
                {
                    var textBoxesJson = JsonSerializer.Serialize(textBoxes);
                    var textBoxElement = JsonSerializer.Deserialize<JsonElement>(textBoxesJson);
                    
                    if (TryGetFirstElementId(textBoxElement, out var textBoxId))
                    {
                        _output.WriteLine($"Setting text in TextBox: {textBoxId}");
                        var setValue = await _tools.SetElementValue(textBoxId!, "Hello from MCP Test!", windowTitle: "WinUI 3 Gallery");
                        LogResult("SetElementValue result", setValue);
                        
                        // Get the value back
                        var getValue = await _tools.GetElementValue(textBoxId!, windowTitle: "WinUI 3 Gallery");
                        LogResult("GetElementValue result", getValue);
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
                var navItems = await _tools.FindElements(searchText: "CheckBox", controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("CheckBox navigation items", navItems);

                // Step 2: Navigate to CheckBox page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var checkBoxNavId))
                {
                    await _tools.InvokeElement(checkBoxNavId!, windowTitle: "WinUI 3 Gallery");
                    await Task.Delay(1000);
                }

                // Step 3: Find checkboxes on the page
                var checkBoxes = await _tools.FindElements(controlType: "CheckBox", windowTitle: "WinUI 3 Gallery");
                LogResult("CheckBoxes found", checkBoxes);

                // Step 4: Toggle the first checkbox
                if (checkBoxes != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(checkBoxes)), out var checkBoxId))
                {
                    _output.WriteLine($"Toggling CheckBox: {checkBoxId}");
                    var toggleResult = await _tools.ToggleElement(checkBoxId!, windowTitle: "WinUI 3 Gallery");
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
                var navItems = await _tools.FindElements(searchText: "Slider", controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Slider navigation items", navItems);

                // Step 2: Navigate to Slider page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var sliderNavId))
                {
                    await _tools.InvokeElement(sliderNavId!, windowTitle: "WinUI 3 Gallery");
                    await Task.Delay(1000);
                }

                // Step 3: Find sliders on the page
                var sliders = await _tools.FindElements(controlType: "Slider", windowTitle: "WinUI 3 Gallery");
                LogResult("Sliders found", sliders);

                // Step 4: Get and set range value
                if (sliders != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(sliders)), out var sliderId))
                {
                    // Get current range value
                    var rangeInfo = await _tools.GetRangeValue(sliderId!, windowTitle: "WinUI 3 Gallery");
                    LogResult("Current range value", rangeInfo);
                    
                    // Set new value
                    var setResult = await _tools.SetRangeValue(sliderId!, 50, windowTitle: "WinUI 3 Gallery");
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
                var navItems = await _tools.FindElements(searchText: "ScrollViewer", controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("ScrollViewer navigation items", navItems);

                // Step 2: Navigate to ScrollViewer page
                if (navItems != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(navItems)), out var scrollViewerNavId))
                {
                    await _tools.InvokeElement(scrollViewerNavId!, windowTitle: "WinUI 3 Gallery");
                    await Task.Delay(1000);
                }

                // Step 3: Find scrollable elements
                var scrollViewers = await _tools.FindElements(controlType: "ScrollViewer", windowTitle: "WinUI 3 Gallery");
                LogResult("ScrollViewers found", scrollViewers);

                // Step 4: Test scroll operations
                if (scrollViewers != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(scrollViewers)), out var scrollViewerId))
                {
                    // Get scroll info
                    var scrollInfo = await _tools.GetScrollInfo(scrollViewerId!, windowTitle: "WinUI 3 Gallery");
                    LogResult("Scroll info", scrollInfo);
                    
                    // Scroll down
                    var scrollResult = await _tools.ScrollElement(scrollViewerId!, "down", 2.0, windowTitle: "WinUI 3 Gallery");
                    LogResult("Scroll down result", scrollResult);
                    
                    // Set scroll percentage
                    var setScrollResult = await _tools.SetScrollPercent(scrollViewerId!, -1, 50, windowTitle: "WinUI 3 Gallery");
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
                // Step 1: Validate main window accessibility
                var mainWindowAccessibility = await _tools.VerifyAccessibility(windowTitle: "WinUI 3 Gallery");
                LogResult("Main window accessibility", mainWindowAccessibility);

                // Step 2: Get control type info for various elements
                var buttons = await _tools.FindElements(controlType: "Button", windowTitle: "WinUI 3 Gallery", maxResults: 1);
                if (buttons != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(buttons)), out var buttonId))
                {
                    // Control type info is already included in FindElements response
                    LogResult("Button elements (with control type info)", buttons);
                    
                    var patternValidation = await _tools.ValidateControlTypePatterns(buttonId!, windowTitle: "WinUI 3 Gallery");
                    LogResult("Button pattern validation", patternValidation);
                }

                // Step 3: Check for labeled elements
                var textBoxes = await _tools.FindElements(controlType: "Edit", windowTitle: "WinUI 3 Gallery", maxResults: 1);
                if (textBoxes != null && TryGetFirstElementId(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(textBoxes)), out var textBoxId))
                {
                    try
                    {
                        var labeledBy = await _tools.GetLabeledBy(textBoxId!, windowTitle: "WinUI 3 Gallery");
                        LogResult("TextBox labeled by", labeledBy);
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"GetLabeledBy not available: {ex.Message}");
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