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
}