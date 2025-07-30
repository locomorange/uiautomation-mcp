using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === TreeNavigation操作 ===

    public class GetAncestorsRequest : ElementTargetRequest
    {
        public override string Operation => "GetAncestors";
    }

    public class GetChildrenRequest : ElementTargetRequest
    {
        public override string Operation => "GetChildren";
    }

    public class GetDescendantsRequest : ElementTargetRequest
    {
        public override string Operation => "GetDescendants";
    }

    public class GetElementTreeRequest : TypedWorkerRequest
    {
        public override string Operation => "GetElementTree";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }

        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; } = 3;
    }

    public class GetParentRequest : ElementTargetRequest
    {
        public override string Operation => "GetParent";
    }

    public class GetSiblingsRequest : ElementTargetRequest
    {
        public override string Operation => "GetSiblings";
    }
}