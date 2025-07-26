using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Worker.Extensions;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WaitForInputIdleOperation : BaseUIAutomationOperation<WaitForInputIdleRequest, WaitForInputIdleResult>
    {
        public WaitForInputIdleOperation(
            ElementFinderService elementFinderService, 
            ILogger<WaitForInputIdleOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<WaitForInputIdleResult> ExecuteOperationAsync(WaitForInputIdleRequest request)
        {
            var windowTitle = request.WindowTitle ?? "";
            var processId = request.ProcessId ?? 0;
            var timeoutMilliseconds = request.TimeoutMilliseconds;

            var window = _elementFinderService.GetSearchRoot(processId, windowTitle);
            if (window == null)
            {
                throw new UIAutomationElementNotFoundException("Window", windowTitle);
            }

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
            {
                throw new UIAutomationInvalidOperationException("WaitForInputIdle", windowTitle, "WindowPattern not supported");
            }

            var startTime = DateTime.Now;
            var success = windowPattern.WaitForInputIdle(timeoutMilliseconds);
            var elapsed = DateTime.Now - startTime;

            return Task.FromResult(new WaitForInputIdleResult
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
            });
        }

        protected override Core.Validation.ValidationResult ValidateRequest(WaitForInputIdleRequest request)
        {
            if (request.TimeoutMilliseconds <= 0)
            {
                return Core.Validation.ValidationResult.Failure("TimeoutMilliseconds must be greater than 0");
            }

            if (request.ProcessId.HasValue && request.ProcessId <= 0)
            {
                return Core.Validation.ValidationResult.Failure("ProcessId must be greater than 0 when specified");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}