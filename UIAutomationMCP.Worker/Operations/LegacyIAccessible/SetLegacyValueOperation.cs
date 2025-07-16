using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.LegacyIAccessible
{
    public class SetLegacyValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SetLegacyValueOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SetValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<SetLegacyValueRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected SetLegacyValueRequest.",
                    Data = new SetValueResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var value = typedRequest.Value;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new SetValueResult()
                });

            if (!element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out var pattern) || pattern is not LegacyIAccessiblePattern legacyPattern)
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "LegacyIAccessiblePattern not supported",
                    Data = new SetValueResult()
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                var previousValue = legacyPattern.Current.Value ?? "";
                
                legacyPattern.SetValue(value);
                
                var result = new SetValueResult
                {
                    ElementName = elementInfo.Name,
                    ElementId = elementInfo.AutomationId,
                    PreviousValue = previousValue,
                    NewValue = value,
                    Success = true
                };
                
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = $"Failed to set legacy value: {ex.Message}",
                    Data = new SetValueResult()
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