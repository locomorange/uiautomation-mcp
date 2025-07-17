using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class ElementSearchResult : CollectionOperationResult<UIAutomationMCP.Shared.ElementInfo>
    {
        public List<UIAutomationMCP.Shared.ElementInfo> Elements 
        { 
            get => Items;
            set => Items = value ?? new List<UIAutomationMCP.Shared.ElementInfo>();
        }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("searchCriteria")]
        public string? SearchCriteria { get; set; }
        
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
    }

    public class DetailedElementInfo : UIAutomationMCP.Shared.ElementInfo
    {
        public string ElementId { get; set; } = "";
        public string LocalizedControlType { get; set; } = "";
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsPassword { get; set; }
        public bool IsOffscreen { get; set; }
        public string RuntimeId { get; set; } = "";
        public string FrameworkId { get; set; } = "";
        public int NativeWindowHandle { get; set; }
        public List<string> SupportedPatterns { get; set; } = new();
        public new List<string> AvailableActions { get; set; } = new();
        
        // 拡張プロパティ（必要に応じて）
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}