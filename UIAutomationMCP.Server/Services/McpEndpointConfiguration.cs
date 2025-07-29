using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// Background service to configure MCP endpoint for logging after server initialization
    /// </summary>
    public class McpEndpointConfiguration : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public McpEndpointConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait longer for MCP server to fully initialize
            await Task.Delay(1000, stoppingToken);

            // Try multiple times with retries
            for (int i = 0; i < 10 && !stoppingToken.IsCancellationRequested; i++)
            {
                try
                {
                    var mcpEndpoint = _serviceProvider.GetService<IMcpEndpoint>();
                    var mcpLogService = _serviceProvider.GetService<IMcpLogService>();

                    if (mcpEndpoint != null && mcpLogService is McpLoggingService loggingService)
                    {
                        loggingService.SetMcpEndpoint(mcpEndpoint);
                        return; // Success, exit
                    }
                }
                catch
                {
                    // Ignore errors during initialization
                }

                // Wait before retry
                await Task.Delay(500, stoppingToken);
            }
        }
    }
}