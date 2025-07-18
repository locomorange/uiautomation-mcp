using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class RemoveFromSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<RemoveFromSelectionOperation> _logger;

        public RemoveFromSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<RemoveFromSelectionOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<RemoveFromSelectionRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new SelectionActionResult { ActionName = "RemoveFromSelection", SelectionType = "Remove" }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionItemPattern not supported",
                        Data = new SelectionActionResult { ActionName = "RemoveFromSelection", SelectionType = "Remove" }
                    });
                }

                selectionPattern.RemoveFromSelection();

                var result = new SelectionActionResult
                {
                    ActionName = "RemoveFromSelection",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    SelectionType = "Remove",
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveFromSelection operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to remove element from selection: {ex.Message}",
                    Data = new SelectionActionResult 
                    { 
                        ActionName = "RemoveFromSelection",
                        SelectionType = "Remove",
                        Completed = false
                    }
                });
            }
        }
    }
}