using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class ClearSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ClearSelectionOperation> _logger;

        public ClearSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<ClearSelectionOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ClearSelectionRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Container element not found",
                        Data = new SelectionActionResult { ActionName = "ClearSelection", SelectionType = "Clear" }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionPattern not supported",
                        Data = new SelectionActionResult { ActionName = "ClearSelection", SelectionType = "Clear" }
                    });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearSelection operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to clear selection: {ex.Message}",
                    Data = new SelectionActionResult 
                    { 
                        ActionName = "ClearSelection",
                        SelectionType = "Clear",
                        Completed = false
                    }
                });
            }
        }
    }
}