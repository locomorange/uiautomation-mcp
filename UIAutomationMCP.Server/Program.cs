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
