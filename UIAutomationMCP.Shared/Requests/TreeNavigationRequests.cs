using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
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

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; } = 3;

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
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