using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === ElementInspection操作 ===

    public class GetElementPropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementProperties";
    }

    public class GetElementPatternsRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementPatterns";
    }

    public class GetLabeledByRequest : ElementTargetRequest
    {
        public override string Operation => "GetLabeledBy";
    }

    public class GetDescribedByRequest : ElementTargetRequest
    {
        public override string Operation => "GetDescribedBy";
    }

    public class GetCustomPropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetCustomProperties";

        [JsonPropertyName("propertyIds")]
        public string[] PropertyIds { get; set; } = [];
    }

    public class SetCustomPropertyRequest : ElementTargetRequest
    {
        public override string Operation => "SetCustomProperty";

        [JsonPropertyName("propertyId")]
        public string PropertyId { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    public class ValidateControlTypePatternsRequest : ElementTargetRequest
    {
        public override string Operation => "ValidateControlTypePatterns";
    }

    public class GetAccessibilityInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetAccessibilityInfo";
    }
}