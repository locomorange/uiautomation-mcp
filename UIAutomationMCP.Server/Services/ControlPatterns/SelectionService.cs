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
    public class SelectionService : BaseUIAutomationService<SelectionServiceMetadata>, ISelectionService
    {
        public SelectionService(IProcessManager processManager, ILogger<SelectionService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "selection";

        public async Task<ServerEnhancedResponse<ActionResult>> SelectItemAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SelectItemRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<SelectItemRequest, ActionResult>(
                "SelectItem",
                request,
                nameof(SelectItemAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> AddToSelectionAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new AddToSelectionRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<AddToSelectionRequest, ActionResult>(
                "AddToSelection",
                request,
                nameof(AddToSelectionAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> RemoveFromSelectionAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new RemoveFromSelectionRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<RemoveFromSelectionRequest, ActionResult>(
                "RemoveFromSelection",
                request,
                nameof(RemoveFromSelectionAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ClearSelectionAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new ClearSelectionRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<ClearSelectionRequest, ActionResult>(
                "ClearSelection",
                request,
                nameof(ClearSelectionAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> GetSelectionContainerAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetSelectionContainerRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetSelectionContainerRequest, ActionResult>(
                "GetSelectionContainer",
                request,
                nameof(GetSelectionContainerAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<SelectionInfoResult>> GetSelectionAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetSelectionRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetSelectionRequest, SelectionInfoResult>(
                "GetSelection",
                request,
                nameof(GetSelectionAsync),
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

        protected override SelectionServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ActionResult actionResult)
            {
                metadata.OperationSuccessful = actionResult.Success;

                if (context.MethodName.Contains("SelectItem"))
                {
                    metadata.ActionPerformed = "itemSelected";
                }
                else if (context.MethodName.Contains("AddToSelection"))
                {
                    metadata.ActionPerformed = "itemAddedToSelection";
                }
                else if (context.MethodName.Contains("RemoveFromSelection"))
                {
                    metadata.ActionPerformed = "itemRemovedFromSelection";
                }
                else if (context.MethodName.Contains("ClearSelection"))
                {
                    metadata.ActionPerformed = "selectionCleared";
                }
                else if (context.MethodName.Contains("GetSelectionContainer"))
                {
                    metadata.ActionPerformed = "selectionContainerRetrieved";
                }
            }
            else if (data is SelectionInfoResult selectionResult)
            {
                metadata.ActionPerformed = "selectionRetrieved";
                metadata.SelectedItemsCount = selectionResult.SelectedItems?.Count ?? 0;
                metadata.SupportsMultipleSelection = selectionResult.CanSelectMultiple;
                metadata.IsSelectionRequired = selectionResult.IsSelectionRequired;
                metadata.OperationSuccessful = selectionResult.Success;
            }

            return metadata;
        }
    }
}