using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UiAutomationWorker.Core;
using UiAutomationWorker.ElementTree;
using UiAutomationWorker.Helpers;
using UiAutomationWorker.Patterns.Interaction;
using UiAutomationWorker.Patterns.Layout;
using UiAutomationWorker.Patterns.Selection;
using UiAutomationWorker.Patterns.Text;
using UiAutomationWorker.Patterns.Window;
using UiAutomationWorker.Services;

namespace UiAutomationWorker
{
    /// <summary>
    /// Standalone worker process for UI Automation operations
    /// Prevents main process from hanging due to COM/native API blocking
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Initialize dependency injection and logging
            var services = new ServiceCollection();
 
            // Initialize logging - disable console output to avoid interfering with JSON output
            services.AddLogging(builder =>
                builder.SetMinimumLevel(LogLevel.Error)); // Only log errors, and not to console

            // Register helper services
            services.AddSingleton<AutomationHelper>();
            services.AddSingleton<ElementInfoExtractor>();

            // Register pattern handlers
            services.AddSingleton<InvokePatternHandler>();
            services.AddSingleton<ValuePatternHandler>();
            services.AddSingleton<TogglePatternHandler>();
            services.AddSingleton<SelectionItemPatternHandler>();
            services.AddSingleton<LayoutPatternHandler>();
            services.AddSingleton<TreeNavigationHandler>();
            services.AddSingleton<TextPatternHandler>();
            services.AddSingleton<WindowPatternHandler>();
            services.AddSingleton<ElementSearchHandler>();

            // Register main services
            services.AddSingleton<OperationExecutor>();

            // Register application services
            services.AddSingleton<InputProcessor>();
            services.AddSingleton<OutputProcessor>();
            
            using var serviceProvider = services.BuildServiceProvider();

            // Create and run the application host
            var applicationHost = new WorkerApplicationHost(serviceProvider);
            return await applicationHost.RunAsync();
        }

    }
}
