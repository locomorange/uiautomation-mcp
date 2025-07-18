using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === SynchronizedInput Pattern ===

    public class StartSynchronizedInputRequest : ElementTargetRequest
    {
        public override string Operation => "StartSynchronizedInput";

        [JsonPropertyName("inputType")]
        public string InputType { get; set; } = "";
    }

    public class CancelSynchronizedInputRequest : ElementTargetRequest
    {
        public override string Operation => "CancelSynchronizedInput";
    }
}