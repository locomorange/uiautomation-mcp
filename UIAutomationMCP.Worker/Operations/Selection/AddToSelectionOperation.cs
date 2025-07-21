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
    public class AddToSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<AddToSelectionOperation> _logger;

        public AddToSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<AddToSelectionOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<AddToSelectionRequest>(parametersJson)!;
                
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
                        Error = "Element not found",
                        Data = new SelectionActionResult { ActionName = "AddToSelection", SelectionType = "Add" }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionItemPattern not supported",
                        Data = new SelectionActionResult { ActionName = "AddToSelection", SelectionType = "Add" }
                    });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddToSelection operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to add element to selection: {ex.Message}",
                    Data = new SelectionActionResult 
                    { 
                        ActionName = "AddToSelection",
                        SelectionType = "Add",
                        Completed = false
                    }
                });
            }
        }
    }
}