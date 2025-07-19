using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ElementInspectionResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }
        public string? LocalizedControlType { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsPassword { get; set; }
        public bool IsOffscreen { get; set; }
        public int ProcessId { get; set; }
        public string? RuntimeId { get; set; }
        public string? FrameworkId { get; set; }
        public string? ProviderDescription { get; set; }
        public string? HelpText { get; set; }
        public string? AcceleratorKey { get; set; }
        public string? AccessKey { get; set; }
        public int NativeWindowHandle { get; set; }
        public string? ItemType { get; set; }
        public string? ItemStatus { get; set; }
        public bool IsControlElement { get; set; }
        public bool IsContentElement { get; set; }
        public bool IsRequiredForForm { get; set; }
        public bool IsDataValidForForm { get; set; }
        public string? LabeledBy { get; set; }
        public string? DescribedBy { get; set; }
        public string? FlowsTo { get; set; }
        public string? FlowsFrom { get; set; }
        public int Culture { get; set; }
        public string? Orientation { get; set; }
        public string? LiveSetting { get; set; }
        public string? OptimizeForVisualContent { get; set; }
        public string? ClickablePoint { get; set; }
    }
}