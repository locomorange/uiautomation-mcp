using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ElementTreeResult : BaseOperationResult
    {
        public string? RootElementId { get; set; }
        public TreeNode? RootNode { get; set; }
        public int TotalElements { get; set; }
        public int MaxDepth { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> TreeProperties { get; set; } = new();
        public List<string> ExpandedNodes { get; set; } = new();
        public TimeSpan BuildDuration { get; set; }
        public bool IncludeInvisible { get; set; }
        public bool IncludeOffscreen { get; set; }
        public string? TreeScope { get; set; }
    }

    public class TreeNode
    {
        public UIAutomationMCP.Shared.ElementInfo? Element { get; set; }
        public string? ElementId { get; set; }
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
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public int ProcessId { get; set; }
        public string? RuntimeId { get; set; }
        public string? FrameworkId { get; set; }
        public List<TreeNode> Children { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public int Depth { get; set; }
        public bool IsExpanded { get; set; }
        public bool HasChildren { get; set; }
        public string? ParentElementId { get; set; }
    }
}