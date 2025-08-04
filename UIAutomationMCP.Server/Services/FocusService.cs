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
    public class FocusService : BaseUIAutomationService<FocusServiceMetadata>, IFocusService
    {
        public FocusService(IProcessManager processManager, ILogger<FocusService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "focus";

        public async Task<ServerEnhancedResponse<ActionResult>> SetFocusAsync(
            string? automationId = null,
            string? name = null,
            string? controlType = null,
            string? requiredPattern = null,
            long? windowHandle = null,
            int timeoutSeconds = 30)
        {
            var request = new SetFocusRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle,
                RequiredPattern = requiredPattern
            };

            return await ExecuteServiceOperationAsync<SetFocusRequest, ActionResult>(
                "SetFocus",
                request,
                nameof(SetFocusAsync),
                timeoutSeconds
            );
        }

        protected override FocusServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ActionResult actionResult)
            {
                metadata.FocusSuccessful = actionResult.Success;

                // Extract focus target information from request context if available
                // Note: In a more advanced implementation, we could access the original request
                // through the context to populate target element information
            }

            return metadata;
        }
    }
}
