using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using UiAutomationMcpServer.Services;
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

            // Register services
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

            // Run the MCP server without logging to avoid interference
            await host.RunAsync();
        }
    }
}