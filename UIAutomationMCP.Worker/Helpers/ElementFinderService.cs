using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// 要素検索の共通ロジックを提供するヘルパーサービス
    /// </summary>
    public class ElementFinderService
    {
        private readonly ILogger<ElementFinderService>? _logger;

        public ElementFinderService() : this(null) { }
        
        public ElementFinderService(ILogger<ElementFinderService>? logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// AutomationIdまたはNameで要素を検索（推奨メソッド）
        /// AutomationIdが指定されている場合は優先、Nameも指定されている場合はフォールバック
        /// </summary>
        /// <param name="automationId">UI Automation要素のAutomationIdプロパティ（推奨）</param>
        /// <param name="name">UI Automation要素のNameプロパティ（フォールバック）</param>
        /// <param name="controlType">UI Automation要素のControlType（パフォーマンス向上）</param>
        /// <param name="windowTitle">検索対象ウィンドウのタイトル（省略可）</param>
        /// <param name="processId">検索対象プロセスのID（省略可）</param>
        /// <param name="scope">検索範囲（デフォルト: Descendants）</param>
        /// <param name="cacheRequest">キャッシュリクエスト（パフォーマンス最適化用、省略可）</param>
        /// <param name="timeoutMs">検索タイムアウト（ミリ秒、デフォルト: 1000ms）</param>
        /// <returns>見つかった要素、見つからない場合はnull</returns>
        public AutomationElement? FindElement(string? automationId = null, string? name = null, 
            string? controlType = null, string? windowTitle = null, int? processId = null, 
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null, int timeoutMs = 1000)
        {
            // 少なくとも一つの識別子が必要
            if (string.IsNullOrEmpty(automationId) && string.IsNullOrEmpty(name))
            {
                _logger?.LogWarning("Both AutomationId and Name are null or empty");
                return null;
            }

            // Check UI Automation availability early
            if (!UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.IsAvailable)
            {
                _logger?.LogError("UI Automation is not available: {Reason}", UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.UnavailabilityReason);
                return null;
            }

            var searchRoot = GetSearchRoot(windowTitle ?? "", processId ?? 0);
            
            // Prevent searching from RootElement without scope limitation
            if (searchRoot == null && string.IsNullOrEmpty(windowTitle) && (processId ?? 0) == 0)
            {
                _logger?.LogWarning("Search scope too broad. Consider specifying windowTitle or processId for better performance. Full desktop search may take significant time.");
                searchRoot = AutomationElement.RootElement;
            }
            
            searchRoot ??= AutomationElement.RootElement;
            
            _logger?.LogDebug("Searching for element with AutomationId: '{AutomationId}', Name: '{Name}', ControlType: '{ControlType}' in window: '{WindowTitle}' (PID: {ProcessId}), Scope: {Scope}, Timeout: {TimeoutMs}ms", 
                automationId, name, controlType, windowTitle, processId, scope, timeoutMs);
            
            return UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.ExecuteWithTimeout(() =>
            {
                AutomationElement? element = null;
                
                // ControlTypeフィルタ条件を準備
                Condition? controlTypeCondition = null;
                if (!string.IsNullOrEmpty(controlType))
                {
                    var controlTypeEnum = GetControlTypeFromString(controlType);
                    if (controlTypeEnum != null)
                    {
                        controlTypeCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeEnum);
                    }
                }
                
                // 1. AutomationIdで検索（優先）
                if (!string.IsNullOrEmpty(automationId))
                {
                    var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
                    
                    // ControlTypeフィルタと組み合わせ
                    Condition searchCondition = controlTypeCondition != null 
                        ? new AndCondition(automationIdCondition, controlTypeCondition)
                        : automationIdCondition;
                    
                    if (cacheRequest != null)
                    {
                        using (cacheRequest.Activate())
                        {
                            element = searchRoot.FindFirst(scope, searchCondition);
                        }
                    }
                    else
                    {
                        element = searchRoot.FindFirst(scope, searchCondition);
                    }
                    
                    if (element != null)
                    {
                        _logger?.LogDebug("Element found by AutomationId: '{AutomationId}' with ControlType: '{ControlType}'", automationId, controlType);
                        return element;
                    }
                    else
                    {
                        _logger?.LogDebug("Element not found by AutomationId: '{AutomationId}' with ControlType: '{ControlType}'", automationId, controlType);
                    }
                }
                
                // 2. Nameで検索（フォールバック）
                if (!string.IsNullOrEmpty(name))
                {
                    _logger?.LogDebug("Trying to find element by Name: '{Name}' with ControlType: '{ControlType}'", name, controlType);
                    var nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);
                    
                    // ControlTypeフィルタと組み合わせ
                    Condition searchCondition = controlTypeCondition != null 
                        ? new AndCondition(nameCondition, controlTypeCondition)
                        : nameCondition;
                    
                    if (cacheRequest != null)
                    {
                        using (cacheRequest.Activate())
                        {
                            element = searchRoot.FindFirst(scope, searchCondition);
                        }
                    }
                    else
                    {
                        element = searchRoot.FindFirst(scope, searchCondition);
                    }
                    
                    if (element != null)
                    {
                        _logger?.LogDebug("Element found by Name: '{Name}' with ControlType: '{ControlType}'", name, controlType);
                    }
                    else
                    {
                        _logger?.LogDebug("Element not found by Name: '{Name}' with ControlType: '{ControlType}'", name, controlType);
                    }
                }
                
                return element;
            }, $"FindElement(AutomationId: {automationId}, Name: {name})", timeoutMs / 1000);
        }

        /// <summary>
        /// 文字列からControlTypeを取得する
        /// </summary>
        /// <param name="controlTypeString">ControlType文字列（例: "Button", "TextBox", "List"）</param>
        /// <returns>対応するControlType、見つからない場合はnull</returns>
        private ControlType? GetControlTypeFromString(string controlTypeString)
        {
            if (string.IsNullOrEmpty(controlTypeString))
                return null;
                
            // Case-insensitive comparison
            return controlTypeString.ToLowerInvariant() switch
            {
                "button" => ControlType.Button,
                "calendar" => ControlType.Calendar,
                "checkbox" => ControlType.CheckBox,
                "combobox" => ControlType.ComboBox,
                "edit" => ControlType.Edit,
                "hyperlink" => ControlType.Hyperlink,
                "image" => ControlType.Image,
                "listitem" => ControlType.ListItem,
                "list" => ControlType.List,
                "menu" => ControlType.Menu,
                "menubar" => ControlType.MenuBar,
                "menuitem" => ControlType.MenuItem,
                "progressbar" => ControlType.ProgressBar,
                "radiobutton" => ControlType.RadioButton,
                "scrollbar" => ControlType.ScrollBar,
                "slider" => ControlType.Slider,
                "spinner" => ControlType.Spinner,
                "statusbar" => ControlType.StatusBar,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "text" => ControlType.Text,
                "toolbar" => ControlType.ToolBar,
                "tooltip" => ControlType.ToolTip,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "custom" => ControlType.Custom,
                "group" => ControlType.Group,
                "thumb" => ControlType.Thumb,
                "datagrid" => ControlType.DataGrid,
                "dataitem" => ControlType.DataItem,
                "document" => ControlType.Document,
                "splitbutton" => ControlType.SplitButton,
                "window" => ControlType.Window,
                "pane" => ControlType.Pane,
                "header" => ControlType.Header,
                "headeritem" => ControlType.HeaderItem,
                "table" => ControlType.Table,
                "titlebar" => ControlType.TitleBar,
                "separator" => ControlType.Separator,
                _ => null
            };
        }

        /// <summary>
        /// 非推奨: FindElementを使用してください
        /// </summary>
        [Obsolete("Use FindElement(automationId, name, windowTitle, processId) instead. This method will be removed in future versions.")]
        public AutomationElement? FindElementById(string elementId, string windowTitle = "", int processId = 0, 
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null, int timeoutMs = 1000)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                _logger?.LogWarning("Element ID is null or empty");
                return null;
            }

            // 後方互換性のため、elementIdをautomationIdとして扱い、Nameもフォールバックとして使用
            return FindElement(automationId: elementId, name: elementId, windowTitle: windowTitle, 
                processId: processId, scope: scope, cacheRequest: cacheRequest, timeoutMs: timeoutMs);
        }

        /// <summary>
        /// AutomationIdで要素を検索（単一プロパティ検索）
        /// </summary>
        /// <param name="automationId">UI Automation要素のAutomationId</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <param name="scope">検索範囲</param>
        /// <param name="cacheRequest">キャッシュリクエスト</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByAutomationId(string automationId, string? windowTitle = null, int? processId = null,
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null)
        {
            if (string.IsNullOrEmpty(automationId))
            {
                _logger?.LogWarning("AutomationId is null or empty");
                return null;
            }

            return FindElement(automationId: automationId, name: null, windowTitle: windowTitle, 
                processId: processId, scope: scope, cacheRequest: cacheRequest);
        }

        /// <summary>
        /// Nameで要素を検索（単一プロパティ検索）
        /// </summary>
        /// <param name="name">UI Automation要素のName</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <param name="scope">検索範囲</param>
        /// <param name="cacheRequest">キャッシュリクエスト</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByName(string name, string? windowTitle = null, int? processId = null,
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger?.LogWarning("Name is null or empty");
                return null;
            }

            return FindElement(automationId: null, name: name, windowTitle: windowTitle, 
                processId: processId, scope: scope, cacheRequest: cacheRequest);
        }

        /// <summary>
        /// 複数の条件で要素を検索
        /// </summary>
        /// <param name="conditions">検索条件のリスト</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByConditions(Condition[] conditions, string windowTitle = "", int processId = 0,
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null)
        {
            if (conditions == null || conditions.Length == 0)
            {
                _logger?.LogWarning("No conditions provided for element search");
                return null;
            }

            var searchRoot = GetSearchRoot(windowTitle, processId);
            
            // Prevent searching from RootElement without scope limitation
            if (searchRoot == null && string.IsNullOrEmpty(windowTitle) && processId == 0)
            {
                _logger?.LogWarning("Search scope too broad. Consider specifying windowTitle or processId for better performance. Full desktop search may take significant time.");
                searchRoot = AutomationElement.RootElement;
            }
            
            searchRoot ??= AutomationElement.RootElement;
            
            var combinedCondition = conditions.Length == 1 ? conditions[0] : new AndCondition(conditions);
            
            _logger?.LogDebug("Searching for element with {ConditionCount} conditions in window: {WindowTitle} (PID: {ProcessId}), Scope: {Scope}", 
                conditions.Length, windowTitle, processId, scope);
            
            if (cacheRequest != null)
            {
                using (cacheRequest.Activate())
                {
                    return searchRoot.FindFirst(scope, combinedCondition);
                }
            }
            
            return searchRoot.FindFirst(scope, combinedCondition);
        }

        /// <summary>
        /// 複数の要素を検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素のコレクション</returns>
        public AutomationElementCollection FindElements(Condition condition, string windowTitle = "", int processId = 0,
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId);
            
            // Prevent searching from RootElement without scope limitation
            if (searchRoot == null && string.IsNullOrEmpty(windowTitle) && processId == 0)
            {
                _logger?.LogWarning("Search scope too broad. Consider specifying windowTitle or processId for better performance. Full desktop search may take significant time.");
                searchRoot = AutomationElement.RootElement;
            }
            
            searchRoot ??= AutomationElement.RootElement;
            
            _logger?.LogDebug("Searching for multiple elements in window: {WindowTitle} (PID: {ProcessId}), Scope: {Scope}", 
                windowTitle, processId, scope);
            
            if (cacheRequest != null)
            {
                using (cacheRequest.Activate())
                {
                    return searchRoot.FindAll(scope, condition);
                }
            }
            
            return searchRoot.FindAll(scope, condition);
        }

        /// <summary>
        /// 検索ルートを取得
        /// </summary>
        /// <param name="windowTitle">ウィンドウタイトル</param>
        /// <param name="processId">プロセスID</param>
        /// <returns>検索ルート要素またはnull</returns>
        public AutomationElement? GetSearchRoot(string windowTitle, int processId)
        {
            if (processId > 0)
            {
                _logger?.LogDebug("Finding search root by process ID: {ProcessId}", processId);
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                _logger?.LogDebug("Finding search root by window title: {WindowTitle}", windowTitle);
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            
            _logger?.LogDebug("Using root element as search root");
            return null;
        }

        /// <summary>
        /// AutomationIdまたはNameで要素を非同期検索（推奨メソッド）
        /// </summary>
        /// <param name="automationId">UI Automation要素のAutomationIdプロパティ（推奨）</param>
        /// <param name="name">UI Automation要素のNameプロパティ（フォールバック）</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <param name="scope">検索スコープ</param>
        /// <param name="timeoutMs">タイムアウト（ミリ秒）</param>
        /// <returns>見つかった要素またはnull</returns>
        public async Task<AutomationElement?> FindElementAsync(string? automationId = null, string? name = null, 
            string? windowTitle = null, int? processId = null, TreeScope scope = TreeScope.Descendants, int timeoutMs = 5000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                return await Task.Run(() => 
                    FindElement(automationId, name, controlType: null, windowTitle, processId, scope), cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Search for element AutomationId: '{AutomationId}', Name: '{Name}' timed out after {TimeoutMs}ms", 
                    automationId, name, timeoutMs);
                return null;
            }
        }

        /// <summary>
        /// 非推奨: FindElementAsyncを使用してください
        /// </summary>
        [Obsolete("Use FindElementAsync(automationId, name, windowTitle, processId) instead")]
        public async Task<AutomationElement?> FindElementByIdAsync(string elementId, string windowTitle = "", 
            int processId = 0, TreeScope scope = TreeScope.Descendants, int timeoutMs = 5000)
        {
            return await FindElementAsync(automationId: elementId, name: elementId, windowTitle: windowTitle, 
                processId: processId, scope: scope, timeoutMs: timeoutMs);
        }

        /// <summary>
        /// 非推奨: FindElementAsyncを使用してください
        /// </summary>
        [Obsolete("Use FindElementAsync(automationId: null, name: elementName, ...) instead")]
        public async Task<AutomationElement?> FindElementByNameAsync(string elementName, string windowTitle = "",
            int processId = 0, TreeScope scope = TreeScope.Descendants, int timeoutMs = 5000)
        {
            return await FindElementAsync(automationId: null, name: elementName, windowTitle: windowTitle, 
                processId: processId, scope: scope, timeoutMs: timeoutMs);
        }

        /// <summary>
        /// 要素が存在するかチェック
        /// </summary>
        /// <param name="automationId">UI Automation要素のAutomationId</param>
        /// <param name="name">UI Automation要素のName</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>要素が存在する場合はtrue</returns>
        public bool ElementExists(string? automationId = null, string? name = null, string? windowTitle = null, int? processId = null)
        {
            return FindElement(automationId, name, controlType: null, windowTitle, processId) != null;
        }

        /// <summary>
        /// 非推奨: ElementExistsを使用してください
        /// </summary>
        [Obsolete("Use ElementExists(automationId, name, windowTitle, processId) instead")]
        public bool ElementExists(string elementId, string windowTitle = "", int processId = 0)
        {
            return ElementExists(automationId: elementId, name: elementId, windowTitle: windowTitle, processId: processId);
        }

        /// <summary>
        /// デフォルトのCacheRequestを作成
        /// </summary>
        /// <returns>基本プロパティを含むCacheRequest</returns>
        public static CacheRequest CreateDefaultCacheRequest()
        {
            var cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.ClassNameProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.ProcessIdProperty);
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);
            cacheRequest.Add(AutomationElement.IsOffscreenProperty);
            cacheRequest.AutomationElementMode = AutomationElementMode.None;
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            return cacheRequest;
        }

        /// <summary>
        /// 高度な検索パラメータを使用して要素を検索
        /// </summary>
        /// <param name="searchParams">検索パラメータ</param>
        /// <returns>見つかった要素のコレクション</returns>
        public AutomationElementCollection FindElementsAdvanced(AdvancedSearchParameters searchParams)
        {
            var searchRoot = GetSearchRoot(searchParams.WindowTitle ?? "", searchParams.ProcessId ?? 0);
            searchRoot ??= AutomationElement.RootElement;

            // Build complex search conditions
            var conditions = BuildAdvancedSearchConditions(searchParams);
            var combinedCondition = conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray());

            // Determine TreeScope
            var treeScope = ParseTreeScope(searchParams.Scope);

            _logger?.LogDebug("Advanced search with {ConditionCount} conditions, scope: {Scope}", 
                conditions.Count, treeScope);

            return UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.ExecuteWithTimeout(() =>
            {
                if (searchParams.CacheRequest != null)
                {
                    using (searchParams.CacheRequest.Activate())
                    {
                        return searchRoot.FindAll(treeScope, combinedCondition);
                    }
                }
                else
                {
                    return searchRoot.FindAll(treeScope, combinedCondition);
                }
            }, $"FindElementsAdvanced", searchParams.TimeoutMs / 1000);
        }

        /// <summary>
        /// 高度な検索条件を構築
        /// </summary>
        private List<Condition> BuildAdvancedSearchConditions(AdvancedSearchParameters searchParams)
        {
            var conditions = new List<Condition>();

            // SearchText - Name, AutomationId, ClassName を横断検索
            if (!string.IsNullOrEmpty(searchParams.SearchText))
            {
                var searchConditions = new List<Condition>();
                
                if (searchParams.FuzzyMatch)
                {
                    // ファジーマッチング: 部分一致
                    searchConditions.Add(CreateFuzzyCondition(AutomationElement.NameProperty, searchParams.SearchText));
                    searchConditions.Add(CreateFuzzyCondition(AutomationElement.AutomationIdProperty, searchParams.SearchText));
                    searchConditions.Add(CreateFuzzyCondition(AutomationElement.ClassNameProperty, searchParams.SearchText));
                }
                else
                {
                    // 完全一致
                    searchConditions.Add(new PropertyCondition(AutomationElement.NameProperty, searchParams.SearchText));
                    searchConditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, searchParams.SearchText));
                    searchConditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, searchParams.SearchText));
                }
                
                conditions.Add(new OrCondition(searchConditions.ToArray()));
            }

            // 個別プロパティ検索
            if (!string.IsNullOrEmpty(searchParams.AutomationId))
            {
                conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, searchParams.AutomationId));
            }

            if (!string.IsNullOrEmpty(searchParams.Name))
            {
                conditions.Add(new PropertyCondition(AutomationElement.NameProperty, searchParams.Name));
            }

            if (!string.IsNullOrEmpty(searchParams.ClassName))
            {
                conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, searchParams.ClassName));
            }

            // ControlType条件
            if (!string.IsNullOrEmpty(searchParams.ControlType))
            {
                if (TryGetControlTypeByName(searchParams.ControlType, out var controlType))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
                }
            }

            // VisibleOnly条件
            if (searchParams.VisibleOnly)
            {
                conditions.Add(new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
            }

            // EnabledOnly条件
            if (searchParams.EnabledOnly)
            {
                conditions.Add(new PropertyCondition(AutomationElement.IsEnabledProperty, true));
            }

            // RequiredPatterns条件 - すべてのパターンが必要
            if (searchParams.RequiredPatterns?.Length > 0)
            {
                var patternConditions = searchParams.RequiredPatterns
                    .Select(CreatePatternCondition)
                    .Where(c => c != null)
                    .Cast<Condition>()
                    .ToList();
                
                if (patternConditions.Count > 0)
                {
                    conditions.AddRange(patternConditions);
                }
            }

            // AnyOfPatterns条件 - いずれかのパターンがあればOK
            if (searchParams.AnyOfPatterns?.Length > 0)
            {
                var patternConditions = searchParams.AnyOfPatterns
                    .Select(CreatePatternCondition)
                    .Where(c => c != null)
                    .Cast<Condition>()
                    .ToArray();
                
                if (patternConditions.Length > 0)
                {
                    conditions.Add(new OrCondition(patternConditions));
                }
            }

            // デフォルト条件
            if (conditions.Count == 0)
            {
                conditions.Add(Condition.TrueCondition);
            }

            return conditions;
        }

        /// <summary>
        /// ファジーマッチング用の条件を作成（部分一致）
        /// </summary>
        private Condition CreateFuzzyCondition(AutomationProperty property, string searchText)
        {
            // UI Automationでは完全一致のみサポートされているため、
            // ファジーマッチングは後処理で実装する必要がある
            // ここでは一旦 PropertyCondition を返し、後でフィルタリング
            return new PropertyCondition(property, searchText);
        }

        /// <summary>
        /// パターン名からパターン条件を作成
        /// </summary>
        private Condition? CreatePatternCondition(string patternName)
        {
            var patterns = new Dictionary<string, AutomationPattern>(StringComparer.OrdinalIgnoreCase)
            {
                ["Invoke"] = InvokePattern.Pattern,
                ["Value"] = ValuePattern.Pattern,
                ["Toggle"] = TogglePattern.Pattern,
                ["Selection"] = SelectionPattern.Pattern,
                ["SelectionItem"] = SelectionItemPattern.Pattern,
                ["Text"] = TextPattern.Pattern,
                ["Range"] = RangeValuePattern.Pattern,
                ["Scroll"] = ScrollPattern.Pattern,
                ["Grid"] = GridPattern.Pattern,
                ["GridItem"] = GridItemPattern.Pattern,
                ["Table"] = TablePattern.Pattern,
                ["TableItem"] = TableItemPattern.Pattern,
                ["Transform"] = TransformPattern.Pattern,
                ["Window"] = WindowPattern.Pattern,
                ["Dock"] = DockPattern.Pattern,
                ["ExpandCollapse"] = ExpandCollapsePattern.Pattern,
                ["MultipleView"] = MultipleViewPattern.Pattern
            };

            // UI Automationではパターンフィルタは実行時チェックが必要
            // 検索後に手動でフィルタする必要がある
            return patterns.TryGetValue(patternName, out var pattern)
                ? Condition.TrueCondition // 一旦全て取得し、後でパターンをチェック
                : null;
        }

        /// <summary>
        /// ControlType名からControlTypeオブジェクトを取得
        /// </summary>
        private bool TryGetControlTypeByName(string controlTypeName, out ControlType controlType)
        {
            controlType = ControlType.Custom;
            
            var controlTypes = new Dictionary<string, ControlType>(StringComparer.OrdinalIgnoreCase)
            {
                ["Button"] = ControlType.Button,
                ["Text"] = ControlType.Text,
                ["Edit"] = ControlType.Edit,
                ["ComboBox"] = ControlType.ComboBox,
                ["List"] = ControlType.List,
                ["ListBox"] = ControlType.List,
                ["CheckBox"] = ControlType.CheckBox,
                ["RadioButton"] = ControlType.RadioButton,
                ["Group"] = ControlType.Group,
                ["Window"] = ControlType.Window,
                ["Menu"] = ControlType.Menu,
                ["MenuItem"] = ControlType.MenuItem,
                ["Tab"] = ControlType.Tab,
                ["TabItem"] = ControlType.TabItem,
                ["Tree"] = ControlType.Tree,
                ["TreeItem"] = ControlType.TreeItem,
                ["Table"] = ControlType.Table,
                ["DataGrid"] = ControlType.DataGrid,
                ["Image"] = ControlType.Image,
                ["Slider"] = ControlType.Slider,
                ["ProgressBar"] = ControlType.ProgressBar,
                ["Hyperlink"] = ControlType.Hyperlink,
                ["Calendar"] = ControlType.Calendar,
                ["Document"] = ControlType.Document,
                ["Pane"] = ControlType.Pane,
                ["Separator"] = ControlType.Separator,
                ["StatusBar"] = ControlType.StatusBar,
                ["ToolBar"] = ControlType.ToolBar,
                ["ToolTip"] = ControlType.ToolTip,
                ["TitleBar"] = ControlType.TitleBar,
                ["ScrollBar"] = ControlType.ScrollBar,
                ["Spinner"] = ControlType.Spinner,
                ["SplitButton"] = ControlType.SplitButton,
                ["Header"] = ControlType.Header,
                ["HeaderItem"] = ControlType.HeaderItem,
                ["Thumb"] = ControlType.Thumb
            };

            if (controlTypes.TryGetValue(controlTypeName, out var foundType))
            {
                controlType = foundType;
                return true;
            }

            return false;
        }

        /// <summary>
        /// TreeScope文字列をTreeScope列挙型に変換
        /// </summary>
        private TreeScope ParseTreeScope(string? scope)
        {
            return scope?.ToLowerInvariant() switch
            {
                "children" => TreeScope.Children,
                "descendants" => TreeScope.Descendants,
                "subtree" => TreeScope.Subtree,
                "element" => TreeScope.Element,
                _ => TreeScope.Descendants // デフォルト
            };
        }

        /// <summary>
        /// ファジーマッチング後処理フィルタ
        /// </summary>
        public List<AutomationElement> ApplyFuzzyFilter(AutomationElementCollection elements, AdvancedSearchParameters searchParams)
        {
            var result = new List<AutomationElement>();

            foreach (AutomationElement element in elements)
            {
                if (element != null)
                {
                    try
                    {
                        if (MatchesFuzzySearch(element, searchParams))
                        {
                            result.Add(element);
                        }
                    }
                    catch (ElementNotAvailableException)
                    {
                        continue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// パターンフィルタリングを適用
        /// </summary>
        public List<AutomationElement> ApplyPatternFilter(List<AutomationElement> elements, AdvancedSearchParameters searchParams)
        {
            if ((searchParams.RequiredPatterns?.Length ?? 0) == 0 && (searchParams.AnyOfPatterns?.Length ?? 0) == 0)
                return elements;

            var result = new List<AutomationElement>();

            foreach (var element in elements)
            {
                try
                {
                    bool includeElement = true;

                    // RequiredPatterns - すべてのパターンが必要
                    if (searchParams.RequiredPatterns?.Length > 0)
                    {
                        foreach (var patternName in searchParams.RequiredPatterns)
                        {
                            if (!ElementSupportsPattern(element, patternName))
                            {
                                includeElement = false;
                                break;
                            }
                        }
                    }

                    // AnyOfPatterns - いずれかのパターンがあればOK
                    if (includeElement && searchParams.AnyOfPatterns?.Length > 0)
                    {
                        bool hasAnyPattern = false;
                        foreach (var patternName in searchParams.AnyOfPatterns)
                        {
                            if (ElementSupportsPattern(element, patternName))
                            {
                                hasAnyPattern = true;
                                break;
                            }
                        }
                        if (!hasAnyPattern)
                        {
                            includeElement = false;
                        }
                    }

                    if (includeElement)
                    {
                        result.Add(element);
                    }
                }
                catch (ElementNotAvailableException)
                {
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// 要素が指定されたパターンをサポートしているかチェック
        /// </summary>
        private bool ElementSupportsPattern(AutomationElement element, string patternName)
        {
            var patterns = new Dictionary<string, AutomationPattern>(StringComparer.OrdinalIgnoreCase)
            {
                ["Invoke"] = InvokePattern.Pattern,
                ["Value"] = ValuePattern.Pattern,
                ["Toggle"] = TogglePattern.Pattern,
                ["Selection"] = SelectionPattern.Pattern,
                ["SelectionItem"] = SelectionItemPattern.Pattern,
                ["Text"] = TextPattern.Pattern,
                ["Range"] = RangeValuePattern.Pattern,
                ["Scroll"] = ScrollPattern.Pattern,
                ["Grid"] = GridPattern.Pattern,
                ["GridItem"] = GridItemPattern.Pattern,
                ["Table"] = TablePattern.Pattern,
                ["TableItem"] = TableItemPattern.Pattern,
                ["Transform"] = TransformPattern.Pattern,
                ["Window"] = WindowPattern.Pattern,
                ["Dock"] = DockPattern.Pattern,
                ["ExpandCollapse"] = ExpandCollapsePattern.Pattern,
                ["MultipleView"] = MultipleViewPattern.Pattern
            };

            if (patterns.TryGetValue(patternName, out var pattern))
            {
                try
                {
                    return element.TryGetCurrentPattern(pattern, out _);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 要素がファジー検索条件にマッチするかチェック
        /// </summary>
        private bool MatchesFuzzySearch(AutomationElement element, AdvancedSearchParameters searchParams)
        {
            if (!searchParams.FuzzyMatch || string.IsNullOrEmpty(searchParams.SearchText))
                return true;

            var searchText = searchParams.SearchText.ToLowerInvariant();
            
            // Name, AutomationId, ClassName の部分一致チェック
            var name = element.Current.Name?.ToLowerInvariant() ?? "";
            var automationId = element.Current.AutomationId?.ToLowerInvariant() ?? "";
            var className = element.Current.ClassName?.ToLowerInvariant() ?? "";

            return name.Contains(searchText) || 
                   automationId.Contains(searchText) || 
                   className.Contains(searchText);
        }

        /// <summary>
        /// 検索結果をソート
        /// </summary>
        public List<AutomationElement> SortElements(List<AutomationElement> elements, string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy) || elements.Count <= 1)
                return elements;

            return sortBy.ToLowerInvariant() switch
            {
                "name" => elements.OrderBy(e => GetElementProperty(e, e => e.Current.Name)).ToList(),
                "controltype" => elements.OrderBy(e => GetElementProperty(e, e => e.Current.ControlType.LocalizedControlType)).ToList(),
                "position" => elements.OrderBy(e => GetElementProperty(e, e => e.Current.BoundingRectangle.Y))
                                    .ThenBy(e => GetElementProperty(e, e => e.Current.BoundingRectangle.X)).ToList(),
                "size" => elements.OrderByDescending(e => GetElementProperty(e, e => e.Current.BoundingRectangle.Width * e.Current.BoundingRectangle.Height)).ToList(),
                _ => elements
            };
        }

        /// <summary>
        /// 要素プロパティを安全に取得
        /// </summary>
        private T GetElementProperty<T>(AutomationElement element, Func<AutomationElement, T> propertySelector)
        {
            try
            {
                return propertySelector(element);
            }
            catch (ElementNotAvailableException)
            {
                return default(T)!;
            }
        }

        /// <summary>
        /// 要素の基本情報を取得
        /// </summary>
        /// <param name="element">対象要素</param>
        /// <returns>要素の基本情報</returns>
        public ElementBasicInfo GetElementBasicInfo(AutomationElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return new ElementBasicInfo
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ClassName = element.Current.ClassName,
                ControlType = element.Current.ControlType.ProgrammaticName,
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
                BoundingRectangle = element.Current.BoundingRectangle,
                ProcessId = element.Current.ProcessId
            };
        }
    }

    /// <summary>
    /// 高度な検索パラメータクラス
    /// </summary>
    public class AdvancedSearchParameters
    {
        public string? SearchText { get; set; }
        public string? AutomationId { get; set; }
        public string? Name { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int? ProcessId { get; set; }
        public string? Scope { get; set; } = "descendants";
        public string[]? RequiredPatterns { get; set; }
        public string[]? AnyOfPatterns { get; set; }
        public bool VisibleOnly { get; set; } = true;
        public bool FuzzyMatch { get; set; } = false;
        public bool EnabledOnly { get; set; } = false;
        public string? SortBy { get; set; }
        public int TimeoutMs { get; set; } = 10000;
        public CacheRequest? CacheRequest { get; set; }
    }

    /// <summary>
    /// 要素の基本情報を格納するクラス
    /// </summary>
    public class ElementBasicInfo
    {
        public string AutomationId { get; set; } = "";
        public string Name { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string ControlType { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public System.Windows.Rect BoundingRectangle { get; set; }
        public int ProcessId { get; set; }
    }
}