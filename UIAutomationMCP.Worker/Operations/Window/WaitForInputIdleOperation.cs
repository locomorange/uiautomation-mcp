using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WaitForInputIdleOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public WaitForInputIdleOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<WaitForInputIdleResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<WaitForInputIdleRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<WaitForInputIdleResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new WaitForInputIdleResult()
                });
            
            var windowTitle = typedRequest.WindowTitle ?? "";
            var processId = typedRequest.ProcessId ?? 0;
            var timeoutMilliseconds = typedRequest.TimeoutMilliseconds;

            try
            {
                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    var failureResult = new WaitForInputIdleResult
                    {
                        ActionName = "WaitForInputIdle",
                        Completed = false,
                        TimeoutMilliseconds = timeoutMilliseconds,
                        ElapsedMilliseconds = 0,
                        TimedOut = false,
                        Message = "Window not found"
                    };
                    return Task.FromResult(new OperationResult<WaitForInputIdleResult> { Success = false, Error = "Window not found", Data = failureResult });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    var failureResult = new WaitForInputIdleResult
                    {
                        ActionName = "WaitForInputIdle",
                        Completed = false,
                        TimeoutMilliseconds = timeoutMilliseconds,
                        ElapsedMilliseconds = 0,
                        TimedOut = false,
                        Message = "WindowPattern not supported"
                    };
                    return Task.FromResult(new OperationResult<WaitForInputIdleResult> { Success = false, Error = "WindowPattern not supported", Data = failureResult });
                }

                var startTime = DateTime.Now;
                var success = windowPattern.WaitForInputIdle(timeoutMilliseconds);
                var elapsed = DateTime.Now - startTime;

                var result = new WaitForInputIdleResult
                {
                    ActionName = "WaitForInputIdle",
                    Completed = success,
                    TimeoutMilliseconds = timeoutMilliseconds,
                    ElapsedMilliseconds = (int)elapsed.TotalMilliseconds,
                    TimedOut = !success,
                    WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString(),
                    Message = success 
                        ? "Window became idle within the specified timeout"
                        : $"Window did not become idle within {timeoutMilliseconds}ms timeout"
                };

                return Task.FromResult(new OperationResult<WaitForInputIdleResult> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                var failureResult = new WaitForInputIdleResult
                {
                    ActionName = "WaitForInputIdle",
                    Completed = false,
                    TimeoutMilliseconds = timeoutMilliseconds,
                    ElapsedMilliseconds = 0,
                    TimedOut = false,
                    Message = $"Error waiting for input idle: {ex.Message}"
                };
                return Task.FromResult(new OperationResult<WaitForInputIdleResult> { Success = false, Error = $"Error waiting for input idle: {ex.Message}", Data = failureResult });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }
    }
}