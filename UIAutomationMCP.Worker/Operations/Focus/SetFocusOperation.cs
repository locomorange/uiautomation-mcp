using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;

namespace UIAutomationMCP.Worker.Operations.Focus
{
    public class SetFocusOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetFocusOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SetFocusRequest>(parametersJson)!;
                
                // パターン変換（リクエストから取得）
                var requiredPattern = AutomationPatternHelper.GetAutomationPattern(typedRequest.RequiredPattern);
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId,
                    requiredPattern: requiredPattern);
                    
                if (element == null)
                {
                    return Task.FromResult(new OperationResult
                    {
                        Success = false,
                        Error = "Element not found",
                        Data = new ActionResult { ActionName = "SetFocus" }
                    });
                }

                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                // フォーカス前の状態を取得
                var beforeFocused = element.Current.HasKeyboardFocus;
                
                // SetFocusを実行
                element.SetFocus();
                
                // 少し待機してフォーカス状態を確認
                Thread.Sleep(100);
                var afterFocused = element.Current.HasKeyboardFocus;
                
                var result = new ActionResult
                {
                    ActionName = "SetFocus",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Set focus to element: {elementInfo.Name} (Type: {elementInfo.ControlType}, ID: {elementInfo.AutomationId})",
                    TargetName = elementInfo.Name,
                    TargetAutomationId = elementInfo.AutomationId,
                    TargetControlType = elementInfo.ControlType,
                    BeforeState = new Dictionary<string, object>
                    {
                        { "HasKeyboardFocus", beforeFocused }
                    },
                    AfterState = new Dictionary<string, object>
                    {
                        { "HasKeyboardFocus", afterFocused }
                    },
                    StateChanged = beforeFocused != afterFocused,
                    ActionParameters = new Dictionary<string, object>
                    {
                        { "RequiredPattern", typedRequest.RequiredPattern ?? "None" }
                    }
                };
                
                return Task.FromResult(new OperationResult
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult
                {
                    Success = false,
                    Error = $"Failed to set focus: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "SetFocus",
                        Completed = false
                    }
                });
            }
        }
    }
}