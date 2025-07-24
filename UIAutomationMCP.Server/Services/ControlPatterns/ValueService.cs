using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ValueService : BaseUIAutomationService, IValueService
    {
        public ValueService(IOperationExecutor executor, ILogger<ValueService> logger)
            : base(executor, logger)
        {
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetValueAsync(
            string value, 
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetValueRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                Value = value,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<SetValueRequest, ActionResult>(
                "SetElementValue",
                request,
                nameof(SetValueAsync),
                timeoutSeconds,
                ValidateSetValueRequest
            );
        }

        public async Task<ServerEnhancedResponse<TextInfoResult>> GetValueAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetValueRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetValueRequest, TextInfoResult>(
                "GetElementValue",
                request,
                nameof(GetValueAsync),
                timeoutSeconds,
                ValidateGetValueRequest
            );
        }

        private static ValidationResult ValidateSetValueRequest(SetValueRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for set value operation");
            }

            if (string.IsNullOrWhiteSpace(request.Value))
            {
                errors.Add("Value is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateGetValueRequest(GetValueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for get value operation");
            }

            return ValidationResult.Success;
        }

        protected override Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            var baseInfo = base.CreateSuccessAdditionalInfo(data, context);
            baseInfo["operationType"] = "value";
            
            if (data is ActionResult)
            {
                baseInfo["actionPerformed"] = "valueSet";
            }
            else if (data is TextInfoResult textResult)
            {
                baseInfo["actionPerformed"] = "valueRetrieved";
                baseInfo["valueLength"] = textResult.Text?.Length ?? 0;
                baseInfo["hasValue"] = !string.IsNullOrEmpty(textResult.Text);
            }
            
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
