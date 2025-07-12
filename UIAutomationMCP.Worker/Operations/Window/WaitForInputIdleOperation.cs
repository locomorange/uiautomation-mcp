using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
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
                    return Task.FromResult(new OperationResult { Success = false, Error = "Window not found" });

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                    return Task.FromResult(new OperationResult { Success = false, Error = "WindowPattern not supported" });

                var startTime = DateTime.Now;
                var success = windowPattern.WaitForInputIdle(timeoutMilliseconds);
                var elapsed = DateTime.Now - startTime;

                var result = new Dictionary<string, object>
                {
                    ["Success"] = success,
                    ["TimeoutMilliseconds"] = timeoutMilliseconds,
                    ["ElapsedMilliseconds"] = elapsed.TotalMilliseconds,
                    ["TimedOut"] = !success,
                    ["WindowInteractionState"] = windowPattern.Current.WindowInteractionState.ToString()
                };

                if (success)
                {
                    result["Message"] = "Window became idle within the specified timeout";
                }
                else
                {
                    result["Message"] = $"Window did not become idle within {timeoutMilliseconds}ms timeout";
                }

                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error waiting for input idle: {ex.Message}" });
            }
        }
    }
}