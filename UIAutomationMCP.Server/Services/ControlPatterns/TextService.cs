using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TextService : BaseUIAutomationService, ITextService
    {
        public TextService(IOperationExecutor executor, ILogger<TextService> logger)
            : base(executor, logger)
        {
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SelectTextAsync(
            string? automationId = null, 
            string? name = null, 
            int startIndex = 0, 
            int length = 1, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SelectTextRequest
            {
                AutomationId = automationId,
                Name = name,
                StartIndex = startIndex,
                Length = length,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SelectTextRequest, ActionResult>(
                "SelectText",
                request,
                nameof(SelectTextAsync),
                timeoutSeconds,
                ValidateSelectTextRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetTextAsync(
            string? automationId = null, 
            string? name = null, 
            string text = "", 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetTextRequest
            {
                AutomationId = automationId,
                Name = name,
                Text = text ?? "",
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SetTextRequest, ActionResult>(
                "SetText",
                request,
                nameof(SetTextAsync),
                timeoutSeconds,
                ValidateSetTextRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> AppendTextAsync(
            string? automationId = null, 
            string? name = null, 
            string text = "", 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetTextRequest
            {
                AutomationId = automationId,
                Name = name,
                Text = text ?? "",
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<SetTextRequest, ActionResult>(
                "AppendText",
                request,
                nameof(AppendTextAsync),
                timeoutSeconds,
                ValidateAppendTextRequest
            );
        }

        public async Task<ServerEnhancedResponse<TextAttributesResult>> GetTextAttributesAsync(
            string? automationId = null, 
            string? name = null, 
            int startIndex = 0, 
            int length = -1, 
            string? attributeName = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new UIAutomationMCP.Shared.Requests.GetTextAttributesRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId,
                StartIndex = startIndex,
                Length = length,
                AttributeName = attributeName
            };

            return await ExecuteServiceOperationAsync<UIAutomationMCP.Shared.Requests.GetTextAttributesRequest, TextAttributesResult>(
                "GetTextAttributes",
                request,
                nameof(GetTextAttributesAsync),
                timeoutSeconds,
                ValidateGetTextAttributesRequest
            );
        }

        public async Task<ServerEnhancedResponse<TextSearchResult>> FindTextAsync(
            string? automationId = null, 
            string? name = null, 
            string searchText = "", 
            bool backward = false, 
            bool ignoreCase = true, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new UIAutomationMCP.Shared.Requests.FindTextRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId,
                SearchText = searchText,
                Backward = backward,
                IgnoreCase = ignoreCase
            };

            return await ExecuteServiceOperationAsync<UIAutomationMCP.Shared.Requests.FindTextRequest, TextSearchResult>(
                "FindText",
                request,
                nameof(FindTextAsync),
                timeoutSeconds,
                ValidateFindTextRequest
            );
        }

        private static ValidationResult ValidateSelectTextRequest(SelectTextRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for select text operation");
            }

            if (request.StartIndex < 0)
            {
                errors.Add("StartIndex must be non-negative");
            }

            if (request.Length <= 0)
            {
                errors.Add("Length must be greater than 0");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateSetTextRequest(SetTextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for set text operation");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateAppendTextRequest(SetTextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for append text operation");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetTextAttributesRequest(UIAutomationMCP.Shared.Requests.GetTextAttributesRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for get text attributes operation");
            }

            if (request.StartIndex < 0)
            {
                errors.Add("StartIndex must be non-negative");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateFindTextRequest(UIAutomationMCP.Shared.Requests.FindTextRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for find text operation");
            }

            if (string.IsNullOrWhiteSpace(request.SearchText))
            {
                errors.Add("SearchText is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            var baseInfo = base.CreateSuccessAdditionalInfo(data, context);
            baseInfo["operationType"] = "text";
            
            if (data is ActionResult)
            {
                baseInfo["actionPerformed"] = context.MethodName.Replace("Async", "").ToLowerInvariant();
            }
            else if (data is TextAttributesResult attributesResult)
            {
                baseInfo["actionPerformed"] = "textAttributesRetrieved";
                baseInfo["hasAttributes"] = attributesResult.HasAttributes;
            }
            else if (data is TextSearchResult searchResult)
            {
                baseInfo["actionPerformed"] = "textFound";
                baseInfo["textFound"] = searchResult.Found;
                baseInfo["startIndex"] = searchResult.StartIndex;
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