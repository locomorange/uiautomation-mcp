using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPatternsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetElementPatternsOperation(ElementFinderService elementFinderService)
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

            var supportedPatterns = new List<string>();
            var patternIds = element.GetSupportedPatterns();

            foreach (var patternId in patternIds)
            {
                supportedPatterns.Add(patternId.ProgrammaticName);
            }

            return Task.FromResult(new OperationResult { Success = true, Data = supportedPatterns });
        }
    }
}