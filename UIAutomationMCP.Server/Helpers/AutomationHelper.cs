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
                    var processResult = FindWindowByProcessId(processId);
                    if (processResult != null)
                    {
                        _logger.LogInformation("[AutomationHelper] Successfully found window by ProcessId: {ProcessId}", processId);
                        return processResult;
                    }
                    
                    // プロセスIDが指定されているが見つからない場合は、より詳細な診断を実行
                    _logger.LogError("[AutomationHelper] Failed to find window for ProcessId: {ProcessId}. This will cause GetElementInfo to fail.", processId);
                    DiagnoseProcessId(processId);
                    
                    // フォールバック：プロセスIDが指定されている場合でも、デスクトップルートを返すかどうか
                    // 厳密にはエラーにすべきだが、一部の操作では動作する可能性がある
                    _logger.LogWarning("[AutomationHelper] Falling back to desktop root despite ProcessId specification. This may lead to incorrect results.");
                    return AutomationElement.RootElement;
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

        /// <summary>
        /// 操作がWindow ControlTypeを対象としているかチェックします
        /// </summary>
        private bool IsWindowControlType(WorkerOperation operation)
        {
            return operation.Parameters.TryGetValue("ControlType", out var controlTypeValue) && 
                   controlTypeValue?.ToString()?.ToLowerInvariant() == "window";
        }

        /// <summary>
        /// 指定されたプロセスIDの詳細診断を実行
        /// </summary>
        public void DiagnoseProcessId(int processId)
        {
            try
            {
                _logger.LogInformation("[AutomationHelper] Starting detailed diagnosis for ProcessId: {ProcessId}", processId);

                // プロセス情報を取得
                var process = System.Diagnostics.Process.GetProcesses()
                    .FirstOrDefault(p => p.Id == processId);

                if (process == null)
                {
                    _logger.LogError("[AutomationHelper] Process {ProcessId} does not exist", processId);
                    return;
                }

                _logger.LogInformation("[AutomationHelper] Process details - Name: {ProcessName}, MainWindowTitle: '{MainWindowTitle}', HasExited: {HasExited}", 
                    process.ProcessName, process.MainWindowTitle, process.HasExited);

                if (process.HasExited)
                {
                    _logger.LogError("[AutomationHelper] Process {ProcessId} has already exited", processId);
                    return;
                }

                // メインウィンドウハンドルをチェック
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    _logger.LogInformation("[AutomationHelper] Process has MainWindowHandle: {MainWindowHandle}", process.MainWindowHandle);
                }
                else
                {
                    _logger.LogWarning("[AutomationHelper] Process {ProcessId} has no MainWindowHandle (may be a background process or system service)", processId);
                }

                // そのプロセスのすべてのUI要素を検索
                var processCondition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                var allElements = AutomationElement.RootElement.FindAll(TreeScope.Descendants, processCondition);
                
                _logger.LogInformation("[AutomationHelper] Found {Count} UI elements for ProcessId {ProcessId}", allElements.Count, processId);

                if (allElements.Count > 0)
                {
                    var controlTypes = new Dictionary<string, int>();
                    foreach (AutomationElement element in allElements)
                    {
                        try
                        {
                            var controlType = element.Current.ControlType.ProgrammaticName;
                            controlTypes[controlType] = controlTypes.GetValueOrDefault(controlType, 0) + 1;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "[AutomationHelper] Failed to get ControlType for an element");
                        }
                    }

                    _logger.LogInformation("[AutomationHelper] ControlType distribution for ProcessId {ProcessId}: {ControlTypes}", 
                        processId, string.Join(", ", controlTypes.Select(kvp => $"{kvp.Key}({kvp.Value})")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AutomationHelper] Failed to diagnose ProcessId: {ProcessId}", processId);
            }
        }
    }
}