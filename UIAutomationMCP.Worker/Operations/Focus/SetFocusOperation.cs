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
            // パターン変換�E�リクエストから取得！E
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
            
            // フォーカス前�E状態を取征E
            var beforeFocused = element.Current.HasKeyboardFocus;
            
            // SetFocusを実衁E
            element.SetFocus();
            
            // 少し征E��してフォーカス状態を確誁E
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