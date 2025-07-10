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

        private ControlType? GetControlTypeFromString(string controlType)
        {
            return controlType.ToLower() switch
            {
                "button" => ControlType.Button,
                "text" => ControlType.Text,
                "edit" => ControlType.Edit,
                "combobox" => ControlType.ComboBox,
                "listbox" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "checkbox" => ControlType.CheckBox,
                "radiobutton" => ControlType.RadioButton,
                "window" => ControlType.Window,
                "menu" => ControlType.Menu,
                "menuitem" => ControlType.MenuItem,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "group" => ControlType.Group,
                "pane" => ControlType.Pane,
                "document" => ControlType.Document,
                "table" => ControlType.Table,
                "hyperlink" => ControlType.Hyperlink,
                "image" => ControlType.Image,
                "spinner" => ControlType.Spinner,
                "slider" => ControlType.Slider,
                "progressbar" => ControlType.ProgressBar,
                "scrollbar" => ControlType.ScrollBar,
                "statusbar" => ControlType.StatusBar,
                "toolbar" => ControlType.ToolBar,
                "tooltip" => ControlType.ToolTip,
                _ => null
            };
        }
    }
}