using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// Type-safe action parameters for UI automation operations
    /// </summary>
    [MessagePackObject]
    public class ActionParameters
    {
        [Key(0)]
        [JsonPropertyName("targetValue")]
        public object? TargetValue { get; set; }
        
        [Key(1)]
        [JsonPropertyName("position")]
        public Point? Position { get; set; }
        
        [Key(2)]
        [JsonPropertyName("bounds")]
        public BoundingRectangle? Bounds { get; set; }
        
        [Key(3)]
        [JsonPropertyName("scrollAmount")]
        public double? ScrollAmount { get; set; }
        
        [Key(4)]
        [JsonPropertyName("scrollDirection")]
        public string? ScrollDirection { get; set; }
        
        [Key(5)]
        [JsonPropertyName("dockPosition")]
        public string? DockPosition { get; set; }
        
        [Key(6)]
        [JsonPropertyName("windowState")]
        public string? WindowState { get; set; }
        
        [Key(7)]
        [JsonPropertyName("toggleState")]
        public string? ToggleState { get; set; }
        
        [Key(8)]
        [JsonPropertyName("selectionMode")]
        public string? SelectionMode { get; set; }
        
        [Key(9)]
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
        
        [Key(10)]
        [JsonPropertyName("rangeValue")]
        public double? RangeValue { get; set; }
        
        [Key(11)]
        [JsonPropertyName("timeout")]
        public int? Timeout { get; set; }
        
        [Key(12)]
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
    [MessagePackObject]
    public class ElementState
    {
        [Key(0)]
        [JsonPropertyName("isEnabled")]
        public bool? IsEnabled { get; set; }
        
        [Key(1)]
        [JsonPropertyName("isVisible")]
        public bool? IsVisible { get; set; }
        
        [Key(2)]
        [JsonPropertyName("hasFocus")]
        public bool? HasFocus { get; set; }
        
        [Key(3)]
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [Key(4)]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [Key(5)]
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [Key(6)]
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [Key(7)]
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle? BoundingRectangle { get; set; }
        
        [Key(8)]
        [JsonPropertyName("toggleState")]
        public ToggleStateInfo? ToggleState { get; set; }
        
        [Key(9)]
        [JsonPropertyName("windowState")]
        public WindowStateInfo? WindowState { get; set; }
        
        [Key(10)]
        [JsonPropertyName("selectionState")]
        public SelectionState? SelectionState { get; set; }
        
        [Key(11)]
        [JsonPropertyName("scrollState")]
        public ScrollState? ScrollState { get; set; }
        
        [Key(12)]
        [JsonPropertyName("rangeState")]
        public RangeState? RangeState { get; set; }
        
        [Key(13)]
        [JsonPropertyName("isReadOnly")]
        public bool? IsReadOnly { get; set; }
        
        [Key(14)]
        [JsonPropertyName("isSelected")]
        public bool? IsSelected { get; set; }
        
        [Key(15)]
        [JsonPropertyName("expandCollapseState")]
        public ExpandCollapseStateInfo? ExpandCollapseState { get; set; }
        
        [Key(16)]
        [JsonPropertyName("dockPosition")]
        public DockPositionInfo? DockPosition { get; set; }
        
        [Key(17)]
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
    [MessagePackObject]
    public class SelectionState
    {
        [Key(0)]
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
        
        [Key(1)]
        [JsonPropertyName("selectionCount")]
        public int SelectionCount { get; set; }
        
        [Key(2)]
        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }
        
        [Key(3)]
        [JsonPropertyName("isSelectionRequired")]
        public bool IsSelectionRequired { get; set; }
    }

    /// <summary>
    /// Scroll state information
    /// </summary>
    [MessagePackObject]
    public class ScrollState
    {
        [Key(0)]
        [JsonPropertyName("horizontalPercent")]
        public double HorizontalPercent { get; set; }
        
        [Key(1)]
        [JsonPropertyName("verticalPercent")]
        public double VerticalPercent { get; set; }
        
        [Key(2)]
        [JsonPropertyName("horizontalViewSize")]
        public double HorizontalViewSize { get; set; }
        
        [Key(3)]
        [JsonPropertyName("verticalViewSize")]
        public double VerticalViewSize { get; set; }
        
        [Key(4)]
        [JsonPropertyName("horizontallyScrollable")]
        public bool HorizontallyScrollable { get; set; }
        
        [Key(5)]
        [JsonPropertyName("verticallyScrollable")]
        public bool VerticallyScrollable { get; set; }
    }

    /// <summary>
    /// Range value state information
    /// </summary>
    [MessagePackObject]
    public class RangeState
    {
        [Key(0)]
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [Key(1)]
        [JsonPropertyName("minimum")]
        public double Minimum { get; set; }
        
        [Key(2)]
        [JsonPropertyName("maximum")]
        public double Maximum { get; set; }
        
        [Key(3)]
        [JsonPropertyName("smallChange")]
        public double SmallChange { get; set; }
        
        [Key(4)]
        [JsonPropertyName("largeChange")]
        public double LargeChange { get; set; }
        
        [Key(5)]
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Window state information
    /// </summary>
    [MessagePackObject]
    public class WindowStateInfo
    {
        [Key(0)]
        [JsonPropertyName("visualState")]
        public string VisualState { get; set; } = string.Empty; // Normal, Maximized, Minimized
        
        [Key(1)]
        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = string.Empty; // ReadyForUserInteraction, Blocked, NotResponding, Running, Closing
        
        [Key(2)]
        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }
        
        [Key(3)]
        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }
        
        [Key(4)]
        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }
        
        [Key(5)]
        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }
        
        [Key(6)]
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [Key(7)]
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
    [MessagePackObject]
    public class ToggleStateInfo
    {
        [Key(0)]
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty; // Off, On, Indeterminate
        
        [Key(1)]
        [JsonPropertyName("isIndeterminate")]
        public bool IsIndeterminate { get; set; }
        
        [Key(2)]
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
    [MessagePackObject]
    public class ExpandCollapseStateInfo
    {
        [Key(0)]
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty; // Collapsed, Expanded, PartiallyExpanded, LeafNode
        
        [Key(1)]
        [JsonPropertyName("canExpand")]
        public bool CanExpand { get; set; }
        
        [Key(2)]
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
    [MessagePackObject]
    public class DockPositionInfo
    {
        [Key(0)]
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty; // Top, Left, Bottom, Right, Fill, None
        
        [Key(1)]
        [JsonPropertyName("supportedPositions")]
        public List<string> SupportedPositions { get; set; } = new List<string>();
        
        public override string ToString()
        {
            return Position;
        }
    }
}