using System.Text.Json.Serialization;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Models.Results
{
    public class ActionResult : BaseOperationResult
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }
        
        [JsonPropertyName("actionName")]
        public string? ActionName { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("targetName")]
        public string? TargetName { get; set; }
        
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }
        
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }
        
        [JsonPropertyName("actionParameters")]
        public string ActionParameters { get; set; } = string.Empty;
        
        [JsonPropertyName("elementState")]
        public string ElementState { get; set; } = string.Empty;
        
        [JsonPropertyName("executionTimeMs")]
        public double ExecutionTimeMs { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("requiredRetries")]
        public bool RequiredRetries { get; set; }
        
        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("patternMethod")]
        public string? PatternMethod { get; set; }
        
        [JsonPropertyName("returnValue")]
        public object? ReturnValue { get; set; }
        
        [JsonPropertyName("stateChanged")]
        public bool StateChanged { get; set; }
        
        [JsonPropertyName("beforeState")]
        public string BeforeState { get; set; } = string.Empty;
        
        [JsonPropertyName("afterState")]
        public string AfterState { get; set; } = string.Empty;
        
        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
        
        [JsonPropertyName("details")]
        public string? Details { get; set; }
        
        [JsonPropertyName("executedAt")]
        public new DateTime ExecutedAt { get; set; }
    }
}