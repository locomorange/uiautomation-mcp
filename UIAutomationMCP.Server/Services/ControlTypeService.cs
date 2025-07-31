using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services
{
    public class ControlTypeService : BaseUIAutomationService<ControlTypeServiceMetadata>, IControlTypeService
    {
        public ControlTypeService(IProcessManager processManager, ILogger<ControlTypeService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "controlType";

        protected override ControlTypeServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            // Service kept for potential future control type operations
            return metadata;
        }
    }
}
