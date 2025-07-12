using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetColumnHeaderItemsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetColumnHeaderItemsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TableItemPattern.Pattern, out var pattern) || pattern is not TableItemPattern tableItemPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TableItemPattern not supported" });

            try
            {
                var columnHeaderItems = tableItemPattern.Current.GetColumnHeaderItems();
                if (columnHeaderItems == null || columnHeaderItems.Length == 0)
                    return Task.FromResult(new OperationResult { Success = false, Error = "No column header items found" });

                var headerInfos = new List<Dictionary<string, object>>();
                foreach (var header in columnHeaderItems)
                {
                    var headerInfo = new Dictionary<string, object>
                    {
                        ["AutomationId"] = header.Current.AutomationId,
                        ["Name"] = header.Current.Name,
                        ["ControlType"] = header.Current.ControlType.LocalizedControlType,
                        ["IsEnabled"] = header.Current.IsEnabled,
                        ["BoundingRectangle"] = new BoundingRectangle
                        {
                            X = header.Current.BoundingRectangle.X,
                            Y = header.Current.BoundingRectangle.Y,
                            Width = header.Current.BoundingRectangle.Width,
                            Height = header.Current.BoundingRectangle.Height
                        }
                    };
                    headerInfos.Add(headerInfo);
                }

                return Task.FromResult(new OperationResult { Success = true, Data = headerInfos });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting column header items: {ex.Message}" });
            }
        }
    }
}