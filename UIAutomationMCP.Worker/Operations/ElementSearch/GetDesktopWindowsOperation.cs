using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class GetDesktopWindowsOperation : IUIAutomationOperation
    {
        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var rootElement = AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window);
            var windows = rootElement.FindAll(TreeScope.Children, condition);
            
            var windowList = new List<WindowInfo>();
            foreach (AutomationElement window in windows)
            {
                if (window != null)
                {
                    windowList.Add(new WindowInfo
                    {
                        Name = window.Current.Name,
                        ProcessId = window.Current.ProcessId,
                        AutomationId = window.Current.AutomationId
                    });
                }
            }

            return Task.FromResult(new OperationResult
            {
                Success = true,
                Data = windowList
            });
        }
    }
}