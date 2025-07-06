using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Services
{
    public interface IValueService
    {
        Task<object> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class ValueService : IValueService
    {
        private readonly ILogger<ValueService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public ValueService(ILogger<ValueService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting element value: {ElementId} = {Value}", elementId, value);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                    {
                        valuePattern.SetValue(value);
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support ValuePattern");
                    }
                }, timeoutSeconds, $"SetValue_{elementId}");

                _logger.LogInformation("Element value set successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element value set to: {value}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set element value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting element value: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var currentValue = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                    {
                        return valuePattern.Current.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support ValuePattern");
                    }
                }, timeoutSeconds, $"GetValue_{elementId}");

                _logger.LogInformation("Element value retrieved successfully: {ElementId}", elementId);
                return new { Success = true, Data = currentValue ?? "" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}