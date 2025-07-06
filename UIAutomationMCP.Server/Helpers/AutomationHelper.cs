using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Helpers
{
    /// <summary>
    /// UI Automation関連のヘルパーメソッドを提供するクラス
    /// </summary>
    public class AutomationHelper
    {
        private readonly ILogger<AutomationHelper> _logger;

        public AutomationHelper(ILogger<AutomationHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 操作の検索ルート要素を取得します
        /// </summary>
        public AutomationElement? GetSearchRoot(WorkerOperation operation)
        {
            try
            {
                _logger.LogInformation("[AutomationHelper] Getting search root for operation");

                // ウィンドウタイトルが指定されている場合
                if (operation.Parameters.TryGetValue("WindowTitle", out var windowTitle) && 
                    windowTitle?.ToString() is string windowTitleStr && !string.IsNullOrEmpty(windowTitleStr))
                {
                    _logger.LogInformation("[AutomationHelper] Searching for window with title: {WindowTitle}", windowTitleStr);
                    return FindWindowByTitle(windowTitleStr);
                }

                // プロセスIDが指定されている場合
                if (operation.Parameters.TryGetValue("ProcessId", out var processIdObj) && 
                    processIdObj != null && int.TryParse(processIdObj.ToString(), out var processId) && processId > 0)
                {
                    _logger.LogInformation("[AutomationHelper] Searching for window with ProcessId: {ProcessId}", processId);
                    return FindWindowByProcessId(processId);
                }

                // Window操作の場合はデスクトップから直接検索
                if (IsWindowControlType(operation))
                {
                    _logger.LogInformation("[AutomationHelper] Using desktop as search root for window operation");
                    return AutomationElement.RootElement;
                }

                // デフォルトはデスクトップルート
                _logger.LogInformation("[AutomationHelper] Using desktop root as default search root");
                return AutomationElement.RootElement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to get search root");
                return AutomationElement.RootElement;
            }
        }

        /// <summary>
        /// 操作の検索条件を構築します
        /// </summary>
        public Condition? BuildCondition(WorkerOperation operation)
        {
            try
            {
                var conditions = new List<Condition>();

                // ControlType条件
                if (operation.Parameters.TryGetValue("ControlType", out var controlTypeValue) && 
                    controlTypeValue?.ToString() is string controlTypeStr && !string.IsNullOrEmpty(controlTypeStr))
                {
                    var controlType = ParseControlType(controlTypeStr);
                    if (controlType != null)
                    {
                        conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
                        _logger.LogInformation("[AutomationHelper] Added ControlType condition: {ControlType}", controlTypeStr);
                    }
                }

                // Name条件
                if (operation.Parameters.TryGetValue("Name", out var nameValue) && 
                    nameValue?.ToString() is string nameStr && !string.IsNullOrEmpty(nameStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, nameStr));
                    _logger.LogInformation("[AutomationHelper] Added Name condition: {Name}", nameStr);
                }

                // AutomationId条件
                if (operation.Parameters.TryGetValue("AutomationId", out var automationIdValue) && 
                    automationIdValue?.ToString() is string automationIdStr && !string.IsNullOrEmpty(automationIdStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, automationIdStr));
                    _logger.LogInformation("[AutomationHelper] Added AutomationId condition: {AutomationId}", automationIdStr);
                }

                // 要素ID検索（NameまたはAutomationIdのいずれかにマッチ）
                if (operation.Parameters.TryGetValue("ElementId", out var elementIdValue) && 
                    elementIdValue?.ToString() is string elementIdStr && !string.IsNullOrEmpty(elementIdStr))
                {
                    var nameCondition = new PropertyCondition(AutomationElement.NameProperty, elementIdStr);
                    var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementIdStr);
                    conditions.Add(new OrCondition(nameCondition, automationIdCondition));
                    _logger.LogInformation("[AutomationHelper] Added ElementId condition (Name OR AutomationId): {ElementId}", elementIdStr);
                }

                if (conditions.Count == 0)
                {
                    _logger.LogWarning("[AutomationHelper] No search conditions specified, using TrueCondition");
                    return Condition.TrueCondition;
                }

                var result = conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray());
                _logger.LogInformation("[AutomationHelper] Built condition with {Count} sub-conditions", conditions.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to build search condition");
                return null;
            }
        }

        /// <summary>
        /// 指定した要素IDで要素を検索します
        /// </summary>
        public AutomationElement? FindElementById(string elementId, AutomationElement searchRoot)
        {
            try
            {
                _logger.LogInformation("[AutomationHelper] Searching for element by ID: {ElementId}", elementId);

                var conditions = new List<Condition>();

                // Name条件
                if (!string.IsNullOrEmpty(elementId))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, elementId));
                }

                // AutomationId条件  
                if (!string.IsNullOrEmpty(elementId))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, elementId));
                }

                if (conditions.Count == 0)
                {
                    _logger.LogWarning("[AutomationHelper] No valid element identifier provided");
                    return null;
                }

                var condition = conditions.Count == 1 ? conditions[0] : new OrCondition(conditions.ToArray());
                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to find element by ID");
                return null;
            }
        }

        /// <summary>
        /// ウィンドウタイトルでウィンドウを検索します
        /// </summary>
        private AutomationElement? FindWindowByTitle(string windowTitle)
        {
            try
            {
                var condition = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                    new PropertyCondition(AutomationElement.NameProperty, windowTitle));

                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to find window by title: {WindowTitle}", windowTitle);
                return null;
            }
        }

        /// <summary>
        /// プロセスIDでウィンドウを検索します
        /// </summary>
        private AutomationElement? FindWindowByProcessId(int processId)
        {
            try
            {
                var condition = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                    new PropertyCondition(AutomationElement.ProcessIdProperty, processId));

                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to find window by ProcessId: {ProcessId}", processId);
                return null;
            }
        }

        /// <summary>
        /// 文字列からControlTypeを解析します
        /// </summary>
        private ControlType? ParseControlType(string controlTypeStr)
        {
            try
            {
                return controlTypeStr.ToLowerInvariant() switch
                {
                    "window" => ControlType.Window,
                    "button" => ControlType.Button,
                    "edit" => ControlType.Edit,
                    "text" => ControlType.Text,
                    "menu" => ControlType.Menu,
                    "menuitem" => ControlType.MenuItem,
                    "list" => ControlType.List,
                    "listitem" => ControlType.ListItem,
                    "tree" => ControlType.Tree,
                    "treeitem" => ControlType.TreeItem,
                    "tab" => ControlType.Tab,
                    "tabitem" => ControlType.TabItem,
                    "checkbox" => ControlType.CheckBox,
                    "radiobutton" => ControlType.RadioButton,
                    "combobox" => ControlType.ComboBox,
                    "slider" => ControlType.Slider,
                    "progressbar" => ControlType.ProgressBar,
                    "scrollbar" => ControlType.ScrollBar,
                    "group" => ControlType.Group,
                    "pane" => ControlType.Pane,
                    "document" => ControlType.Document,
                    "image" => ControlType.Image,
                    "hyperlink" => ControlType.Hyperlink,
                    "table" => ControlType.Table,
                    "calendar" => ControlType.Calendar,
                    "datagrid" => ControlType.DataGrid,
                    "dataitem" => ControlType.DataItem,
                    "toolbar" => ControlType.ToolBar,
                    "tooltip" => ControlType.ToolTip,
                    "statusbar" => ControlType.StatusBar,
                    "spinner" => ControlType.Spinner,
                    "splitbutton" => ControlType.SplitButton,
                    "thumb" => ControlType.Thumb,
                    "titlebar" => ControlType.TitleBar,
                    "separator" => ControlType.Separator,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AutomationHelper] Failed to parse ControlType: {ControlType}", controlTypeStr);
                return null;
            }
        }

        /// <summary>
        /// 操作がWindow ControlTypeを対象としているかチェックします
        /// </summary>
        private bool IsWindowControlType(WorkerOperation operation)
        {
            return operation.Parameters.TryGetValue("ControlType", out var controlTypeValue) && 
                   controlTypeValue?.ToString()?.ToLowerInvariant() == "window";
        }
    }
}