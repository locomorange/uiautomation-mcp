using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.SynchronizedInput
{
    public class StartSynchronizedInputOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public StartSynchronizedInputOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<StartSynchronizedInputRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected StartSynchronizedInputRequest.",
                    Data = new ActionResult { ActionName = "StartSynchronizedInput" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var inputType = typedRequest.InputType;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ActionResult { ActionName = "StartSynchronizedInput" }
                });

            if (!element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) || pattern is not SynchronizedInputPattern synchronizedInputPattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "SynchronizedInputPattern not supported",
                    Data = new ActionResult { ActionName = "StartSynchronizedInput" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                // Convert input type string to SynchronizedInputType enum
                var synchronizedInputType = ConvertToSynchronizedInputType(inputType);
                
                synchronizedInputPattern.StartListening(synchronizedInputType);
                
                var result = new ActionResult
                {
                    ActionName = "StartSynchronizedInput",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId,
                        ["InputType"] = inputType,
                        ["Message"] = $"Started listening for {inputType} events"
                    }
                };
                
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to start synchronized input: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "StartSynchronizedInput",
                        Completed = false
                    }
                });
            }
        }

        private SynchronizedInputType ConvertToSynchronizedInputType(string inputType)
        {
            return inputType.ToLower() switch
            {
                "keyup" => SynchronizedInputType.KeyUp,
                "keydown" => SynchronizedInputType.KeyDown,
                "leftmouseup" => SynchronizedInputType.LeftMouseUp,
                "leftmousedown" => SynchronizedInputType.LeftMouseDown,
                "rightmouseup" => SynchronizedInputType.RightMouseUp,
                "rightmousedown" => SynchronizedInputType.RightMouseDown,
                _ => throw new ArgumentException($"Invalid input type: {inputType}")
            };
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