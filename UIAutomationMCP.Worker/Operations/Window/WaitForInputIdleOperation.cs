using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WaitForInputIdleOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public WaitForInputIdleOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<WaitForInputIdleResult>> ExecuteAsync(WorkerRequest request)
        {
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var timeoutMilliseconds = request.Parameters?.GetValueOrDefault("timeoutMilliseconds")?.ToString() is string timeoutStr && 
                int.TryParse(timeoutStr, out var timeout) ? timeout : 10000;

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
                    ElapsedMilliseconds = elapsed.TotalMilliseconds,
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