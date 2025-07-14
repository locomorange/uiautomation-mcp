using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// 型安全なWorkerRequestの基底クラス
    /// </summary>
    public abstract class TypedWorkerRequest
    {
        [JsonPropertyName("operation")]
        public abstract string Operation { get; }
    }

    /// <summary>
    /// 要素を特定するための共通パラメータ
    /// </summary>
    public abstract class ElementTargetRequest : TypedWorkerRequest
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    // === 基本操作 ===

    public class InvokeElementRequest : ElementTargetRequest
    {
        public override string Operation => "InvokeElement";
    }

    public class ToggleElementRequest : ElementTargetRequest
    {
        public override string Operation => "ToggleElement";
    }

    public class GetToggleStateRequest : ElementTargetRequest
    {
        public override string Operation => "GetToggleState";
    }

    public class SetToggleStateRequest : ElementTargetRequest
    {
        public override string Operation => "SetToggleState";

        [JsonPropertyName("state")]
        public string State { get; set; } = ""; // "on", "off", "indeterminate"
    }

    // === 値操作 ===

    public class SetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetElementValue";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    public class GetElementValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementValue";
    }

    public class IsReadOnlyRequest : ElementTargetRequest
    {
        public override string Operation => "IsReadOnly";
    }

    // === 要素検索 ===

    public class FindElementsRequest : TypedWorkerRequest
    {
        public override string Operation => "FindElements";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }

        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }

        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "descendants"; // "descendants", "children", "subtree"

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;

        [JsonPropertyName("useCache")]
        public bool UseCache { get; set; } = true;

        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; set; } = false;

        [JsonPropertyName("useWildcard")]
        public bool UseWildcard { get; set; } = false;
    }

    public class FindElementsByControlTypeRequest : TypedWorkerRequest
    {
        public override string Operation => "FindElementsByControlType";

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "descendants";

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class FindElementsByPatternRequest : TypedWorkerRequest
    {
        public override string Operation => "FindElementsByPattern";

        [JsonPropertyName("patternName")]
        public string PatternName { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "descendants";

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }

    // === ウィンドウ操作 ===

    public class GetDesktopWindowsRequest : TypedWorkerRequest
    {
        public override string Operation => "GetDesktopWindows";

        [JsonPropertyName("includeInvisible")]
        public bool IncludeInvisible { get; set; } = false;
    }

    public class WindowActionRequest : TypedWorkerRequest
    {
        public override string Operation => "WindowAction";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "close", "minimize", "maximize", "restore", "setfocus"
    }

    public class GetWindowInfoRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInfo";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    public class GetWindowInteractionStateRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInteractionState";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    public class GetWindowCapabilitiesRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowCapabilities";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    // === Range操作 ===

    public class SetRangeValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetRangeValue";

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public class GetRangeValueRequest : ElementTargetRequest
    {
        public override string Operation => "GetRangeValue";
    }

    public class GetRangePropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetRangeProperties";
    }

    // === Text操作 ===

    public class SetTextRequest : ElementTargetRequest
    {
        public override string Operation => "SetText";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }

    public class GetTextRequest : ElementTargetRequest
    {
        public override string Operation => "GetText";
    }

    public class FindTextRequest : ElementTargetRequest
    {
        public override string Operation => "FindText";

        [JsonPropertyName("searchText")]
        public string SearchText { get; set; } = "";

        [JsonPropertyName("backward")]
        public bool Backward { get; set; } = false;

        [JsonPropertyName("ignoreCase")]
        public bool IgnoreCase { get; set; } = false;
    }

    public class SelectTextRequest : ElementTargetRequest
    {
        public override string Operation => "SelectText";

        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    public class TraverseTextRequest : ElementTargetRequest
    {
        public override string Operation => "TraverseText";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "character"; // "character", "word", "line", "paragraph" with optional "-back"

        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;
    }

    // === Transform操作 ===

    public class MoveElementRequest : ElementTargetRequest
    {
        public override string Operation => "MoveElement";

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class ResizeElementRequest : ElementTargetRequest
    {
        public override string Operation => "ResizeElement";

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }
    }

    public class RotateElementRequest : ElementTargetRequest
    {
        public override string Operation => "RotateElement";

        [JsonPropertyName("degrees")]
        public double Degrees { get; set; }
    }

    // === Wait操作 ===

    public class WaitForInputIdleRequest : TypedWorkerRequest
    {
        public override string Operation => "WaitForInputIdle";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("timeoutMilliseconds")]
        public int TimeoutMilliseconds { get; set; } = 10000;
    }
}