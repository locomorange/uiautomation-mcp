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
    public class SelectItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SelectItemOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SelectionActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<SelectItemRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new SelectionActionResult
                    {
                        ActionName = "SelectItem",
                        Completed = false,
                        SelectionType = "Select"
                    }
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
                    Data = new SelectionActionResult
                    {
                        ActionName = "SelectItem",
                        Completed = false,
                        SelectionType = "Select"
                    }
                });

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return Task.FromResult(new OperationResult<SelectionActionResult> 
                { 
                    Success = false, 
                    Error = "SelectionItemPattern not supported",
                    Data = new SelectionActionResult
                    {
                        ActionName = "SelectItem",
                        Completed = false,
                        SelectionType = "Select"
                    }
                });

            try
            {
                selectionPattern.Select();
                
                var selectedElement = CreateElementInfo(element);
                var result = new SelectionActionResult
                {
                    ActionName = "SelectItem",
                    Completed = true,
                    SelectionType = "Select",
                    SelectedElement = selectedElement,
                    CurrentSelectionCount = 1
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
                    Error = $"Failed to select element: {ex.Message}",
                    Data = new SelectionActionResult
                    {
                        ActionName = "SelectItem",
                        Completed = false,
                        SelectionType = "Select"
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

        private ElementInfo CreateElementInfo(AutomationElement element)
        {
            return new ElementInfo
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ControlType = element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                ProcessId = element.Current.ProcessId,
                ClassName = element.Current.ClassName,
                HelpText = element.Current.HelpText,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                IsVisible = !element.Current.IsOffscreen
            };
        }
    }
}
