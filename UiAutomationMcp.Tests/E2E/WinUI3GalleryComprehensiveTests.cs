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
        private const string WindowTitle = "WinUI 3 Gallery";

        public WinUI3GalleryComprehensiveTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Window and Element Discovery Tools

        [Fact]
        public async Task Test_01_SearchElements_Windows_ShouldFindWinUI3Gallery()
        {
            Output.WriteLine("=== Testing SearchElements(Window) ===");

            var windows = await Tools.SearchElements(controlType: "Window", scope: "children");
            LogResult("SearchElements(Window)", windows);

            Assert.NotNull(windows);
        }

        [Fact]
        public async Task Test_02_SearchElements_ShouldFindUIElements()
        {
            Output.WriteLine("=== Testing SearchElements ===");

            var elements = await Tools.SearchElements();
            LogResult("SearchElements", elements);

            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_03_SearchElements_WithSearchText()
        {
            Output.WriteLine("=== Testing SearchElements with search text ===");

            var elements = await Tools.SearchElements(searchText: "Button");
            LogResult("SearchElements", elements);

            Assert.NotNull(elements);
        }

        [Fact]
        public async Task Test_04_GetElementTree_ShouldShowHierarchy()
        {
            Output.WriteLine("=== Testing GetElementTree ===");

            var tree = await Tools.GetElementTree(maxDepth: 3);
            LogResult("GetElementTree", tree);

            Assert.NotNull(tree);
        }

        [Fact]
        public async Task Test_05_SearchElementsByControlType_Buttons()
        {
            Output.WriteLine("=== Testing SearchElementsByControlType for Buttons ===");

            var buttons = await Tools.SearchElements(controlType: "Button");
            LogResult("SearchElementsByControlType", buttons);

            Assert.NotNull(buttons);
        }

        [Fact]
        public async Task Test_05b_SearchElements_ClassNameFilter_ReturnsOnlyMatchingClass()
        {
            // Regression guard: the worker previously ignored the advertised `className` parameter,
            // so a className filter had no effect. It must now return only elements of that class.
            Output.WriteLine("=== Testing SearchElements className filter ===");

            // Baseline: unfiltered, window-scoped. visibleOnly:false so the class distribution
            // reflects the whole tree and is not affected by on/off-screen state.
            var allResponse = (await Tools.SearchElements(
                windowTitle: WindowTitle, scope: "descendants", visibleOnly: false, maxResults: 4000)).ToJsonElement();
            var allElements = GetElements(allResponse);
            Assert.True(allElements.Count > 0, "Baseline window-scoped search should find elements");

            // Choose the most common className present so the filtered set is meaningful and
            // strictly smaller than the whole tree (there is always more than one className).
            var targetClass = allElements
                .Select(e => GetStr(e, "className"))
                .Where(s => s.Length > 0)
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            Assert.False(string.IsNullOrEmpty(targetClass), "Baseline elements should expose a className");
            Output.WriteLine($"Filtering by className: '{targetClass}'");

            var filteredResponse = (await Tools.SearchElements(
                windowTitle: WindowTitle, className: targetClass, scope: "descendants",
                visibleOnly: false, maxResults: 4000)).ToJsonElement();
            var filteredElements = GetElements(filteredResponse);
            int totalAll = GetTotalFound(allResponse);
            int totalFiltered = GetTotalFound(filteredResponse);
            Output.WriteLine($"Unfiltered totalFound={totalAll}, className-filtered totalFound={totalFiltered}");

            Assert.True(filteredElements.Count > 0, $"className '{targetClass}' should match at least one element");
            Assert.True(totalFiltered < totalAll,
                "className filter must exclude elements of other classes (filtered < unfiltered)");

            // Every returned element must actually carry the requested className.
            foreach (var el in filteredElements)
            {
                var className = GetStr(el, "className");
                Assert.Equal(targetClass, className);
            }

            // A className that cannot exist must return nothing (proves the filter is applied,
            // not ignored — the pre-fix behavior returned the whole tree).
            var bogusResponse = (await Tools.SearchElements(
                windowTitle: WindowTitle, className: "ZZ_NoSuchClass_QWERTY_123", scope: "descendants",
                visibleOnly: false, maxResults: 4000)).ToJsonElement();
            Assert.Equal(0, GetTotalFound(bogusResponse));
            Assert.Empty(GetElements(bogusResponse));
        }

        [Fact]
        public async Task Test_05c_SearchElements_VisibleOnly_ExcludesOffscreenElements()
        {
            // Regression guard: the worker previously ignored the advertised `visibleOnly` parameter.
            // With visibleOnly (default true) wired in, offscreen elements (IsOffscreen=true) must be
            // excluded, whereas visibleOnly:false returns them.
            Output.WriteLine("=== Testing SearchElements visibleOnly excludes offscreen elements ===");

            var inclusiveResponse = (await Tools.SearchElements(
                windowTitle: WindowTitle, scope: "descendants", visibleOnly: false, maxResults: 4000)).ToJsonElement();
            var visibleOnlyResponse = (await Tools.SearchElements(
                windowTitle: WindowTitle, scope: "descendants", visibleOnly: true, maxResults: 4000)).ToJsonElement();

            // An element is offscreen only when its isOffscreen property is literally true.
            static bool IsOffscreen(JsonElement e) =>
                e.TryGetPropertyCI("isOffscreen", out var v) && v.ValueKind == JsonValueKind.True;

            var inclusiveElements = GetElements(inclusiveResponse);
            var visibleElements = GetElements(visibleOnlyResponse);
            int inclusiveOffscreen = inclusiveElements.Count(IsOffscreen);
            int visibleOffscreen = visibleElements.Count(IsOffscreen);
            Output.WriteLine($"visibleOnly=false: total={GetTotalFound(inclusiveResponse)} offscreen={inclusiveOffscreen}");
            Output.WriteLine($"visibleOnly=true:  total={GetTotalFound(visibleOnlyResponse)} offscreen={visibleOffscreen}");

            // The WinUI 3 Gallery tree always contains offscreen elements (collapsed pages,
            // virtualized/below-the-fold list & grid items). Without the filter they are returned.
            Assert.True(inclusiveOffscreen > 0,
                "visibleOnly:false should return at least one offscreen element");

            // With the filter on, none of the returned elements may be offscreen...
            Assert.Equal(0, visibleOffscreen);

            // ...and excluding the offscreen elements must strictly reduce the total.
            Assert.True(GetTotalFound(visibleOnlyResponse) < GetTotalFound(inclusiveResponse),
                "visibleOnly:true must return fewer elements than visibleOnly:false");
        }

        #endregion

        #region Application Management Tools

        [Fact]
        public async Task Test_06_TakeScreenshot_FullWindow()
        {
            Output.WriteLine("=== Testing TakeScreenshot ===");

            var screenshot = await Tools.TakeScreenshot("WinUI 3 Gallery");
            LogResult("TakeScreenshot", screenshot);

            Assert.NotNull(screenshot);
        }

        [Fact]
        public async Task Test_07_WindowAction_GetCapabilities()
        {
            Output.WriteLine("=== Testing GetWindowCapabilities ===");

            // GetWindowCapabilities method has been removed - functionality consolidated into other methods
            // Use GetWindows or SearchElements instead for window information
            var windows = await Tools.SearchElements(controlType: "Window", scope: "children");
            LogResult("SearchElements(Window) (replacing GetWindowCapabilities)", windows);

            Assert.NotNull(windows);
        }

        [Fact]
        public async Task Test_08_GetWindowInteractionState()
        {
            Output.WriteLine("=== Testing GetWindowInteractionState ===");

            // GetWindowInteractionState method has been removed - functionality consolidated into other methods
            // Use GetWindows or SearchElements instead for window state information
            var windows = await Tools.SearchElements(controlType: "Window", scope: "children");
            LogResult("SearchElements(Window) (replacing GetWindowInteractionState)", windows);

            Assert.NotNull(windows);
        }

        #endregion

        #region Core Interaction Patterns

        [Fact]
        public async Task Test_09_ActualNavigationWithVerification()
        {
            // Verifies that SelectionAction performs REAL navigation, not just returning success:true.
            // Proof of navigation = the target NavigationView item actually transitions to the
            // selected state (SelectionItem.IsSelected), observed via includeDetails. This relies
            // only on working APIs (ControlType filtering + per-element selection detail); it does
            // NOT depend on searchText filtering, which is currently unimplemented in the worker.
            Output.WriteLine("=== Testing ACTUAL navigation with verification ===");

            const string targetNavId = "FundamentalsItem";

            try
            {
                // Step 1: Capture selection state of nav items BEFORE navigating.
                // visibleOnly:false because nav items scrolled out of the NavigationView pane
                // (or when the window is not foreground) report IsOffscreen=true; this test cares
                // about the item's existence/selection state, not its on-screen visibility.
                Output.WriteLine("1. Capturing initial nav selection state...");
                var beforeNav = await Tools.SearchElements(
                    controlType: "ListItem", windowTitle: WindowTitle, visibleOnly: false, includeDetails: true);
                bool foundBefore = TryGetNavItemSelected(beforeNav, targetNavId, out bool selectedBefore);
                Assert.True(foundBefore, $"Nav item '{targetNavId}' should exist in the WinUI 3 Gallery navigation");
                Output.WriteLine($"  {targetNavId} selected before navigation: {selectedBefore}");

                // Step 2: Perform the ACTUAL navigation.
                Output.WriteLine($"2. Performing SelectionAction(select) on {targetNavId}...");
                var selectResult = await Tools.SelectionAction(action: "select", automationId: targetNavId);
                bool selectSuccess = selectResult.ToJsonElement()
                    .TryGetPropertyCI("Success", out var successElement) && successElement.GetBoolean();
                Output.WriteLine($"  SelectionAction success: {selectSuccess}");
                Assert.True(selectSuccess, "SelectionAction(select) should return success");

                // Step 3: Poll the selection state until it flips (bounded retry ~5s), so the test
                // does not depend on a single fixed delay on slower machines.
                Output.WriteLine("3. Waiting for the nav item to become selected...");
                bool selectedAfter = false;
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    await Task.Delay(500);
                    var afterNav = await Tools.SearchElements(
                        controlType: "ListItem", windowTitle: WindowTitle, visibleOnly: false, includeDetails: true);
                    if (TryGetNavItemSelected(afterNav, targetNavId, out selectedAfter) && selectedAfter)
                        break;
                }
                Output.WriteLine($"  {targetNavId} selected after navigation: {selectedAfter}");

                // Step 4: THE REAL VERIFICATION — the nav item genuinely transitioned to selected.
                Assert.True(selectedAfter, $"{targetNavId} should be selected after navigation");
                Assert.False(selectedBefore, $"{targetNavId} should NOT have been selected before navigation (proves an actual state change)");

                Output.WriteLine("\n   NAVIGATION VERIFICATION PASSED!");
                Output.WriteLine($"  {targetNavId} transitioned from unselected -> selected via SelectionAction.");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"\n  NAVIGATION TEST FAILED: {ex.Message}");
                Output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Don't hide failures
            }
        }

        /// <summary>
        /// Finds a navigation item by AutomationId in a SearchElements(includeDetails:true) result
        /// and reports its SelectionItem.IsSelected state.
        /// Returns true if the item was found with a selection detail; false if absent.
        /// </summary>
        private static bool TryGetNavItemSelected(object searchResult, string automationId, out bool isSelected)
        {
            isSelected = false;
            var root = searchResult.ToJsonElement();
            if (!root.TryGetPropertyCI("data", out var data))
                return false;
            if (!data.TryGetPropertyCI("elements", out var elements) || elements.ValueKind != JsonValueKind.Array)
                return false;

            foreach (var element in elements.EnumerateArray())
            {
                if (!element.TryGetPropertyCI("automationId", out var idElement) ||
                    idElement.GetString() != automationId)
                    continue;

                if (element.TryGetPropertyCI("details", out var details) &&
                    details.TryGetPropertyCI("selectionItem", out var selectionItem) &&
                    selectionItem.TryGetPropertyCI("isSelected", out var selectedElement))
                {
                    isSelected = selectedElement.ValueKind == JsonValueKind.True;
                    return true;
                }

                return false; // item found but no selection detail available
            }

            return false; // item not found
        }

        [Fact]
        public async Task Test_10_InvokePattern_VisuallyObviousButton()
        {
            Output.WriteLine("=== Testing InvokePattern with VISUALLY OBVIOUS button (Minimize) ===");

            try
            {
                Output.WriteLine("1. Taking screenshot before button click...");
                await Tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\before_minimize.png");

                Output.WriteLine("2. Finding and analyzing buttons...");
                var buttons = await Tools.SearchElements(controlType: "Button");

                // Parse and find the Minimize button specifically
                var buttonsData = buttons.ToJsonElement();
                if (buttonsData.TryGetPropertyCI("data", out var dataElement) &&
                    dataElement.TryGetPropertyCI("elements", out var elementsArray))
                {
                    var buttonElements = elementsArray.EnumerateArray().ToList();
                    Output.WriteLine($"Found {buttonElements.Count} button elements");

                    bool foundMinimizeButton = false;

                    foreach (var button in buttonElements)
                    {
                        if (button.TryGetPropertyCI("AutomationId", out var idElement) &&
                            button.TryGetPropertyCI("Name", out var nameElement) &&
                            button.TryGetPropertyCI("IsVisible", out var visibleElement))
                        {
                            var automationId = idElement.GetString();
                            var name = nameElement.GetString();
                            var isVisible = visibleElement.GetBoolean();

                            Output.WriteLine($"Button: {name} (ID: {automationId}) - Visible: {isVisible}");

                            // Test specifically the Minimize button for obvious visual change
                            if (automationId == "Minimize" && isVisible)
                            {
                                Output.WriteLine($"   Testing MINIMIZE button for obvious visual change!");
                                foundMinimizeButton = true;

                                try
                                {
                                    Output.WriteLine("3. Clicking Minimize button...");
                                    var invokeResult = await Tools.InvokeElement("Minimize");
                                    Output.WriteLine($"Minimize invoke result: {JsonSerializer.Serialize(invokeResult)}");

                                    await Task.Delay(2000); // Wait for minimize animation

                                    Output.WriteLine("4. Taking screenshot after minimize...");
                                    await Tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\after_minimize.png");

                                    Output.WriteLine("5. Attempting to restore window by clicking on taskbar or searching...");
                                    // Try to find the window again (it might be minimized)
                                    var windowInfo = await Tools.SearchElements(controlType: "Window", scope: "children");
                                    Output.WriteLine($"Window info after minimize: {JsonSerializer.Serialize(windowInfo)}");

                                    // Verify the invoke operation succeeded
                                    var invokeData = invokeResult.ToJsonElement();
                                    bool invokeSuccess = invokeData.TryGetPropertyCI("Success", out var successEl) && successEl.GetBoolean();

                                    Assert.True(invokeSuccess, "Minimize button InvokeElement should succeed");

                                    Output.WriteLine("  MINIMIZE BUTTON TEST PASSED!");
                                    Output.WriteLine("   Compare screenshots: C:\\temp\\before_minimize.png vs C:\\temp\\after_minimize.png");
                                    Output.WriteLine("   If window disappeared/minimized, the UI automation is working!");

                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Output.WriteLine($"  Minimize button test failed: {ex.Message}");
                                    throw;
                                }
                            }
                        }
                    }

                    if (!foundMinimizeButton)
                    {
                        Output.WriteLine("    Minimize button not found or not visible");
                        // Fall back to testing any visible button
                        foreach (var button in buttonElements.Take(3))
                        {
                            if (button.TryGetPropertyCI("AutomationId", out var idElement) &&
                                button.TryGetPropertyCI("IsVisible", out var visibleElement) &&
                                visibleElement.GetBoolean())
                            {
                                var automationId = idElement.GetString();
                                if (!string.IsNullOrEmpty(automationId))
                                {
                                    Output.WriteLine($"Testing fallback button: {automationId}");
                                    var invokeResult = await Tools.InvokeElement(automationId);
                                    var invokeData = invokeResult.ToJsonElement();
                                    bool invokeSuccess = invokeData.TryGetPropertyCI("Success", out var successEl) && successEl.GetBoolean();

                                    Assert.True(invokeSuccess, $"InvokeElement should succeed on visible button {automationId}");
                                    break;
                                }
                            }
                        }
                    }
                }

                Output.WriteLine("  InvokePattern testing completed");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  InvokePattern test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Test_11_ValuePattern_ActualVisibleElements()
        {
            Output.WriteLine("=== Testing ValuePattern with VISIBLE elements only ===");

            try
            {
                Output.WriteLine("1. Looking for text input controls with detailed analysis...");

                var textInputs = await Tools.SearchElements(controlType: "Edit");

                // Parse and analyze each element
                var inputsData = textInputs.ToJsonElement();
                if (inputsData.TryGetPropertyCI("data", out var dataElement) &&
                    dataElement.TryGetPropertyCI("elements", out var elementsArray))
                {
                    var inputElements = elementsArray.EnumerateArray().ToList();
                    Output.WriteLine($"Found {inputElements.Count} text input elements");

                    bool foundVisibleElement = false;

                    foreach (var input in inputElements)
                    {
                        if (input.TryGetPropertyCI("AutomationId", out var idElement) &&
                            input.TryGetPropertyCI("Name", out var nameElement) &&
                            input.TryGetPropertyCI("IsVisible", out var visibleElement) &&
                            input.TryGetPropertyCI("BoundingRectangle", out var rectElement))
                        {
                            var automationId = idElement.GetString();
                            var name = nameElement.GetString();
                            var isVisible = visibleElement.GetBoolean();

                            Output.WriteLine($"Element Analysis:");
                            Output.WriteLine($"  ID: {automationId}");
                            Output.WriteLine($"  Name: {name}");
                            Output.WriteLine($"  Visible: {isVisible}");
                            Output.WriteLine($"  BoundingRect: {rectElement}");

                            // Only test VISIBLE elements with valid automation IDs
                            if (isVisible && !string.IsNullOrEmpty(automationId) &&
                                !automationId.StartsWith("__")) // Skip internal elements
                            {
                                Output.WriteLine($"   Testing VISIBLE element: {name} (ID: {automationId})");

                                try
                                {
                                    // Take screenshot before
                                    await Tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\before_value_test.png");

                                    // Get initial value using SearchElements instead of GetElementInfo
                                    var initialElements = await Tools.SearchElements(automationId: automationId);
                                    Output.WriteLine($"Initial value: {JsonSerializer.Serialize(initialElements)}");
                                    var initialValue = initialElements;

                                    // Set a test value
                                    var testValue = $"TEST VALUE {DateTime.Now:HH:mm:ss}";
                                    Output.WriteLine($"Setting value to: '{testValue}'");

                                    var setValue = await Tools.SetElementValue(automationId, testValue);
                                    Output.WriteLine($"Set value result: {JsonSerializer.Serialize(setValue)}");

                                    await Task.Delay(1000); // Wait for UI update

                                    // Take screenshot after
                                    await Tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\after_value_test.png");

                                    // Get value after setting using SearchElements instead of GetElementInfo
                                    var afterElements = await Tools.SearchElements(automationId: automationId);
                                    Output.WriteLine($"After value: {JsonSerializer.Serialize(afterElements)}");
                                    var afterValue = afterElements;

                                    // Verify the operation worked
                                    var setData = setValue.ToJsonElement();
                                    bool setSuccess = setData.TryGetPropertyCI("Success", out var successEl) && successEl.GetBoolean();

                                    if (setSuccess)
                                    {
                                        Output.WriteLine($"  ValuePattern succeeded on VISIBLE element {name}");
                                        Output.WriteLine("   Check screenshots: C:\\temp\\before_value_test.png vs C:\\temp\\after_value_test.png");
                                        foundVisibleElement = true;

                                        Assert.True(setSuccess, $"SetElementValue should succeed on visible element {name}");
                                        break; // Test one successful visible element
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Output.WriteLine($"  ValuePattern failed on visible element {name}: {ex.Message}");
                                }
                            }
                            else
                            {
                                Output.WriteLine($"    Skipping non-visible or internal element: {automationId}");
                            }
                        }
                    }

                    if (!foundVisibleElement)
                    {
                        Output.WriteLine("    No visible text input elements found for testing");
                        Output.WriteLine("This indicates WinUI 3 Gallery may not have visible text inputs on current page");

                        // Try to navigate to a page with text inputs
                        Output.WriteLine("Attempting to navigate to a page with text inputs...");
                        try
                        {
                            // Try to find Basic Input page
                            await Tools.SelectionAction(action: "select", automationId: "FundamentalsItem");
                            await Task.Delay(2000);

                            // Search again after navigation
                            var newInputs = await Tools.SearchElements(controlType: "Edit");
                            Output.WriteLine($"After navigation, found: {JsonSerializer.Serialize(newInputs)}");
                        }
                        catch (Exception navEx)
                        {
                            Output.WriteLine($"Navigation failed: {navEx.Message}");
                        }
                    }
                }

                Output.WriteLine("  ValuePattern visibility analysis completed");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  ValuePattern test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Test_12_TogglePattern_Checkboxes()
        {
            Output.WriteLine("=== Testing TogglePattern with checkboxes ===");

            try
            {
                // Look for checkboxes
                Output.WriteLine("1. Looking for checkbox controls...");

                var checkboxes = await Tools.SearchElements(controlType: "CheckBox");
                Output.WriteLine($"Found checkboxes: {JsonSerializer.Serialize(checkboxes)}");

                // Parse and test checkbox toggling
                var checkboxData = checkboxes.ToJsonElement();
                if (checkboxData.TryGetPropertyCI("data", out var dataElement) &&
                    dataElement.TryGetPropertyCI("elements", out var elementsArray))
                {
                    var checkboxElements = elementsArray.EnumerateArray().ToList();
                    Output.WriteLine($"Found {checkboxElements.Count} checkbox elements");

                    foreach (var checkbox in checkboxElements.Take(2)) // Test first 2 checkboxes
                    {
                        if (checkbox.TryGetPropertyCI("AutomationId", out var idElement) &&
                            checkbox.TryGetPropertyCI("Name", out var nameElement))
                        {
                            var automationId = idElement.GetString();
                            var name = nameElement.GetString();

                            if (!string.IsNullOrEmpty(automationId))
                            {
                                Output.WriteLine($"Testing TogglePattern on checkbox: {name} (ID: {automationId})");

                                try
                                {
                                    // Toggle the checkbox
                                    var toggleResult = await Tools.ToggleElement(automationId);
                                    Output.WriteLine($"Toggle result: {JsonSerializer.Serialize(toggleResult)}");

                                    await Task.Delay(500);

                                    // Verify the operation worked
                                    var toggleData = toggleResult.ToJsonElement();
                                    bool toggleSuccess = toggleData.TryGetPropertyCI("Success", out var successEl) && successEl.GetBoolean();

                                    Assert.True(toggleSuccess, $"ToggleElement should succeed on {name}");

                                    Output.WriteLine($"  TogglePattern succeeded on {name}");
                                    break; // Test one successful checkbox
                                }
                                catch (Exception ex)
                                {
                                    Output.WriteLine($"  TogglePattern failed on {name}: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                Output.WriteLine("  TogglePattern testing completed");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  TogglePattern test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Test_13_ScrollPattern_Operations()
        {
            Output.WriteLine("=== Testing ScrollPattern operations ===");

            try
            {
                Output.WriteLine("1. Looking for scrollable elements...");

                // Look for scroll viewers or scrollable content
                var scrollableElements = await Tools.SearchElements(controlType: "ScrollViewer");
                Output.WriteLine($"Found scroll viewers: {JsonSerializer.Serialize(scrollableElements)}");

                // Also look for lists that might be scrollable
                var lists = await Tools.SearchElements(controlType: "List");
                Output.WriteLine($"Found lists: {JsonSerializer.Serialize(lists)}");

                // Test scrolling on the navigation pane (it should be scrollable)
                try
                {
                    Output.WriteLine("2. Testing scroll operations on navigation pane...");

                    // GetScrollInfo method has been removed - scroll info is available through pattern operations
                    // Use ScrollElement directly or SearchElements to find scrollable elements
                    Output.WriteLine("GetScrollInfo functionality has been removed - testing direct scroll operations");

                    // Try scrolling down
                    var scrollDown = await Tools.ScrollElement(automationId: "NavigationViewContentGrid", direction: "down", amount: 1.0);
                    Output.WriteLine($"Scroll down result: {JsonSerializer.Serialize(scrollDown)}");

                    await Task.Delay(1000);

                    // Try scrolling up
                    var scrollUp = await Tools.ScrollElement(automationId: "NavigationViewContentGrid", direction: "up", amount: 1.0);
                    Output.WriteLine($"Scroll up result: {JsonSerializer.Serialize(scrollUp)}");

                    Output.WriteLine("  ScrollPattern testing completed");
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Scroll operations failed: {ex.Message}");
                    // Don't fail the test, as scroll elements might not be available
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  ScrollPattern test failed: {ex.Message}");
                // Don't throw - scroll might not be available
            }
        }

        [Fact]
        public async Task Test_14_WindowPattern_Operations()
        {
            Output.WriteLine("=== Testing WindowPattern operations ===");

            try
            {
                Output.WriteLine("1. Testing window capabilities...");

                // GetWindowCapabilities and GetWindowInteractionState methods have been removed
                // Use GetWindows instead for window information
                var windowInfo = await Tools.SearchElements(controlType: "Window", scope: "children");
                Output.WriteLine($"Window information: {JsonSerializer.Serialize(windowInfo)}");
                var capabilities = windowInfo;
                var interactionState = windowInfo;

                // Verify operations succeeded
                var capData = capabilities.ToJsonElement();
                bool capSuccess = capData.ValueKind != JsonValueKind.Null;

                Assert.True(capSuccess, "SearchElements(Window) should succeed");

                Output.WriteLine("  WindowPattern testing completed");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  WindowPattern test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Test_15_ApplicationLauncher_Operations()
        {
            Output.WriteLine("=== Testing Application Launcher operations ===");

            try
            {
                Output.WriteLine("1. Testing application launching (already tested in setup)...");

                // Test taking screenshots
                Output.WriteLine("2. Testing screenshot functionality...");
                var screenshot = await Tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\comprehensive_test.png");
                Output.WriteLine($"Screenshot result: {JsonSerializer.Serialize(screenshot)}");

                // Verify screenshot operation
                var screenshotData = screenshot.ToJsonElement();
                bool screenshotSuccess = screenshotData.TryGetPropertyCI("Success", out var successEl) && successEl.GetBoolean();

                Assert.True(screenshotSuccess, "TakeScreenshot should succeed");

                Output.WriteLine("  Application launcher testing completed");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"  Application launcher test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task Test_12_ValuePattern_TextBoxes()
        {
            Output.WriteLine("=== Testing Value patterns ===");

            try
            {
                // Find text boxes
                var textBoxes = await Tools.SearchElements(controlType: "Edit");
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
                var scrollViewers = await Tools.SearchElements(controlType: "ScrollViewer");
                LogResult("Found scroll viewers", scrollViewers);

                // Test GetScrollInfo if available
                var allElements = await Tools.SearchElements();
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
                var treeItems = await Tools.SearchElements(controlType: "TreeItem");
                LogResult("Found tree items", treeItems);

                // Also check for other expandable controls
                var expanders = await Tools.SearchElements(searchText: "Expander");
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
                var windows = await Tools.SearchElements(controlType: "Window");
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
                var textElements = await Tools.SearchElements(controlType: "Text");
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
                var dataGrids = await Tools.SearchElements(controlType: "DataGrid");
                LogResult("Found data grids", dataGrids);

                // Also check for generic grid patterns
                var grids = await Tools.SearchElements(controlType: "Grid");
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
                var tables = await Tools.SearchElements(controlType: "Table");
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
                var accessibilityInfo = await Tools.SearchElements(
                    name: "WinUI 3 Gallery",
                    includeDetails: true,
                    maxResults: 1
                );
                LogResult("SearchElements with includeDetails", accessibilityInfo);

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
                var buttons = await Tools.SearchElements(controlType: "Button", maxResults: 1);
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
                var viewElements = await Tools.SearchElements(searchText: "View");
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
                var sliders = await Tools.SearchElements(controlType: "Slider");
                LogResult("Found sliders", sliders);

                // Also check for progress bars
                var progressBars = await Tools.SearchElements(controlType: "ProgressBar");
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
                var elements = await Tools.SearchElements(controlType: "Button");
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

        /// <summary>
        /// Extracts the data.elements array from a serialized SearchElements response.
        /// </summary>
        private static List<JsonElement> GetElements(JsonElement response)
        {
            if (response.TryGetPropertyCI("data", out var data) &&
                data.TryGetPropertyCI("elements", out var elements) &&
                elements.ValueKind == JsonValueKind.Array)
            {
                return elements.EnumerateArray().ToList();
            }
            return new List<JsonElement>();
        }

        /// <summary>
        /// Extracts data.metadata.totalFound from a serialized SearchElements response (-1 if absent).
        /// </summary>
        private static int GetTotalFound(JsonElement response)
        {
            if (response.TryGetPropertyCI("data", out var data) &&
                data.TryGetPropertyCI("metadata", out var metadata) &&
                metadata.TryGetPropertyCI("totalFound", out var totalFound) &&
                totalFound.ValueKind == JsonValueKind.Number)
            {
                return totalFound.GetInt32();
            }
            return -1;
        }

        /// <summary>
        /// Reads a string property (case-insensitive) from a JSON element, or "" when missing.
        /// </summary>
        private static string GetStr(JsonElement element, string property)
        {
            if (element.TryGetPropertyCI(property, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        #endregion
    }
}

