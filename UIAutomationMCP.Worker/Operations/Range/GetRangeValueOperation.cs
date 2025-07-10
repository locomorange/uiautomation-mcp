using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class GetRangeValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRangeValueOperation(ElementFinderService elementFinderService)
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
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" });

            var rangeInfo = new
            {
                Value = rangePattern.Current.Value,
                Minimum = rangePattern.Current.Minimum,
                Maximum = rangePattern.Current.Maximum,
                LargeChange = rangePattern.Current.LargeChange,
                SmallChange = rangePattern.Current.SmallChange,
                IsReadOnly = rangePattern.Current.IsReadOnly
            };

            return Task.FromResult(new OperationResult { Success = true, Data = rangeInfo });
        }
    }
}
