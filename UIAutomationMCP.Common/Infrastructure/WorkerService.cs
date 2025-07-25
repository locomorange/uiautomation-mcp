using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace UIAutomationMCP.Common.Infrastructure
{
    /// <summary>
    /// Worker service using common base functionality
    /// </summary>
    public class WorkerService : ProcessHostBase
    {
        public WorkerService(ILogger<WorkerService> logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        protected override string GetProcessType() => "Worker";
    }
}