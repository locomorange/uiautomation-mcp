using Microsoft.Extensions.DependencyInjection;
using UiAutomationWorker.Configuration;
using UiAutomationWorker.Core;

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
            DependencyInjectionConfig.ConfigureServices(services);
            
            using var serviceProvider = services.BuildServiceProvider();

            // Create and run the application host
            var applicationHost = new WorkerApplicationHost(serviceProvider);
            return await applicationHost.RunAsync();
        }
    }
}
