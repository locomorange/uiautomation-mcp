using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === Range操作 ===

    public class SetRangeValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetRangeValue";

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public class GetRangeValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetRangeValue";
    }

    public class GetRangePropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetRangeProperties";
    }
}