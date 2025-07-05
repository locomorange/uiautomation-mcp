using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Core;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.Patterns.Interaction
{
    /// <summary>
    /// Microsoft UI Automation InvokePattern handler
    /// Provides functionality for controls that can be invoked (buttons, menu items, etc.)
    /// </summary>
    public class InvokePatternHandler : BaseAutomationHandler
    {
        public InvokePatternHandler(
            ILogger<InvokePatternHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Executes the Invoke pattern operation
        /// </summary>
        public async Task<WorkerResult> ExecuteInvokeAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Invoke");
                }

                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var patternObj) && 
                    patternObj is InvokePattern invokePattern)
                {
                    _logger.LogInformation("[InvokePatternHandler] Invoking element: {ElementName}", 
                        SafeGetElementName(element));
                    
                    invokePattern.Invoke();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element invoked successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("InvokePattern");
                }
            }, "Invoke");
        }
    }
}