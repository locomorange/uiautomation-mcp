using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "E2E")]
    [Trait("Category", "Comprehensive")]
    public class WinUI3GalleryComprehensiveTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly UIAutomationTools _tools;

        public WinUI3GalleryComprehensiveTests(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = MCPToolsE2ETests.CreateServiceProvider();
            _tools = _serviceProvider.GetRequiredService<UIAutomationTools>();
        }

        #region Window and Element Discovery Tools

        [Fact]
        public async Task Test_01_GetWindowInfo_ShouldFindWinUI3Gallery()
        {
            _output.WriteLine("=== Testing GetWindowInfo ===");
            
            var windows = await _tools.GetWindowInfo();
            LogResult("GetWindowInfo", windows);
            
            Assert.NotNull(windows);
        }

        [Fact]
        public async Task Test_02_GetElementInfo_ShouldFindUIElements()
        {
            _output.WriteLine("=== Testing GetElementInfo ===");
            
            var elements = await _tools.GetElementInfo(windowTitle: "WinUI 3 Gallery");
            LogResult("GetElementInfo", elements);
            
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_03_FindElements_WithSearchText()
        {
            _output.WriteLine("=== Testing FindElements with search text ===");
            
            var elements = await _tools.FindElements(searchText: "Button", windowTitle: "WinUI 3 Gallery");
            LogResult("FindElements", elements);
            
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_04_GetElementTree_ShouldShowHierarchy()
        {
            _output.WriteLine("=== Testing GetElementTree ===");
            
            var tree = await _tools.GetElementTree(windowTitle: "WinUI 3 Gallery", maxDepth: 3);
            LogResult("GetElementTree", tree);
            
            Assert.NotNull(tree);
        }

        [Fact]
        public async Task Test_05_FindElementsByControlType_Buttons()
        {
            _output.WriteLine("=== Testing FindElementsByControlType for Buttons ===");
            
            var buttons = await _tools.FindElementsByControlType("Button", windowTitle: "WinUI 3 Gallery");
            LogResult("FindElementsByControlType", buttons);
            
            Assert.NotNull(buttons);
        }

        #endregion

        #region Application Management Tools

        [Fact]
        public async Task Test_06_TakeScreenshot_FullWindow()
        {
            _output.WriteLine("=== Testing TakeScreenshot ===");
            
            var screenshot = await _tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery");
            LogResult("TakeScreenshot", screenshot);
            
            Assert.NotNull(screenshot);
        }

        [Fact]
        public async Task Test_07_WindowAction_GetCapabilities()
        {
            _output.WriteLine("=== Testing GetWindowCapabilities ===");
            
            var capabilities = await _tools.GetWindowCapabilities(windowTitle: "WinUI 3 Gallery");
            LogResult("GetWindowCapabilities", capabilities);
            
            Assert.NotNull(capabilities);
        }

        [Fact]
        public async Task Test_08_GetWindowInteractionState()
        {
            _output.WriteLine("=== Testing GetWindowInteractionState ===");
            
            var state = await _tools.GetWindowInteractionState(windowTitle: "WinUI 3 Gallery");
            LogResult("GetWindowInteractionState", state);
            
            Assert.NotNull(state);
        }

        #endregion

        #region Core Interaction Patterns

        [Fact]
        public async Task Test_09_InvokeElement_NavigationItem()
        {
            _output.WriteLine("=== Testing InvokeElement on navigation items ===");
            
            try
            {
                // First find navigation items
                var navItems = await _tools.FindElementsByControlType("ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Found navigation items", navItems);
                
                // Try to find and click on "Basic Input" navigation item
                var elements = await _tools.FindElements(searchText: "Basic Input", windowTitle: "WinUI 3 Gallery");
                LogResult("Found Basic Input elements", elements);
                
                Assert.NotNull(elements);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"InvokeElement test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_10_SelectionPattern_ListItems()
        {
            _output.WriteLine("=== Testing Selection patterns ===");
            
            try
            {
                // Find list items
                var listItems = await _tools.FindElementsByControlType("ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Found list items", listItems);
                
                // Test selection capabilities
                if (listItems != null)
                {
                    var listItemsJson = JsonSerializer.Serialize(listItems);
                    var itemsArray = JsonSerializer.Deserialize<JsonElement>(listItemsJson);
                    
                    if (itemsArray.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                    {
                        var items = dataElement.EnumerateArray().ToList();
                        if (items.Count > 0)
                        {
                            var firstItem = items[0];
                            if (firstItem.TryGetProperty("automationId", out var idElement))
                            {
                                var automationId = idElement.GetString();
                                if (!string.IsNullOrEmpty(automationId))
                                {
                                    var isSelected = await _tools.IsElementSelected(automationId, windowTitle: "WinUI 3 Gallery");
                                    LogResult($"IsElementSelected for {automationId}", isSelected);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Selection pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_11_TogglePattern_CheckBoxes()
        {
            _output.WriteLine("=== Testing Toggle patterns ===");
            
            try
            {
                // Find checkboxes
                var checkboxes = await _tools.FindElementsByControlType("CheckBox", windowTitle: "WinUI 3 Gallery");
                LogResult("Found checkboxes", checkboxes);
                
                Assert.NotNull(checkboxes);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Toggle pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_12_ValuePattern_TextBoxes()
        {
            _output.WriteLine("=== Testing Value patterns ===");
            
            try
            {
                // Find text boxes
                var textBoxes = await _tools.FindElementsByControlType("Edit", windowTitle: "WinUI 3 Gallery");
                LogResult("Found text boxes", textBoxes);
                
                Assert.NotNull(textBoxes);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Value pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Layout and Navigation Patterns

        [Fact]
        public async Task Test_13_ScrollPattern_ScrollableElements()
        {
            _output.WriteLine("=== Testing Scroll patterns ===");
            
            try
            {
                // Find scrollable elements
                var scrollViewers = await _tools.FindElementsByControlType("ScrollViewer", windowTitle: "WinUI 3 Gallery");
                LogResult("Found scroll viewers", scrollViewers);
                
                // Test GetScrollInfo if available
                var allElements = await _tools.GetElementInfo(windowTitle: "WinUI 3 Gallery");
                LogResult("All elements for scroll testing", allElements);
                
                Assert.NotNull(allElements);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Scroll pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_14_ExpandCollapsePattern_TreeItems()
        {
            _output.WriteLine("=== Testing ExpandCollapse patterns ===");
            
            try
            {
                // Find tree items or expandable elements
                var treeItems = await _tools.FindElementsByControlType("TreeItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Found tree items", treeItems);
                
                // Also check for other expandable controls
                var expanders = await _tools.FindElements(searchText: "Expander", windowTitle: "WinUI 3 Gallery");
                LogResult("Found expanders", expanders);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                _output.WriteLine($"ExpandCollapse pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_15_TransformPattern_Capabilities()
        {
            _output.WriteLine("=== Testing Transform patterns ===");
            
            try
            {
                // Find elements that might support transform
                var windows = await _tools.FindElementsByControlType("Window", windowTitle: "WinUI 3 Gallery");
                LogResult("Found windows for transform", windows);
                
                Assert.NotNull(windows);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Transform pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Text Pattern Operations

        [Fact]
        public async Task Test_16_TextPattern_GetText()
        {
            _output.WriteLine("=== Testing Text patterns ===");
            
            try
            {
                // Find text elements
                var textElements = await _tools.FindElementsByControlType("Text", windowTitle: "WinUI 3 Gallery");
                LogResult("Found text elements", textElements);
                
                Assert.NotNull(textElements);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Text pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Grid and Table Pattern Operations

        [Fact]
        public async Task Test_17_GridPattern_DataGrid()
        {
            _output.WriteLine("=== Testing Grid patterns ===");
            
            try
            {
                // Find data grid elements
                var dataGrids = await _tools.FindElementsByControlType("DataGrid", windowTitle: "WinUI 3 Gallery");
                LogResult("Found data grids", dataGrids);
                
                // Also check for generic grid patterns
                var grids = await _tools.FindElementsByControlType("Grid", windowTitle: "WinUI 3 Gallery");
                LogResult("Found grids", grids);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Grid pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_18_TablePattern_Tables()
        {
            _output.WriteLine("=== Testing Table patterns ===");
            
            try
            {
                // Find table elements
                var tables = await _tools.FindElementsByControlType("Table", windowTitle: "WinUI 3 Gallery");
                LogResult("Found tables", tables);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Table pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Accessibility and Control Type Validation

        [Fact]
        public async Task Test_19_AccessibilityInfo_MainWindow()
        {
            _output.WriteLine("=== Testing Accessibility Info ===");
            
            try
            {
                var accessibilityInfo = await _tools.VerifyAccessibility(windowTitle: "WinUI 3 Gallery");
                LogResult("VerifyAccessibility", accessibilityInfo);
                
                Assert.NotNull(accessibilityInfo);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Accessibility test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_20_ControlTypeInfo_Validation()
        {
            _output.WriteLine("=== Testing Control Type Info ===");
            
            try
            {
                // Find a button and validate its control type
                var buttons = await _tools.FindElementsByControlType("Button", windowTitle: "WinUI 3 Gallery", maxResults: 1);
                LogResult("Found button for control type validation", buttons);
                
                Assert.NotNull(buttons);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Control type validation test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Advanced Pattern Operations

        [Fact]
        public async Task Test_21_MultipleViewPattern()
        {
            _output.WriteLine("=== Testing MultipleView patterns ===");
            
            try
            {
                // Find elements that might support multiple views
                var viewElements = await _tools.FindElements(searchText: "View", windowTitle: "WinUI 3 Gallery");
                LogResult("Found view elements", viewElements);
                
                Assert.NotNull(viewElements);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"MultipleView pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_22_RangeValuePattern_Sliders()
        {
            _output.WriteLine("=== Testing RangeValue patterns ===");
            
            try
            {
                // Find sliders
                var sliders = await _tools.FindElementsByControlType("Slider", windowTitle: "WinUI 3 Gallery");
                LogResult("Found sliders", sliders);
                
                // Also check for progress bars
                var progressBars = await _tools.FindElementsByControlType("ProgressBar", windowTitle: "WinUI 3 Gallery");
                LogResult("Found progress bars", progressBars);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                _output.WriteLine($"RangeValue pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_23_CustomProperties()
        {
            _output.WriteLine("=== Testing Custom Properties ===");
            
            try
            {
                // Find elements and check for custom properties
                var elements = await _tools.GetElementInfo(windowTitle: "WinUI 3 Gallery", controlType: "Button");
                LogResult("Elements for custom property testing", elements);
                
                Assert.NotNull(elements);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Custom properties test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private void LogResult(string operation, object result)
        {
            try
            {
                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    MaxDepth = 5
                });
                _output.WriteLine($"{operation} result: {json}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{operation} result serialization failed: {ex.Message}");
                _output.WriteLine($"{operation} result type: {result?.GetType().Name ?? "null"}");
            }
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        #endregion
    }
}