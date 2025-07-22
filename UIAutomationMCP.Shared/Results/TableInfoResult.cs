using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Table pattern information result
    /// </summary>
    public class TableInfoResult : BaseOperationResult
    {
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";

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

        [JsonPropertyName("hasRowHeaders")]
        public bool HasRowHeaders { get; set; }

        [JsonPropertyName("hasColumnHeaders")]
        public bool HasColumnHeaders { get; set; }

        [JsonPropertyName("isScrollable")]
        public bool IsScrollable { get; set; }

        [JsonPropertyName("selectionMode")]
        public string SelectionMode { get; set; } = "";

        [JsonPropertyName("totalCellCount")]
        public int TotalCellCount { get; set; }

        [JsonPropertyName("visibleCellCount")]
        public int VisibleCellCount { get; set; }

        [JsonPropertyName("rowOrColumnMajor")]
        public string RowOrColumnMajor { get; set; } = "";
    }
}