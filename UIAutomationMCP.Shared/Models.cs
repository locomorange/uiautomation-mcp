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
        public Dictionary<string, object> Properties { get; set; } = new();
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
