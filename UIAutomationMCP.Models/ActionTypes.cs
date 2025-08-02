using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// Type-safe action parameters for UI automation operations
    /// </summary>
    public class ActionParameters
    {
        [JsonPropertyName("targetValue")]
        public object? TargetValue { get; set; }
        
        [JsonPropertyName("position")]
        public Point? Position { get; set; }
        
        [JsonPropertyName("bounds")]
        public BoundingRectangle? Bounds { get; set; }
        
        [JsonPropertyName("scrollAmount")]
        public double? ScrollAmount { get; set; }
        
        [JsonPropertyName("scrollDirection")]
        public string? ScrollDirection { get; set; }
        
        [JsonPropertyName("dockPosition")]
        public string? DockPosition { get; set; }
        
        [JsonPropertyName("windowState")]
        public string? WindowState { get; set; }
        
        [JsonPropertyName("toggleState")]
        public string? ToggleState { get; set; }
        
        [JsonPropertyName("selectionMode")]
        public string? SelectionMode { get; set; }
        
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
        
        [JsonPropertyName("rangeValue")]
        public double? RangeValue { get; set; }
        
        [JsonPropertyName("timeout")]
        public int? Timeout { get; set; }
        
        [JsonPropertyName("additionalProperties")]
        public Dictionary<string, object>? AdditionalProperties { get; set; }
        
        public override string ToString()
        {
            var properties = new List<string>();
            
            if (TargetValue != null) properties.Add($"TargetValue: {TargetValue}");
            if (Position != null) properties.Add($"Position: {Position}");
            if (Bounds != null) properties.Add($"Bounds: {Bounds}");
            if (ScrollAmount.HasValue) properties.Add($"ScrollAmount: {ScrollAmount}");
            if (ScrollDirection != null) properties.Add($"ScrollDirection: {ScrollDirection}");
            if (DockPosition != null) properties.Add($"DockPosition: {DockPosition}");
            if (WindowState != null) properties.Add($"WindowState: {WindowState}");
            if (ToggleState != null) properties.Add($"ToggleState: {ToggleState}");
            if (SelectionMode != null) properties.Add($"SelectionMode: {SelectionMode}");
            if (SearchText != null) properties.Add($"SearchText: {SearchText}");
            if (RangeValue.HasValue) properties.Add($"RangeValue: {RangeValue}");
            if (Timeout.HasValue) properties.Add($"Timeout: {Timeout}");
            
            return properties.Count > 0 ? string.Join(", ", properties) : "No parameters";
        }
    }

    /// <summary>
    /// Type-safe element state representation
    /// </summary>
    public class ElementState
    {
        [JsonPropertyName("isEnabled")]
        public bool? IsEnabled { get; set; }
        
        [JsonPropertyName("isVisible")]
        public bool? IsVisible { get; set; }
        
        [JsonPropertyName("hasFocus")]
        public bool? HasFocus { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle? BoundingRectangle { get; set; }
        
        [JsonPropertyName("toggleState")]
        public ToggleStateInfo? ToggleState { get; set; }
        
        [JsonPropertyName("windowState")]
        public WindowStateInfo? WindowState { get; set; }
        
        [JsonPropertyName("selectionState")]
        public SelectionState? SelectionState { get; set; }
        
        [JsonPropertyName("scrollState")]
        public ScrollState? ScrollState { get; set; }
        
        [JsonPropertyName("rangeState")]
        public RangeState? RangeState { get; set; }
        
        [JsonPropertyName("isReadOnly")]
        public bool? IsReadOnly { get; set; }
        
        [JsonPropertyName("isSelected")]
        public bool? IsSelected { get; set; }
        
        [JsonPropertyName("expandCollapseState")]
        public ExpandCollapseStateInfo? ExpandCollapseState { get; set; }
        
        [JsonPropertyName("dockPosition")]
        public DockPositionInfo? DockPosition { get; set; }
        
        [JsonPropertyName("additionalProperties")]
        public Dictionary<string, object>? AdditionalProperties { get; set; }
        
        public override string ToString()
        {
            var properties = new List<string>();
            
            if (IsEnabled.HasValue) properties.Add($"Enabled: {IsEnabled}");
            if (IsVisible.HasValue) properties.Add($"Visible: {IsVisible}");
            if (HasFocus.HasValue) properties.Add($"Focus: {HasFocus}");
            if (Value != null) properties.Add($"Value: {Value}");
            if (Name != null) properties.Add($"Name: {Name}");
            if (ControlType != null) properties.Add($"Type: {ControlType}");
            if (ToggleState != null) properties.Add($"Toggle: {ToggleState}");
            if (WindowState != null) properties.Add($"Window: {WindowState}");
            if (IsSelected.HasValue) properties.Add($"Selected: {IsSelected}");
            if (ExpandCollapseState != null) properties.Add($"ExpandCollapse: {ExpandCollapseState}");
            if (DockPosition != null) properties.Add($"Dock: {DockPosition}");
            
            return properties.Count > 0 ? string.Join(", ", properties) : "No state data";
        }
    }

    /// <summary>
    /// Selection state information
    /// </summary>
    public class SelectionState
    {
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
        
        [JsonPropertyName("selectionCount")]
        public int SelectionCount { get; set; }
        
        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }
        
        [JsonPropertyName("isSelectionRequired")]
        public bool IsSelectionRequired { get; set; }
    }

    /// <summary>
    /// Scroll state information
    /// </summary>
    public class ScrollState
    {
        [JsonPropertyName("horizontalPercent")]
        public double HorizontalPercent { get; set; }
        
        [JsonPropertyName("verticalPercent")]
        public double VerticalPercent { get; set; }
        
        [JsonPropertyName("horizontalViewSize")]
        public double HorizontalViewSize { get; set; }
        
        [JsonPropertyName("verticalViewSize")]
        public double VerticalViewSize { get; set; }
        
        [JsonPropertyName("horizontallyScrollable")]
        public bool HorizontallyScrollable { get; set; }
        
        [JsonPropertyName("verticallyScrollable")]
        public bool VerticallyScrollable { get; set; }
    }

    /// <summary>
    /// Range value state information
    /// </summary>
    public class RangeState
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [JsonPropertyName("minimum")]
        public double Minimum { get; set; }
        
        [JsonPropertyName("maximum")]
        public double Maximum { get; set; }
        
        [JsonPropertyName("smallChange")]
        public double SmallChange { get; set; }
        
        [JsonPropertyName("largeChange")]
        public double LargeChange { get; set; }
        
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Window state information
    /// </summary>
    public class WindowStateInfo
    {
        [JsonPropertyName("visualState")]
        public string VisualState { get; set; } = string.Empty; // Normal, Maximized, Minimized
        
        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = string.Empty; // ReadyForUserInteraction, Blocked, NotResponding, Running, Closing
        
        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }
        
        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }
        
        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }
        
        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("bounds")]
        public BoundingRectangle? Bounds { get; set; }
        
        public override string ToString()
        {
            return $"Visual: {VisualState}, Interaction: {InteractionState}, Modal: {IsModal}, Topmost: {IsTopmost}";
        }
    }

    /// <summary>
    /// Toggle state information
    /// </summary>
    public class ToggleStateInfo
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty; // Off, On, Indeterminate
        
        [JsonPropertyName("isIndeterminate")]
        public bool IsIndeterminate { get; set; }
        
        [JsonPropertyName("supportedStates")]
        public List<string> SupportedStates { get; set; } = new List<string>();
        
        public override string ToString()
        {
            return State;
        }
    }

    /// <summary>
    /// Expand/Collapse state information
    /// </summary>
    public class ExpandCollapseStateInfo
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty; // Collapsed, Expanded, PartiallyExpanded, LeafNode
        
        [JsonPropertyName("canExpand")]
        public bool CanExpand { get; set; }
        
        [JsonPropertyName("canCollapse")]
        public bool CanCollapse { get; set; }
        
        public override string ToString()
        {
            return State;
        }
    }

    /// <summary>
    /// Dock position information
    /// </summary>
    public class DockPositionInfo
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty; // Top, Left, Bottom, Right, Fill, None
        
        [JsonPropertyName("supportedPositions")]
        public List<string> SupportedPositions { get; set; } = new List<string>();
        
        public override string ToString()
        {
            return Position;
        }
    }
}