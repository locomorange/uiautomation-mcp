using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Subprocess.Monitor.Infrastructure;
using UIAutomationMCP.Subprocess.Monitor.Operations;
using UIAutomationMCP.Subprocess.Core.Extensions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Models.Requests;

namespace UIAutomationMCP.Subprocess.Monitor
{
    /// <summary>
    /// Monitor process for long-term event monitoring operations
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging - disable console logging to avoid interference with JSON responses
            builder.Logging.ClearProviders();
            // Add debug logging for diagnostics but avoid console output pollution
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Core services
            builder.Services.AddSingleton<ElementFinderService>();
            builder.Services.AddSingleton<SessionManager>(provider =>
                new SessionManager(
                    provider.GetRequiredService<ILogger<SessionManager>>(),
                    provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<ElementFinderService>()));

            // Register monitor operations using new extension methods
            builder.Services.AddOperation<StartEventMonitoringOperation, StartEventMonitoringRequest>();
            builder.Services.AddOperation<StopEventMonitoringOperation, StopEventMonitoringRequest>();
            builder.Services.AddOperation<GetEventLogOperation, GetEventLogRequest>();

            // Register Monitor service
            builder.Services.AddSingleton<MonitorService>();

            var host = builder.Build();

            try
            {
                await ProcessLogRelay.LogInfoAsync("Monitor.Program", "Monitor process starting", "monitor");

                var monitorService = host.Services.GetRequiredService<MonitorService>();
                await monitorService.RunAsync();
            }
            catch (Exception ex)
            {
                await ProcessLogRelay.LogErrorAsync("Monitor.Program", "Monitor process failed", "monitor", ex);

                var logger = host.Services.GetService<ILogger<Program>>();
                logger?.LogCritical(ex, "Monitor process failed");
                Environment.Exit(1);
            }
            finally
            {
                await ProcessLogRelay.LogInfoAsync("Monitor.Program", "Monitor process shutting down", "monitor");
                host.Dispose();
            }
        }

    }
}

