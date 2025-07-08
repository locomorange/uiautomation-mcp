using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InvokeService : IInvokeService
    {
        private readonly ILogger<InvokeService> _logger;
        private readonly WorkerExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public InvokeService(ILogger<InvokeService> logger, WorkerExecutor executor, AutomationHelper automationHelper)
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

                var result = await _executor.ExecuteAsync(() =>
                {
                    return _executor.Invoke.Invoke(element);
                }, timeoutSeconds, $"InvokeElement_{elementId}");

                if (!result.Success)
                {
                    return new { Success = false, Error = result.Error };
                }

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