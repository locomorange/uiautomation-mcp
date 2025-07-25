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
    public class ToggleService : BaseUIAutomationService<ToggleServiceMetadata>, IToggleService
    {
        public ToggleService(IProcessManager processManager, ILogger<ToggleService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "toggle";

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

        protected override ToggleServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            metadata.ActionPerformed = context.MethodName.Replace("Async", "").ToLowerInvariant();
            
            // Note: ActionResult doesn't contain state information like ToggleActionResult would
            // If we need state information, we'd need to update the result types or use a different approach
            
            return metadata;
        }
    }
}