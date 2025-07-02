using System.Text.Json;

namespace UiAutomationMcpServer.Models
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
        public string AutomationId { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string ClassName { get; set; } = string.Empty;
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
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public string HelpText { get; set; } = string.Empty;
        public string? Value { get; set; }
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
}