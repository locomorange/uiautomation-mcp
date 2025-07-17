using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for desktop windows operations
    /// </summary>
    public class DesktopWindowsResult : CollectionOperationResult<WindowInfo>
    {
        [JsonPropertyName("windows")]
        public List<WindowInfo> Windows 
        { 
            get => Items;
            set => Items = value ?? new List<WindowInfo>();
        }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("visibleCount")]
        public int VisibleCount { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

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