using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ViewResult : BaseOperationResult
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
        [JsonPropertyName("currentView")]
        public int CurrentView { get; set; }
        
        [JsonPropertyName("currentViewName")]
        public string? CurrentViewName { get; set; }
        
        [JsonPropertyName("availableViews")]
        public List<int> AvailableViews { get; set; } = new();
        
        [JsonPropertyName("availableViewNames")]
        public List<string> AvailableViewNames { get; set; } = new();
        
        [JsonPropertyName("canSetView")]
        public bool CanSetView { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("viewProperties")]
        public Dictionary<string, object> ViewProperties { get; set; } = new();
        
        [JsonPropertyName("returnValue")]
        public object? ReturnValue { get; set; }
        
        [JsonPropertyName("viewChanged")]
        public bool ViewChanged { get; set; }
        
        [JsonPropertyName("previousView")]
        public int PreviousView { get; set; }
        
        [JsonPropertyName("previousViewName")]
        public string? PreviousViewName { get; set; }
        
        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
        
        [JsonPropertyName("viewName")]
        public string? ViewName { get; set; }
    }
}