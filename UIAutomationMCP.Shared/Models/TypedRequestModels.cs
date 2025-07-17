using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Models
{
    /// <summary>
    /// Base class for all typed request parameters
    /// </summary>
    public abstract class TypedRequestBase
    {
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Find element request parameters
    /// </summary>
    public class FindElementRequest : TypedRequestBase
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
    }

    /// <summary>
    /// Element action request parameters
    /// </summary>
    public class ElementActionRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("actionType")]
        public string? ActionType { get; set; }
        
        [JsonPropertyName("actionData")]
        public Dictionary<string, object>? ActionData { get; set; }
    }

    /// <summary>
    /// Set element value request parameters
    /// </summary>
    public class SetElementValueRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("value")]
        public object Value { get; set; } = "";
    }

    /// <summary>
    /// Element inspection request parameters
    /// </summary>
    public class ElementInspectionRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("includeProperties")]
        public bool IncludeProperties { get; set; } = true;
        
        [JsonPropertyName("includePatterns")]
        public bool IncludePatterns { get; set; } = true;
        
        [JsonPropertyName("includeBoundingRect")]
        public bool IncludeBoundingRect { get; set; } = true;
    }

    /// <summary>
    /// Tree navigation request parameters
    /// </summary>
    public class TreeNavigationRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "parent", "children", "nextSibling", "previousSibling"
        
        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; } = 1;
    }

    /// <summary>
    /// Screenshot request parameters
    /// </summary>
    public class ScreenshotRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [JsonPropertyName("outputPath")]
        public string? OutputPath { get; set; }
        
        [JsonPropertyName("maxTokens")]
        public int MaxTokens { get; set; } = 0;
        
        [JsonPropertyName("captureFullWindow")]
        public bool CaptureFullWindow { get; set; } = true;
    }

    /// <summary>
    /// Scroll request parameters
    /// </summary>
    public class ScrollRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "up", "down", "left", "right"
        
        [JsonPropertyName("amount")]
        public double Amount { get; set; } = 1.0;
    }

    /// <summary>
    /// Range value request parameters
    /// </summary>
    public class RangeValueRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [JsonPropertyName("largeChange")]
        public bool LargeChange { get; set; } = false;
    }

    /// <summary>
    /// Selection request parameters
    /// </summary>
    public class SelectionRequest : TypedRequestBase
    {
        [JsonPropertyName("containerElementId")]
        public string ContainerElementId { get; set; } = "";
        
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [JsonPropertyName("selectionAction")]
        public string SelectionAction { get; set; } = ""; // "select", "addToSelection", "removeFromSelection"
    }

    /// <summary>
    /// Text manipulation request parameters
    /// </summary>
    public class TextRequest : TypedRequestBase
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";
        
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; } = 0;
        
        [JsonPropertyName("length")]
        public int Length { get; set; } = -1;
        
        [JsonPropertyName("replaceAll")]
        public bool ReplaceAll { get; set; } = false;
    }

    /// <summary>
    /// Grid operation request parameters
    /// </summary>
    public class GridRequest : TypedRequestBase
    {
        [JsonPropertyName("gridElementId")]
        public string GridElementId { get; set; } = "";
        
        [JsonPropertyName("row")]
        public int Row { get; set; } = -1;
        
        [JsonPropertyName("column")]
        public int Column { get; set; } = -1;
        
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = ""; // "getItem", "getInfo", "selectItem"
    }

    /// <summary>
    /// Application launch request parameters
    /// </summary>
    public class LaunchApplicationRequest
    {
        [JsonPropertyName("applicationPath")]
        public string ApplicationPath { get; set; } = "";
        
        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
        
        [JsonPropertyName("workingDirectory")]
        public string? WorkingDirectory { get; set; }
        
        [JsonPropertyName("waitForWindow")]
        public bool WaitForWindow { get; set; } = true;
        
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Window operation request parameters
    /// </summary>
    public class WindowOperationRequest : TypedRequestBase
    {
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = ""; // "minimize", "maximize", "restore", "close", "activate"
        
        [JsonPropertyName("newState")]
        public string? NewState { get; set; }
        
        [JsonPropertyName("position")]
        public Point? Position { get; set; }
        
        [JsonPropertyName("size")]
        public Size? Size { get; set; }
    }

    /// <summary>
    /// Point structure for window operations
    /// </summary>
    public class Point
    {
        [JsonPropertyName("x")]
        public int X { get; set; }
        
        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// Size structure for window operations
    /// </summary>
    public class Size
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }
        
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}