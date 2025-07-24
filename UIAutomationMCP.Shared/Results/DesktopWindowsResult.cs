using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{

    /// <summary>
    /// Information about a window
    /// </summary>
    public class WindowInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

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

        [JsonPropertyName("windowHandle")]
        public string WindowHandle { get; set; } = string.Empty;
        
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }
        
        [JsonPropertyName("handle")]
        public long Handle { get; set; }
    }
}