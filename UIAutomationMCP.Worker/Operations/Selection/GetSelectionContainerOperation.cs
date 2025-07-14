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
    public class GetSelectionContainerOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetSelectionContainerOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetSelectionContainerRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new ElementSearchResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

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