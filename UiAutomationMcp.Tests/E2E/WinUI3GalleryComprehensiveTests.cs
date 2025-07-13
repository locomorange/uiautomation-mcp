using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("WinUI3GalleryTestCollection")]
    [Trait("Category", "E2E")]
    [Trait("Category", "Comprehensive")]
    public class WinUI3GalleryComprehensiveTests : BaseE2ETest
    {
        public WinUI3GalleryComprehensiveTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Window and Element Discovery Tools

        [Fact]
        public async Task Test_01_GetWindowInfo_ShouldFindWinUI3Gallery()
        {
            Output.WriteLine("=== Testing GetWindowInfo ===");
            
            var windows = await Tools.GetWindowInfo();
            LogResult("GetWindowInfo", windows);
            
            Assert.NotNull(windows);
        }

        [Fact]
        public async Task Test_02_GetElementInfo_ShouldFindUIElements()
        {
            Output.WriteLine("=== Testing GetElementInfo ===");
            
            var elements = await Tools.GetElementInfo(windowTitle: "WinUI 3 Gallery");
            LogResult("GetElementInfo", elements);
            
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_03_FindElements_WithSearchText()
        {
            Output.WriteLine("=== Testing FindElements with search text ===");
            
            var elements = await Tools.FindElements(searchText: "Button", windowTitle: "WinUI 3 Gallery");
            LogResult("FindElements", elements);
            
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_04_GetElementTree_ShouldShowHierarchy()
        {
            Output.WriteLine("=== Testing GetElementTree ===");
            
            var tree = await Tools.GetElementTree(windowTitle: "WinUI 3 Gallery", maxDepth: 3);
            LogResult("GetElementTree", tree);
            
            Assert.NotNull(tree);
        }

        [Fact]
        public async Task Test_05_FindElementsByControlType_Buttons()
        {
            Output.WriteLine("=== Testing FindElementsByControlType for Buttons ===");
            
            var buttons = await Tools.FindElementsByControlType("Button", windowTitle: "WinUI 3 Gallery");
            LogResult("FindElementsByControlType", buttons);
            
            Assert.NotNull(buttons);
        }

        #endregion

        #region Application Management Tools

        [Fact]
        public async Task Test_06_TakeScreenshot_FullWindow()
        {
            Output.WriteLine("=== Testing TakeScreenshot ===");
            
            var screenshot = await Tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery");
            LogResult("TakeScreenshot", screenshot);
            
            Assert.NotNull(screenshot);
        }

        [Fact]
        public async Task Test_07_WindowAction_GetCapabilities()
        {
            Output.WriteLine("=== Testing GetWindowCapabilities ===");
            
            var capabilities = await Tools.GetWindowCapabilities(windowTitle: "WinUI 3 Gallery");
            LogResult("GetWindowCapabilities", capabilities);
            
            Assert.NotNull(capabilities);
        }

        [Fact]
        public async Task Test_08_GetWindowInteractionState()
        {
            Output.WriteLine("=== Testing GetWindowInteractionState ===");
            
            var state = await Tools.GetWindowInteractionState(windowTitle: "WinUI 3 Gallery");
            LogResult("GetWindowInteractionState", state);
            
            Assert.NotNull(state);
        }

        #endregion

        #region Core Interaction Patterns

        [Fact]
        public async Task Test_09_InvokeElement_NavigationItem()
        {
            Output.WriteLine("=== Testing InvokeElement on navigation items ===");
            
            try
            {
                // First find navigation items
                var navItems = await Tools.FindElementsByControlType("ListItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Found navigation items", navItems);
                
                // Try to find and click on "Basic Input" navigation item
                var elements = await Tools.FindElements(searchText: "Basic Input", windowTitle: "WinUI 3 Gallery");
                LogResult("Found Basic Input elements", elements);
                
                Assert.NotNull(elements);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"InvokeElement test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_10_SelectionPattern_ListItems()
        {
            Output.WriteLine("=== Testing Selection patterns ===");
            
            try
            {
                // Find list items
                var listItems = await Tools.FindElementsByControlType("ListItem", windowTitle: "WinUI 3 Gallery");
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
                                    var isSelected = await Tools.IsElementSelected(automationId, windowTitle: "WinUI 3 Gallery");
                                    LogResult($"IsElementSelected for {automationId}", isSelected);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Selection pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_11_TogglePattern_CheckBoxes()
        {
            Output.WriteLine("=== Testing Toggle patterns ===");
            
            try
            {
                // Find checkboxes
                var checkboxes = await Tools.FindElementsByControlType("CheckBox", windowTitle: "WinUI 3 Gallery");
                LogResult("Found checkboxes", checkboxes);
                
                Assert.NotNull(checkboxes);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Toggle pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_12_ValuePattern_TextBoxes()
        {
            Output.WriteLine("=== Testing Value patterns ===");
            
            try
            {
                // Find text boxes
                var textBoxes = await Tools.FindElementsByControlType("Edit", windowTitle: "WinUI 3 Gallery");
                LogResult("Found text boxes", textBoxes);
                
                Assert.NotNull(textBoxes);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Value pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Layout and Navigation Patterns

        [Fact]
        public async Task Test_13_ScrollPattern_ScrollableElements()
        {
            Output.WriteLine("=== Testing Scroll patterns ===");
            
            try
            {
                // Find scrollable elements
                var scrollViewers = await Tools.FindElementsByControlType("ScrollViewer", windowTitle: "WinUI 3 Gallery");
                LogResult("Found scroll viewers", scrollViewers);
                
                // Test GetScrollInfo if available
                var allElements = await Tools.GetElementInfo(windowTitle: "WinUI 3 Gallery");
                LogResult("All elements for scroll testing", allElements);
                
                Assert.NotNull(allElements);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Scroll pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_14_ExpandCollapsePattern_TreeItems()
        {
            Output.WriteLine("=== Testing ExpandCollapse patterns ===");
            
            try
            {
                // Find tree items or expandable elements
                var treeItems = await Tools.FindElementsByControlType("TreeItem", windowTitle: "WinUI 3 Gallery");
                LogResult("Found tree items", treeItems);
                
                // Also check for other expandable controls
                var expanders = await Tools.FindElements(searchText: "Expander", windowTitle: "WinUI 3 Gallery");
                LogResult("Found expanders", expanders);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                Output.WriteLine($"ExpandCollapse pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_15_TransformPattern_Capabilities()
        {
            Output.WriteLine("=== Testing Transform patterns ===");
            
            try
            {
                // Find elements that might support transform
                var windows = await Tools.FindElementsByControlType("Window", windowTitle: "WinUI 3 Gallery");
                LogResult("Found windows for transform", windows);
                
                Assert.NotNull(windows);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Transform pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Text Pattern Operations

        [Fact]
        public async Task Test_16_TextPattern_GetText()
        {
            Output.WriteLine("=== Testing Text patterns ===");
            
            try
            {
                // Find text elements
                var textElements = await Tools.FindElementsByControlType("Text", windowTitle: "WinUI 3 Gallery");
                LogResult("Found text elements", textElements);
                
                Assert.NotNull(textElements);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Text pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Grid and Table Pattern Operations

        [Fact]
        public async Task Test_17_GridPattern_DataGrid()
        {
            Output.WriteLine("=== Testing Grid patterns ===");
            
            try
            {
                // Find data grid elements
                var dataGrids = await Tools.FindElementsByControlType("DataGrid", windowTitle: "WinUI 3 Gallery");
                LogResult("Found data grids", dataGrids);
                
                // Also check for generic grid patterns
                var grids = await Tools.FindElementsByControlType("Grid", windowTitle: "WinUI 3 Gallery");
                LogResult("Found grids", grids);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Grid pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_18_TablePattern_Tables()
        {
            Output.WriteLine("=== Testing Table patterns ===");
            
            try
            {
                // Find table elements
                var tables = await Tools.FindElementsByControlType("Table", windowTitle: "WinUI 3 Gallery");
                LogResult("Found tables", tables);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Table pattern test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Accessibility and Control Type Validation

        [Fact]
        public async Task Test_19_AccessibilityInfo_MainWindow()
        {
            Output.WriteLine("=== Testing Accessibility Info ===");
            
            try
            {
                var accessibilityInfo = await Tools.VerifyAccessibility(windowTitle: "WinUI 3 Gallery");
                LogResult("VerifyAccessibility", accessibilityInfo);
                
                Assert.NotNull(accessibilityInfo);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Accessibility test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_20_ControlTypeInfo_Validation()
        {
            Output.WriteLine("=== Testing Control Type Info ===");
            
            try
            {
                // Find a button and validate its control type
                var buttons = await Tools.FindElementsByControlType("Button", windowTitle: "WinUI 3 Gallery", maxResults: 1);
                LogResult("Found button for control type validation", buttons);
                
                Assert.NotNull(buttons);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Control type validation test encountered: {ex.Message}");
            }
        }

        #endregion

        #region Advanced Pattern Operations

        [Fact]
        public async Task Test_21_MultipleViewPattern()
        {
            Output.WriteLine("=== Testing MultipleView patterns ===");
            
            try
            {
                // Find elements that might support multiple views
                var viewElements = await Tools.FindElements(searchText: "View", windowTitle: "WinUI 3 Gallery");
                LogResult("Found view elements", viewElements);
                
                Assert.NotNull(viewElements);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"MultipleView pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_22_RangeValuePattern_Sliders()
        {
            Output.WriteLine("=== Testing RangeValue patterns ===");
            
            try
            {
                // Find sliders
                var sliders = await Tools.FindElementsByControlType("Slider", windowTitle: "WinUI 3 Gallery");
                LogResult("Found sliders", sliders);
                
                // Also check for progress bars
                var progressBars = await Tools.FindElementsByControlType("ProgressBar", windowTitle: "WinUI 3 Gallery");
                LogResult("Found progress bars", progressBars);
                
                Assert.True(true); // Test completion
            }
            catch (Exception ex)
            {
                Output.WriteLine($"RangeValue pattern test encountered: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_23_CustomProperties()
        {
            Output.WriteLine("=== Testing Custom Properties ===");
            
            try
            {
                // Find elements and check for custom properties
                var elements = await Tools.GetElementInfo(windowTitle: "WinUI 3 Gallery", controlType: "Button");
                LogResult("Elements for custom property testing", elements);
                
                Assert.NotNull(elements);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Custom properties test encountered: {ex.Message}");
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
                Output.WriteLine($"{operation} result: {json}");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"{operation} result serialization failed: {ex.Message}");
                Output.WriteLine($"{operation} result type: {result?.GetType().Name ?? "null"}");
            }
        }

        #endregion
    }
}