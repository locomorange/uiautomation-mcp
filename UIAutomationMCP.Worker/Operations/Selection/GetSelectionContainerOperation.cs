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
    public class GetSelectionContainerOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetSelectionContainerOperation> _logger;

        public GetSelectionContainerOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetSelectionContainerOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetSelectionContainerRequest>(parametersJson)!;
                
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
                        Data = new ElementSearchResult()
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionItemPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionItemPattern not supported",
                        Data = new ElementSearchResult()
                    });
                }

                var selectionContainer = selectionItemPattern.Current.SelectionContainer;
                
                var result = new ElementSearchResult
                {
                    SearchCriteria = "Selection container search"
                };
                
                if (selectionContainer != null)
                {
                    var containerElement = new ElementInfo
                    {
                        AutomationId = selectionContainer.Current.AutomationId,
                        Name = selectionContainer.Current.Name,
                        ControlType = selectionContainer.Current.ControlType.LocalizedControlType,
                        IsEnabled = selectionContainer.Current.IsEnabled,
                        ProcessId = selectionContainer.Current.ProcessId,
                        ClassName = selectionContainer.Current.ClassName,
                        HelpText = selectionContainer.Current.HelpText,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = selectionContainer.Current.BoundingRectangle.X,
                            Y = selectionContainer.Current.BoundingRectangle.Y,
                            Width = selectionContainer.Current.BoundingRectangle.Width,
                            Height = selectionContainer.Current.BoundingRectangle.Height
                        },
                        IsVisible = !selectionContainer.Current.IsOffscreen
                    };
                    result.Elements.Add(containerElement);
                }
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSelectionContainer operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get selection container: {ex.Message}",
                    Data = new ElementSearchResult()
                });
            }
        }
    }
}