using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Services
{
    public interface IInvokeService
    {
        Task<object> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class InvokeService : IInvokeService
    {
        private readonly ILogger<InvokeService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public InvokeService(ILogger<InvokeService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Invoking element: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) && pattern is InvokePattern invokePattern)
                    {
                        invokePattern.Invoke();
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support InvokePattern");
                    }
                }, timeoutSeconds, $"InvokeElement_{elementId}");

                _logger.LogInformation("Element invoked successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element invoked successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}