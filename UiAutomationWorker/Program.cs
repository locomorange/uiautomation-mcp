using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UiAutomationWorker.Core;
using UiAutomationWorker.Services;
using UiAutomationWorker.PatternExecutors;
using UiAutomationWorker.Helpers;

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

            // Register pattern executors
            services.AddSingleton<CorePatternExecutor>();
            services.AddSingleton<LayoutPatternExecutor>();
            services.AddSingleton<TreePatternExecutor>();
            services.AddSingleton<TextPatternExecutor>();
            services.AddSingleton<WindowPatternExecutor>();
            services.AddSingleton<ElementSearchExecutor>();

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
