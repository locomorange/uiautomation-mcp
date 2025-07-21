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
    public class SelectItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SelectItemOperation> _logger;

        public SelectItemOperation(
            ElementFinderService elementFinderService, 
            ILogger<SelectItemOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SelectItemRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
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
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
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
                }

                selectionPattern.Select();
                
                var selectedElement = new ElementInfo
                {
                    AutomationId = element.Current.AutomationId,
                    Name = element.Current.Name,
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    LocalizedControlType = element.Current.ControlType.LocalizedControlType,
                    IsEnabled = element.Current.IsEnabled,
                    ProcessId = element.Current.ProcessId,
                    ClassName = element.Current.ClassName,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    },
                    IsVisible = !element.Current.IsOffscreen,
                    IsOffscreen = element.Current.IsOffscreen,
                    FrameworkId = element.Current.FrameworkId,
                    SupportedPatterns = new string[0], // Basic info only
                    Details = new ElementDetails
                    {
                        HelpText = element.Current.HelpText ?? "",
                        HasKeyboardFocus = element.Current.HasKeyboardFocus,
                        IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                        IsPassword = element.Current.IsPassword
                    }
                };
                
                var result = new SelectionActionResult
                {
                    ActionName = "SelectItem",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    SelectionType = "Select",
                    SelectedElement = selectedElement,
                    CurrentSelectionCount = 1
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectItem operation failed");
                return Task.FromResult(new OperationResult 
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
    }
}