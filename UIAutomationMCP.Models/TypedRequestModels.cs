using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// Base class for all typed request parameters
    /// </summary>
    [MessagePackObject]
    public abstract class TypedRequestBase
    {
        [Key(0)]
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [Key(1)]
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [Key(2)]
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Find element request parameters
    /// </summary>
    [MessagePackObject]
    public class FindElementRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [Key(4)]
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [Key(5)]
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }
        
        [Key(6)]
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [Key(7)]
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
    }

    /// <summary>
    /// Element action request parameters
    /// </summary>
    [MessagePackObject]
    public class ElementActionRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("actionType")]
        public string? ActionType { get; set; }
        
        [Key(5)]
        [JsonPropertyName("actionData")]
        public Dictionary<string, object>? ActionData { get; set; }
    }


    /// <summary>
    /// Element inspection request parameters
    /// </summary>
    [MessagePackObject]
    public class ElementInspectionRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("includeProperties")]
        public bool IncludeProperties { get; set; } = true;
        
        [Key(5)]
        [JsonPropertyName("includePatterns")]
        public bool IncludePatterns { get; set; } = true;
        
        [Key(6)]
        [JsonPropertyName("includeBoundingRect")]
        public bool IncludeBoundingRect { get; set; } = true;
    }

    /// <summary>
    /// Tree navigation request parameters
    /// </summary>
    [MessagePackObject]
    public class TreeNavigationRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "parent", "children", "nextSibling", "previousSibling"
        
        [Key(5)]
        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; } = 1;
    }

    /// <summary>
    /// Screenshot request parameters
    /// </summary>
    [MessagePackObject]
    public class ScreenshotRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [Key(4)]
        [JsonPropertyName("outputPath")]
        public string? OutputPath { get; set; }
        
        [Key(5)]
        [JsonPropertyName("maxTokens")]
        public int MaxTokens { get; set; } = 0;
        
        [Key(6)]
        [JsonPropertyName("captureFullWindow")]
        public bool CaptureFullWindow { get; set; } = true;
    }

    /// <summary>
    /// Scroll request parameters
    /// </summary>
    [MessagePackObject]
    public class ScrollRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "up", "down", "left", "right"
        
        [Key(5)]
        [JsonPropertyName("amount")]
        public double Amount { get; set; } = 1.0;
    }

    /// <summary>
    /// Range value request parameters
    /// </summary>
    [MessagePackObject]
    public class RangeValueRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [Key(5)]
        [JsonPropertyName("largeChange")]
        public bool LargeChange { get; set; } = false;
    }

    /// <summary>
    /// Selection request parameters
    /// </summary>
    [MessagePackObject]
    public class SelectionRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("containerElementId")]
        public string ContainerElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [Key(5)]
        [JsonPropertyName("selectionAction")]
        public string SelectionAction { get; set; } = ""; // "select", "addToSelection", "removeFromSelection"
    }

    /// <summary>
    /// Text manipulation request parameters
    /// </summary>
    [MessagePackObject]
    public class TextRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        [Key(5)]
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; } = 0;
        
        [Key(6)]
        [JsonPropertyName("length")]
        public int Length { get; set; } = -1;
        
        [Key(7)]
        [JsonPropertyName("replaceAll")]
        public bool ReplaceAll { get; set; } = false;
    }

    /// <summary>
    /// Grid operation request parameters
    /// </summary>
    [MessagePackObject]
    public class GridRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("gridElementId")]
        public string GridElementId { get; set; } = "";
        
        [Key(4)]
        [JsonPropertyName("row")]
        public int Row { get; set; } = -1;
        
        [Key(5)]
        [JsonPropertyName("column")]
        public int Column { get; set; } = -1;
        
        [Key(6)]
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = ""; // "getItem", "getInfo", "selectItem"
    }

    /// <summary>
    /// Application launch request parameters
    /// </summary>
    [MessagePackObject]
    public class LaunchApplicationRequest
    {
        [Key(0)]
        [JsonPropertyName("applicationPath")]
        public string ApplicationPath { get; set; } = "";
        
        [Key(1)]
        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
        
        [Key(2)]
        [JsonPropertyName("workingDirectory")]
        public string? WorkingDirectory { get; set; }
        
        [Key(3)]
        [JsonPropertyName("waitForWindow")]
        public bool WaitForWindow { get; set; } = true;
        
        [Key(4)]
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Window operation request parameters
    /// </summary>
    [MessagePackObject]
    public class WindowOperationRequest : TypedRequestBase
    {
        [Key(3)]
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = ""; // "minimize", "maximize", "restore", "close", "activate"
        
        [Key(4)]
        [JsonPropertyName("newState")]
        public string? NewState { get; set; }
        
        [Key(5)]
        [JsonPropertyName("position")]
        public Point? Position { get; set; }
        
        [Key(6)]
        [JsonPropertyName("size")]
        public Size? Size { get; set; }
    }

    /// <summary>
    /// Point structure for window operations
    /// </summary>
    [MessagePackObject]
    public class Point
    {
        [Key(0)]
        [JsonPropertyName("x")]
        public int X { get; set; }
        
        [Key(1)]
        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// Size structure for window operations
    /// </summary>
    [MessagePackObject]
    public class Size
    {
        [Key(0)]
        [JsonPropertyName("width")]
        public int Width { get; set; }
        
        [Key(1)]
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}