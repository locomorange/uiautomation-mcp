using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Transform pattern capabilities result
    /// </summary>
    public class TransformCapabilitiesResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }

        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }

        [JsonPropertyName("canRotate")]
        public bool CanRotate { get; set; }
    }

    /// <summary>
    /// Grid pattern information result
    /// </summary>
    public class GridInfoResult : BaseOperationResult
    {
        [JsonPropertyName("gridElementId")]
        public string GridElementId { get; set; } = "";

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }

        [JsonPropertyName("hasHeaders")]
        public bool HasHeaders { get; set; }

        [JsonPropertyName("isScrollable")]
        public bool IsScrollable { get; set; }

        [JsonPropertyName("isSelectable")]
        public bool IsSelectable { get; set; }

        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }

        // Additional properties referenced in tests
        [JsonPropertyName("selectionMode")]
        public string SelectionMode { get; set; } = "";

        [JsonPropertyName("totalItemCount")]
        public int TotalItemCount { get; set; }

        [JsonPropertyName("visibleItemCount")]
        public int VisibleItemCount { get; set; }
    }

    /// <summary>
    /// Element value result
    /// </summary>
    public class ElementValueResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        [JsonPropertyName("stringValue")]
        public string StringValue { get; set; } = "";

        [JsonPropertyName("hasValue")]
        public bool HasValue { get; set; }

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
    }
}