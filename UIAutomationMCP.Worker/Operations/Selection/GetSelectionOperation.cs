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
    public class GetSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetSelectionOperation> _logger;

        public GetSelectionOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetSelectionOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetSelectionRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Container element not found",
                        Data = new SelectionInfoResult()
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionPattern not supported",
                        Data = new SelectionInfoResult()
                    });
                }

                var result = new SelectionInfoResult
                {
                    CanSelectMultiple = selectionPattern.Current.CanSelectMultiple,
                    IsSelectionRequired = selectionPattern.Current.IsSelectionRequired
                };

                var selection = selectionPattern.Current.GetSelection();
                foreach (AutomationElement selectedElement in selection)
                {
                    if (selectedElement != null)
                    {
                        result.SelectedItems.Add(new SelectionItem
                        {
                            AutomationId = selectedElement.Current.AutomationId,
                            Name = selectedElement.Current.Name,
                            ControlType = selectedElement.Current.ControlType.LocalizedControlType,
                            IsEnabled = selectedElement.Current.IsEnabled,
                            IsOffscreen = selectedElement.Current.IsOffscreen,
                            BoundingRectangle = new Rectangle
                            {
                                X = selectedElement.Current.BoundingRectangle.X,
                                Y = selectedElement.Current.BoundingRectangle.Y,
                                Width = selectedElement.Current.BoundingRectangle.Width,
                                Height = selectedElement.Current.BoundingRectangle.Height
                            }
                        });
                    }
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSelection operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get selection: {ex.Message}",
                    Data = new SelectionInfoResult()
                });
            }
        }
    }
}