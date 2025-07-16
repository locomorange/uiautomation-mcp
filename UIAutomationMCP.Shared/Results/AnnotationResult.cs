using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class AnnotationInfoResult : BaseOperationResult
    {
        [JsonPropertyName("annotationType")]
        public int AnnotationType { get; set; }

        [JsonPropertyName("annotationTypeName")]
        public string AnnotationTypeName { get; set; } = "";

        [JsonPropertyName("author")]
        public string Author { get; set; } = "";

        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; } = "";

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = "";
    }

    public class AnnotationTargetResult : BaseOperationResult
    {
        [JsonPropertyName("targetElementId")]
        public string TargetElementId { get; set; } = "";

        [JsonPropertyName("targetElementName")]
        public string TargetElementName { get; set; } = "";

        [JsonPropertyName("targetElementType")]
        public string TargetElementType { get; set; } = "";

        [JsonPropertyName("targetElementBounds")]
        public Dictionary<string, double>? TargetElementBounds { get; set; }
    }
}