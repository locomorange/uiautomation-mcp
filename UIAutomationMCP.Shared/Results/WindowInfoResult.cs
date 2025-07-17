using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for window information operations
    /// </summary>
    public class WindowInfoResult : BaseOperationResult
    {
        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = string.Empty;

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        [JsonPropertyName("processName")]
        public string ProcessName { get; set; } = string.Empty;

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = string.Empty;

        [JsonPropertyName("windowHandle")]
        public string WindowHandle { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("className")]
        public string ClassName { get; set; } = string.Empty;

        [JsonPropertyName("handle")]
        public IntPtr Handle { get; set; }

        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }

        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }
    }
}