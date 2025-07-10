using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetTextOperation(ElementFinderService elementFinderService)
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

            // Try TextPattern first
            if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern) && textPattern is TextPattern tp)
            {
                var documentRange = tp.DocumentRange;
                var text = documentRange.GetText(-1);
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = text, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                });
            }
            // Try ValuePattern for text input controls
            else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = vp.Current.Value, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                });
            }
            else
            {
                // Fallback to Name property
                var text = element.Current.Name ?? "";
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = text, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                });
            }
        }
    }
}
