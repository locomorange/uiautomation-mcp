using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{

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