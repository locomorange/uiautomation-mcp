using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Scroll pattern information result
    /// </summary>
    public class ScrollInfoResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("horizontalScrollPercent")]
        public double HorizontalScrollPercent { get; set; }

        [JsonPropertyName("verticalScrollPercent")]
        public double VerticalScrollPercent { get; set; }

        [JsonPropertyName("horizontalViewSize")]
        public double HorizontalViewSize { get; set; }

        [JsonPropertyName("verticalViewSize")]
        public double VerticalViewSize { get; set; }

        [JsonPropertyName("horizontallyScrollable")]
        public bool HorizontallyScrollable { get; set; }

        [JsonPropertyName("verticallyScrollable")]
        public bool VerticallyScrollable { get; set; }

        [JsonPropertyName("canScrollHorizontally")]
        public bool CanScrollHorizontally { get; set; }

        [JsonPropertyName("canScrollVertically")]
        public bool CanScrollVertically { get; set; }
    }
}