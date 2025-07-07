using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IToggleService
    {
        Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class ToggleService : IToggleService
    {
        private readonly ILogger<ToggleService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public ToggleService(ILogger<ToggleService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Toggling element: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) && pattern is TogglePattern togglePattern)
                    {
                        var currentState = togglePattern.Current.ToggleState;
                        togglePattern.Toggle();
                        var newState = togglePattern.Current.ToggleState;
                        return new { CurrentState = currentState.ToString(), NewState = newState.ToString() };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TogglePattern");
                    }
                }, timeoutSeconds, $"ToggleElement_{elementId}");

                _logger.LogInformation("Element toggled successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element toggled successfully", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}