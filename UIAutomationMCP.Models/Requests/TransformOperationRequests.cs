using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === Transform操作 ===

    public class TransformElementRequest : ElementTargetRequest
    {
        public override string Operation => "TransformElement";

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "move", "resize", "rotate"

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }
    }

    public class MoveElementRequest : ElementTargetRequest
    {
        public override string Operation => "MoveElement";

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class ResizeElementRequest : ElementTargetRequest
    {
        public override string Operation => "ResizeElement";

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }
    }

    public class RotateElementRequest : ElementTargetRequest
    {
        public override string Operation => "RotateElement";

        [JsonPropertyName("degrees")]
        public double Degrees { get; set; }
    }

    public class GetTransformCapabilitiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetTransformCapabilities";
    }
}
