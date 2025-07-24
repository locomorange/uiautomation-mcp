using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InvokeService : BaseUIAutomationService, IInvokeService
    {
        public InvokeService(IOperationExecutor executor, ILogger<InvokeService> logger)
            : base(executor, logger)
        {
        }

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

        protected override Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            var baseInfo = base.CreateSuccessAdditionalInfo(data, context);
            baseInfo["operationType"] = "invoke";
            baseInfo["actionPerformed"] = "elementInvoked";
            return baseInfo;
        }

        protected override Dictionary<string, object> CreateRequestParameters(IServiceContext context)
        {
            var baseParams = base.CreateRequestParameters(context);
            baseParams["operation"] = "InvokeElement";
            return baseParams;
        }
    }
}