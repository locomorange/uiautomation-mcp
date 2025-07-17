using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Represents text range attributes
    /// </summary>
    public class TextRangeAttributes
    {
        [JsonPropertyName("fontName")]
        public string? FontName { get; set; }

        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; }

        [JsonPropertyName("fontWeight")]
        public string? FontWeight { get; set; }

        [JsonPropertyName("fontStyle")]
        public string? FontStyle { get; set; }

        [JsonPropertyName("textColor")]
        public string? TextColor { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string? BackgroundColor { get; set; }

        [JsonPropertyName("isItalic")]
        public bool IsItalic { get; set; }

        [JsonPropertyName("isBold")]
        public bool IsBold { get; set; }

        [JsonPropertyName("isUnderline")]
        public bool IsUnderline { get; set; }

        [JsonPropertyName("isStrikethrough")]
        public bool IsStrikethrough { get; set; }

        [JsonPropertyName("isSubscript")]
        public bool IsSubscript { get; set; }

        [JsonPropertyName("isSuperscript")]
        public bool IsSuperscript { get; set; }

        [JsonPropertyName("horizontalTextAlignment")]
        public string? HorizontalTextAlignment { get; set; }

        [JsonPropertyName("indentationFirstLine")]
        public double IndentationFirstLine { get; set; }

        [JsonPropertyName("indentationLeading")]
        public double IndentationLeading { get; set; }

        [JsonPropertyName("indentationTrailing")]
        public double IndentationTrailing { get; set; }

        [JsonPropertyName("marginBottom")]
        public double MarginBottom { get; set; }

        [JsonPropertyName("marginLeading")]
        public double MarginLeading { get; set; }

        [JsonPropertyName("marginTop")]
        public double MarginTop { get; set; }

        [JsonPropertyName("marginTrailing")]
        public double MarginTrailing { get; set; }

        [JsonPropertyName("textFlowDirections")]
        public string? TextFlowDirections { get; set; }

        [JsonPropertyName("animationStyle")]
        public string? AnimationStyle { get; set; }

        [JsonPropertyName("bulletStyle")]
        public string? BulletStyle { get; set; }

        [JsonPropertyName("capStyle")]
        public string? CapStyle { get; set; }

        [JsonPropertyName("culture")]
        public string? Culture { get; set; }

        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonPropertyName("isHidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("outlineStyles")]
        public string? OutlineStyles { get; set; }

        [JsonPropertyName("overlineColor")]
        public string? OverlineColor { get; set; }

        [JsonPropertyName("overlineStyle")]
        public string? OverlineStyle { get; set; }

        [JsonPropertyName("strikethroughColor")]
        public string? StrikethroughColor { get; set; }

        [JsonPropertyName("strikethroughStyle")]
        public string? StrikethroughStyle { get; set; }

        [JsonPropertyName("tabs")]
        public List<string> Tabs { get; set; } = new();

        [JsonPropertyName("underlineColor")]
        public string? UnderlineColor { get; set; }

        [JsonPropertyName("underlineStyle")]
        public string? UnderlineStyle { get; set; }

        // Missing properties for GetTextAttributesOperation
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new();

        [JsonPropertyName("boundingRectangle")]
        public UIAutomationMCP.Shared.BoundingRectangle BoundingRectangle { get; set; } = new();
    }
}