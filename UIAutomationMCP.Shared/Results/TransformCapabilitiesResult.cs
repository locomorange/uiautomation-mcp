using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for transform capabilities operations
    /// </summary>
    public class TransformCapabilitiesResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }

        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }

        [JsonPropertyName("canRotate")]
        public bool CanRotate { get; set; }

        [JsonPropertyName("currentX")]
        public double CurrentX { get; set; }

        [JsonPropertyName("currentY")]
        public double CurrentY { get; set; }

        [JsonPropertyName("currentWidth")]
        public double CurrentWidth { get; set; }

        [JsonPropertyName("currentHeight")]
        public double CurrentHeight { get; set; }

        [JsonPropertyName("currentRotation")]
        public double CurrentRotation { get; set; }

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        // Missing property for GetTransformCapabilitiesOperation
        [JsonPropertyName("currentBounds")]
        public UIAutomationMCP.Shared.BoundingRectangle CurrentBounds { get; set; } = new();
    }
}