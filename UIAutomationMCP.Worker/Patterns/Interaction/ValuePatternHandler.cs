using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Core;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.Patterns.Interaction
{
    /// <summary>
    /// Microsoft UI Automation ValuePattern handler
    /// Provides functionality for controls that have a value (textboxes, etc.)
    /// </summary>
    public class ValuePatternHandler : BaseAutomationHandler
    {
        public ValuePatternHandler(
            ILogger<ValuePatternHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Sets the value of an element
        /// </summary>
        public async Task<WorkerResult> ExecuteSetValueAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("SetValue");
                }

                if (!operation.Parameters.TryGetValue("value", out var valueObj) || valueObj == null)
                {
                    return CreateParameterMissingResult("Value", "SetValue");
                }

                var value = valueObj.ToString() ?? "";

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObj) && 
                    patternObj is ValuePattern valuePattern)
                {
                    if (valuePattern.Current.IsReadOnly)
                    {
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element is read-only and cannot be modified"
                        };
                    }

                    _logger.LogInformation("[ValuePatternHandler] Setting value on element: {ElementName} to: {Value}", 
                        SafeGetElementName(element), value);
                    
                    valuePattern.SetValue(value);
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = $"Value set to: {value}"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ValuePattern");
                }
            }, "SetValue");
        }

        /// <summary>
        /// Gets the value of an element
        /// </summary>
        public async Task<WorkerResult> ExecuteGetValueAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("GetValue");
                }

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObj) && 
                    patternObj is ValuePattern valuePattern)
                {
                    var currentValue = valuePattern.Current.Value ?? "";
                    
                    _logger.LogInformation("[ValuePatternHandler] Got value from element: {ElementName}, value: {Value}", 
                        SafeGetElementName(element), currentValue);
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = new Dictionary<string, object>
                        {
                            ["Value"] = currentValue,
                            ["IsReadOnly"] = valuePattern.Current.IsReadOnly
                        }
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ValuePattern");
                }
            }, "GetValue");
        }
    }
}