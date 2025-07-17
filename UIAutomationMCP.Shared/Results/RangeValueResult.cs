using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for range value operations
    /// </summary>
    public class RangeValueResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        [JsonPropertyName("currentValue")]
        public double CurrentValue { get; set; }

        [JsonPropertyName("minimumValue")]
        public double MinimumValue { get; set; }

        [JsonPropertyName("maximumValue")]
        public double MaximumValue { get; set; }

        [JsonPropertyName("smallChange")]
        public double SmallChange { get; set; }

        [JsonPropertyName("largeChange")]
        public double LargeChange { get; set; }

        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [JsonPropertyName("minimum")]
        public double Minimum { get; set; }
        
        [JsonPropertyName("maximum")]
        public double Maximum { get; set; }
    }
}