using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetRowOrColumnMajorOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRowOrColumnMajorOperation(ElementFinderService elementFinderService)
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

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TablePattern not supported" });

            var rowOrColumnMajor = tablePattern.Current.RowOrColumnMajor;

            var result = new Dictionary<string, object>
            {
                ["RowOrColumnMajor"] = rowOrColumnMajor.ToString(),
                ["Element"] = new Dictionary<string, object>
                {
                    ["Name"] = element.Current.Name ?? string.Empty,
                    ["AutomationId"] = element.Current.AutomationId ?? string.Empty,
                    ["ControlType"] = element.Current.ControlType.ProgrammaticName,
                    ["ProcessId"] = element.Current.ProcessId
                }
            };

            return Task.FromResult(new OperationResult { Success = true, Data = result });
        }
    }
}