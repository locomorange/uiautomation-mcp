using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.ElementTree;
using UiAutomationMcpServer.Patterns.Interaction;
using UiAutomationMcpServer.Patterns.Layout;
using UiAutomationMcpServer.Patterns.Text;
using UiAutomationMcpServer.Patterns.Window;
using UiAutomationMcpServer.Patterns.Selection;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging for MCP - disable console logging to avoid MCP protocol interference
            builder.Logging.ClearProviders();
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "mcp_debug.log");
            builder.Logging.AddProvider(new FileLoggerProvider(logPath));
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Register application services
            builder.Services.AddSingleton<IApplicationLauncher, ApplicationLauncher>();
            builder.Services.AddSingleton<IScreenshotService, ScreenshotService>();
            
            // Register UI Automation pattern handlers
            builder.Services.AddSingleton<ElementSearchHandler>();
            builder.Services.AddSingleton<TreeNavigationHandler>();
            builder.Services.AddSingleton<InvokePatternHandler>();
            builder.Services.AddSingleton<ValuePatternHandler>();
            builder.Services.AddSingleton<TogglePatternHandler>();
            builder.Services.AddSingleton<SelectionItemPatternHandler>();
            builder.Services.AddSingleton<LayoutPatternHandler>();
            builder.Services.AddSingleton<TextPatternHandler>();
            builder.Services.AddSingleton<WindowPatternHandler>();
            
            // Register helper services
            builder.Services.AddSingleton<AutomationHelper>();
            builder.Services.AddSingleton<ElementInfoExtractor>();
            builder.Services.AddSingleton<IDiagnosticService, DiagnosticService>();
            
            // Register in-process UI automation worker (no external process needed)
            builder.Services.AddSingleton<IUIAutomationWorker, InProcessUIAutomationWorker>();
            builder.Services.AddSingleton<IUIAutomationService, UIAutomationService>();

            // Configure MCP Server
            builder.Services
                .AddMcpServer(options =>
                {
                    options.ServerInfo = new()
                    {
                        Name = "UIAutomation MCP Server",
                        Version = "1.0.0"
                    };
                })
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            var host = builder.Build();

            // Setup graceful shutdown handling
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("[Program] MCP Server shutdown requested - cleaning up threads");
            });

            // Run the MCP server with proper shutdown handling
            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "[Program] MCP Server terminated with error");
                throw;
            }
            finally
            {
                // Cleanup thread-based worker
                try
                {
                    var worker = host?.Services?.GetService<IUIAutomationWorker>();
                    if (worker is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Services already disposed, nothing to clean up
                }
            }
        }
    }
}