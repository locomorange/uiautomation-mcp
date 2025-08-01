using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Selection
{
    public class SelectItemOperation : BaseUIAutomationOperation<SelectItemRequest, SelectionActionResult>
    {
        public SelectItemOperation(
            ElementFinderService elementFinderService, 
            ILogger<SelectItemOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<SelectionActionResult> ExecuteOperationAsync(SelectItemRequest request)
        {
            // Pattern conversion (get from request, default to SelectionItemPattern)
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? SelectionItemPattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                RequiredPattern = requiredPattern?.ProgrammaticName, WindowHandle = request.WindowHandle };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SelectItem", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
            {
                throw new UIAutomationInvalidOperationException("SelectItem", request.AutomationId, "SelectionItemPattern not supported");
            }

            selectionPattern.Select();
            
            var selectedElement = new ElementInfo
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ControlType = element.Current.ControlType.LocalizedControlType,
                LocalizedControlType = element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                ProcessId = element.Current.ProcessId,
                ClassName = element.Current.ClassName,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                IsVisible = !element.Current.IsOffscreen,
                IsOffscreen = element.Current.IsOffscreen,
                SupportedPatterns = new string[0], // Basic info only
                Details = new ElementDetails
                {
                    HelpText = element.Current.HelpText ?? "",
                    HasKeyboardFocus = element.Current.HasKeyboardFocus,
                    IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                    IsPassword = element.Current.IsPassword,
                    FrameworkId = element.Current.FrameworkId
                }
            };
            
            var result = new SelectionActionResult
            {
                ActionName = "SelectItem",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                SelectionType = "Select",
                SelectedElement = selectedElement,
                CurrentSelectionCount = 1
            };
            
            return Task.FromResult(result);
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(SelectItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}

