using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;
using UIAutomationMCP.Shared.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InvokeService : BaseUIAutomationService<InvokeServiceMetadata>, IInvokeService
    {
        public InvokeService(IOperationExecutor executor, ILogger<InvokeService> logger)
            : base(executor, logger)
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