using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared
{
    /// <summary>
    /// Selection item information for test compatibility
    /// </summary>
    public class SelectionItem
    {
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = "";

        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }

        [JsonPropertyName("selectionContainer")]
        public string? SelectionContainer { get; set; }
    }
}