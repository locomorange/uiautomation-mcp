using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class MenuService : IMenuService
    {
        private readonly ILogger<MenuService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public MenuService(ILogger<MenuService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> MenuOperationAsync(string menuPath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Menu operation: {MenuPath}", menuPath);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var menuItems = menuPath.Split(new[] { '/', '\\', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (menuItems.Length == 0)
                    {
                        return new { error = "Invalid menu path" };
                    }

                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    AutomationElement currentElement = searchRoot;

                    foreach (var menuItem in menuItems)
                    {
                        var menuElement = currentElement.FindFirst(TreeScope.Children,
                            new AndCondition(
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem),
                                new PropertyCondition(AutomationElement.NameProperty, menuItem)
                            ));

                        if (menuElement == null)
                        {
                            return new { error = $"Menu item '{menuItem}' not found" };
                        }

                        // If this is the last item, invoke it
                        if (menuItem == menuItems.Last())
                        {
                            if (menuElement.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) &&
                                invokePattern is InvokePattern invokePatternInstance)
                            {
                                invokePatternInstance.Invoke();
                                return new { menuPath, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = $"Menu item '{menuItem}' cannot be invoked" };
                        }

                        currentElement = menuElement;
                    }

                    return new { error = "Menu operation failed" };
                }, timeoutSeconds, $"MenuOperation_{menuPath}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Menu operation: {MenuPath}", menuPath);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}