using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;
using UIAutomationMCP.Worker.Extensions;

namespace UIAutomationMCP.Worker.Operations.Focus
{
    public class SetFocusOperation : BaseUIAutomationOperation<SetFocusRequest, ActionResult>
    {
        public SetFocusOperation(ElementFinderService elementFinderService, ILogger<SetFocusOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ActionResult> ExecuteOperationAsync(SetFocusRequest request)
        {
            // 繝代ち繝ｼ繝ｳ螟画鋤・医Μ繧ｯ繧ｨ繧ｹ繝医°繧牙叙蠕暦ｼ・
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern);
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = requiredPattern?.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SetFocus", request.AutomationId);
            }

            var elementInfo = _elementFinderService.GetElementBasicInfo(element);
            
            // 繝輔か繝ｼ繧ｫ繧ｹ蜑阪・迥ｶ諷九ｒ蜿門ｾ・
            var beforeFocused = element.Current.HasKeyboardFocus;
            
            // SetFocus繧貞ｮ溯｡・
            element.SetFocus();
            
            // 蟆代＠蠕・ｩ溘＠縺ｦ繝輔か繝ｼ繧ｫ繧ｹ迥ｶ諷九ｒ遒ｺ隱・
            await Task.Delay(100);
            var afterFocused = element.Current.HasKeyboardFocus;
            
            return new ActionResult
            {
                ActionName = "SetFocus",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Set focus to element: {elementInfo.Name} (Type: {elementInfo.ControlType}, ID: {elementInfo.AutomationId})",
                TargetName = elementInfo.Name,
                TargetAutomationId = elementInfo.AutomationId,
                TargetControlType = elementInfo.ControlType,
                BeforeState = new ElementState { HasFocus = beforeFocused },
                AfterState = new ElementState { HasFocus = afterFocused },
                StateChanged = beforeFocused != afterFocused,
                ActionParameters = new ActionParameters { AdditionalProperties = new Dictionary<string, object> { { "RequiredPattern", request.RequiredPattern ?? "None" } } }
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SetFocusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && 
                string.IsNullOrWhiteSpace(request.Name) && 
                string.IsNullOrWhiteSpace(request.ControlType))
            {
                return Core.Validation.ValidationResult.Failure("At least one element identifier (AutomationId, Name, or ControlType) is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}