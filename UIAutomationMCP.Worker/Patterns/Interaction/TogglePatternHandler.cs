using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Core;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.Patterns.Interaction
{
    /// <summary>
    /// Microsoft UI Automation TogglePattern handler
    /// Provides functionality for controls that can toggle state (checkboxes, radio buttons, etc.)
    /// </summary>
    public class TogglePatternHandler : BaseAutomationHandler
    {
        public TogglePatternHandler(
            ILogger<TogglePatternHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Executes the Toggle pattern operation
        /// </summary>
        public async Task<WorkerResult> ExecuteToggleAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Toggle");
                }

                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var patternObj) && 
                    patternObj is TogglePattern togglePattern)
                {
                    var currentState = togglePattern.Current.ToggleState;
                    
                    _logger.LogInformation("[TogglePatternHandler] Toggling element: {ElementName}, current state: {State}", 
                        SafeGetElementName(element), currentState);
                    
                    togglePattern.Toggle();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = $"Element toggled from {currentState} state"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("TogglePattern");
                }
            }, "Toggle");
        }

        /// <summary>
        /// Gets the current toggle state of an element
        /// </summary>
        public async Task<WorkerResult> ExecuteGetToggleStateAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("GetToggleState");
                }

                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var patternObj) && 
                    patternObj is TogglePattern togglePattern)
                {
                    var currentState = togglePattern.Current.ToggleState;
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = new Dictionary<string, object>
                        {
                            ["ToggleState"] = currentState.ToString(),
                            ["IsOn"] = currentState == ToggleState.On,
                            ["IsOff"] = currentState == ToggleState.Off,
                            ["IsIndeterminate"] = currentState == ToggleState.Indeterminate
                        }
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("TogglePattern");
                }
            }, "GetToggleState");
        }
    }
}