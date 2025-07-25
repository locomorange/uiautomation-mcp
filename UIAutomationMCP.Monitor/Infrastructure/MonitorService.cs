using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace UIAutomationMCP.Monitor.Infrastructure
{
    /// <summary>
    /// Monitor service using common base functionality
    /// </summary>
    public class MonitorService : UIAutomationMCP.Common.Infrastructure.ProcessHostBase
    {
        public MonitorService(ILogger<MonitorService> logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        protected override string GetProcessType() => "Monitor";
    }
}