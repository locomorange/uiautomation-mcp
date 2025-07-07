using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IRangeService
    {
        Task<object> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class RangeService : IRangeService
    {
        private readonly ILogger<RangeService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public RangeService(ILogger<RangeService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting range value: {ElementId} = {Value}", elementId, value);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) && pattern is RangeValuePattern rangePattern)
                    {
                        var currentValue = rangePattern.Current.Value;
                        var minimum = rangePattern.Current.Minimum;
                        var maximum = rangePattern.Current.Maximum;
                        var isReadOnly = rangePattern.Current.IsReadOnly;

                        if (isReadOnly)
                        {
                            throw new InvalidOperationException("Range element is read-only");
                        }

                        if (value < minimum || value > maximum)
                        {
                            throw new ArgumentOutOfRangeException(nameof(value), 
                                $"Value {value} is out of range. Valid range: {minimum} - {maximum}");
                        }

                        rangePattern.SetValue(value);
                        var newValue = rangePattern.Current.Value;
                        
                        return new 
                        { 
                            PreviousValue = currentValue, 
                            NewValue = newValue,
                            Minimum = minimum,
                            Maximum = maximum
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support RangeValuePattern");
                    }
                }, timeoutSeconds, $"SetRangeValue_{elementId}");

                _logger.LogInformation("Range value set successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Range value set to: {value}", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set range value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting range value: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var rangeInfo = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) && pattern is RangeValuePattern rangePattern)
                    {
                        return new
                        {
                            Value = rangePattern.Current.Value,
                            Minimum = rangePattern.Current.Minimum,
                            Maximum = rangePattern.Current.Maximum,
                            LargeChange = rangePattern.Current.LargeChange,
                            SmallChange = rangePattern.Current.SmallChange,
                            IsReadOnly = rangePattern.Current.IsReadOnly
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support RangeValuePattern");
                    }
                }, timeoutSeconds, $"GetRangeValue_{elementId}");

                _logger.LogInformation("Range value retrieved successfully: {ElementId}", elementId);
                return new { Success = true, Data = rangeInfo };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get range value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}