using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services.Elements;
using UiAutomationMcpServer.Services.Patterns;
using System.IO;

namespace UiAutomationMcpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging for MCP - temporarily enable file logging for debugging
            builder.Logging.ClearProviders();
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "mcp_debug.log");
            builder.Logging.AddProvider(new UiAutomationMcpServer.Services.FileLoggerProvider(logPath));
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Register Windows services
            builder.Services.AddSingleton<IWindowService, WindowService>();
            builder.Services.AddSingleton<IScreenshotService, ScreenshotService>();
            
            // Register Element services
            builder.Services.AddSingleton<IElementDiscoveryService, ElementDiscoveryService>();
            builder.Services.AddSingleton<IElementTreeService, ElementTreeService>();
            builder.Services.AddSingleton<IElementPropertiesService, ElementPropertiesService>();
            
            // Register Pattern services
            builder.Services.AddSingleton<ICorePatternService, CorePatternService>();
            builder.Services.AddSingleton<ILayoutPatternService, LayoutPatternService>();
            builder.Services.AddSingleton<IRangePatternService, RangePatternService>();
            builder.Services.AddSingleton<IWindowPatternService, WindowPatternService>();
            builder.Services.AddSingleton<ITextPatternService, TextPatternService>();
            builder.Services.AddSingleton<IAdvancedPatternService, AdvancedPatternService>();

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

            // Run the MCP server without logging to avoid interference
            await host.RunAsync();
        }
    }
}