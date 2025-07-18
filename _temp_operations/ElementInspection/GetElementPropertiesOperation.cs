using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetElementPropertiesOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.ElementPropertiesResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetElementPropertiesRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.ElementPropertiesResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new UIAutomationMCP.Shared.Results.ElementPropertiesResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.ElementPropertiesResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new UIAutomationMCP.Shared.Results.ElementPropertiesResult()
                });

            var result = new UIAutomationMCP.Shared.Results.ElementPropertiesResult
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

            return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.ElementPropertiesResult> 
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