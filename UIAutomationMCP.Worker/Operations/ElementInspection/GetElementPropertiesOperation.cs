using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetElementPropertiesOperation> _logger;

        public GetElementPropertiesOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetElementPropertiesOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetElementPropertiesRequest>(parametersJson)!;
                
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
                        Data = new ElementPropertiesResult()
                    });
                }

                var result = new ElementPropertiesResult
                {
                    BasicInfo = new ElementInfo
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
                        },
                        HelpText = element.Current.HelpText
                    },
                    ExtendedProperties = new Dictionary<string, object>
                    {
                        ["LocalizedControlType"] = element.Current.LocalizedControlType,
                        ["HasKeyboardFocus"] = element.Current.HasKeyboardFocus,
                        ["IsKeyboardFocusable"] = element.Current.IsKeyboardFocusable,
                        ["IsContentElement"] = element.Current.IsContentElement,
                        ["IsControlElement"] = element.Current.IsControlElement,
                        ["AcceleratorKey"] = element.Current.AcceleratorKey,
                        ["AccessKey"] = element.Current.AccessKey,
                        ["ItemStatus"] = element.Current.ItemStatus,
                        ["ItemType"] = element.Current.ItemType
                    },
                    FrameworkId = element.Current.FrameworkId,
                    RuntimeId = string.Join(",", element.GetRuntimeId() ?? Array.Empty<int>())
                };

                // Get supported patterns
                var patterns = element.GetSupportedPatterns();
                result.SupportedPatterns = patterns.Select(p => p.ProgrammaticName).ToList();

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetElementProperties operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get element properties: {ex.Message}",
                    Data = new ElementPropertiesResult()
                });
            }
        }
    }
}