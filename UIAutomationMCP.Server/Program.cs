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
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Server
{
    class Program
    {
        /// <summary>
        /// Find the solution directory by looking for .sln files or known project directories
        /// </summary>
        private static string? FindSolutionDirectory(string startDir)
        {
            var current = new DirectoryInfo(startDir);
            
            // Search up the directory tree for solution indicators
            while (current != null)
            {
                // Look for .sln files
                if (current.GetFiles("*.sln").Length > 0)
                {
                    return current.FullName;
                }
                
                // Look for the Worker project directory as an indicator
                var workerDir = Path.Combine(current.FullName, "UIAutomationMCP.Subprocess.Worker");
                if (Directory.Exists(workerDir))
                {
                    return current.FullName;
                }
                
                current = current.Parent;
            }
            
            return null;
        }

        static async Task Main(string[] args)
        {
            // Configure Console output for MCP STDIO transport
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var builder = Host.CreateApplicationBuilder(args);

            // Register shutdown CancellationTokenSource for graceful shutdown
            var shutdownCts = new CancellationTokenSource();
            builder.Services.AddSingleton(shutdownCts);

            // Configure logging for MCP - stderr only to avoid stdout protocol interference
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Trace;
            });
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Register MCP logging service
            builder.Services.AddSingleton<IMcpLogService, McpLoggingService>();

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
                    // Simple approach: Use project directories directly
                    // SubprocessExecutor will use 'dotnet run' which handles building automatically
                    var solutionDir = FindSolutionDirectory(baseDir);
                    if (solutionDir != null)
                    {
                        var workerProjectDir = Path.Combine(solutionDir, "UIAutomationMCP.Subprocess.Worker");
                        var monitorProjectDir = Path.Combine(solutionDir, "UIAutomationMCP.Subprocess.Monitor");
                        
                        if (Directory.Exists(workerProjectDir))
                        {
                            workerPath = workerProjectDir;
                            logger.LogInformation("Using Worker project directory: {WorkerPath}", workerPath);
                        }
                        
                        if (Directory.Exists(monitorProjectDir))
                        {
                            monitorPath = monitorProjectDir;
                            logger.LogInformation("Using Monitor project directory: {MonitorPath}", monitorPath);
                        }
                    }
                }
                else
                {
                    // In production/tool deployment, try multiple possible locations
                    var parentDir = Directory.GetParent(baseDir);
                    var searchPaths = new[]
                    {
                        // Same directory as server
                        Path.Combine(baseDir, "UIAutomationMCP.Subprocess.Worker.exe"),
                        // Worker subdirectory under current directory
                        Path.Combine(baseDir, "Worker", "UIAutomationMCP.Subprocess.Worker.exe"),
                        // Parent Worker directory (for publish structure like publish/aot-win-x64/Server -> publish/aot-win-x64/Worker)
                        Path.Combine(parentDir?.FullName ?? baseDir, "Worker", "UIAutomationMCP.Subprocess.Worker.exe"),
                        // Grandparent Worker directory (for nested publish structure)
                        Path.Combine(parentDir?.Parent?.FullName ?? baseDir, "Worker", "UIAutomationMCP.Subprocess.Worker.exe")
                    };

                    workerPath = searchPaths.FirstOrDefault(File.Exists);

                    // Monitor search paths
                    var monitorSearchPaths = new[]
                    {
                        // Same directory as server
                        Path.Combine(baseDir, "UIAutomationMCP.Subprocess.Monitor.exe"),
                        // Monitor subdirectory under current directory
                        Path.Combine(baseDir, "Monitor", "UIAutomationMCP.Subprocess.Monitor.exe"),
                        // Parent Monitor directory (for publish structure like publish/aot-win-x64/Server -> publish/aot-win-x64/Monitor)
                        Path.Combine(parentDir?.FullName ?? baseDir, "Monitor", "UIAutomationMCP.Subprocess.Monitor.exe"),
                        // Grandparent Monitor directory (for nested publish structure)
                        Path.Combine(parentDir?.Parent?.FullName ?? baseDir, "Monitor", "UIAutomationMCP.Subprocess.Monitor.exe")
                    };

                    monitorPath = monitorSearchPaths.FirstOrDefault(File.Exists);
                }

                // Validate worker path (required)
                if (workerPath == null || !Directory.Exists(workerPath))
                {
                    logger.LogError("Worker project directory not found. Base directory: {BaseDir}", baseDir);
                    throw new InvalidOperationException("UIAutomationMCP.Subprocess.Worker project directory not found. Ensure the project is in the solution.");
                }

                // Validate monitor path (optional - will fallback to worker if not available)
                if (monitorPath != null && !Directory.Exists(monitorPath))
                {
                    logger.LogWarning("Monitor project directory not found. Monitor operations will fallback to Worker process.");
                    monitorPath = null;
                }

                logger.LogInformation("ProcessManager configured - Worker: {WorkerPath}, Monitor: {MonitorPath}",
                    workerPath, monitorPath ?? "Not available (fallback to Worker)");

                var shutdownCts = provider.GetRequiredService<CancellationTokenSource>();
                var processManager = new ProcessManager(logger, loggerFactory, shutdownCts, workerPath, monitorPath);

                // Set MCP log service for subprocess log relay
                var mcpLogService = provider.GetRequiredService<IMcpLogService>();
                processManager.SetMcpLogService(mcpLogService);

                return processManager;
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

            // Configure hosted service to set MCP endpoint after initialization
            builder.Services.AddHostedService<McpEndpointConfiguration>();

            var host = builder.Build();

            // Test ApplicationLauncher directly if no arguments provided
            if (args.Length > 0 && args[0] == "--test-app-launcher")
            {
                var mcpLog = host.Services.GetRequiredService<IMcpLogService>();
                await mcpLog.LogInformationAsync("Program", "Testing ApplicationLauncher directly...");

                var launcher = host.Services.GetRequiredService<IApplicationLauncher>();

                try
                {
                    await mcpLog.LogInformationAsync("Program", "Launching calculator...");
                    var result = await launcher.LaunchApplicationAsync("calc", null, null, 60);

                    await mcpLog.LogInformationAsync("Program", $"Launch result: Success={result.Success}, ProcessId={result.ProcessId}");
                    if (!result.Success)
                    {
                        await mcpLog.LogErrorAsync("Program", $"Launch failed: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    await mcpLog.LogErrorAsync("Program", "Launch exception occurred", ex);
                }

                return;
            }

            // Simple MCP server - let the framework handle lifecycle
            await host.RunAsync();
        }
    }
}

