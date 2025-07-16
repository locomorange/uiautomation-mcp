using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class LegacyPropertiesResult : BaseOperationResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("state")]
        public uint State { get; set; }

        [JsonPropertyName("stateText")]
        public string StateText { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("help")]
        public string Help { get; set; } = "";

        [JsonPropertyName("keyboardShortcut")]
        public string KeyboardShortcut { get; set; } = "";

        [JsonPropertyName("defaultAction")]
        public string DefaultAction { get; set; } = "";

        [JsonPropertyName("childId")]
        public int ChildId { get; set; }
    }

    public class LegacyStateResult : BaseOperationResult
    {
        [JsonPropertyName("state")]
        public uint State { get; set; }

        [JsonPropertyName("stateText")]
        public string StateText { get; set; } = "";

        [JsonPropertyName("stateFlags")]
        public Dictionary<string, bool> StateFlags { get; set; } = new();
    }
}