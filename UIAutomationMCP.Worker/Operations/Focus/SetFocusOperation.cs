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
            // パターン変換（リクエストから取得）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern);
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId,
                requiredPattern: requiredPattern);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SetFocus", request.AutomationId);
            }

            var elementInfo = _elementFinderService.GetElementBasicInfo(element);
            
            // フォーカス前の状態を取得
            var beforeFocused = element.Current.HasKeyboardFocus;
            
            // SetFocusを実行
            element.SetFocus();
            
            // 少し待機してフォーカス状態を確認
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
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}