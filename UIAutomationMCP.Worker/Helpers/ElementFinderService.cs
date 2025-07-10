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

        public ElementFinderService(ILogger<ElementFinderService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// 要素IDで要素を検索
        /// </summary>
        /// <param name="elementId">要素ID</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementById(string elementId, string windowTitle = "", int processId = 0)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                _logger?.LogWarning("Element ID is null or empty");
                return null;
            }

            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
            
            _logger?.LogDebug("Searching for element with ID: {ElementId} in window: {WindowTitle} (PID: {ProcessId})", 
                elementId, windowTitle, processId);
            
            return searchRoot.FindFirst(TreeScope.Descendants, condition);
        }

        /// <summary>
        /// 要素名で要素を検索
        /// </summary>
        /// <param name="elementName">要素名</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByName(string elementName, string windowTitle = "", int processId = 0)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                _logger?.LogWarning("Element name is null or empty");
                return null;
            }

            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.NameProperty, elementName);
            
            _logger?.LogDebug("Searching for element with name: {ElementName} in window: {WindowTitle} (PID: {ProcessId})", 
                elementName, windowTitle, processId);
            
            return searchRoot.FindFirst(TreeScope.Descendants, condition);
        }

        /// <summary>
        /// 複数の条件で要素を検索
        /// </summary>
        /// <param name="conditions">検索条件のリスト</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素またはnull</returns>
        public AutomationElement? FindElementByConditions(Condition[] conditions, string windowTitle = "", int processId = 0)
        {
            if (conditions == null || conditions.Length == 0)
            {
                _logger?.LogWarning("No conditions provided for element search");
                return null;
            }

            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var combinedCondition = conditions.Length == 1 ? conditions[0] : new AndCondition(conditions);
            
            _logger?.LogDebug("Searching for element with {ConditionCount} conditions in window: {WindowTitle} (PID: {ProcessId})", 
                conditions.Length, windowTitle, processId);
            
            return searchRoot.FindFirst(TreeScope.Descendants, combinedCondition);
        }

        /// <summary>
        /// 複数の要素を検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="windowTitle">ウィンドウタイトル（省略可）</param>
        /// <param name="processId">プロセスID（省略可）</param>
        /// <returns>見つかった要素のコレクション</returns>
        public AutomationElementCollection FindElements(Condition condition, string windowTitle = "", int processId = 0)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            
            _logger?.LogDebug("Searching for multiple elements in window: {WindowTitle} (PID: {ProcessId})", 
                windowTitle, processId);
            
            return searchRoot.FindAll(TreeScope.Descendants, condition);
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