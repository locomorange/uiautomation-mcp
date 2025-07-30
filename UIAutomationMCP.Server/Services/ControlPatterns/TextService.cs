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
    public class TextService : BaseUIAutomationService<TextServiceMetadata>, ITextService
    {
        public TextService(IProcessManager processManager, ILogger<TextService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "text";

        public async Task<ServerEnhancedResponse<ActionResult>> SelectTextAsync(
            string? automationId = null, 
            string? name = null, 
            int startIndex = 0, 
            int length = 1, 
            string? controlType = null, 
            long? windowHandle = null, 
            int timeoutSeconds = 30)
        {
            var request = new SelectTextRequest
            {
                AutomationId = automationId,
                Name = name,
                StartIndex = startIndex,
                Length = length,
                ControlType = controlType,
                WindowHandle = windowHandle
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
            long? windowHandle = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetTextRequest
            {
                AutomationId = automationId,
                Name = name,
                Text = text ?? "",
                ControlType = controlType,
                WindowHandle = windowHandle
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
            long? windowHandle = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetTextRequest
            {
                AutomationId = automationId,
                Name = name,
                Text = text ?? "",
                ControlType = controlType,
                WindowHandle = windowHandle
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
            long? windowHandle = null, 
            int timeoutSeconds = 30)
        {
            var request = new UIAutomationMCP.Models.Requests.GetTextAttributesRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle,
                StartIndex = startIndex,
                Length = length,
                AttributeName = attributeName
            };

            return await ExecuteServiceOperationAsync<UIAutomationMCP.Models.Requests.GetTextAttributesRequest, TextAttributesResult>(
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
            long? windowHandle = null, 
            int timeoutSeconds = 30)
        {
            var request = new UIAutomationMCP.Models.Requests.FindTextRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle,
                SearchText = searchText,
                Backward = backward,
                IgnoreCase = ignoreCase
            };

            return await ExecuteServiceOperationAsync<UIAutomationMCP.Models.Requests.FindTextRequest, TextSearchResult>(
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

        private static ValidationResult ValidateGetTextAttributesRequest(UIAutomationMCP.Models.Requests.GetTextAttributesRequest request)
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

        private static ValidationResult ValidateFindTextRequest(UIAutomationMCP.Models.Requests.FindTextRequest request)
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

        protected override TextServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            
            if (data is ActionResult)
            {
                metadata.ActionPerformed = context.MethodName.Replace("Async", "").ToLowerInvariant();
                
                // For text operations, we can extract text length from the request context if needed
                if (context.MethodName.Contains("SetText") || context.MethodName.Contains("AppendText"))
                {
                    // Text length would need to be extracted from request if we want to populate it
                    // For now, we'll leave it null as we don't have easy access to the request data here
                }
            }
            else if (data is TextAttributesResult attributesResult)
            {
                metadata.ActionPerformed = "textAttributesRetrieved";
                metadata.HasAttributes = attributesResult.HasAttributes;
            }
            else if (data is TextSearchResult searchResult)
            {
                metadata.ActionPerformed = "textFound";
                metadata.TextFound = searchResult.Found;
                metadata.StartIndex = searchResult.StartIndex;
            }
            
            return metadata;
        }
    }
}