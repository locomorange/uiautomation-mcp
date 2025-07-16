using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ObjectModel
{
    public class GetUnderlyingObjectModelOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetUnderlyingObjectModelOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ObjectModelResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetUnderlyingObjectModelRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ObjectModelResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected GetUnderlyingObjectModelRequest.",
                    Data = new ObjectModelResult 
                    { 
                        IsAvailable = false,
                        ErrorMessage = "Invalid request format"
                    }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ObjectModelResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ObjectModelResult 
                    { 
                        IsAvailable = false,
                        ErrorMessage = "Element not found"
                    }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                // Check if ObjectModelPattern is supported
                if (!element.TryGetCurrentPattern(ObjectModelPattern.Pattern, out var pattern) || pattern is not ObjectModelPattern objectModelPattern)
                {
                    return Task.FromResult(new OperationResult<ObjectModelResult> 
                    { 
                        Success = false, 
                        Error = "ObjectModelPattern not supported",
                        Data = new ObjectModelResult 
                        { 
                            IsAvailable = false,
                            ErrorMessage = "ObjectModelPattern is not supported on this element",
                            ElementInfo = new Dictionary<string, object>
                            {
                                ["ElementName"] = elementInfo.Name,
                                ["ElementType"] = elementInfo.ControlType,
                                ["ElementId"] = elementInfo.AutomationId
                            }
                        }
                    });
                }
                
                // Get the underlying object model
                var underlyingObject = objectModelPattern.GetUnderlyingObjectModel();
                
                var result = new ObjectModelResult
                {
                    IsAvailable = true,
                    ObjectModel = underlyingObject,
                    TypeName = underlyingObject?.GetType().FullName ?? "null",
                    ElementInfo = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId
                    }
                };
                
                return Task.FromResult(new OperationResult<ObjectModelResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ObjectModelResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get underlying object model: {ex.Message}",
                    Data = new ObjectModelResult 
                    { 
                        IsAvailable = false,
                        ErrorMessage = ex.Message
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