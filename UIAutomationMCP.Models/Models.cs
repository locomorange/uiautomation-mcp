using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models
{

    [MessagePackObject]
    public class WindowInfo
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;
        
        [Key(1)]
        public string Title { get; set; } = string.Empty;
        
        [Key(2)]
        public string AutomationId { get; set; } = string.Empty;
        
        [Key(3)]
        public int ProcessId { get; set; }
        
        [Key(4)]
        public string ProcessName { get; set; } = string.Empty;
        
        [Key(5)]
        public string ClassName { get; set; } = string.Empty;
        
        [Key(6)]
        public long Handle { get; set; } // Changed from IntPtr to long for JSON compatibility
        
        [Key(7)]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [Key(8)]
        public bool IsEnabled { get; set; }
        
        [Key(9)]
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// 軽量なUI要素基本情報クラス
    /// 検索結果で使用される最低限の識別情報とオプショナルな詳細情報を提供
    /// </summary>
    [MessagePackObject]
    public class ElementInfo
    {
        // === 基本プロパティ ===
        
        /// <summary>
        /// 要素の表示名
        /// </summary>
        [Key(0)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// UI Automation要素の一意識別子
        /// </summary>
        [Key(1)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;
        
        /// <summary>
        /// コントロールタイプ（英語）
        /// </summary>
        [Key(2)]
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;
        
        /// <summary>
        /// ローカライズされたコントロールタイプ
        /// </summary>
        [Key(3)]
        [JsonPropertyName("localizedControlType")]
        public string? LocalizedControlType { get; set; }
        
        /// <summary>
        /// クラス名
        /// </summary>
        [Key(4)]
        [JsonPropertyName("className")]
        public string ClassName { get; set; } = string.Empty;
        
        /// <summary>
        /// プロセスID
        /// </summary>
        [Key(5)]
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        
        /// <summary>
        /// 要素の境界矩形
        /// </summary>
        [Key(6)]
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        /// <summary>
        /// 要素が有効かどうか
        /// </summary>
        [Key(7)]
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// 要素が可視かどうか
        /// </summary>
        [Key(8)]
        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// 要素が画面外にあるかどうか
        /// </summary>
        [Key(9)]
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }
        
        
        /// <summary>
        /// サポートされているUI Automationパターンのリスト
        /// </summary>
        [Key(10)]
        [JsonPropertyName("supportedPatterns")]
        public string[] SupportedPatterns { get; set; } = [];
        
        /// <summary>
        /// ネイティブウィンドウハンドル（階層で最も近くで見つかったHWND）
        /// </summary>
        [Key(11)]
        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }

        /// <summary>
        /// ルートウィンドウハンドル（RootElementの直下レベルのHWND、検索ルート用）
        /// </summary>
        [Key(12)]
        [JsonPropertyName("rootWindowHandle")]
        public long? RootWindowHandle { get; set; }
        
        // === オプショナル詳細情報 ===
        
        /// <summary>
        /// 詳細なパターン情報とアクセシビリティ情報（includeDetails=trueの場合のみ）
        /// </summary>
        [Key(13)]
        [JsonPropertyName("details")]
        public ElementDetails? Details { get; set; }
        
    }
    
    /// <summary>
    /// UI要素の詳細な情報（パターン、アクセシビリティ、階層情報をすべて含む）
    /// includeDetails=trueの場合にElementInfo.Detailsに含まれる
    /// </summary>
    [MessagePackObject]
    public class ElementDetails
    {
        // === 基本詳細情報 ===
        
        /// <summary>
        /// ヘルプテキスト
        /// </summary>
        [Key(0)]
        [JsonPropertyName("helpText")]
        public string? HelpText { get; set; }
        
        /// <summary>
        /// 要素の値
        /// </summary>
        [Key(1)]
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        /// <summary>
        /// キーボードフォーカスを持っているかどうか
        /// </summary>
        [Key(2)]
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }
        
        /// <summary>
        /// キーボードフォーカス可能かどうか
        /// </summary>
        [Key(3)]
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }
        
        /// <summary>
        /// パスワードフィールドかどうか
        /// </summary>
        [Key(4)]
        [JsonPropertyName("isPassword")]
        public bool IsPassword { get; set; }
        
        /// <summary>
        /// フレームワークID（Win32、XAML等）
        /// </summary>
        [Key(5)]
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }
        
        /// <summary>
        /// RuntimeId（UI Automation内部識別子）- デバッグ・診断用
        /// </summary>
        [Key(6)]
        [JsonPropertyName("runtimeId")]
        public string? RuntimeId { get; set; }
        
        // === 型安全なパターン情報プロパティ ===
        
        [Key(7)]
        [JsonPropertyName("toggle")]
        public ToggleInfo? Toggle { get; set; }
        
        [Key(8)]
        [JsonPropertyName("range")]
        public RangeInfo? Range { get; set; }
        
        [Key(9)]
        [JsonPropertyName("window")]
        public WindowPatternInfo? Window { get; set; }
        
        [Key(10)]
        [JsonPropertyName("selection")]
        public SelectionInfo? Selection { get; set; }
        
        [Key(11)]
        [JsonPropertyName("grid")]
        public GridInfo? Grid { get; set; }
        
        [Key(12)]
        [JsonPropertyName("scroll")]
        public ScrollInfo? Scroll { get; set; }
        
        [Key(13)]
        [JsonPropertyName("text")]
        public TextInfo? Text { get; set; }
        
        [Key(14)]
        [JsonPropertyName("transform")]
        public TransformInfo? Transform { get; set; }
        
        [Key(15)]
        [JsonPropertyName("valueInfo")]
        public ValueInfo? ValueInfo { get; set; }
        
        [Key(16)]
        [JsonPropertyName("expandCollapse")]
        public ExpandCollapseInfo? ExpandCollapse { get; set; }
        
        [Key(17)]
        [JsonPropertyName("dock")]
        public DockInfo? Dock { get; set; }
        
        [Key(18)]
        [JsonPropertyName("multipleView")]
        public MultipleViewInfo? MultipleView { get; set; }
        
        [Key(19)]
        [JsonPropertyName("gridItem")]
        public GridItemInfo? GridItem { get; set; }
        
        [Key(20)]
        [JsonPropertyName("tableItem")]
        public TableItemInfo? TableItem { get; set; }
        
        [Key(21)]
        [JsonPropertyName("table")]
        public TableInfo? Table { get; set; }
        
        [Key(22)]
        [JsonPropertyName("invoke")]
        public InvokeInfo? Invoke { get; set; }
        
        [Key(23)]
        [JsonPropertyName("scrollItem")]
        public ScrollItemInfo? ScrollItem { get; set; }
        
        [Key(24)]
        [JsonPropertyName("virtualizedItem")]
        public VirtualizedItemInfo? VirtualizedItem { get; set; }
        
        [Key(25)]
        [JsonPropertyName("itemContainer")]
        public ItemContainerInfo? ItemContainer { get; set; }
        
        [Key(26)]
        [JsonPropertyName("synchronizedInput")]
        public SynchronizedInputInfo? SynchronizedInput { get; set; }
        
        [Key(27)]
        [JsonPropertyName("accessibility")]
        public AccessibilityInfo? Accessibility { get; set; }
        
        // === 階層情報（includeHierarchy=true時のみ） ===
        
        /// <summary>
        /// 親要素の基本情報
        /// </summary>
        [Key(28)]
        [JsonPropertyName("parent")]
        public ElementInfo? Parent { get; set; }
        
        /// <summary>
        /// 子要素の基本情報配列（includeChildren=trueの場合のみ）
        /// </summary>
        [Key(29)]
        [JsonPropertyName("children")]
        public ElementInfo[]? Children { get; set; }
    }

    [MessagePackObject]
    public class BoundingRectangle
    {
        [Key(0)]
        public double X { get; set; }
        
        [Key(1)]
        public double Y { get; set; }
        
        [Key(2)]
        public double Width { get; set; }
        
        [Key(3)]
        public double Height { get; set; }
    }

    [MessagePackObject]
    public class OperationResult
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public string? Error { get; set; }
        
        [Key(2)]
        public object? Data { get; set; }
        
        [Key(3)]
        public double ExecutionSeconds { get; set; }
        
        public static OperationResult FromSuccess() => new() { Success = true };
        public static OperationResult FromError(string error) => new() { Success = false, Error = error };
    }

    [MessagePackObject]
    public class OperationResult<T>
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public T? Data { get; set; }
        
        [Key(2)]
        public string? Error { get; set; }
        
        [Key(3)]
        public double ExecutionSeconds { get; set; }
        
        public static OperationResult<T> FromSuccess(T data) => new() { Success = true, Data = data };
        public static OperationResult<T> FromError(string error) => new() { Success = false, Error = error };
    }

    [MessagePackObject]
    public class ScreenshotResult : OperationResult
    {
        [Key(4)]
        public string OutputPath { get; set; } = string.Empty;
        
        [Key(5)]
        public string Base64Image { get; set; } = string.Empty;
        
        [Key(6)]
        public int Width { get; set; }
        
        [Key(7)]
        public int Height { get; set; }
        
        [Key(8)]
        public long FileSize { get; set; }
        
        [Key(9)]
        public string Timestamp { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class ProcessResult : OperationResult
    {
        [Key(4)]
        public int ProcessId { get; set; }
        
        [Key(5)]
        public string ProcessName { get; set; } = string.Empty;
        
        [Key(6)]
        public bool HasExited { get; set; }
    }



    // Detailed element operation results
    [MessagePackObject]
    public class ElementOperationResult : OperationResult
    {
        [Key(4)]
        public ElementInfo? Element { get; set; }
        
        [Key(5)]
        public List<ElementInfo>? Elements { get; set; }
    }


    // Pattern information
    [MessagePackObject]
    public class PatternInfo
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;
        
        [Key(1)]
        public bool IsSupported { get; set; }
        
        [Key(2)]
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    // Advanced operation parameters
    [MessagePackObject]
    public class AdvancedOperationParameters
    {
        [Key(0)]
        public string Operation { get; set; } = string.Empty;
        
        [Key(1)]
        public string? AutomationId { get; set; }
        
        [Key(2)]
        public string? WindowTitle { get; set; }
        
        [Key(3)]
        public int? ProcessId { get; set; }
        
        [Key(4)]
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        [Key(5)]
        public int TimeoutSeconds { get; set; } = 30;
    }


    // Worker communication models
    [MessagePackObject]
    public class WorkerRequest
    {
        [Key(0)]
        public string Operation { get; set; } = "";
        
        [Key(1)]
        public object? Parameters { get; set; }  // Direct MessagePack serializable parameters
    }

    /// <summary>
    /// Type-safe Worker response (generic version)
    /// </summary>
    [MessagePackObject]
    public class WorkerResponse<T>
    {
        [Key(0)]
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [Key(1)]
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        
        [Key(2)]
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [Key(3)]
        [JsonPropertyName("errorDetails")]
        public Results.ErrorResult? ErrorDetails { get; set; }

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
                Error = error,
                ErrorDetails = null
            };
        }
        
        /// <summary>
        /// Create error response with structured error details
        /// </summary>
        public static WorkerResponse<T> CreateError(Results.ErrorResult errorDetails)
        {
            return new WorkerResponse<T>
            {
                Success = false,
                Data = default,
                Error = errorDetails.Error,
                ErrorDetails = errorDetails
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
        
        /// <summary>
        /// Alias for object type WorkerResponse with structured error details
        /// </summary>
        public static WorkerResponse<object> CreateError(Results.ErrorResult errorDetails) => WorkerResponse<object>.CreateError(errorDetails);
    }
}