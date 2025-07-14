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
    public class SelectElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SelectElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SelectionActionResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try to get typed request first, fallback to legacy dictionary method
            var typedRequest = request.GetTypedRequest<SelectElementRequest>(_options);
            
            string elementId;
            string windowTitle;
            int processId;
            
            if (typedRequest != null)
            {
                elementId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
            }
            else
            {
                // Legacy dictionary method
                elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            }

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new SelectionActionResult { ActionName = "Select" }
                });

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "SelectionItemPattern not supported",
                    Data = new SelectionActionResult { ActionName = "Select" }
                });

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

            return Task.FromResult(new OperationResult<SelectionActionResult> 
            { 
                Success = true, 
                Data = result 
            });
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