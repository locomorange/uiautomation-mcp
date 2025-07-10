using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// 要素検索操作の最小粒度API
    /// </summary>
    public class ElementSearchOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public ElementSearchOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        /// <summary>
        /// 単一要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElement(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            // Let exceptions flow naturally - no try-catch
            var element = searchRoot.FindFirst(scope, condition);
            return new OperationResult<AutomationElement>
            {
                Success = true,
                Data = element
            };
        }

        /// <summary>
        /// 複数要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElements(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            // Let exceptions flow naturally - no try-catch
            var elements = searchRoot.FindAll(scope, condition);
            return new OperationResult<AutomationElementCollection>
            {
                Success = true,
                Data = elements
            };
        }

        /// <summary>
        /// AutomationIdで要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementById(
            AutomationElement searchRoot,
            string automationId)
        {
            // Let exceptions flow naturally - no try-catch
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
            var element = searchRoot.FindFirst(TreeScope.Descendants, condition);
            return new OperationResult<AutomationElement>
            {
                Success = true,
                Data = element
            };
        }

        /// <summary>
        /// 名前で要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementByName(
            AutomationElement searchRoot,
            string name)
        {
            // Let exceptions flow naturally - no try-catch
            var condition = new PropertyCondition(AutomationElement.NameProperty, name);
            var element = searchRoot.FindFirst(TreeScope.Descendants, condition);
            return new OperationResult<AutomationElement>
            {
                Success = true,
                Data = element
            };
        }

        /// <summary>
        /// コントロールタイプで要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElementsByControlType(
            AutomationElement searchRoot,
            ControlType controlType)
        {
            // Let exceptions flow naturally - no try-catch
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
            var elements = searchRoot.FindAll(TreeScope.Descendants, condition);
            return new OperationResult<AutomationElementCollection>
            {
                Success = true,
                Data = elements
            };
        }

        /// <summary>
        /// ルート要素を取得
        /// </summary>
        public OperationResult<AutomationElement> GetRootElement()
        {
            // Let exceptions flow naturally - no try-catch
            var rootElement = AutomationElement.RootElement;
            return new OperationResult<AutomationElement>
            {
                Success = true,
                Data = rootElement
            };
        }

        /// <summary>
        /// デスクトップのウィンドウを取得
        /// </summary>
        public OperationResult GetDesktopWindows()
        {
            // Let exceptions flow naturally - no try-catch
            var rootElement = AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
            var windows = rootElement.FindAll(TreeScope.Children, condition);
            
            var windowList = new List<WindowInfo>();
            foreach (AutomationElement window in windows)
            {
                // Basic null check only - let exceptions flow naturally
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

            return new OperationResult
            {
                Success = true,
                Data = windowList
            };
        }

        /// <summary>
        /// パラメータベースで要素を検索
        /// </summary>
        public OperationResult FindElements(string searchText = "", string controlType = "", string windowTitle = "", int processId = 0)
        {
            // Let exceptions flow naturally - no try-catch
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
                // Basic null check only - let exceptions flow naturally
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

            return new OperationResult
            {
                Success = true,
                Data = elementList
            };
        }

        private ControlType? GetControlTypeFromString(string controlType)
        {
            return controlType.ToLowerInvariant() switch
            {
                "button" => ControlType.Button,
                "edit" => ControlType.Edit,
                "text" => ControlType.Text,
                "window" => ControlType.Window,
                "combobox" => ControlType.ComboBox,
                "list" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "menu" => ControlType.Menu,
                "menuitem" => ControlType.MenuItem,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "checkbox" => ControlType.CheckBox,
                "radiobutton" => ControlType.RadioButton,
                "hyperlink" => ControlType.Hyperlink,
                "table" => ControlType.Table,
                "group" => ControlType.Group,
                _ => null
            };
        }

    }
}
