using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class FindElementsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public FindElementsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var searchText = request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "";
            var controlType = request.Parameters?.GetValueOrDefault("controlType")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            
            Condition condition = Condition.TrueCondition;
            
            // Build search condition
            var conditions = new List<Condition>();
            
            if (!string.IsNullOrEmpty(searchText))
            {
                var nameCondition = new PropertyCondition(AutomationElement.NameProperty, searchText);
                var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, searchText);
                conditions.Add(new OrCondition(nameCondition, automationIdCondition));
            }
            
            if (!string.IsNullOrEmpty(controlType))
            {
                var controlTypeObj = GetControlTypeFromString(controlType);
                if (controlTypeObj != null)
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj));
                }
            }
            
            if (conditions.Count > 0)
            {
                condition = conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray());
            }
            
            var elements = searchRoot.FindAll(TreeScope.Descendants, condition);
            var elementList = new List<ElementInfo>();
            
            foreach (AutomationElement element in elements)
            {
                if (element != null)
                {
                    elementList.Add(new ElementInfo
                    {
                        AutomationId = element.Current.AutomationId,
                        Name = element.Current.Name,
                        ControlType = element.Current.ControlType.LocalizedControlType,
                        IsEnabled = element.Current.IsEnabled,
                        ProcessId = element.Current.ProcessId,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = element.Current.BoundingRectangle.X,
                            Y = element.Current.BoundingRectangle.Y,
                            Width = element.Current.BoundingRectangle.Width,
                            Height = element.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return Task.FromResult(new OperationResult
            {
                Success = true,
                Data = elementList
            });
        }

        private System.Windows.Automation.ControlType? GetControlTypeFromString(string controlType)
        {
            return controlType.ToLower() switch
            {
                "button" => System.Windows.Automation.ControlType.Button,
                "text" => System.Windows.Automation.ControlType.Text,
                "edit" => System.Windows.Automation.ControlType.Edit,
                "combobox" => System.Windows.Automation.ControlType.ComboBox,
                "listbox" => System.Windows.Automation.ControlType.List,
                "listitem" => System.Windows.Automation.ControlType.ListItem,
                "checkbox" => System.Windows.Automation.ControlType.CheckBox,
                "radiobutton" => System.Windows.Automation.ControlType.RadioButton,
                "window" => System.Windows.Automation.ControlType.Window,
                "menu" => System.Windows.Automation.ControlType.Menu,
                "menuitem" => System.Windows.Automation.ControlType.MenuItem,
                "tree" => System.Windows.Automation.ControlType.Tree,
                "treeitem" => System.Windows.Automation.ControlType.TreeItem,
                "tab" => System.Windows.Automation.ControlType.Tab,
                "tabitem" => System.Windows.Automation.ControlType.TabItem,
                "group" => System.Windows.Automation.ControlType.Group,
                "pane" => System.Windows.Automation.ControlType.Pane,
                "document" => System.Windows.Automation.ControlType.Document,
                "table" => System.Windows.Automation.ControlType.Table,
                "hyperlink" => System.Windows.Automation.ControlType.Hyperlink,
                "image" => System.Windows.Automation.ControlType.Image,
                "spinner" => System.Windows.Automation.ControlType.Spinner,
                "slider" => System.Windows.Automation.ControlType.Slider,
                "progressbar" => System.Windows.Automation.ControlType.ProgressBar,
                "scrollbar" => System.Windows.Automation.ControlType.ScrollBar,
                "statusbar" => System.Windows.Automation.ControlType.StatusBar,
                "toolbar" => System.Windows.Automation.ControlType.ToolBar,
                "tooltip" => System.Windows.Automation.ControlType.ToolTip,
                _ => null
            };
        }
    }
}