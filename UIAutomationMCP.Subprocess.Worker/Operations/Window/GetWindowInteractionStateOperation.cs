using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Worker.Extensions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Window
{
    public class GetWindowInteractionStateOperation : BaseUIAutomationOperation<GetWindowInteractionStateRequest, WindowInteractionStateResult>
    {
        public GetWindowInteractionStateOperation(
            ElementFinderService elementFinderService,
            ILogger<GetWindowInteractionStateOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<WindowInteractionStateResult> ExecuteOperationAsync(GetWindowInteractionStateRequest request)
        {
            var windowTitle = request.WindowTitle ?? "";

            var searchCriteria = new ElementSearchCriteria
            {
                WindowHandle = request.WindowHandle,
                WindowTitle = windowTitle,
                UseWindowHandleAsFilter = true,
                RequiredPattern = "Window"
            };
            var window = _elementFinderService.FindElement(searchCriteria);
            if (window == null)
            {
                var elementIdentifier = request.WindowHandle.HasValue
                    ? $"WindowHandle={request.WindowHandle.Value}"
                    : !string.IsNullOrEmpty(windowTitle) ? $"WindowTitle='{windowTitle}'" : "unknown";
                throw new UIAutomationElementNotFoundException("GetWindowInteractionState", elementIdentifier);
            }

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
            {
                var elementIdentifier = request.WindowHandle.HasValue
                    ? $"WindowHandle={request.WindowHandle.Value}"
                    : !string.IsNullOrEmpty(windowTitle) ? $"WindowTitle='{windowTitle}'" : "unknown";
                throw new UIAutomationInvalidOperationException("GetWindowInteractionState", elementIdentifier, "WindowPattern not supported");
            }

            return Task.FromResult(new WindowInteractionStateResult
            {
                WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString(),
                WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                IsModal = windowPattern.Current.IsModal,
                IsTopmost = windowPattern.Current.IsTopmost
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetWindowInteractionStateRequest request)
        {
            if (request.WindowHandle.HasValue && request.WindowHandle <= 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("WindowHandle must be greater than 0 when specified");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
