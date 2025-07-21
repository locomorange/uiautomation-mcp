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
    public class SelectElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SelectElementOperation> _logger;

        public SelectElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<SelectElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SelectElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new SelectionActionResult { ActionName = "Select" }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionItemPattern not supported",
                        Data = new SelectionActionResult { ActionName = "Select" }
                    });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to select element: {ex.Message}",
                    Data = new SelectionActionResult { ActionName = "Select" }
                });
            }
        }
    }
}