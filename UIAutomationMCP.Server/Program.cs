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
            builder.Services.AddSingleton<IScreenshotService, ScreenshotService>();
            
            // Register simple UI Automation services
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
            
            // Register additional UI Automation services
            builder.Services.AddSingleton<IGridService, GridService>();
            builder.Services.AddSingleton<ITableService, TableService>();
            builder.Services.AddSingleton<IMultipleViewService, MultipleViewService>();
            // Register specialized control type services
            builder.Services.AddSingleton<IComboBoxService, ComboBoxService>();
            builder.Services.AddSingleton<IMenuService, MenuService>();
            builder.Services.AddSingleton<ITabService, TabService>();
            builder.Services.AddSingleton<ITreeViewService, TreeViewService>();
            builder.Services.AddSingleton<IListService, ListService>();
            builder.Services.AddSingleton<ICalendarService, CalendarService>();
            builder.Services.AddSingleton<IButtonService, ButtonService>();
            builder.Services.AddSingleton<IHyperlinkService, HyperlinkService>();
            builder.Services.AddSingleton<IAccessibilityService, AccessibilityService>();
            builder.Services.AddSingleton<ICustomPropertyService, CustomPropertyService>();
            
            // Register helper services
            builder.Services.AddSingleton<UIAutomationExecutor>();
            builder.Services.AddSingleton<AutomationHelper>();
            builder.Services.AddSingleton<ElementInfoExtractor>();
            
            // UI Automation services are now handled directly through pattern handlers

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