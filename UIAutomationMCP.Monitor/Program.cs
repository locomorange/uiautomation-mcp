using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Core.Infrastructure;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Monitor.Operations;
using UIAutomationMCP.Common.Services;
using System.Reflection;

namespace UIAutomationMCP.Monitor
{
    /// <summary>
    /// Monitor process for long-term event monitoring operations
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            
            try
            {
                var monitorHost = serviceProvider.GetRequiredService<MonitorHost>();
                await monitorHost.RunAsync();
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger?.LogCritical(ex, "Monitor process failed");
                Environment.Exit(1);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Core services
            services.AddSingleton<OperationRegistry>();
            services.AddSingleton<MonitorHost>();
            services.AddSingleton<SessionManager>(provider =>
                new SessionManager(
                    provider.GetRequiredService<ILogger<SessionManager>>(),
                    provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<ElementFinderService>()));
            services.AddSingleton<ElementFinderService>();

            // Register operations
            services.AddTransient<StartEventMonitoringOperation>();
            services.AddTransient<StopEventMonitoringOperation>();
            services.AddTransient<GetEventLogOperation>();
        }
    }
}