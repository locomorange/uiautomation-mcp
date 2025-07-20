using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// Get element value request parameters
    /// </summary>
    public class GetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementValue";
    }

    /// <summary>
    /// Set element value request parameters
    /// </summary>
    public class SetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetElementValue";
        
        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// Get value request parameters
    /// </summary>
    public class GetValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementValue";
    }

    /// <summary>
    /// Check if element is read-only request parameters
    /// </summary>
    public class IsReadOnlyRequest : ElementTargetRequest
    {
        public override string Operation => "IsReadOnly";
    }

}