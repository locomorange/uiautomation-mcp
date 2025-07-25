using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WaitForInputIdleOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<WaitForInputIdleOperation> _logger;

        public WaitForInputIdleOperation(
            ElementFinderService elementFinderService, 
            ILogger<WaitForInputIdleOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<WaitForInputIdleRequest>(parametersJson)!;
                
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;
                var timeoutMilliseconds = typedRequest.TimeoutMilliseconds;

                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WaitForInputIdleResult
                        {
                            ActionName = "WaitForInputIdle",
                            Completed = false,
                            TimeoutMilliseconds = timeoutMilliseconds,
                            ElapsedMilliseconds = 0,
                            TimedOut = false,
                            Message = "Window not found"
                        }
                    });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "WindowPattern not supported",
                        Data = new WaitForInputIdleResult
                        {
                            ActionName = "WaitForInputIdle",
                            Completed = false,
                            TimeoutMilliseconds = timeoutMilliseconds,
                            ElapsedMilliseconds = 0,
                            TimedOut = false,
                            Message = "WindowPattern not supported"
                        }
                    });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WaitForInputIdle operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to wait for input idle: {ex.Message}",
                    Data = new WaitForInputIdleResult
                    {
                        ActionName = "WaitForInputIdle",
                        Completed = false,
                        TimeoutMilliseconds = 0,
                        ElapsedMilliseconds = 0,
                        TimedOut = false,
                        Message = $"Error waiting for input idle: {ex.Message}"
                    }
                });
            }
        }
    }
}