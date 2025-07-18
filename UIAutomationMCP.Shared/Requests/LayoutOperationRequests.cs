using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Layout操作 ===

    public class DockElementRequest : ElementTargetRequest
    {
        public override string Operation => "DockElement";

        [JsonPropertyName("dockPosition")]
        public string DockPosition { get; set; } = ""; // "top", "bottom", "left", "right", "fill", "none"
    }

    public class RealizeVirtualizedItemRequest : ElementTargetRequest
    {
        public override string Operation => "RealizeVirtualizedItem";
    }
}