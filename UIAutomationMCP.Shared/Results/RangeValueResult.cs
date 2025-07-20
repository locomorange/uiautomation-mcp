using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Range value pattern result
    /// </summary>
    public class RangeValueResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("minimum")]
        public double Minimum { get; set; }

        [JsonPropertyName("maximum")]
        public double Maximum { get; set; }

        [JsonPropertyName("smallChange")]
        public double SmallChange { get; set; }

        [JsonPropertyName("largeChange")]
        public double LargeChange { get; set; }

        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonPropertyName("hasValue")]
        public bool HasValue { get; set; }

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
    }
}