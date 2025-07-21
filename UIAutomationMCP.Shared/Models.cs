using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared
{
    public class McpRequest
    {
        public string? Jsonrpc { get; set; }
        public JsonElement? Id { get; set; }
        public string? Method { get; set; }
        public Dictionary<string, object>? Params { get; set; }
    }

    public class McpResponse
    {
        public string Jsonrpc { get; set; } = "2.0";
        public JsonElement? Id { get; set; }
        public object? Result { get; set; }
        public McpError? Error { get; set; }
    }

    public class McpError
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class ToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object InputSchema { get; set; } = new object();
    }

    public class WindowInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public long Handle { get; set; } // Changed from IntPtr to long for JSON compatibility
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// 軽量なUI要素基本情報クラス
    /// 検索結果で使用される最低限の識別情報とオプショナルな詳細情報を提供
    /// </summary>
    public class ElementInfo
    {
        // === 基本プロパティ ===
        
        /// <summary>
        /// 要素の表示名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// UI Automation要素の一意識別子
        /// </summary>
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;
        
        /// <summary>
        /// コントロールタイプ（英語）
        /// </summary>
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;
        
        /// <summary>
        /// ローカライズされたコントロールタイプ
        /// </summary>
        [JsonPropertyName("localizedControlType")]
        public string? LocalizedControlType { get; set; }
        
        /// <summary>
        /// クラス名
        /// </summary>
        [JsonPropertyName("className")]
        public string ClassName { get; set; } = string.Empty;
        
        /// <summary>
        /// プロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        /// <summary>
        /// 要素の境界矩形
        /// </summary>
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        /// <summary>
        /// 要素が有効かどうか
        /// </summary>
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// 要素が可視かどうか
        /// </summary>
        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// 要素が画面外にあるかどうか
        /// </summary>
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }
        
        /// <summary>
        /// フレームワークID（Win32、XAML等）
        /// </summary>
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }
        
        /// <summary>
        /// サポートされているUI Automationパターンのリスト
        /// </summary>
        [JsonPropertyName("supportedPatterns")]
        public string[] SupportedPatterns { get; set; } = [];
        
        // === オプショナル詳細情報 ===
        
        /// <summary>
        /// 詳細なパターン情報とアクセシビリティ情報（includeDetails=trueの場合のみ）
        /// </summary>
        [JsonPropertyName("details")]
        public ElementDetails? Details { get; set; }
        
    }
    
    /// <summary>
    /// UI要素の詳細な情報（パターン、アクセシビリティ、階層情報をすべて含む）
    /// includeDetails=trueの場合にElementInfo.Detailsに含まれる
    /// </summary>
    public class ElementDetails
    {
        // === 基本詳細情報 ===
        
        /// <summary>
        /// ヘルプテキスト
        /// </summary>
        [JsonPropertyName("helpText")]
        public string HelpText { get; set; } = string.Empty;
        
        /// <summary>
        /// 要素の値
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        /// <summary>
        /// キーボードフォーカスを持っているかどうか
        /// </summary>
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }
        
        /// <summary>
        /// キーボードフォーカス可能かどうか
        /// </summary>
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }
        
        /// <summary>
        /// パスワードフィールドかどうか
        /// </summary>
        [JsonPropertyName("isPassword")]
        public bool IsPassword { get; set; }
        
        // === 型安全なパターン情報プロパティ ===
        
        [JsonPropertyName("toggle")]
        public ToggleInfo? Toggle { get; set; }
        
        [JsonPropertyName("range")]
        public RangeInfo? Range { get; set; }
        
        [JsonPropertyName("window")]
        public WindowPatternInfo? Window { get; set; }
        
        [JsonPropertyName("selection")]
        public SelectionInfo? Selection { get; set; }
        
        [JsonPropertyName("grid")]
        public GridInfo? Grid { get; set; }
        
        [JsonPropertyName("scroll")]
        public ScrollInfo? Scroll { get; set; }
        
        [JsonPropertyName("text")]
        public TextInfo? Text { get; set; }
        
        [JsonPropertyName("transform")]
        public TransformInfo? Transform { get; set; }
        
        [JsonPropertyName("valueInfo")]
        public ValueInfo? ValueInfo { get; set; }
        
        [JsonPropertyName("expandCollapse")]
        public ExpandCollapseInfo? ExpandCollapse { get; set; }
        
        [JsonPropertyName("dock")]
        public DockInfo? Dock { get; set; }
        
        [JsonPropertyName("multipleView")]
        public MultipleViewInfo? MultipleView { get; set; }
        
        [JsonPropertyName("gridItem")]
        public GridItemInfo? GridItem { get; set; }
        
        [JsonPropertyName("tableItem")]
        public TableItemInfo? TableItem { get; set; }
        
        [JsonPropertyName("table")]
        public TableInfo? Table { get; set; }
        
        [JsonPropertyName("invoke")]
        public InvokeInfo? Invoke { get; set; }
        
        [JsonPropertyName("scrollItem")]
        public ScrollItemInfo? ScrollItem { get; set; }
        
        [JsonPropertyName("virtualizedItem")]
        public VirtualizedItemInfo? VirtualizedItem { get; set; }
        
        [JsonPropertyName("itemContainer")]
        public ItemContainerInfo? ItemContainer { get; set; }
        
        [JsonPropertyName("synchronizedInput")]
        public SynchronizedInputInfo? SynchronizedInput { get; set; }
        
        [JsonPropertyName("accessibility")]
        public AccessibilityInfo? Accessibility { get; set; }
        
        // === 階層情報（includeHierarchy=true時のみ） ===
        
        /// <summary>
        /// 親要素の基本情報
        /// </summary>
        [JsonPropertyName("parent")]
        public ElementInfo? Parent { get; set; }
        
        /// <summary>
        /// 子要素の基本情報配列（includeChildren=trueの場合のみ）
        /// </summary>
        [JsonPropertyName("children")]
        public ElementInfo[]? Children { get; set; }
    }

    public class BoundingRectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public object? Data { get; set; }
        public double ExecutionSeconds { get; set; }
    }

    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public double ExecutionSeconds { get; set; }
    }

    public class ScreenshotResult : OperationResult
    {
        public string OutputPath { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSize { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }

    public class ProcessResult : OperationResult
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public bool HasExited { get; set; }
    }


    // Element search parameters (removes UIAutomation dependency)
    public class ElementSearchParameters
    {
        public string? ElementId { get; set; }
        public string? AutomationId { get; set; }
        public string? WindowTitle { get; set; }
        public int? ProcessId { get; set; }
        public string? ControlType { get; set; }
        public string? TreeScope { get; set; } = "descendants";
        public string? Scope { get; set; } = "descendants";
        public int TimeoutSeconds { get; set; } = 30;
        // Use identification information instead of AutomationElement
        public string? SearchRootId { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
        public string? SearchText { get; set; }
    }

    // Detailed element operation results
    public class ElementOperationResult : OperationResult
    {
        public ElementInfo? Element { get; set; }
        public List<ElementInfo>? Elements { get; set; }
    }


    // Pattern information
    public class PatternInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSupported { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    // Advanced operation parameters
    public class AdvancedOperationParameters
    {
        public string Operation { get; set; } = string.Empty;
        public string? ElementId { get; set; }
        public string? WindowTitle { get; set; }
        public int? ProcessId { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int TimeoutSeconds { get; set; } = 30;
    }

    // Element tree node for hierarchical UI structure representation
    public class ElementTreeNode
    {
        public string AutomationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public List<ElementTreeNode> Children { get; set; } = new();
    }

    // Worker communication models
    public class WorkerRequest
    {
        public string Operation { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
        public string? ParametersJson { get; set; }  // Raw JSON string for typed requests
    }

    /// <summary>
    /// Type-safe Worker response (generic version)
    /// </summary>
    public class WorkerResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// Create success response
        /// </summary>
        public static WorkerResponse<T> CreateSuccess(T data)
        {
            return new WorkerResponse<T>
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        /// <summary>
        /// Create error response
        /// </summary>
        public static WorkerResponse<T> CreateError(string error)
        {
            return new WorkerResponse<T>
            {
                Success = false,
                Data = default,
                Error = error
            };
        }
    }

    /// <summary>
    /// Aliases for commonly used types
    /// </summary>
    public static class WorkerResponseAliases
    {
        /// <summary>
        /// Alias for object type WorkerResponse
        /// </summary>
        public static WorkerResponse<object> CreateSuccess(object data) => WorkerResponse<object>.CreateSuccess(data);
        
        /// <summary>
        /// Alias for object type WorkerResponse
        /// </summary>
        public static WorkerResponse<object> CreateError(string error) => WorkerResponse<object>.CreateError(error);
    }
}
