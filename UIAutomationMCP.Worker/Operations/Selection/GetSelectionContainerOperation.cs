using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class GetSelectionContainerOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetSelectionContainerOperation(ElementFinderService elementFinderService)
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
            
            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element does not support SelectionItemPattern: {elementId}" });
            }

            var selectionItemPattern = (SelectionItemPattern)patternObject;
            var selectionContainer = selectionItemPattern.Current.SelectionContainer;
            
            if (selectionContainer == null)
            {
                return Task.FromResult(new OperationResult { Success = true, Data = new { SelectionContainer = (object?)null } });
            }
            
            var containerData = new
            {
                SelectionContainer = new
                {
                    AutomationId = selectionContainer.Current.AutomationId,
                    Name = selectionContainer.Current.Name,
                    ControlType = selectionContainer.Current.ControlType.ProgrammaticName,
                    ClassName = selectionContainer.Current.ClassName,
                    ProcessId = selectionContainer.Current.ProcessId,
                    RuntimeId = selectionContainer.GetRuntimeId()
                }
            };
            
            return Task.FromResult(new OperationResult { Success = true, Data = containerData });
        }
    }
}