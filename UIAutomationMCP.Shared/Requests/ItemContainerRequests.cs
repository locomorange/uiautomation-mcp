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

        // Backward compatibility: ContainerId maps to AutomationId/Name for container identification
        [JsonPropertyName("containerId")]
        [Obsolete("Use AutomationId or Name to identify the container element")]
        public string ContainerId 
        { 
            get => AutomationId ?? Name ?? "";
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (string.IsNullOrEmpty(AutomationId))
                        AutomationId = value;
                }
            }
        }
    }
}