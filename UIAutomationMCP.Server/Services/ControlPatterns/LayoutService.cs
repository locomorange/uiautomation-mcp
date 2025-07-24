using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;
using UIAutomationMCP.Shared.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class LayoutService : BaseUIAutomationService<LayoutServiceMetadata>, ILayoutService
    {
        public LayoutService(IOperationExecutor executor, ILogger<LayoutService> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "layout";

        public async Task<ServerEnhancedResponse<ActionResult>> ExpandCollapseElementAsync(string? automationId = null, string? name = null, string action = "toggle", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new ExpandCollapseElementRequest
            {
                AutomationId = automationId,
                Name = name,
                Action = action,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<ExpandCollapseElementRequest, ActionResult>(
                "ExpandCollapseElement",
                request,
                nameof(ExpandCollapseElementAsync),
                timeoutSeconds,
                ValidateExpandCollapseRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ScrollElementAsync(string? automationId = null, string? name = null, string direction = "down", double amount = 1.0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new ScrollElementRequest
            {
                AutomationId = automationId,
                Name = name,
                Direction = direction,
                Amount = amount,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<ScrollElementRequest, ActionResult>(
                "ScrollElement",
                request,
                nameof(ScrollElementAsync),
                timeoutSeconds,
                ValidateScrollRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ScrollElementIntoViewAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new ScrollElementIntoViewRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<ScrollElementIntoViewRequest, ActionResult>(
                "ScrollElementIntoView",
                request,
                nameof(ScrollElementIntoViewAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string? automationId = null, string? name = null, double horizontalPercent = -1, double verticalPercent = -1, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new SetScrollPercentRequest
            {
                AutomationId = automationId,
                Name = name,
                HorizontalPercent = horizontalPercent,
                VerticalPercent = verticalPercent,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SetScrollPercentRequest, ActionResult>(
                "SetScrollPercent",
                request,
                nameof(SetScrollPercentAsync),
                timeoutSeconds,
                ValidateSetScrollPercentRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> DockElementAsync(string? automationId = null, string? name = null, string dockPosition = "none", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new DockElementRequest
            {
                AutomationId = automationId,
                Name = name,
                DockPosition = dockPosition,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<DockElementRequest, ActionResult>(
                "DockElement",
                request,
                nameof(DockElementAsync),
                timeoutSeconds,
                ValidateDockRequest
            );
        }

        public async Task<ServerEnhancedResponse<ScrollInfoResult>> GetScrollInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetScrollInfoRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetScrollInfoRequest, ScrollInfoResult>(
                "GetScrollInfo",
                request,
                nameof(GetScrollInfoAsync),
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

        private static ValidationResult ValidateExpandCollapseRequest(ExpandCollapseElementRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (string.IsNullOrWhiteSpace(request.Action) || !new[] { "expand", "collapse" }.Contains(request.Action.ToLowerInvariant()))
            {
                errors.Add("Action must be either 'expand' or 'collapse'");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateScrollRequest(ScrollElementRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (string.IsNullOrWhiteSpace(request.Direction) || !new[] { "up", "down", "left", "right" }.Contains(request.Direction.ToLowerInvariant()))
            {
                errors.Add("Direction must be one of: up, down, left, right");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateSetScrollPercentRequest(SetScrollPercentRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (request.HorizontalPercent < -1 || request.HorizontalPercent > 100 || request.VerticalPercent < -1 || request.VerticalPercent > 100)
            {
                errors.Add("Scroll percentages must be between -1 and 100 (-1 means no change)");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateDockRequest(DockElementRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (string.IsNullOrWhiteSpace(request.DockPosition) || !new[] { "top", "bottom", "left", "right", "fill", "none" }.Contains(request.DockPosition.ToLowerInvariant()))
            {
                errors.Add("Dock position must be one of: top, bottom, left, right, fill, none");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override LayoutServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ActionResult actionResult)
            {
                metadata.OperationSuccessful = actionResult.Success;

                if (context.MethodName.Contains("ExpandCollapse"))
                {
                    metadata.ActionPerformed = "expandCollapsePerformed";
                }
                else if (context.MethodName.Contains("ScrollElement") && !context.MethodName.Contains("IntoView"))
                {
                    metadata.ActionPerformed = "elementScrolled";
                }
                else if (context.MethodName.Contains("ScrollElementIntoView"))
                {
                    metadata.ActionPerformed = "elementScrolledIntoView";
                }
                else if (context.MethodName.Contains("SetScrollPercent"))
                {
                    metadata.ActionPerformed = "scrollPercentSet";
                }
                else if (context.MethodName.Contains("Dock"))
                {
                    metadata.ActionPerformed = "elementDocked";
                }
            }
            else if (data is ScrollInfoResult scrollResult)
            {
                metadata.OperationSuccessful = scrollResult.Success;
                metadata.ActionPerformed = "scrollInfoRetrieved";
            }

            return metadata;
        }
    }
}