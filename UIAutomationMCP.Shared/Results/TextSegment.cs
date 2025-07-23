using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Represents a text segment with consistent attributes.
    /// Each segment represents a portion of text where all attributes are uniform.
    /// </summary>
    public class TextSegment
    {
        [JsonPropertyName("startPosition")]
        public int StartPosition { get; set; }

        [JsonPropertyName("endPosition")]
        public int EndPosition { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("attributes")]
        public SegmentAttributes Attributes { get; set; } = new();
    }

    /// <summary>
    /// Simplified text attributes for segments without mixed state flags.
    /// Since each segment has consistent attributes, mixed flags are no longer needed.
    /// </summary>
    public class SegmentAttributes
    {
        [JsonPropertyName("fontName")]
        public string? FontName { get; set; }

        [JsonPropertyName("fontSize")]
        public double? FontSize { get; set; }

        [JsonPropertyName("fontWeight")]
        public string? FontWeight { get; set; }

        [JsonPropertyName("isItalic")]
        public bool? IsItalic { get; set; }

        [JsonPropertyName("isBold")]
        public bool? IsBold { get; set; }

        [JsonPropertyName("foregroundColor")]
        public string? ForegroundColor { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string? BackgroundColor { get; set; }

        [JsonPropertyName("isUnderline")]
        public bool? IsUnderline { get; set; }

        [JsonPropertyName("underlineStyle")]
        public string? UnderlineStyle { get; set; }

        [JsonPropertyName("underlineColor")]
        public string? UnderlineColor { get; set; }

        [JsonPropertyName("isStrikethrough")]
        public bool? IsStrikethrough { get; set; }

        [JsonPropertyName("strikethroughStyle")]
        public string? StrikethroughStyle { get; set; }

        [JsonPropertyName("strikethroughColor")]
        public string? StrikethroughColor { get; set; }

        [JsonPropertyName("horizontalTextAlignment")]
        public string? HorizontalTextAlignment { get; set; }

        [JsonPropertyName("culture")]
        public string? Culture { get; set; }

        [JsonPropertyName("isReadOnly")]
        public bool? IsReadOnly { get; set; }

        [JsonPropertyName("isHidden")]
        public bool? IsHidden { get; set; }

        [JsonPropertyName("isSubscript")]
        public bool? IsSubscript { get; set; }

        [JsonPropertyName("isSuperscript")]
        public bool? IsSuperscript { get; set; }

        [JsonPropertyName("capStyle")]
        public string? CapStyle { get; set; }

        [JsonPropertyName("outlineStyle")]
        public string? OutlineStyle { get; set; }

        [JsonPropertyName("animationStyle")]
        public string? AnimationStyle { get; set; }

        [JsonPropertyName("bulletStyle")]
        public string? BulletStyle { get; set; }

        [JsonPropertyName("indentationFirstLine")]
        public double? IndentationFirstLine { get; set; }

        [JsonPropertyName("indentationLeading")]
        public double? IndentationLeading { get; set; }

        [JsonPropertyName("indentationTrailing")]
        public double? IndentationTrailing { get; set; }

        [JsonPropertyName("marginBottom")]
        public double? MarginBottom { get; set; }

        [JsonPropertyName("marginLeading")]
        public double? MarginLeading { get; set; }

        [JsonPropertyName("marginTop")]
        public double? MarginTop { get; set; }

        [JsonPropertyName("marginTrailing")]
        public double? MarginTrailing { get; set; }

        [JsonPropertyName("tabs")]
        public string? Tabs { get; set; }
    }
}
