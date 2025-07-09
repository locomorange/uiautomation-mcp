using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Helpers
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
        /// 操作の検索ルート要素を取得します（簡単なパラメータ版）
        /// </summary>
        public AutomationElement? GetSearchRoot(string? windowTitle = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("[AutomationHelper] Getting search root for windowTitle: {WindowTitle}, processId: {ProcessId}", windowTitle, processId);

                // ウィンドウタイトルが指定されている場合
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    _logger.LogInformation("[AutomationHelper] Searching for window with title: {WindowTitle}", windowTitle);
                    return FindWindowByTitle(windowTitle);
                }

                // プロセスIDが指定されている場合
                if (processId.HasValue && processId.Value > 0)
                {
                    _logger.LogInformation("[AutomationHelper] Searching for window with ProcessId: {ProcessId}", processId.Value);
                    return FindWindowByProcessId(processId.Value);
                }

                // デフォルトはルート要素
                _logger.LogInformation("[AutomationHelper] Using root element as search root");
                return AutomationElement.RootElement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Error getting search root");
                return AutomationElement.RootElement;
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
        /// ControlType文字列をパースします
        /// </summary>
        public bool TryParseControlType(string controlType, out ControlType parsedControlType)
        {
            try
            {
                // Try direct mapping first
                parsedControlType = controlType.ToLowerInvariant() switch
                {
                    "button" => ControlType.Button,
                    "text" => ControlType.Text,
                    "edit" => ControlType.Edit,
                    "window" => ControlType.Window,
                    "pane" => ControlType.Pane,
                    "checkbox" => ControlType.CheckBox,
                    "radiobutton" => ControlType.RadioButton,
                    "combobox" => ControlType.ComboBox,
                    "listbox" => ControlType.List,
                    "listitem" => ControlType.ListItem,
                    "tree" => ControlType.Tree,
                    "treeitem" => ControlType.TreeItem,
                    "tab" => ControlType.Tab,
                    "tabitem" => ControlType.TabItem,
                    "slider" => ControlType.Slider,
                    "progressbar" => ControlType.ProgressBar,
                    "menu" => ControlType.Menu,
                    "menuitem" => ControlType.MenuItem,
                    "toolbar" => ControlType.ToolBar,
                    "statusbar" => ControlType.StatusBar,
                    "table" => ControlType.Table,
                    "document" => ControlType.Document,
                    "image" => ControlType.Image,
                    "hyperlink" => ControlType.Hyperlink,
                    _ => throw new ArgumentException($"Unknown control type: {controlType}")
                };
                return true;
            }
            catch
            {
                parsedControlType = ControlType.Pane;
                return false;
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
        /// プロセスIDでウィンドウを検索します（診断情報付き）
        /// </summary>
        private AutomationElement? FindWindowByProcessId(int processId)
        {
            try
            {
                _logger.LogInformation("[AutomationHelper] Searching for window with ProcessId: {ProcessId}", processId);

                // まずプロセスが存在するかチェック
                var process = System.Diagnostics.Process.GetProcesses()
                    .FirstOrDefault(p => p.Id == processId);
                
                if (process == null)
                {
                    _logger.LogWarning("[AutomationHelper] Process with ID {ProcessId} not found", processId);
                    return null;
                }

                _logger.LogInformation("[AutomationHelper] Process found: {ProcessName} (ID: {ProcessId})", 
                    process.ProcessName, processId);

                // 複数の検索戦略を試行
                
                // 戦略1: 標準的なWindow検索
                var windowCondition = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                    new PropertyCondition(AutomationElement.ProcessIdProperty, processId));

                var window = AutomationElement.RootElement.FindFirst(TreeScope.Children, windowCondition);
                
                if (window != null)
                {
                    _logger.LogInformation("[AutomationHelper] Window found using standard search for ProcessId: {ProcessId}", processId);
                    return window;
                }

                // 戦略2: より深い階層での検索（子要素まで）
                _logger.LogInformation("[AutomationHelper] Standard search failed, trying deeper search for ProcessId: {ProcessId}", processId);
                
                window = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, windowCondition);
                
                if (window != null)
                {
                    _logger.LogInformation("[AutomationHelper] Window found using deep search for ProcessId: {ProcessId}", processId);
                    return window;
                }

                // 戦略3: プロセスIDのみで検索（ControlTypeを問わない）
                _logger.LogInformation("[AutomationHelper] Deep search failed, trying ProcessId-only search for ProcessId: {ProcessId}", processId);
                
                var processCondition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                var anyElement = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, processCondition);
                
                if (anyElement != null)
                {
                    _logger.LogInformation("[AutomationHelper] Element found with ProcessId: {ProcessId}, ControlType: {ControlType}", 
                        processId, anyElement.Current.ControlType.ProgrammaticName);
                    
                    // そのプロセスのWindow要素を探す
                    var processWindow = anyElement.FindFirst(TreeScope.Ancestors, 
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                    
                    if (processWindow != null)
                    {
                        _logger.LogInformation("[AutomationHelper] Parent window found for ProcessId: {ProcessId}", processId);
                        return processWindow;
                    }
                    
                    // 祖先にWindowが見つからない場合、そのまま返す
                    _logger.LogInformation("[AutomationHelper] Using non-window element for ProcessId: {ProcessId}", processId);
                    return anyElement;
                }

                // 戦略4: 利用可能なすべてのウィンドウを列挙して診断
                _logger.LogWarning("[AutomationHelper] All search strategies failed, enumerating all windows for diagnosis");
                
                var allWindowsCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
                var allWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, allWindowsCondition);
                
                _logger.LogInformation("[AutomationHelper] Found {Count} total windows", allWindows.Count);
                
                var processIds = new List<int>();
                foreach (AutomationElement w in allWindows)
                {
                    try
                    {
                        var pid = w.Current.ProcessId;
                        var title = w.Current.Name;
                        processIds.Add(pid);
                        
                        if (pid == processId)
                        {
                            _logger.LogInformation("[AutomationHelper] Found matching window: '{Title}' (ProcessId: {ProcessId})", title, pid);
                            return w;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[AutomationHelper] Failed to get properties for a window");
                    }
                }
                
                _logger.LogWarning("[AutomationHelper] ProcessId {ProcessId} not found. Available ProcessIds: [{ProcessIds}]", 
                    processId, string.Join(", ", processIds.Distinct().OrderBy(x => x)));

                return null;
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
    }
}
