using System;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services
{
    public class ButtonService : IButtonService
    {
        private readonly ILogger<ButtonService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public ButtonService(ILogger<ButtonService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> ButtonOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Button operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var button = _automationHelper.FindElementById(elementId, searchRoot);

                    if (button == null)
                    {
                        return new { error = $"Button '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "click":
                            if (button.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) &&
                                invokePattern is InvokePattern invokePatternInstance)
                            {
                                invokePatternInstance.Invoke();
                                return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Button does not support click operation" };

                        case "getstatus":
                            var isEnabled = button.Current.IsEnabled;
                            var isVisible = !button.Current.IsOffscreen;
                            var buttonText = button.Current.Name;
                            var hasKeyboardFocus = button.Current.HasKeyboardFocus;

                            return new { 
                                elementId, 
                                operation, 
                                isEnabled, 
                                isVisible, 
                                buttonText, 
                                hasKeyboardFocus, 
                                success = true, 
                                timestamp = DateTime.UtcNow 
                            };

                        case "toggle":
                            if (button.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) &&
                                togglePattern is TogglePattern togglePatternInstance)
                            {
                                togglePatternInstance.Toggle();
                                var newState = togglePatternInstance.Current.ToggleState;
                                return new { elementId, operation, newState = newState.ToString(), success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Button does not support toggle operation" };

                        case "gettext":
                            var text = button.Current.Name;
                            return new { elementId, operation, text, success = true, timestamp = DateTime.UtcNow };

                        case "isfocused":
                            var focused = button.Current.HasKeyboardFocus;
                            return new { elementId, operation, isFocused = focused, success = true, timestamp = DateTime.UtcNow };

                        case "setfocus":
                            button.SetFocus();
                            return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: click, getstatus, toggle, gettext, isfocused, setfocus" };
                    }
                }, timeoutSeconds, $"ButtonOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Button operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}