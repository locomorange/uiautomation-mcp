using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Reflection;
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

            // Configure logging for MCP - remove all providers and use only CompositeMcpLoggerProvider
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Configure MCP logging after host is built
            builder.Services.Configure<HostOptions>(options =>
            {
                options.ServicesStartConcurrently = true;
                options.ServicesStopConcurrently = true;
            });

            // Register log relay service for subprocess logs
            builder.Services.AddSingleton<IMcpLogService, LogRelayService>();

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
                var baseDir = ExecutablePathResolver.GetExecutableRealPath();
                logger.LogInformation("ProcessManager initialization - Base directory: {BaseDir}", baseDir);

                // Use centralized path resolution
                var isDevelopment = ExecutablePathResolver.IsDevEnvironment(baseDir);
                logger.LogInformation("Development mode: {IsDevelopment}", isDevelopment);

                if (isDevelopment)
                {
                    var solutionDir = ExecutablePathResolver.FindSolutionDirectory(baseDir);
                    logger.LogInformation("Solution directory: {SolutionDir}", solutionDir ?? "null");
                }

                // Resolve Worker and Monitor paths using centralized logic
                var workerPath = ExecutablePathResolver.ResolveWorkerPath(baseDir);
                var monitorPath = ExecutablePathResolver.ResolveMonitorPath(baseDir);

                // Validate worker path (required)
                if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
                {
                    var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Worker", baseDir);

                    logger.LogError("Worker not found. Base directory: {BaseDir}. Searched paths: {SearchedPaths}",
                        baseDir, string.Join(", ", searchedPaths));

                    throw new InvalidOperationException($"UIAutomationMCP.Worker not found. Searched: {string.Join(", ", searchedPaths)}");
                }

                // Validate monitor path (required)
                if (monitorPath == null || (!File.Exists(monitorPath) && !Directory.Exists(monitorPath)))
                {
                    var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Monitor", baseDir);

                    logger.LogError("Monitor not found. Base directory: {BaseDir}. Searched paths: {SearchedPaths}",
                        baseDir, string.Join(", ", searchedPaths));

                    throw new InvalidOperationException($"UIAutomationMCP.Monitor not found. Searched: {string.Join(", ", searchedPaths)}");
                }

                logger.LogInformation("ProcessManager configured - Worker: {WorkerPath}, Monitor: {MonitorPath}",
                    workerPath, monitorPath);

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

            var host = builder.Build();

            // Configure MCP logging after host is built
            try
            {
                var mcpServer = host.Services.GetService<IMcpServer>();
                var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                
                if (mcpServer != null)
                {
                    // Create MCP options for logging
                    var mcpOptions = new UIAutomationMCP.Server.Infrastructure.Logging.McpLoggerOptions
                    {
                        EnableNotifications = true,
                        EnableFileOutput = true,
                        FileOutputPath = "mcp-logs.json",
                        FileOutputFormat = "json"
                    };
                    
                    // Add McpLoggerProvider as the main logger provider
                    loggerFactory.AddProvider(new UIAutomationMCP.Server.Infrastructure.Logging.McpLoggerProvider(mcpServer, mcpOptions));
                    
                }
                else
                {
                    Console.Error.WriteLine("Warning: IMcpServer not found - MCP logging disabled");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Failed to configure MCP logging: {ex.Message}");
            }


            // Simple MCP server - let the framework handle lifecycle
            await host.RunAsync();
        }
    }
}

