using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.Invoke;
using UIAutomationMCP.Worker.Operations.Toggle;
using UIAutomationMCP.Worker.Operations.Value;
using UIAutomationMCP.Worker.Operations.ElementSearch;
using UIAutomationMCP.Worker.Operations.Window;
using UIAutomationMCP.Worker.Operations.Selection;
using UIAutomationMCP.Worker.Operations.Text;
using UIAutomationMCP.Worker.Operations.Layout;
using UIAutomationMCP.Worker.Operations.Range;
using UIAutomationMCP.Worker.Operations.TreeNavigation;
using UIAutomationMCP.Worker.Operations.ElementInspection;
using UIAutomationMCP.Worker.Operations.Grid;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Operations.MultipleView;
using UIAutomationMCP.Worker.Operations.ControlType;
using UIAutomationMCP.Worker.Operations.Accessibility;
using UIAutomationMCP.Worker.Operations.CustomProperty;
using UIAutomationMCP.Worker.Operations.Screenshot;
using UIAutomationMCP.Worker.Helpers;
using System.Text.Json;

namespace UIAutomationMCP.Worker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging - disable console logging to avoid interference with JSON responses
            builder.Logging.ClearProviders();
            // Add file logging only for Worker to avoid standard output pollution
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            // Register helper services
            builder.Services.AddSingleton<ElementFinderService>();

            // Register handlers as keyed services
            // Invoke handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            
            // Toggle handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            
            // Value handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            
            // Element search handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, FindElementsOperation>("FindElements");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");
            
            // Window handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, WindowActionOperation>("WindowAction");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TransformElementOperation>("TransformElement");
            
            // Selection handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SelectElementOperation>("SelectElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SelectItemOperation>("SelectItem");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, AddToSelectionOperation>("AddToSelection");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, RemoveFromSelectionOperation>("RemoveFromSelection");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ClearSelectionOperation>("ClearSelection");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetSelectionOperation>("GetSelection");
            
            // Text handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetTextOperation>("GetText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SelectTextOperation>("SelectText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, FindTextOperation>("FindText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetTextSelectionOperation>("GetTextSelection");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetTextOperation>("SetText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TraverseTextOperation>("TraverseText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetTextAttributesOperation>("GetTextAttributes");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, AppendTextOperation>("AppendText");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetSelectedTextOperation>("GetSelectedText");
            
            // Layout handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ExpandCollapseElementOperation>("ExpandCollapseElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ScrollElementOperation>("ScrollElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ScrollElementIntoViewOperation>("ScrollElementIntoView");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, DockElementOperation>("DockElement");
            
            // Range handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetRangeValueOperation>("SetRangeValue");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetRangeValueOperation>("GetRangeValue");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetRangePropertiesOperation>("GetRangeProperties");
            
            // Tree navigation handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetChildrenOperation>("GetChildren");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetParentOperation>("GetParent");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetSiblingsOperation>("GetSiblings");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetDescendantsOperation>("GetDescendants");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetAncestorsOperation>("GetAncestors");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementTreeOperation>("GetElementTree");
            
            // Element inspection handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");
            
            // Grid handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetColumnHeaderOperation>("GetColumnHeader");
            
            // Table handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetTableInfoOperation>("GetTableInfo");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetRowHeadersOperation>("GetRowHeaders");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetColumnHeadersOperation>("GetColumnHeaders");
            
            // Multiple view handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetAvailableViewsOperation>("GetAvailableViews");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetViewOperation>("SetView");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetCurrentViewOperation>("GetCurrentView");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetViewNameOperation>("GetViewName");
            
            // Control type handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ButtonOperation>("ButtonOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, CalendarOperation>("CalendarOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ComboBoxOperation>("ComboBoxOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, HyperlinkOperation>("HyperlinkOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ListOperation>("ListOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, MenuOperation>("MenuOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TabOperation>("TabOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TreeViewOperation>("TreeViewOperation");
            
            // Accessibility handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetAccessibilityInfoOperation>("GetAccessibilityInfo");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, VerifyAccessibilityOperation>("VerifyAccessibility");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetLabeledByOperation>("GetLabeledBy");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetDescribedByOperation>("GetDescribedBy");
            
            // Custom property handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetCustomPropertiesOperation>("GetCustomProperties");
            
            // Screenshot handlers
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TakeScreenshotOperation>("TakeScreenshot");

            // Register Worker service
            builder.Services.AddSingleton<WorkerService>();

            var host = builder.Build();

            var workerService = host.Services.GetRequiredService<WorkerService>();
            await workerService.RunAsync();
        }
    }
}
