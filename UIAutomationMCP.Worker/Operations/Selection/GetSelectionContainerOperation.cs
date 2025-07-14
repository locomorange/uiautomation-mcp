using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class GetSelectionContainerOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetSelectionContainerOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ElementSearchResult()
                });
            
            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = $"Element does not support SelectionItemPattern: {elementId}",
                    Data = new ElementSearchResult()
                });
            }

            try
            {
                var selectionItemPattern = (SelectionItemPattern)patternObject;
                var selectionContainer = selectionItemPattern.Current.SelectionContainer;
                
                var result = new ElementSearchResult();
                
                if (selectionContainer != null)
                {
                    var containerElement = CreateElementInfo(selectionContainer);
                    result.Elements.Add(containerElement);
                }
                
                result.SearchCriteria = new SearchCriteria
                {
                    SearchText = elementId,
                    PatternType = "SelectionItemPattern",
                    WindowTitle = windowTitle,
                    ProcessId = processId > 0 ? processId : null,
                    Scope = "SelectionContainer"
                };
                
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get selection container: {ex.Message}",
                    Data = new ElementSearchResult()
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