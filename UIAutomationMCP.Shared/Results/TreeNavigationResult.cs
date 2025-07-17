using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TreeNavigationResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [JsonPropertyName("parentElementId")]
        public string? ParentElementId { get; set; }
        
        [JsonPropertyName("children")]
        public List<TreeElement> Children { get; set; } = new();
        
        [JsonPropertyName("firstChildId")]
        public string? FirstChildId { get; set; }
        
        [JsonPropertyName("lastChildId")]
        public string? LastChildId { get; set; }
        
        [JsonPropertyName("nextSiblingId")]
        public string? NextSiblingId { get; set; }
        
        [JsonPropertyName("previousSiblingId")]
        public string? PreviousSiblingId { get; set; }
        
        [JsonPropertyName("childCount")]
        public int ChildCount { get; set; }
        
        [JsonPropertyName("depth")]
        public int Depth { get; set; }
        
        [JsonPropertyName("treePath")]
        public string? TreePath { get; set; }
        
        // 要素の詳細情報（オプション）
        [JsonPropertyName("parentElement")]
        public TreeElementInfo? ParentElement { get; set; }
        
        [JsonPropertyName("firstChildElement")]
        public TreeElementInfo? FirstChildElement { get; set; }
        
        [JsonPropertyName("lastChildElement")]
        public TreeElementInfo? LastChildElement { get; set; }
        
        [JsonPropertyName("nextSiblingElement")]
        public TreeElementInfo? NextSiblingElement { get; set; }
        
        [JsonPropertyName("previousSiblingElement")]
        public TreeElementInfo? PreviousSiblingElement { get; set; }
        
        [JsonPropertyName("navigationType")]
        public string? NavigationType { get; set; }
        
        [JsonPropertyName("elements")]
        public List<TreeElement> Elements { get; set; } = new();
    }

    public class TreeElement
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("parentElementId")]
        public string? ParentElementId { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("localizedControlType")]
        public string? LocalizedControlType { get; set; }
        
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }
        
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }
        
        [JsonPropertyName("isPassword")]
        public bool IsPassword { get; set; }
        
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("runtimeId")]
        public string? RuntimeId { get; set; }
        
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }
        
        [JsonPropertyName("nativeWindowHandle")]
        public int NativeWindowHandle { get; set; }
        
        [JsonPropertyName("isControlElement")]
        public bool IsControlElement { get; set; }
        
        [JsonPropertyName("isContentElement")]
        public bool IsContentElement { get; set; }
        
        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }
        
        [JsonPropertyName("childCount")]
        public int ChildCount { get; set; }
        
        // 子要素は再帰的に含めない（循環参照回避）
        [JsonIgnore]
        public List<TreeElement> Children { get; set; } = new();
    }

    // 基本情報のみを含む軽量版
    public class TreeElementInfo
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
    }
}