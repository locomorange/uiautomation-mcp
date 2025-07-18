using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Grid操作 ===

    public class GetGridInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetGridInfo";
    }

    public class GetGridItemRequest : ElementTargetRequest
    {
        public override string Operation => "GetGridItem";

        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    public class GetColumnHeaderRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeader";

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    public class GetRowHeaderRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeader";

        [JsonPropertyName("row")]
        public int Row { get; set; }
    }
}