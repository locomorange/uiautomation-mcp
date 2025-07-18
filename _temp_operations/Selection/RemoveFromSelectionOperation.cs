using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class RemoveFromSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public RemoveFromSelectionOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SelectionActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<RemoveFromSelectionRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new SelectionActionResult { ActionName = "RemoveFromSelection", SelectionType = "Remove" }
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new SelectionActionResult { ActionName = "RemoveFromSelection", SelectionType = "Remove" }
                });

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "SelectionItemPattern not supported",
                    Data = new SelectionActionResult { ActionName = "RemoveFromSelection", SelectionType = "Remove" }
                });

            try
            {
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

                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<SelectionActionResult> 
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

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
