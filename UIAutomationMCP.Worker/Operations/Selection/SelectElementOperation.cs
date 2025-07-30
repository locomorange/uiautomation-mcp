using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class SelectElementOperation : BaseUIAutomationOperation<SelectElementRequest, SelectionActionResult>
    {
        public SelectElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<SelectElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<SelectionActionResult> ExecuteOperationAsync(SelectElementRequest request)
        {
            // 繝代ち繝ｼ繝ｳ螟画鋤・医Μ繧ｯ繧ｨ繧ｹ繝医°繧牙叙蠕励√ョ繝輔か繝ｫ繝医・SelectionItemPattern・・
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? SelectionItemPattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId,
                RequiredPattern = requiredPattern?.ProgrammaticName,
            }                WindowHandle = request.WindowHandle
            }
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SelectElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
            {
                throw new UIAutomationInvalidOperationException("SelectElement", request.AutomationId, "SelectionItemPattern not supported");
            }

            selectionPattern.Select();

            var result = new SelectionActionResult
            {
                ActionName = "Select",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                SelectionType = "Select",
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

            return Task.FromResult(result);
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SelectElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}