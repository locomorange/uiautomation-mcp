using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === 基本操作 ===

    public class InvokeElementRequest : ElementTargetRequest
    {
        public override string Operation => "InvokeElement";
    }

    public class ToggleElementRequest : ElementTargetRequest
    {
        public override string Operation => "ToggleElement";
    }

    public class GetToggleStateRequest : ElementTargetRequest
    {
        public override string Operation => "GetToggleState";
    }

    public class SetToggleStateRequest : ElementTargetRequest
    {
        public override string Operation => "SetToggleState";

        [JsonPropertyName("state")]
        public string State { get; set; } = ""; // "on", "off", "indeterminate"
    }

    public class SetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetElementValue";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    public class SetFocusRequest : ElementTargetRequest
    {
        public override string Operation => "SetFocus";
    }
}
