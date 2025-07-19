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
        /// 要素IDで要素を検索（AutomationId優先、Nameをフォールバックとする段階的検索）
        /// 
        /// 検索順序:
        /// 1. AutomationIdプロパティで検索（推奨・安定した識別子）
        /// 2. 見つからない場合、Nameプロパティで検索（フォールバック）
        /// 
        /// MSベストプラクティスに従い、OR条件ではなく段階的検索により
        /// より確実で予測可能な要素特定を実現
        /// </summary>
        /// <param name="elementId">検索する要素の識別子（AutomationIdまたはName）</param>
        /// <param name="windowTitle">検索対象ウィンドウのタイトル（省略可、指定すると検索範囲を限定）</param>
        /// <param name="processId">検索対象プロセスのID（省略可、指定すると検索範囲を限定）</param>
        /// <param name="scope">検索範囲（デフォルト: Descendants）</param>
        /// <param name="cacheRequest">キャッシュリクエスト（パフォーマンス最適化用、省略可）</param>
        /// <param name="timeoutMs">検索タイムアウト（ミリ秒、デフォルト: 1000ms）</param>
        /// <returns>見つかった要素、見つからない場合はnull</returns>
        public AutomationElement? FindElementById(string elementId, string windowTitle = "", int processId = 0, 
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null, int timeoutMs = 1000)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                _logger?.LogWarning("Element ID is null or empty");
                return null;
            }

            // Check UI Automation availability early
            if (!UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.IsAvailable)
            {
                _logger?.LogError("UI Automation is not available: {Reason}", UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.UnavailabilityReason);
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
            
            // AutomationId優先の段階的検索：AutomationId → Name（フォールバック）
            // より確実で予測可能な要素特定を実現
            PropertyCondition condition;
            string searchType;
            
            // 1. AutomationIdで検索（推奨・優先）
            condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
            searchType = "AutomationId";
            
            _logger?.LogDebug("Searching for element with ID: {ElementId} using {SearchType} in window: {WindowTitle} (PID: {ProcessId}), Scope: {Scope}, Timeout: {TimeoutMs}ms", 
                elementId, searchType, windowTitle, processId, scope, timeoutMs);
            
            return UIAutomationMCP.Worker.Helpers.UIAutomationEnvironment.ExecuteWithTimeout(() =>
            {
                AutomationElement? element = null;
                
                // 1. AutomationIdで検索
                if (cacheRequest != null)
                {
                    using (cacheRequest.Activate())
                    {
                        element = searchRoot.FindFirst(scope, condition);
                    }
                }
                else
                {
                    element = searchRoot.FindFirst(scope, condition);
                }
                
                // 2. AutomationIdで見つからない場合、Nameで検索（フォールバック）
                if (element == null)
                {
                    _logger?.LogDebug("Element not found by AutomationId, trying Name property for: {ElementId}", elementId);
                    var nameCondition = new PropertyCondition(AutomationElement.NameProperty, elementId);
                    
                    if (cacheRequest != null)
                    {
                        using (cacheRequest.Activate())
                        {
                            element = searchRoot.FindFirst(scope, nameCondition);
                        }
                    }
                    else
                    {
                        element = searchRoot.FindFirst(scope, nameCondition);
                    }
                    
                    if (element != null)
                    {
                        _logger?.LogDebug("Element found by Name property: {ElementId}", elementId);
                    }
                }
                else
                {
                    _logger?.LogDebug("Element found by AutomationId: {ElementId}", elementId);
                }
                
                return element;
            }, $"FindElementById({elementId})", timeoutMs / 1000);
        }

        /// <summary>
        /// 要素名で要素を検索
        /// </summary>
        /// <param name="elementName">要素名</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByName(string elementName, string windowTitle = "", int processId = 0,
            TreeScope scope = TreeScope.Descendants, CacheRequest? cacheRequest = null)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                _logger?.LogWarning("Element name is null or empty");
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
            
            var condition = new PropertyCondition(AutomationElement.NameProperty, elementName);
            
            _logger?.LogDebug("Searching for element with name: {ElementName} in window: {WindowTitle} (PID: {ProcessId}), Scope: {Scope}", 
                elementName, windowTitle, processId, scope);
            
            if (cacheRequest != null)
            {
                using (cacheRequest.Activate())
                {
                    return searchRoot.FindFirst(scope, condition);
                }
            }
            
            return searchRoot.FindFirst(scope, condition);
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
        /// 要素IDで要素を非同期検索（タイムアウト付き）
        /// </summary>
        /// <param name="elementId">要素ID</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <param name="scope">検索スコープ</param>
        /// <param name="timeoutMs">タイムアウト（ミリ秒）</param>
        /// <returns>見つかった要素またはnull</returns>
        public async Task<AutomationElement?> FindElementByIdAsync(string elementId, string windowTitle = "", 
            int processId = 0, TreeScope scope = TreeScope.Descendants, int timeoutMs = 5000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                return await Task.Run(() => 
                    FindElementById(elementId, windowTitle, processId, scope), cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Search for element ID '{ElementId}' timed out after {TimeoutMs}ms", 
                    elementId, timeoutMs);
                return null;
            }
        }

        /// <summary>
        /// 要素名で要素を非同期検索（タイムアウト付き）
        /// </summary>
        /// <param name="elementName">要素名</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <param name="scope">検索スコープ</param>
        /// <param name="timeoutMs">タイムアウト（ミリ秒）</param>
        /// <returns>見つかった要素またはnull</returns>
        public async Task<AutomationElement?> FindElementByNameAsync(string elementName, string windowTitle = "",
            int processId = 0, TreeScope scope = TreeScope.Descendants, int timeoutMs = 5000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                return await Task.Run(() => 
                    FindElementByName(elementName, windowTitle, processId, scope), cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Search for element name '{ElementName}' timed out after {TimeoutMs}ms", 
                    elementName, timeoutMs);
                return null;
            }
        }

        /// <summary>
        /// 要素が存在するかチェック
        /// </summary>
        /// <param name="elementId">要素ID</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>要素が存在する場合はtrue</returns>
        public bool ElementExists(string elementId, string windowTitle = "", int processId = 0)
        {
            return FindElementById(elementId, windowTitle, processId) != null;
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