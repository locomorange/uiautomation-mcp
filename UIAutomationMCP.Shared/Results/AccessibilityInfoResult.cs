using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class AccessibilityInfoResult : BaseOperationResult
    {
        public Dictionary<string, object> AccessibilityInfo { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsPassword { get; set; }
        public string? HelpText { get; set; }
        public string? AcceleratorKey { get; set; }
        public string? AccessKey { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public string? LocalizedControlType { get; set; }
        public string? ItemType { get; set; }
        public string? ItemStatus { get; set; }
        public string? LabeledBy { get; set; }
        public string? DescribedBy { get; set; }
        public string? FlowsTo { get; set; }
        public string? FlowsFrom { get; set; }
    }

    public class Rectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}