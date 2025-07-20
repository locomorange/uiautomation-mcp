using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
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

    public class VerifyAccessibilityRequest : ElementTargetRequest
    {
        public override string Operation => "VerifyAccessibility";
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
    }

    public class SetCustomPropertyRequest : ElementTargetRequest
    {
        public override string Operation => "SetCustomProperty";

        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = "";

        [JsonPropertyName("propertyValue")]
        public string PropertyValue { get; set; } = "";
    }

    public class ValidateControlTypePatternsRequest : ElementTargetRequest
    {
        public override string Operation => "ValidateControlTypePatterns";
    }
}