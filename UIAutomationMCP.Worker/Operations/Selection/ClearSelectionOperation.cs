using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class ClearSelectionOperation : BaseUIAutomationOperation<ClearSelectionRequest, SelectionActionResult>
    {
        public ClearSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<ClearSelectionOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<SelectionActionResult> ExecuteOperationAsync(ClearSelectionRequest request)
        {
            // Pattern conversion (get from request, default to SelectionPattern)
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? SelectionPattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                    RequiredPattern = requiredPattern?.ProgrammaticName,
                    WindowHandle = request.WindowHandle
                };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("ClearSelection", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
            {
                throw new UIAutomationInvalidOperationException("ClearSelection", request.AutomationId, "SelectionPattern not supported");
            }

            var selection = selectionPattern.Current.GetSelection();
            int clearedCount = 0;

            foreach (AutomationElement selectedElement in selection)
            {
                if (selectedElement != null && 
                    selectedElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && 
                    itemPattern is SelectionItemPattern itemSelectionPattern)
                {
                    itemSelectionPattern.RemoveFromSelection();
                    clearedCount++;
                }
            }

            var result = new SelectionActionResult
            {
                ActionName = "ClearSelection",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                SelectionType = "Clear",
                CurrentSelectionCount = 0, // Should be 0 after clearing
                Details = $"Cleared {clearedCount} selected items from element: {element.Current.AutomationId}"
            };

            return Task.FromResult(result);
        }

        protected override Core.Validation.ValidationResult ValidateRequest(ClearSelectionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}