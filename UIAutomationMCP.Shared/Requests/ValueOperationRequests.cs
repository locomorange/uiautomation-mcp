using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === 値操作 ===

    public class SetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetElementValue";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    public class GetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementValue";
    }

    public class IsReadOnlyRequest : ElementTargetRequest
    {
        public override string Operation => "IsReadOnly";
    }
}