using System.Text.Json;

namespace UIAutomationMCP.Server.Models
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

    public class ElementInfo
    {
        public string Name { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public string HelpText { get; set; } = string.Empty;
        public string? Value { get; set; }
        public Dictionary<string, string> AvailableActions { get; set; } = new();
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
    }

    public class ProcessResult : OperationResult
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public bool HasExited { get; set; }
    }


    // 要素検索用のパラメータ（UIAutomationの依存を削除）
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
        // AutomationElementの代わりに識別情報を使用
        public string? SearchRootId { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
        public string? SearchText { get; set; }
    }

    // 要素操作の詳細な結果
    public class ElementOperationResult : OperationResult
    {
        public ElementInfo? Element { get; set; }
        public List<ElementInfo>? Elements { get; set; }
    }

    // プロパティ取得結果
    public class ElementPropertiesResult : OperationResult
    {
        public Dictionary<string, object>? Properties { get; set; }
    }

    // パターン情報
    public class PatternInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSupported { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    // 高度な操作パラメータ
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
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}