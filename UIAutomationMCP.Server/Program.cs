using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Services.ControlTypes;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server
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
            builder.Services.AddSingleton<IScreenshotService, SubprocessBasedScreenshotService>();
            
            // Register subprocess-based UI Automation services
            builder.Services.AddSingleton<IElementSearchService, SubprocessBasedElementSearchService>();
            builder.Services.AddSingleton<ITreeNavigationService, SubprocessBasedTreeNavigationService>();
            builder.Services.AddSingleton<IInvokeService, SubprocessBasedInvokeService>();
            builder.Services.AddSingleton<IValueService, SubprocessBasedValueService>();
            builder.Services.AddSingleton<IToggleService, SubprocessBasedToggleService>();
            builder.Services.AddSingleton<ISelectionService, SubprocessBasedSelectionService>();
            builder.Services.AddSingleton<IWindowService, SubprocessBasedWindowService>();
            builder.Services.AddSingleton<ITextService, SubprocessBasedTextService>();
            builder.Services.AddSingleton<ILayoutService, SubprocessBasedLayoutService>();
            builder.Services.AddSingleton<IRangeService, SubprocessBasedRangeService>();
            builder.Services.AddSingleton<IElementInspectionService, SubprocessBasedElementInspectionService>();
            
            // Register additional subprocess-based UI Automation services
            builder.Services.AddSingleton<IGridService, SubprocessBasedGridService>();
            builder.Services.AddSingleton<ITableService, SubprocessBasedTableService>();
            builder.Services.AddSingleton<IMultipleViewService, SubprocessBasedMultipleViewService>();
            builder.Services.AddSingleton<IAccessibilityService, SubprocessBasedAccessibilityService>();
            builder.Services.AddSingleton<ICustomPropertyService, SubprocessBasedCustomPropertyService>();
            
            // Register specialized control type services (subprocess-based)
            builder.Services.AddSingleton<IComboBoxService, SubprocessBasedComboBoxService>();
            builder.Services.AddSingleton<IMenuService, SubprocessBasedMenuService>();
            builder.Services.AddSingleton<ITabService, SubprocessBasedTabService>();
            builder.Services.AddSingleton<ITreeViewService, SubprocessBasedTreeViewService>();
            builder.Services.AddSingleton<IListService, SubprocessBasedListService>();
            builder.Services.AddSingleton<ICalendarService, SubprocessBasedCalendarService>();
            builder.Services.AddSingleton<IButtonService, SubprocessBasedButtonService>();
            builder.Services.AddSingleton<IHyperlinkService, SubprocessBasedHyperlinkService>();
            
            // Register subprocess executor
            builder.Services.AddSingleton<SubprocessExecutor>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SubprocessExecutor>>();
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var workerPath = Path.Combine(baseDir, "UIAutomationMCP.Worker.exe");
                logger.LogInformation("Worker path configured: {WorkerPath}", workerPath);
                logger.LogInformation("Worker exists: {Exists}", File.Exists(workerPath));
                return new SubprocessExecutor(logger, workerPath);
            });
            
            // All UI Automation services are now handled through subprocess executor

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
                // Cleanup is now handled by the DI container automatically
            }
        }
    }
}
