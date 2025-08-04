using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === Scroll操作 ===

    public class ExpandCollapseElementRequest : ElementTargetRequest
    {
        public override string Operation => "ExpandCollapseElement";

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "expand", "collapse", "toggle"
    }

    public class GetScrollInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetScrollInfo";
    }

    public class ScrollElementIntoViewRequest : ElementTargetRequest
    {
        public override string Operation => "ScrollElementIntoView";
    }

    public class ScrollElementRequest : ElementTargetRequest
    {
        public override string Operation => "ScrollElement";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "up", "down", "left", "right", "pageup", "pagedown", "pageleft", "pageright"

        [JsonPropertyName("amount")]
        public double Amount { get; set; } = 1.0;
    }

    public class SetScrollPercentRequest : ElementTargetRequest
    {
        public override string Operation => "SetScrollPercent";

        [JsonPropertyName("horizontalPercent")]
        public double HorizontalPercent { get; set; } = -1; // -1 = no change, 0-100 = percentage

        [JsonPropertyName("verticalPercent")]
        public double VerticalPercent { get; set; } = -1; // -1 = no change, 0-100 = percentage
    }
}
