using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Abstractions;

namespace UIAutomationMCP.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);

            // Register shutdown CancellationTokenSource for graceful shutdown
            var shutdownCts = new CancellationTokenSource();
            builder.Services.AddSingleton(shutdownCts);

            // Configure logging for MCP - stderr logging to avoid MCP protocol interference
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Debug);
            builder.Logging.SetMinimumLevel(LogLevel.Information);

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
            builder.Services.AddSingleton<IEventMonitorService, EventMonitorService>();
            builder.Services.AddSingleton<IFocusService, FocusService>();
            
            
            // Register ControlType service
            builder.Services.AddSingleton<IControlTypeService, ControlTypeService>();
            
            // Register ProcessManager for worker and monitor process management
            builder.Services.AddSingleton<ProcessManager>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ProcessManager>>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Determine if we're in development or production
                var isDevelopment = baseDir.Contains("bin\\Debug") || baseDir.Contains("bin\\Release");
                
                string? workerPath = null;
                string? monitorPath = null;
                
                if (isDevelopment)
                {
                    // In development, look for the Worker and Monitor projects
                    var solutionDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
                    if (solutionDir != null)
                    {
                        var config = baseDir.Contains("Debug") ? "Debug" : "Release";
                        
                        // Worker path
                        workerPath = Path.Combine(solutionDir, "UIAutomationMCP.Worker");
                        if (!Directory.Exists(workerPath))
                        {
                            workerPath = Path.Combine(solutionDir, "UIAutomationMCP.Worker", "bin", config, "net9.0-windows", "UIAutomationMCP-Worker.exe");
                        }
                        
                        // Monitor path
                        monitorPath = Path.Combine(solutionDir, "UIAutomationMCP.Monitor", "bin", config, "net9.0-windows", "UIAutomationMCP.Monitor.exe");
                        if (!File.Exists(monitorPath))
                        {
                            // Fallback to directory if exe not found
                            monitorPath = Path.Combine(solutionDir, "UIAutomationMCP.Monitor");
                        }
                    }
                }
                else
                {
                    // In production/tool deployment, executables should be in same directory
                    workerPath = Path.Combine(baseDir, "UIAutomationMCP-Worker.exe");
                    monitorPath = Path.Combine(baseDir, "UIAutomationMCP.Monitor.exe");
                }

                // Validate worker path (required)
                if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
                {
                    var searchedPaths = new List<string>();
                    if (!string.IsNullOrEmpty(workerPath))
                        searchedPaths.Add(workerPath);
                    
                    var solutionDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
                    if (solutionDir != null)
                    {
                        searchedPaths.Add(Path.Combine(solutionDir, "UIAutomationMCP.Worker"));
                        var config = baseDir.Contains("Debug") ? "Debug" : "Release";
                        searchedPaths.Add(Path.Combine(solutionDir, "UIAutomationMCP.Worker", "bin", config, "net9.0-windows", "UIAutomationMCP-Worker.exe"));
                    }
                    searchedPaths.Add(Path.Combine(baseDir, "UIAutomationMCP-Worker.exe"));

                    logger.LogError("Worker not found. Base directory: {BaseDir}. Searched paths: {SearchedPaths}", 
                        baseDir, string.Join(", ", searchedPaths));
                    
                    throw new InvalidOperationException($"UIAutomationMCP.Worker not found. Searched: {string.Join(", ", searchedPaths)}");
                }

                // Validate monitor path (optional - will fallback to worker if not available)
                if (monitorPath != null && !File.Exists(monitorPath) && !Directory.Exists(monitorPath))
                {
                    logger.LogWarning("Monitor process not found at {MonitorPath}. Monitor operations will fallback to Worker process.", monitorPath);
                    monitorPath = null;
                }

                logger.LogInformation("ProcessManager configured - Worker: {WorkerPath}, Monitor: {MonitorPath}", 
                    workerPath, monitorPath ?? "Not available (fallback to Worker)");
                
                var shutdownCts = provider.GetRequiredService<CancellationTokenSource>();
                return new ProcessManager(logger, loggerFactory, shutdownCts, workerPath, monitorPath);
            });
            
            // Register ProcessManager as both IProcessManager and IOperationExecutor
            builder.Services.AddSingleton<IProcessManager>(provider => provider.GetRequiredService<ProcessManager>());
            builder.Services.AddSingleton<IOperationExecutor>(provider => provider.GetRequiredService<ProcessManager>());
            
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
                logger.LogInformation("[Program] stdin closed - no new requests will be accepted, waiting for current operations to complete naturally");
                
                // DO NOT cancel shutdown token here - let operations complete with their normal timeouts
                // The server will shut down naturally after:
                // 1. Current operations complete (with normal timeouts like 60s for ApplicationLauncher)
                // 2. JSON responses are fully sent to stdout  
                // 3. MCP protocol completes gracefully
            });

            // Run the MCP server with proper shutdown handling
            try
            {
                // Console.WriteLine("DEBUG: Starting MCP server");
                // Console.Out.Flush();
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
                // Minimal cleanup - let MCP protocol complete naturally
                try
                {
                    // Ensure all output is flushed before disposing
                    Console.Out.Flush();
                    Console.Error.Flush();
                }
                catch (Exception flushEx)
                {
                    Console.Error.WriteLine($"[Program] Error flushing streams: {flushEx.Message}");
                }
                
                // Simple host disposal
                try
                {
                    host.Dispose();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Program] Error disposing host: {ex.Message}");
                }
            }
        }
    }
}
