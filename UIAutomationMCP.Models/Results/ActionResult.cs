using System.Text.Json.Serialization;
using UIAutomationMCP.Models.Results;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    [MessagePackObject]
    public class ActionResult : BaseOperationResult
    {
        [Key(6)]
        [JsonPropertyName("action")]
        public string? Action { get; set; }
        
        [Key(7)]
        [JsonPropertyName("actionName")]
        public string? ActionName { get; set; }
        
        [Key(8)]
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [Key(9)]
        [JsonPropertyName("targetName")]
        public string? TargetName { get; set; }
        
        [Key(10)]
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }
        
        [Key(11)]
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }
        
        [Key(12)]
        [JsonPropertyName("actionParameters")]
        public ActionParameters? ActionParameters { get; set; }
        
        [Key(13)]
        [JsonPropertyName("elementState")]
        public ElementState? ElementState { get; set; }
        
        [Key(14)]
        [JsonPropertyName("executionTimeMs")]
        public double ExecutionTimeMs { get; set; }
        
        [Key(15)]
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [Key(16)]
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [Key(17)]
        [JsonPropertyName("requiredRetries")]
        public bool RequiredRetries { get; set; }
        
        [Key(18)]
        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; }
        
        [Key(19)]
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [Key(20)]
        [JsonPropertyName("patternMethod")]
        public string? PatternMethod { get; set; }
        
        [Key(21)]
        [JsonPropertyName("returnValue")]
        public object? ReturnValue { get; set; }
        
        [Key(22)]
        [JsonPropertyName("stateChanged")]
        public bool StateChanged { get; set; }
        
        [Key(23)]
        [JsonPropertyName("beforeState")]
        public ElementState? BeforeState { get; set; }
        
        [Key(24)]
        [JsonPropertyName("afterState")]
        public ElementState? AfterState { get; set; }
        
        [Key(25)]
        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
        
        [Key(26)]
        [JsonPropertyName("details")]
        public string? Details { get; set; }
        
        [Key(27)]
        [JsonPropertyName("executedAt")]
        public new DateTime ExecutedAt { get; set; }
    }
}