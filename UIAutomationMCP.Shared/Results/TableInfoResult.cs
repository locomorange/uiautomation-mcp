using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Table pattern information result
    /// </summary>
    public class TableInfoResult : BaseOperationResult
    {
        [JsonPropertyName("tableElementId")]
        public string TableElementId { get; set; } = "";

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }

        [JsonPropertyName("hasHeaders")]
        public bool HasHeaders { get; set; }

        [JsonPropertyName("rowHeaders")]
        public string[] RowHeaders { get; set; } = Array.Empty<string>();

        [JsonPropertyName("columnHeaders")]
        public string[] ColumnHeaders { get; set; } = Array.Empty<string>();

        [JsonPropertyName("caption")]
        public string Caption { get; set; } = "";

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = "";

        [JsonPropertyName("isSelectable")]
        public bool IsSelectable { get; set; }

        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }
    }
}