using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Worker.Core;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// 要素検索操作の最小粒度API
    /// </summary>
    public class ElementSearchOperations
    {
        private readonly ILogger<ElementSearchOperations> _logger;

        public ElementSearchOperations(ILogger<ElementSearchOperations> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 単一要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElement(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            try
            {
                var element = searchRoot.FindElementSafe(scope, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element");
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 複数要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElements(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            try
            {
                var elements = searchRoot.FindElementsSafe(scope, condition);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = true,
                    Data = elements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// AutomationIdで要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementById(
            AutomationElement searchRoot,
            string automationId)
        {
            try
            {
                var condition = ConditionBuilder.ByAutomationId(automationId);
                var element = searchRoot.FindElementSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by ID: {AutomationId}", automationId);
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 名前で要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementByName(
            AutomationElement searchRoot,
            string name)
        {
            try
            {
                var condition = ConditionBuilder.ByName(name);
                var element = searchRoot.FindElementSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by name: {Name}", name);
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// コントロールタイプで要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElementsByControlType(
            AutomationElement searchRoot,
            ControlType controlType)
        {
            try
            {
                var condition = ConditionBuilder.ByControlType(controlType);
                var elements = searchRoot.FindElementsSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = true,
                    Data = elements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by control type: {ControlType}", controlType.LocalizedControlType);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// ルート要素を取得
        /// </summary>
        public OperationResult<AutomationElement> GetRootElement()
        {
            try
            {
                var rootElement = AutomationElement.RootElement;
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = rootElement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get root element");
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// デスクトップのウィンドウを取得
        /// </summary>
        public OperationResult GetDesktopWindows()
        {
            try
            {
                var rootElement = AutomationElement.RootElement;
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
                var windows = rootElement.FindAll(TreeScope.Children, condition);
                
                var windowList = new List<WindowInfo>();
                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        windowList.Add(new WindowInfo
                        {
                            Name = window.Current.Name,
                            ProcessId = window.Current.ProcessId,
                            AutomationId = window.Current.AutomationId
                        });
                    }
                    catch (Exception)
                    {
                        // Skip windows that can't be processed
                    }
                }

                return new OperationResult
                {
                    Success = true,
                    Data = windowList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get desktop windows");
                return new OperationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// パラメータベースで要素を検索
        /// </summary>
        public OperationResult FindElements(string searchText = "", string controlType = "", string windowTitle = "", int processId = 0)
        {
            try
            {
                var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                
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
                    try
                    {
                        elementList.Add(new ElementInfo
                        {
                            AutomationId = element.Current.AutomationId,
                            Name = element.Current.Name,
                            ControlType = element.Current.ControlType.LocalizedControlType,
                            IsEnabled = element.Current.IsEnabled,
                            ProcessId = element.Current.ProcessId,
                            BoundingRectangle = element.Current.BoundingRectangle
                        });
                    }
                    catch (Exception)
                    {
                        // Skip elements that can't be processed
                    }
                }

                return new OperationResult
                {
                    Success = true,
                    Data = elementList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new OperationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
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

        private AutomationElement? GetSearchRoot(string windowTitle, int processId)
        {
            if (processId > 0)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            return null;
        }
    }
}