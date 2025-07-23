using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Text操作 ===

    public class SetTextRequest : ElementTargetRequest
    {
        public override string Operation => "SetText";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }


    public class FindTextRequest : ElementTargetRequest
    {
        public override string Operation => "FindText";

        [JsonPropertyName("searchText")]
        public string SearchText { get; set; } = "";

        [JsonPropertyName("backward")]
        public bool Backward { get; set; } = false;

        [JsonPropertyName("ignoreCase")]
        public bool IgnoreCase { get; set; } = false;
    }

    public class GetTextAttributesRequest : ElementTargetRequest
    {
        public override string Operation => "GetTextAttributes";

        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; } = 0;

        [JsonPropertyName("length")]
        public int Length { get; set; } = -1;

        [JsonPropertyName("attributeName")]
        public string? AttributeName { get; set; }
    }

    public class SelectTextRequest : ElementTargetRequest
    {
        public override string Operation => "SelectText";

        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    public class TraverseTextRequest : ElementTargetRequest
    {
        public override string Operation => "TraverseText";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "character"; // "character", "word", "line", "paragraph" with optional "-back"

        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;
    }
}