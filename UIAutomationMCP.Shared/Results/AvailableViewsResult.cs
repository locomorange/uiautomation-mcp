using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class AvailableViewsResult : BaseOperationResult
    {
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        [JsonPropertyName("availableViews")]
        public List<ViewInfo> AvailableViews { get; set; } = new();
        
        [JsonPropertyName("currentView")]
        public int CurrentView { get; set; }
        
        [JsonPropertyName("currentViewName")]
        public string? CurrentViewName { get; set; }
        
        [JsonPropertyName("canSetView")]
        public bool CanSetView { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("viewProperties")]
        public Dictionary<string, object> ViewProperties { get; set; } = new();
        
        [JsonPropertyName("viewCount")]
        public int ViewCount { get; set; }
        
        [JsonPropertyName("hasMultipleViews")]
        public bool HasMultipleViews { get; set; }
        
        [JsonPropertyName("currentViewId")]
        public int CurrentViewId { get; set; }
        
        [JsonPropertyName("views")]
        public List<ViewInfo> Views { get; set; } = new();
    }

    public class ViewInfo
    {
        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
        
        [JsonPropertyName("viewName")]
        public string? ViewName { get; set; }
        
        [JsonPropertyName("isCurrentView")]
        public bool IsCurrentView { get; set; }
        
        [JsonPropertyName("isSelectable")]
        public bool IsSelectable { get; set; }
        
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("isCurrent")]
        public bool IsCurrent { get; set; }
    }
}