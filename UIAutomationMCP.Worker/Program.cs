using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.Invoke;
using UIAutomationMCP.Worker.Operations.Toggle;
using UIAutomationMCP.Worker.Operations.Value;
using UIAutomationMCP.Worker.Operations.ElementSearch;
using UIAutomationMCP.Worker.Operations.ControlTypeInfo;
using UIAutomationMCP.Worker.Operations.Grid;
using UIAutomationMCP.Worker.Operations.ElementInspection;
using UIAutomationMCP.Worker.Operations.Layout;
using UIAutomationMCP.Worker.Operations.MultipleView;
using UIAutomationMCP.Worker.Operations.Range;
using UIAutomationMCP.Worker.Operations.Selection;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Operations.Text;
using UIAutomationMCP.Worker.Operations.TreeNavigation;
using UIAutomationMCP.Worker.Operations.Window;
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
            // Add file logging for debugging but avoid console output pollution
            builder.Logging.AddProvider(new DebugLoggerProvider());
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Register helper services
            builder.Services.AddSingleton<ElementFinderService>();

            // Register basic operations as keyed services (working ones only)
            builder.Services.AddKeyedTransient<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsReadOnlyOperation>("IsReadOnly");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsOperation>("FindElements");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");


            // ControlTypeInfo operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetControlTypeInfoOperation>("GetControlTypeInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ValidateControlTypePatternsOperation>("ValidateControlTypePatterns");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsByControlTypeOperation>("FindElementsByControlType");

            // Grid operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetColumnHeaderOperation>("GetColumnHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");

            // ElementInspection operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");

            // Layout operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, DockElementOperation>("DockElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ExpandCollapseElementOperation>("ExpandCollapseElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ScrollElementOperation>("ScrollElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ScrollElementIntoViewOperation>("ScrollElementIntoView");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetScrollInfoOperation>("GetScrollInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetScrollPercentOperation>("SetScrollPercent");

            // MultipleView operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetAvailableViewsOperation>("GetAvailableViews");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetCurrentViewOperation>("GetCurrentView");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetViewNameOperation>("GetViewName");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetViewOperation>("SetView");

            // Range operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRangePropertiesOperation>("GetRangeProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRangeValueOperation>("GetRangeValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetRangeValueOperation>("SetRangeValue");


            // Selection operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, AddToSelectionOperation>("AddToSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, CanSelectMultipleOperation>("CanSelectMultiple");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ClearSelectionOperation>("ClearSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectionContainerOperation>("GetSelectionContainer");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectionOperation>("GetSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsSelectedOperation>("IsSelected");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsSelectionRequiredOperation>("IsSelectionRequired");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, RemoveFromSelectionOperation>("RemoveFromSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectElementOperation>("SelectElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectItemOperation>("SelectItem");

            // Table operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetColumnHeadersOperation>("GetColumnHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeadersOperation>("GetRowHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTableInfoOperation>("GetTableInfo");

            // Text operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, AppendTextOperation>("AppendText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindTextOperation>("FindText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectedTextOperation>("GetSelectedText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextAttributesOperation>("GetTextAttributes");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextOperation>("GetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextSelectionOperation>("GetTextSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectTextOperation>("SelectText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetTextOperation>("SetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, TraverseTextOperation>("TraverseText");

            // TreeNavigation operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetAncestorsOperation>("GetAncestors");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetChildrenOperation>("GetChildren");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDescendantsOperation>("GetDescendants");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementTreeOperation>("GetElementTree");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetParentOperation>("GetParent");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSiblingsOperation>("GetSiblings");

            // Window operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, TransformElementOperation>("TransformElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, WindowActionOperation>("WindowAction");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetWindowInfoOperation>("GetWindowInfo");

            // Register Worker service
            builder.Services.AddSingleton<WorkerService>();

            var host = builder.Build();

            var workerService = host.Services.GetRequiredService<WorkerService>();
            await workerService.RunAsync();
        }
    }
}