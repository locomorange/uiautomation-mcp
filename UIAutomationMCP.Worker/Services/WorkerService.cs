using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.UIAutomation.Infrastructure;

namespace UIAutomationMCP.Worker.Services
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
