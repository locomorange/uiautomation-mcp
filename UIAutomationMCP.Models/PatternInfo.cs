using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// 軽量なUI Automationパターン情報クラス群
    /// 重複メタデータを排除し、パターン固有データのみを保持
    /// </summary>

    /// <summary>
    /// Toggle Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ToggleInfo
    {
        [Key(0)]
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;  // "On", "Off", "Indeterminate"
        
        [Key(1)]
        [JsonPropertyName("isToggled")]
        public bool IsToggled { get; set; }
        
        [Key(2)]
        [JsonPropertyName("canToggle")]
        public bool CanToggle { get; set; }
    }

    /// <summary>
    /// Range Value Pattern情報
    /// </summary>
    [MessagePackObject]
    public class RangeInfo
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
    /// Window Pattern情報
    /// </summary>
    [MessagePackObject]
    public class WindowPatternInfo
    {
        [Key(0)]
        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }
        
        [Key(1)]
        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }
        
        [Key(2)]
        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }
        
        [Key(3)]
        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }
        
        [Key(4)]
        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = string.Empty;
        
        [Key(5)]
        [JsonPropertyName("visualState")]
        public string VisualState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Selection Pattern情報
    /// </summary>
    [MessagePackObject]
    public class SelectionInfo
    {
        [Key(0)]
        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }
        
        [Key(1)]
        [JsonPropertyName("isSelectionRequired")]
        public bool IsSelectionRequired { get; set; }
        
        [Key(2)]
        [JsonPropertyName("selectedCount")]
        public int SelectedCount { get; set; }
        
        [Key(3)]
        [JsonPropertyName("selectedItems")]
        public List<SelectionItemInfo> SelectedItems { get; set; } = new();
    }

    /// <summary>
    /// Selection Item情報
    /// </summary>
    [MessagePackObject]
    public class SelectionItemInfo
    {
        [Key(0)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;
        
        [Key(1)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [Key(2)]
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;
        
        [Key(3)]
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
        
        [Key(4)]
        [JsonPropertyName("selectionContainer")]
        public string? SelectionContainer { get; set; }
    }

    /// <summary>
    /// Grid Pattern情報
    /// </summary>
    [MessagePackObject]
    public class GridInfo
    {
        [Key(0)]
        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }
        
        [Key(1)]
        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }
        
        [Key(2)]
        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }
        
        [Key(3)]
        [JsonPropertyName("hasRowHeaders")]
        public bool HasRowHeaders { get; set; }
        
        [Key(4)]
        [JsonPropertyName("hasColumnHeaders")]
        public bool HasColumnHeaders { get; set; }
        
        [Key(5)]
        [JsonPropertyName("selectedItems")]
        public List<GridCellReference> SelectedItems { get; set; } = new();
    }

    /// <summary>
    /// Scroll Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ScrollInfo
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
    /// Text Pattern情報
    /// </summary>
    [MessagePackObject]
    public class TextInfo
    {
        [Key(0)]
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        
        [Key(1)]
        [JsonPropertyName("length")]
        public int Length { get; set; }
        
        [Key(2)]
        [JsonPropertyName("selectedText")]
        public string SelectedText { get; set; } = string.Empty;
        
        [Key(3)]
        [JsonPropertyName("hasSelection")]
        public bool HasSelection { get; set; }
    }

    /// <summary>
    /// Transform Pattern情報
    /// </summary>
    [MessagePackObject]
    public class TransformInfo
    {
        [Key(0)]
        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }
        
        [Key(1)]
        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }
        
        [Key(2)]
        [JsonPropertyName("canRotate")]
        public bool CanRotate { get; set; }
        
        [Key(3)]
        [JsonPropertyName("currentX")]
        public double CurrentX { get; set; }
        
        [Key(4)]
        [JsonPropertyName("currentY")]
        public double CurrentY { get; set; }
        
        [Key(5)]
        [JsonPropertyName("currentWidth")]
        public double CurrentWidth { get; set; }
        
        [Key(6)]
        [JsonPropertyName("currentHeight")]
        public double CurrentHeight { get; set; }
    }

    /// <summary>
    /// Value Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ValueInfo
    {
        [Key(0)]
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        
        [Key(1)]
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// ExpandCollapse Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ExpandCollapseInfo
    {
        [Key(0)]
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;  // "Expanded", "Collapsed", "PartiallyExpanded", "LeafNode"
    }

    /// <summary>
    /// Dock Pattern情報
    /// </summary>
    [MessagePackObject]
    public class DockInfo
    {
        [Key(0)]
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;  // "Top", "Left", "Bottom", "Right", "Fill", "None"
    }

    /// <summary>
    /// MultipleView Pattern情報
    /// </summary>
    [MessagePackObject]
    public class MultipleViewInfo
    {
        [Key(0)]
        [JsonPropertyName("currentView")]
        public int CurrentView { get; set; }
        
        [Key(1)]
        [JsonPropertyName("availableViews")]
        public List<PatternViewInfo> AvailableViews { get; set; } = new();
        
        [Key(2)]
        [JsonPropertyName("supportedViewCount")]
        public int SupportedViewCount { get; set; }
        
        [Key(3)]
        [JsonPropertyName("viewChangedEventSupported")]
        public bool ViewChangedEventSupported { get; set; }
    }

    /// <summary>
    /// View情報
    /// </summary>
    [MessagePackObject]
    public class PatternViewInfo
    {
        [Key(0)]
        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }
        
        [Key(1)]
        [JsonPropertyName("viewName")]
        public string ViewName { get; set; } = string.Empty;
    }

    /// <summary>
    /// GridItem Pattern情報
    /// </summary>
    [MessagePackObject]
    public class GridItemInfo
    {
        [Key(0)]
        [JsonPropertyName("row")]
        public int Row { get; set; }
        
        [Key(1)]
        [JsonPropertyName("column")]
        public int Column { get; set; }
        
        [Key(2)]
        [JsonPropertyName("rowSpan")]
        public int RowSpan { get; set; }
        
        [Key(3)]
        [JsonPropertyName("columnSpan")]
        public int ColumnSpan { get; set; }
        
        [Key(4)]
        [JsonPropertyName("containingGrid")]
        public string? ContainingGrid { get; set; }
    }

    /// <summary>
    /// TableItem Pattern情報
    /// </summary>
    [MessagePackObject]
    public class TableItemInfo
    {
        [Key(0)]
        [JsonPropertyName("columnHeaders")]
        public List<ElementInfo> ColumnHeaders { get; set; } = new();
        
        [Key(1)]
        [JsonPropertyName("rowHeaders")]
        public List<ElementInfo> RowHeaders { get; set; } = new();
    }


    /// <summary>
    /// Table Pattern情報
    /// </summary>
    [MessagePackObject]
    public class TableInfo
    {
        [Key(0)]
        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }
        
        [Key(1)]
        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }
        
        [Key(2)]
        [JsonPropertyName("rowOrColumnMajor")]
        public string RowOrColumnMajor { get; set; } = string.Empty;
        
        [Key(3)]
        [JsonPropertyName("columnHeaders")]
        public List<ElementInfo> ColumnHeaders { get; set; } = new();
        
        [Key(4)]
        [JsonPropertyName("rowHeaders")]
        public List<ElementInfo> RowHeaders { get; set; } = new();
        
        [Key(5)]
        [JsonPropertyName("primaryRowHeaderIndex")]
        public int PrimaryRowHeaderIndex { get; set; } = -1;
        
        [Key(6)]
        [JsonPropertyName("primaryColumnHeaderIndex")]
        public int PrimaryColumnHeaderIndex { get; set; } = -1;
        
        [Key(7)]
        [JsonPropertyName("selectedCells")]
        public List<GridCellReference> SelectedCells { get; set; } = new();
    }

    /// <summary>
    /// Invoke Pattern情報
    /// </summary>
    [MessagePackObject]
    public class InvokeInfo
    {
        [Key(0)]
        [JsonPropertyName("isInvokable")]
        public bool IsInvokable { get; set; }
    }

    /// <summary>
    /// ScrollItem Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ScrollItemInfo
    {
        [Key(0)]
        [JsonPropertyName("isScrollable")]
        public bool IsScrollable { get; set; }
    }

    /// <summary>
    /// VirtualizedItem Pattern情報
    /// </summary>
    [MessagePackObject]
    public class VirtualizedItemInfo
    {
        [Key(0)]
        [JsonPropertyName("isVirtualized")]
        public bool IsVirtualized { get; set; }
    }

    /// <summary>
    /// ItemContainer Pattern情報
    /// </summary>
    [MessagePackObject]
    public class ItemContainerInfo
    {
        [Key(0)]
        [JsonPropertyName("isItemContainer")]
        public bool IsItemContainer { get; set; }
    }

    /// <summary>
    /// SynchronizedInput Pattern情報
    /// </summary>
    [MessagePackObject]
    public class SynchronizedInputInfo
    {
        [Key(0)]
        [JsonPropertyName("supportsSynchronizedInput")]
        public bool SupportsSynchronizedInput { get; set; }
    }

    /// <summary>
    /// Accessibility関連情報
    /// </summary>
    [MessagePackObject]
    public class AccessibilityInfo
    {
        [Key(0)]
        [JsonPropertyName("labeledBy")]
        public ElementReference? LabeledBy { get; set; }
        
        [Key(1)]
        [JsonPropertyName("helpText")]
        public string? HelpText { get; set; }
        
        [Key(2)]
        [JsonPropertyName("accessKey")]
        public string? AccessKey { get; set; }
        
        [Key(3)]
        [JsonPropertyName("acceleratorKey")]
        public string? AcceleratorKey { get; set; }
    }

    /// <summary>
    /// 要素参照情報
    /// </summary>
    [MessagePackObject]
    public class ElementReference
    {
        [Key(0)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;
        
        [Key(1)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [Key(2)]
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Grid セル参照情報
    /// </summary>
    [MessagePackObject]
    public class GridCellReference
    {
        [Key(0)]
        [JsonPropertyName("row")]
        public int Row { get; set; }
        
        [Key(1)]
        [JsonPropertyName("column")]
        public int Column { get; set; }
        
        [Key(2)]
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;
        
        [Key(3)]
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}