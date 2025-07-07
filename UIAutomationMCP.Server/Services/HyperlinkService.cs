using System;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services
{
    public class HyperlinkService : IHyperlinkService
    {
        private readonly ILogger<HyperlinkService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public HyperlinkService(ILogger<HyperlinkService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> HyperlinkOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Hyperlink operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var hyperlink = _automationHelper.FindElementById(elementId, searchRoot);

                    if (hyperlink == null)
                    {
                        return new { error = $"Hyperlink '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "click":
                            if (hyperlink.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) &&
                                invokePattern is InvokePattern invokePatternInstance)
                            {
                                invokePatternInstance.Invoke();
                                return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Hyperlink does not support click operation" };

                        case "gettext":
                            var text = hyperlink.Current.Name;
                            return new { elementId, operation, text, success = true, timestamp = DateTime.UtcNow };

                        case "geturl":
                            // Try to get the URL from the hyperlink
                            var url = hyperlink.Current.HelpText ?? hyperlink.Current.Name;
                            return new { elementId, operation, url, success = true, timestamp = DateTime.UtcNow };

                        case "getstatus":
                            var isEnabled = hyperlink.Current.IsEnabled;
                            var isVisible = !hyperlink.Current.IsOffscreen;
                            var linkText = hyperlink.Current.Name;
                            var hasKeyboardFocus = hyperlink.Current.HasKeyboardFocus;

                            return new { 
                                elementId, 
                                operation, 
                                isEnabled, 
                                isVisible, 
                                linkText, 
                                hasKeyboardFocus, 
                                success = true, 
                                timestamp = DateTime.UtcNow 
                            };

                        case "setfocus":
                            hyperlink.SetFocus();
                            return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };

                        case "isfocused":
                            var focused = hyperlink.Current.HasKeyboardFocus;
                            return new { elementId, operation, isFocused = focused, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: click, gettext, geturl, getstatus, setfocus, isfocused" };
                    }
                }, timeoutSeconds, $"HyperlinkOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Hyperlink operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}