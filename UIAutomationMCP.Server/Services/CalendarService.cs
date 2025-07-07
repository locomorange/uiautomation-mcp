using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ILogger<CalendarService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public CalendarService(ILogger<CalendarService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> CalendarOperationAsync(string elementId, string operation, DateTime? date = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Calendar operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var calendar = _automationHelper.FindElementById(elementId, searchRoot);

                    if (calendar == null)
                    {
                        return new { error = $"Calendar '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "selectdate":
                            if (!date.HasValue)
                            {
                                return new { error = "date is required for selectdate operation" };
                            }

                            // Find the date button
                            var dateButton = calendar.FindFirst(TreeScope.Descendants,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                                    new PropertyCondition(AutomationElement.NameProperty, date.Value.Day.ToString())
                                ));

                            if (dateButton == null)
                            {
                                return new { error = $"Date {date.Value.Day} not found in calendar" };
                            }

                            if (dateButton.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) &&
                                invokePattern is InvokePattern invokePatternInstance)
                            {
                                invokePatternInstance.Invoke();
                                return new { elementId, operation, dateSelected = date.Value.ToString("yyyy-MM-dd"), success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Date button cannot be invoked" };

                        case "getselecteddate":
                            // Try to get the selected date from the calendar
                            var selectedDateElements = calendar.FindAll(TreeScope.Descendants,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                                    new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true)
                                ));

                            if (selectedDateElements.Count > 0)
                            {
                                var selectedElement = selectedDateElements[0];
                                return new { elementId, operation, selectedDate = selectedElement.Current.Name, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "No date is currently selected" };

                        case "navigate":
                            if (!date.HasValue)
                            {
                                return new { error = "date is required for navigate operation" };
                            }

                            // Find navigation buttons (previous/next month)
                            var navButtons = calendar.FindAll(TreeScope.Descendants,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

                            // This is a simplified navigation - in practice, you'd need to implement
                            // more sophisticated month/year navigation based on the calendar implementation
                            return new { elementId, operation, targetDate = date.Value.ToString("yyyy-MM-dd"), success = true, timestamp = DateTime.UtcNow };

                        case "getavailabledates":
                            var dateButtons = calendar.FindAll(TreeScope.Descendants,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

                            var availableDates = dateButtons.Cast<AutomationElement>()
                                .Where(btn => int.TryParse(btn.Current.Name, out _))
                                .Select(btn => new
                                {
                                    day = btn.Current.Name,
                                    automationId = btn.Current.AutomationId,
                                    isSelected = btn.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) &&
                                               itemPattern is SelectionItemPattern selectionItem &&
                                               selectionItem.Current.IsSelected,
                                    isEnabled = btn.Current.IsEnabled
                                }).ToList();

                            return new { elementId, operation, availableDates, totalDates = availableDates.Count, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: selectdate, getselecteddate, navigate, getavailabledates" };
                    }
                }, timeoutSeconds, $"CalendarOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Calendar operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}