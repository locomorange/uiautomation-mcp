using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class AddToSelectionOperation : BaseUIAutomationOperation<AddToSelectionRequest, SelectionActionResult>
    {
        public AddToSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<AddToSelectionOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<SelectionActionResult> ExecuteOperationAsync(AddToSelectionRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはSelectionItemPattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? SelectionItemPattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name, 
                controlType: request.ControlType, 
                windowTitle: request.WindowTitle, 
                processId: request.ProcessId ?? 0,
                requiredPattern: requiredPattern);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("AddToSelection", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
            {
                throw new UIAutomationInvalidOperationException("AddToSelection", request.AutomationId, "SelectionItemPattern not supported");
            }

            selectionPattern.AddToSelection();

            var result = new SelectionActionResult
            {
                ActionName = "AddToSelection",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                SelectionType = "Add",
                SelectedElement = new ElementInfo
                {
                    AutomationId = element.Current.AutomationId,
                    Name = element.Current.Name,
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    ClassName = element.Current.ClassName,
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    ProcessId = element.Current.ProcessId,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    }
                }
            };

            // Try to get selection count from parent container
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && 
                itemPattern is SelectionItemPattern selectionItemPattern &&
                selectionItemPattern.Current.SelectionContainer is AutomationElement container &&
                container.TryGetCurrentPattern(SelectionPattern.Pattern, out var containerPattern) && 
                containerPattern is SelectionPattern selectionContainerPattern)
            {
                var currentSelection = selectionContainerPattern.Current.GetSelection();
                result.CurrentSelectionCount = currentSelection.Length;
            }

            return result;
        }

        protected override Core.Validation.ValidationResult ValidateRequest(AddToSelectionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}