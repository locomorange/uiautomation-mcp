using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.LegacyIAccessible
{
    public class GetLegacyPropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetLegacyPropertiesOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<LegacyPropertiesResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetLegacyPropertiesRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<LegacyPropertiesResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected GetLegacyPropertiesRequest.",
                    Data = new LegacyPropertiesResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<LegacyPropertiesResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new LegacyPropertiesResult()
                });

            if (!element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out var pattern) || pattern is not LegacyIAccessiblePattern legacyPattern)
                return Task.FromResult(new OperationResult<LegacyPropertiesResult> 
                { 
                    Success = false, 
                    Error = "LegacyIAccessiblePattern not supported",
                    Data = new LegacyPropertiesResult()
                });

            try
            {
                var current = legacyPattern.Current;
                
                var result = new LegacyPropertiesResult
                {
                    Name = current.Name ?? "",
                    Role = current.Role ?? "",
                    State = current.State,
                    StateText = GetStateText(current.State),
                    Value = current.Value ?? "",
                    Description = current.Description ?? "",
                    Help = current.Help ?? "",
                    KeyboardShortcut = current.KeyboardShortcut ?? "",
                    DefaultAction = current.DefaultAction ?? "",
                    ChildId = current.ChildId
                };
                
                return Task.FromResult(new OperationResult<LegacyPropertiesResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<LegacyPropertiesResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get legacy properties: {ex.Message}",
                    Data = new LegacyPropertiesResult()
                });
            }
        }

        private string GetStateText(uint state)
        {
            var states = new List<string>();
            
            // Check common MSAA states
            if ((state & 0x1) != 0) states.Add("UNAVAILABLE");
            if ((state & 0x2) != 0) states.Add("SELECTED");
            if ((state & 0x4) != 0) states.Add("FOCUSED");
            if ((state & 0x8) != 0) states.Add("PRESSED");
            if ((state & 0x10) != 0) states.Add("CHECKED");
            if ((state & 0x20) != 0) states.Add("MIXED");
            if ((state & 0x40) != 0) states.Add("READONLY");
            if ((state & 0x80) != 0) states.Add("HOTTRACKED");
            if ((state & 0x100) != 0) states.Add("DEFAULT");
            if ((state & 0x200) != 0) states.Add("EXPANDED");
            if ((state & 0x400) != 0) states.Add("COLLAPSED");
            if ((state & 0x800) != 0) states.Add("BUSY");
            if ((state & 0x1000) != 0) states.Add("FLOATING");
            if ((state & 0x2000) != 0) states.Add("MARQUEED");
            if ((state & 0x4000) != 0) states.Add("ANIMATED");
            if ((state & 0x8000) != 0) states.Add("INVISIBLE");
            if ((state & 0x10000) != 0) states.Add("OFFSCREEN");
            if ((state & 0x20000) != 0) states.Add("SIZEABLE");
            if ((state & 0x40000) != 0) states.Add("MOVEABLE");
            if ((state & 0x80000) != 0) states.Add("SELFVOICING");
            if ((state & 0x100000) != 0) states.Add("FOCUSABLE");
            if ((state & 0x200000) != 0) states.Add("SELECTABLE");
            if ((state & 0x400000) != 0) states.Add("LINKED");
            if ((state & 0x800000) != 0) states.Add("TRAVERSED");
            if ((state & 0x1000000) != 0) states.Add("MULTISELECTABLE");
            if ((state & 0x2000000) != 0) states.Add("EXTSELECTABLE");
            if ((state & 0x4000000) != 0) states.Add("ALERT_LOW");
            if ((state & 0x8000000) != 0) states.Add("ALERT_MEDIUM");
            if ((state & 0x10000000) != 0) states.Add("ALERT_HIGH");
            if ((state & 0x20000000) != 0) states.Add("PROTECTED");
            if ((state & 0x40000000) != 0) states.Add("HASPOPUP");

            return states.Count > 0 ? string.Join(", ", states) : "NORMAL";
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}