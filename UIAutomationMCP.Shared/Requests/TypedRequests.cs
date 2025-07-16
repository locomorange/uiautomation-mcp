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

    public class TransformElementRequest : ElementTargetRequest
    {
        public override string Operation => "TransformElement";

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "move", "resize", "rotate"

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }
    }

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

    // === Grid操作 ===

    public class GetGridInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetGridInfo";
    }

    public class GetGridItemRequest : ElementTargetRequest
    {
        public override string Operation => "GetGridItem";

        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    public class GetColumnHeaderRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeader";

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    public class GetRowHeaderRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeader";

        [JsonPropertyName("row")]
        public int Row { get; set; }
    }

    // === Layout操作 ===

    public class DockElementRequest : ElementTargetRequest
    {
        public override string Operation => "DockElement";

        [JsonPropertyName("dockPosition")]
        public string DockPosition { get; set; } = ""; // "top", "bottom", "left", "right", "fill", "none"
    }

    public class RealizeVirtualizedItemRequest : ElementTargetRequest
    {
        public override string Operation => "RealizeVirtualizedItem";
    }

    // === LegacyIAccessible Pattern ===

    public class GetLegacyPropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetLegacyProperties";
    }

    public class DoLegacyDefaultActionRequest : ElementTargetRequest
    {
        public override string Operation => "DoLegacyDefaultAction";
    }

    public class SelectLegacyItemRequest : ElementTargetRequest
    {
        public override string Operation => "SelectLegacyItem";

        [JsonPropertyName("flagsSelect")]
        public int FlagsSelect { get; set; }
    }

    public class SetLegacyValueRequest : ElementTargetRequest
    {
        public override string Operation => "SetLegacyValue";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }

    public class GetLegacyStateRequest : ElementTargetRequest
    {
        public override string Operation => "GetLegacyState";
    }

    // === Annotation Pattern ===

    public class GetAnnotationInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetAnnotationInfo";
    }

    public class GetAnnotationTargetRequest : ElementTargetRequest
    {
        public override string Operation => "GetAnnotationTarget";
    }

    // === ItemContainer Pattern ===

    public class FindItemByPropertyRequest : TypedWorkerRequest
    {
        public override string Operation => "FindItemByProperty";

        [JsonPropertyName("containerId")]
        public string ContainerId { get; set; } = "";

        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        [JsonPropertyName("startAfterId")]
        public string StartAfterId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    public class ExpandCollapseElementRequest : ElementTargetRequest
    {
        public override string Operation => "ExpandCollapseElement";

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "expand", "collapse", "toggle"
    }

    public class GetScrollInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetScrollInfo";
    }

    public class ScrollElementIntoViewRequest : ElementTargetRequest
    {
        public override string Operation => "ScrollElementIntoView";
    }

    public class ScrollElementRequest : ElementTargetRequest
    {
        public override string Operation => "ScrollElement";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = ""; // "up", "down", "left", "right", "pageup", "pagedown", "pageleft", "pageright"

        [JsonPropertyName("amount")]
        public double Amount { get; set; } = 1.0;
    }

    public class SetScrollPercentRequest : ElementTargetRequest
    {
        public override string Operation => "SetScrollPercent";

        [JsonPropertyName("horizontalPercent")]
        public double HorizontalPercent { get; set; } = -1; // -1 = no change, 0-100 = percentage

        [JsonPropertyName("verticalPercent")]
        public double VerticalPercent { get; set; } = -1; // -1 = no change, 0-100 = percentage
    }

    // === MultipleView操作 ===

    public class GetAvailableViewsRequest : ElementTargetRequest
    {
        public override string Operation => "GetAvailableViews";
    }

    public class GetCurrentViewRequest : ElementTargetRequest
    {
        public override string Operation => "GetCurrentView";
    }

    public class GetViewNameRequest : ElementTargetRequest
    {
        public override string Operation => "GetViewName";

        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
    }

    public class SetViewRequest : ElementTargetRequest
    {
        public override string Operation => "SetView";

        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
    }

    // === Table操作 ===

    public class GetColumnHeaderItemsRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeaderItems";
    }

    public class GetColumnHeadersRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeaders";
    }

    public class GetRowHeaderItemsRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeaderItems";
    }

    public class GetRowHeadersRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeaders";
    }

    public class GetRowOrColumnMajorRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowOrColumnMajor";
    }

    public class GetTableInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetTableInfo";
    }

    // === Selection操作 ===

    public class AddToSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "AddToSelection";
    }

    public class CanSelectMultipleRequest : ElementTargetRequest
    {
        public override string Operation => "CanSelectMultiple";
    }

    public class ClearSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "ClearSelection";
    }

    public class GetSelectionContainerRequest : ElementTargetRequest
    {
        public override string Operation => "GetSelectionContainer";
    }

    public class GetSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "GetSelection";
    }

    public class IsSelectedRequest : ElementTargetRequest
    {
        public override string Operation => "IsSelected";
    }

    public class IsSelectionRequiredRequest : ElementTargetRequest
    {
        public override string Operation => "IsSelectionRequired";
    }

    public class RemoveFromSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "RemoveFromSelection";
    }

    public class SelectElementRequest : ElementTargetRequest
    {
        public override string Operation => "SelectElement";
    }

    public class SelectItemRequest : ElementTargetRequest
    {
        public override string Operation => "SelectItem";
    }

    // === ElementInspection操作 ===

    public class GetElementPropertiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementProperties";
    }

    public class GetElementPatternsRequest : ElementTargetRequest
    {
        public override string Operation => "GetElementPatterns";
    }

    // === TreeNavigation操作 ===

    public class GetAncestorsRequest : ElementTargetRequest
    {
        public override string Operation => "GetAncestors";
    }

    public class GetChildrenRequest : ElementTargetRequest
    {
        public override string Operation => "GetChildren";
    }

    public class GetDescendantsRequest : ElementTargetRequest
    {
        public override string Operation => "GetDescendants";
    }

    public class GetElementTreeRequest : TypedWorkerRequest
    {
        public override string Operation => "GetElementTree";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; } = 3;
    }

    public class GetParentRequest : ElementTargetRequest
    {
        public override string Operation => "GetParent";
    }

    public class GetSiblingsRequest : ElementTargetRequest
    {
        public override string Operation => "GetSiblings";
    }
}