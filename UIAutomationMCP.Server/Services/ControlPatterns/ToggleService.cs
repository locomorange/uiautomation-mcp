using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ToggleService : BaseUIAutomationService, IToggleService
    {
        public ToggleService(IOperationExecutor executor, ILogger<ToggleService> logger)
            : base(executor, logger)
        {
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ToggleElementAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new ToggleElementRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<ToggleElementRequest, ActionResult>(
                "ToggleElement",
                request,
                nameof(ToggleElementAsync),
                timeoutSeconds,
                ValidateToggleRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetToggleStateAsync(
            string toggleState,
            string? automationId = null,
            string? name = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var request = new SetToggleStateRequest
            {
                State = toggleState,
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<SetToggleStateRequest, ActionResult>(
                "SetToggleState",
                request,
                nameof(SetToggleStateAsync),
                timeoutSeconds,
                ValidateSetToggleStateRequest
            );
        }

        private static ValidationResult ValidateToggleRequest(ToggleElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for toggle operation");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateSetToggleStateRequest(SetToggleStateRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required");
            }

            if (string.IsNullOrWhiteSpace(request.State))
            {
                errors.Add("ToggleState is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            var baseInfo = base.CreateSuccessAdditionalInfo(data, context);
            baseInfo["operationType"] = "toggle";
            baseInfo["actionPerformed"] = "elementToggled";
            
            return baseInfo;
        }

        protected override Dictionary<string, object> CreateRequestParameters(IServiceContext context)
        {
            var baseParams = base.CreateRequestParameters(context);
            baseParams["operation"] = context.MethodName.Replace("Async", "");
            return baseParams;
        }
    }
}