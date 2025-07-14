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
    public class ClearSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public ClearSelectionOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SelectionActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<ClearSelectionRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new SelectionActionResult { ActionName = "ClearSelection", SelectionType = "Clear" }
                });
            
            var containerElementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(containerElementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Container element not found",
                    Data = new SelectionActionResult { ActionName = "ClearSelection", SelectionType = "Clear" }
                });

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "SelectionPattern not supported",
                    Data = new SelectionActionResult { ActionName = "ClearSelection", SelectionType = "Clear" }
                });

            try
            {
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
                    Details = new Dictionary<string, object>
                    {
                        { "ClearedItemsCount", clearedCount },
                        { "ContainerElement", new ElementInfo
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
                        }
                    }
                };

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
