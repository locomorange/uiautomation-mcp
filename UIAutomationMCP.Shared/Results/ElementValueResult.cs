using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ElementValueResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public object? Value { get; set; }
        public string? ValueType { get; set; }
        public string? StringValue { get; set; }
        public double? NumericValue { get; set; }
        public bool? BooleanValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public bool IsReadOnly { get; set; }
        public string? Pattern { get; set; }
        public string? PropertyName { get; set; }
        public Dictionary<string, object> AdditionalProperties { get; set; } = new();
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public bool HasValue { get; set; }
        public string? RawValue { get; set; }
        public string? FormattedValue { get; set; }
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
        public object? DefaultValue { get; set; }
        public string? Unit { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsValid { get; set; } = true;
    }
}