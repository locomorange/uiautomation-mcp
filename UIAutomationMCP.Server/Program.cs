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
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Register application services
            builder.Services.AddSingleton<IApplicationLauncher, ApplicationLauncher>();
            builder.Services.AddSingleton<IScreenshotService, ScreenshotService>();
            
            // Register subprocess-based UI Automation services
            builder.Services.AddSingleton<IElementSearchService, ElementSearchService>();
            builder.Services.AddSingleton<ITreeNavigationService, TreeNavigationService>();
            builder.Services.AddSingleton<IInvokeService, InvokeService>();
            builder.Services.AddSingleton<IValueService, ValueService>();
            builder.Services.AddSingleton<IToggleService, ToggleService>();
            builder.Services.AddSingleton<ISelectionService, SelectionService>();
            builder.Services.AddSingleton<IWindowService, WindowService>();
            builder.Services.AddSingleton<ITextService, TextService>();
            builder.Services.AddSingleton<ILayoutService, LayoutService>();
            builder.Services.AddSingleton<IRangeService, RangeService>();
            builder.Services.AddSingleton<IElementInspectionService, ElementInspectionService>();
            
            // Register additional subprocess-based UI Automation services
            builder.Services.AddSingleton<IGridService, GridService>();
            builder.Services.AddSingleton<ITableService, TableService>();
            builder.Services.AddSingleton<IMultipleViewService, MultipleViewService>();
            builder.Services.AddSingleton<IAccessibilityService, AccessibilityService>();
            builder.Services.AddSingleton<ICustomPropertyService, CustomPropertyService>();
            builder.Services.AddSingleton<ITransformService, TransformService>();
            builder.Services.AddSingleton<IVirtualizedItemService, VirtualizedItemService>();
            builder.Services.AddSingleton<IItemContainerService, ItemContainerService>();
            builder.Services.AddSingleton<ISynchronizedInputService, SynchronizedInputService>();
            
            
            // Register ControlType service
            builder.Services.AddSingleton<IControlTypeService, ControlTypeService>();
            
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
                            workerPath = Path.Combine(solutionDir, "UIAutomationMCP.Worker", "bin", config, "net9.0-windows", "UIAutomationMCP-Worker.exe");
                        }
                    }
                }
                else
                {
                    // In production/tool deployment, Worker should be in same directory
                    workerPath = Path.Combine(baseDir, "UIAutomationMCP-Worker.exe");
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

            // Setup graceful shutdown handling with proper cleanup
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var cancellationTokenSource = new CancellationTokenSource();
            
            // Handle console cancellation (Ctrl+C)
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("[Program] Shutdown signal received, initiating graceful shutdown");
                cancellationTokenSource.Cancel();
            };

            lifetime.ApplicationStopping.Register(() =>
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("[Program] MCP Server shutdown requested - cleaning up resources");
                
                // Dispose SubprocessExecutor to ensure worker processes are terminated
                try
                {
                    var executor = host.Services.GetRequiredService<SubprocessExecutor>();
                    executor.Dispose();
                    logger.LogInformation("[Program] SubprocessExecutor disposed successfully");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Program] Error disposing SubprocessExecutor");
                }
            });

            // Run the MCP server with proper shutdown handling
            try
            {
                await host.RunAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected during graceful shutdown
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("[Program] MCP Server shutdown completed");
            }
            catch (Exception ex)
            {
                // Don't try to access disposed services during shutdown
                try
                {
                    var logger = host.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "[Program] MCP Server terminated with error");
                }
                catch (ObjectDisposedException)
                {
                    // Services already disposed during shutdown - ignore logging
                }
                throw;
            }
            finally
            {
                // Ensure all resources are cleaned up
                try
                {
                    host.Dispose();
                }
                catch (Exception ex)
                {
                    try
                    {
                        var logger = host.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "[Program] Error during host disposal");
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore if services are already disposed
                    }
                }
            }
        }
    }
}
