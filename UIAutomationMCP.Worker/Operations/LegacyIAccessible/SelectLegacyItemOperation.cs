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
    public class SelectLegacyItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SelectLegacyItemOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<SelectLegacyItemRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected SelectLegacyItemRequest.",
                    Data = new ActionResult { ActionName = "SelectLegacyItem" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;
            var flagsSelect = typedRequest.FlagsSelect;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ActionResult { ActionName = "SelectLegacyItem" }
                });

            if (!element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out var pattern) || pattern is not LegacyIAccessiblePattern legacyPattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "LegacyIAccessiblePattern not supported",
                    Data = new ActionResult { ActionName = "SelectLegacyItem" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                legacyPattern.Select(flagsSelect);
                
                var flagDescription = GetFlagDescription(flagsSelect);
                
                var result = new ActionResult
                {
                    ActionName = "SelectLegacyItem",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId,
                        ["FlagsSelect"] = flagsSelect,
                        ["FlagDescription"] = flagDescription,
                        ["Message"] = $"Selected with flags: {flagDescription}"
                    }
                };
                
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to select legacy item: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "SelectLegacyItem",
                        Completed = false
                    }
                });
            }
        }

        private string GetFlagDescription(int flags)
        {
            var descriptions = new List<string>();
            
            if ((flags & 1) != 0) descriptions.Add("TAKEFOCUS");
            if ((flags & 2) != 0) descriptions.Add("TAKESELECTION");
            if ((flags & 4) != 0) descriptions.Add("EXTENDSELECTION");
            if ((flags & 8) != 0) descriptions.Add("ADDSELECTION");
            if ((flags & 16) != 0) descriptions.Add("REMOVESELECTION");
            
            return descriptions.Count > 0 ? string.Join(" | ", descriptions) : "NONE";
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