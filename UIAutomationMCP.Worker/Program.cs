using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.Invoke;
using UIAutomationMCP.Worker.Operations.Toggle;
using UIAutomationMCP.Worker.Operations.Value;
using UIAutomationMCP.Worker.Operations.ElementSearch;
using UIAutomationMCP.Worker.Operations.ControlType;
using UIAutomationMCP.Worker.Operations.Grid;
using UIAutomationMCP.Worker.Operations.ElementInspection;
using UIAutomationMCP.Worker.Operations.Layout;
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

            // Register basic operations as keyed services (working ones only)
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, FindElementsOperation>("FindElements");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");

            // ControlType operations
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ButtonOperation>("ButtonOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, CalendarOperation>("CalendarOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ComboBoxOperation>("ComboBoxOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, HyperlinkOperation>("HyperlinkOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ListOperation>("ListOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, MenuOperation>("MenuOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TabOperation>("TabOperation");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, TreeViewOperation>("TreeViewOperation");

            // Grid operations
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetColumnHeaderOperation>("GetColumnHeader");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");

            // ElementInspection operations
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");

            // Layout operations
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, DockElementOperation>("DockElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ExpandCollapseElementOperation>("ExpandCollapseElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ScrollElementOperation>("ScrollElement");
            builder.Services.AddKeyedSingleton<IUIAutomationOperation, ScrollElementIntoViewOperation>("ScrollElementIntoView");

            // Register Worker service
            builder.Services.AddSingleton<WorkerService>();

            var host = builder.Build();

            var workerService = host.Services.GetRequiredService<WorkerService>();
            await workerService.RunAsync();
        }
    }
}