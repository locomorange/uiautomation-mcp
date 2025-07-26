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

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetAvailableViewsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetAvailableViewsRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetAvailableViewsRequest, ElementSearchResult>(
                "GetAvailableViews",
                request,
                nameof(GetAvailableViewsAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new SetViewRequest
            {
                AutomationId = automationId,
                Name = name,
                ViewId = viewId,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SetViewRequest, ElementSearchResult>(
                "SetView",
                request,
                nameof(SetViewAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetCurrentViewAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetCurrentViewRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetCurrentViewRequest, ElementSearchResult>(
                "GetCurrentView",
                request,
                nameof(GetCurrentViewAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetViewNameAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetViewNameRequest
            {
                AutomationId = automationId,
                Name = name,
                ViewId = viewId,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetViewNameRequest, ElementSearchResult>(
                "GetViewName",
                request,
                nameof(GetViewNameAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateElementIdentificationRequest<T>(T request) where T : class
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

                if (context.MethodName.Contains("GetAvailableViews"))
                {
                    metadata.ActionPerformed = "availableViewsRetrieved";
                    metadata.ViewsCount = searchResult.Elements?.Count ?? 0;
                }
                else if (context.MethodName.Contains("SetView"))
                {
                    metadata.ActionPerformed = "viewSet";
                }
                else if (context.MethodName.Contains("GetCurrentView"))
                {
                    metadata.ActionPerformed = "currentViewRetrieved";
                }
                else if (context.MethodName.Contains("GetViewName"))
                {
                    metadata.ActionPerformed = "viewNameRetrieved";
                }
            }

            return metadata;
        }
    }
}