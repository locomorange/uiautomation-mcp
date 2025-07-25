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
    public class RangeService : BaseUIAutomationService<RangeServiceMetadata>, IRangeService
    {
        public RangeService(IProcessManager processManager, ILogger<RangeService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "range";

        public async Task<ServerEnhancedResponse<ActionResult>> SetRangeValueAsync(
            string? automationId = null, 
            string? name = null, 
            double value = 0, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetRangeValueRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                Value = value,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SetRangeValueRequest, ActionResult>(
                "SetRangeValue",
                request,
                nameof(SetRangeValueAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<RangeValueResult>> GetRangeValueAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetRangeValueRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetRangeValueRequest, RangeValueResult>(
                "GetRangeValue",
                request,
                nameof(GetRangeValueAsync),
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

        protected override RangeServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ActionResult actionResult)
            {
                metadata.ActionPerformed = "rangeValueSet";
                metadata.OperationSuccessful = actionResult.Success;
                // Range value would need to be extracted from request context if needed
            }
            else if (data is RangeValueResult rangeResult)
            {
                metadata.ActionPerformed = "rangeValueRetrieved";
                metadata.RangeValue = rangeResult.Value;
                metadata.MinimumValue = rangeResult.Minimum;
                metadata.MaximumValue = rangeResult.Maximum;
                metadata.SmallChange = rangeResult.SmallChange;
                metadata.LargeChange = rangeResult.LargeChange;
                metadata.IsReadOnly = rangeResult.IsReadOnly;
                metadata.OperationSuccessful = true;
            }

            return metadata;
        }
    }
}