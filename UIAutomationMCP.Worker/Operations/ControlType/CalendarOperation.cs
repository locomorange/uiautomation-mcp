using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class CalendarOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public CalendarOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var operation = request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "getinfo";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (element.Current.ControlType != ControlType.Calendar)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a calendar" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var calendarInfo = new Dictionary<string, object>
                        {
                            ["Name"] = element.Current.Name,
                            ["AutomationId"] = element.Current.AutomationId,
                            ["IsEnabled"] = element.Current.IsEnabled,
                            ["IsVisible"] = !element.Current.IsOffscreen,
                            ["SupportedPatterns"] = element.GetSupportedPatterns().Select(p => p.ProgrammaticName).ToList()
                        };

                        // Check for GridPattern (common in calendar controls)
                        if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPattern) && gridPattern is GridPattern grid)
                        {
                            calendarInfo["GridRowCount"] = grid.Current.RowCount;
                            calendarInfo["GridColumnCount"] = grid.Current.ColumnCount;
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = calendarInfo });

                    case "select":
                        var dateValue = request.Parameters?.GetValueOrDefault("date")?.ToString() ?? "";
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern selection)
                        {
                            // Calendar selection would require finding the specific date element
                            // This is a simplified implementation
                            return Task.FromResult(new OperationResult { Success = false, Error = "Calendar date selection requires specific date element identification" });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Calendar does not support selection" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing calendar operation: {ex.Message}" });
            }
        }
    }
}