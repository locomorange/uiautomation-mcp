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
    public class GetWindowCapabilitiesOperation : BaseUIAutomationOperation<GetWindowCapabilitiesRequest, WindowCapabilitiesResult>
    {
        public GetWindowCapabilitiesOperation(
            ElementFinderService elementFinderService,
            ILogger<GetWindowCapabilitiesOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<WindowCapabilitiesResult> ExecuteOperationAsync(GetWindowCapabilitiesRequest request)
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
                throw new UIAutomationElementNotFoundException("GetWindowCapabilities", elementIdentifier);
            }

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
            {
                var elementIdentifier = request.WindowHandle.HasValue
                    ? $"WindowHandle={request.WindowHandle.Value}"
                    : !string.IsNullOrEmpty(windowTitle) ? $"WindowTitle='{windowTitle}'" : "unknown";
                throw new UIAutomationInvalidOperationException("GetWindowCapabilities", elementIdentifier, "WindowPattern not supported");
            }

            return Task.FromResult(new WindowCapabilitiesResult
            {
                CanMaximize = windowPattern.Current.CanMaximize,
                CanMinimize = windowPattern.Current.CanMinimize,
                CanMove = true, // TransformPattern check would be needed for precise answer
                CanResize = true, // TransformPattern check would be needed for precise answer
                WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString()
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetWindowCapabilitiesRequest request)
        {
            if (request.WindowHandle.HasValue && request.WindowHandle <= 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("WindowHandle must be greater than 0 when specified");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
