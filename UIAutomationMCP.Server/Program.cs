using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Tools;

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
            builder.Services.AddSingleton<IScreenshotService, DirectScreenshotService>();
            
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
            builder.Services.AddSingleton<ITransformService, SubprocessBasedTransformService>();
            
            
            // Register ControlType service
            builder.Services.AddSingleton<IControlTypeService, SubprocessBasedControlTypeService>();
            
            // Register subprocess executor
            builder.Services.AddSingleton<SubprocessExecutor>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SubprocessExecutor>>();
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Determine if we're in development or production
                var isDevelopment = baseDir.Contains("bin\\Debug") || baseDir.Contains("bin\\Release");
                
                string? workerPath = null;
                
                if (isDevelopment)
                {
                    // In development, look for the Worker project
                    var solutionDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
                    if (solutionDir != null)
                    {
                        workerPath = Path.Combine(solutionDir, "UIAutomationMCP.Worker");
                        if (!Directory.Exists(workerPath))
                        {
                            // Try the built executable
                            var config = baseDir.Contains("Debug") ? "Debug" : "Release";
                            workerPath = Path.Combine(solutionDir, "UIAutomationMCP.Worker", "bin", config, "net9.0-windows", "UIAutomationMCP.Worker.exe");
                        }
                    }
                }
                else
                {
                    // In production/tool deployment, Worker should be in same directory
                    workerPath = Path.Combine(baseDir, "UIAutomationMCP.Worker.exe");
                }

                if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
                {
                    logger.LogError("Worker not found. Searched path: {WorkerPath}", workerPath);
                    throw new InvalidOperationException($"UIAutomationMCP.Worker not found at: {workerPath}");
                }

                logger.LogInformation("Worker path configured: {WorkerPath}", workerPath);
                return new SubprocessExecutor(logger, workerPath);
            });
            
            // Also register as interface
            builder.Services.AddSingleton<ISubprocessExecutor>(provider => provider.GetRequiredService<SubprocessExecutor>());
            
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
                .WithTools<UIAutomationTools>();

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
