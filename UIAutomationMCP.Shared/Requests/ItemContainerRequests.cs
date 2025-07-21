using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === ItemContainer Pattern ===

    public class FindItemByPropertyRequest : ElementTargetRequest
    {
        public override string Operation => "FindItemByProperty";

        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        [JsonPropertyName("startAfterId")]
        public string StartAfterId { get; set; } = "";

    }
}