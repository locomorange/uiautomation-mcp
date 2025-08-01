using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === MultipleView操作 ===

    public class GetAvailableViewsRequest : ElementTargetRequest
    {
        public override string Operation => "GetAvailableViews";
    }

    public class GetCurrentViewRequest : ElementTargetRequest
    {
        public override string Operation => "GetCurrentView";
    }

    public class GetViewNameRequest : ElementTargetRequest
    {
        public override string Operation => "GetViewName";

        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
    }

    public class SetViewRequest : ElementTargetRequest
    {
        public override string Operation => "SetView";

        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
    }
}