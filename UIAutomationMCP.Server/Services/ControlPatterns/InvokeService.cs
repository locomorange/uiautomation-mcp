using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InvokeService : BaseUIAutomationService<InvokeServiceMetadata>, IInvokeService
    {
        public InvokeService(IProcessManager processManager, ILogger<InvokeService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "invoke";

        public async Task<ServerEnhancedResponse<ActionResult>> InvokeElementAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new InvokeElementRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<InvokeElementRequest, ActionResult>(
                "InvokeElement",
                request,
                nameof(InvokeElementAsync),
                timeoutSeconds,
                ValidateInvokeRequest
            );
        }

        private static ValidationResult ValidateInvokeRequest(InvokeElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for invoke operation");
            }

            return ValidationResult.Success;
        }

        protected override InvokeServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            // ActionPerformed is already set to "elementInvoked" by default in InvokeServiceMetadata
            return metadata;
        }
    }
}