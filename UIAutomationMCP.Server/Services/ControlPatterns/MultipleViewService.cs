using System.Diagnostics.CodeAnalysis;
using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class MultipleViewService : BaseUIAutomationService<MultipleViewServiceMetadata>, IMultipleViewService
    {
        public MultipleViewService(IProcessManager processManager, ILogger<MultipleViewService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "multipleView";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new SetViewRequest
            {
                AutomationId = automationId,
                Name = name,
                ViewId = viewId,
                ControlType = controlType,
                WindowHandle = windowHandle,
            };

            return await ExecuteServiceOperationAsync<SetViewRequest, ElementSearchResult>(
                "SetView",
                request,
                nameof(SetViewAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateElementIdentificationRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T request) where T : class
        {
            // Use reflection to check common properties for element identification
            var automationIdProp = typeof(T).GetProperty("AutomationId");
            var nameProp = typeof(T).GetProperty("Name");

            var automationId = automationIdProp?.GetValue(request) as string;
            var name = nameProp?.GetValue(request) as string;

            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for element identification");
            }

            return ValidationResult.Success;
        }

        protected override MultipleViewServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.OperationSuccessful = searchResult.Success;

                if (context.MethodName.Contains("SetView"))
                {
                    metadata.ActionPerformed = "viewSet";
                }
            }

            return metadata;
        }
    }
}
