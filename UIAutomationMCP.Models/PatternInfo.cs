using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// 軽量なUI Automationパターン情報クラス群
    /// 重複メタデータを排除し、パターン固有データのみを保持
    /// </summary>

    /// <summary>
    /// Toggle Pattern情報
    /// </summary>
    public class ToggleInfo
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;  // "On", "Off", "Indeterminate"

        [JsonPropertyName("isToggled")]
        public bool IsToggled { get; set; }

        [JsonPropertyName("canToggle")]
        public bool CanToggle { get; set; }
    }

    /// <summary>
    /// Range Value Pattern情報
    /// </summary>
    public class RangeInfo
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
    /// Window Pattern情報
    /// </summary>
    public class WindowPatternInfo
    {
        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }

        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }

        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = string.Empty;

        [JsonPropertyName("visualState")]
        public string VisualState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Selection Pattern情報
    /// </summary>
    public class SelectionInfo
    {
        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }

        [JsonPropertyName("isSelectionRequired")]
        public bool IsSelectionRequired { get; set; }

        [JsonPropertyName("selectedCount")]
        public int SelectedCount { get; set; }

        [JsonPropertyName("selectedItems")]
        public List<SelectionItemInfo> SelectedItems { get; set; } = new();
    }

    /// <summary>
    /// Selection Item情報
    /// </summary>
    public class SelectionItemInfo
    {
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }

        [JsonPropertyName("selectionContainer")]
        public string? SelectionContainer { get; set; }
    }

    /// <summary>
    /// Grid Pattern情報
    /// </summary>
    public class GridInfo
    {
        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }

        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }

        [JsonPropertyName("hasRowHeaders")]
        public bool HasRowHeaders { get; set; }

        [JsonPropertyName("hasColumnHeaders")]
        public bool HasColumnHeaders { get; set; }

        [JsonPropertyName("selectedItems")]
        public List<GridCellReference> SelectedItems { get; set; } = new();
    }

    /// <summary>
    /// Scroll Pattern情報
    /// </summary>
    public class ScrollInfo
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
    /// Text Pattern情報
    /// </summary>
    public class TextInfo
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("selectedText")]
        public string SelectedText { get; set; } = string.Empty;

        [JsonPropertyName("hasSelection")]
        public bool HasSelection { get; set; }
    }

    /// <summary>
    /// Transform Pattern情報
    /// </summary>
    public class TransformInfo
    {
        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }

        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }

        [JsonPropertyName("canRotate")]
        public bool CanRotate { get; set; }

        [JsonPropertyName("currentX")]
        public double CurrentX { get; set; }

        [JsonPropertyName("currentY")]
        public double CurrentY { get; set; }

        [JsonPropertyName("currentWidth")]
        public double CurrentWidth { get; set; }

        [JsonPropertyName("currentHeight")]
        public double CurrentHeight { get; set; }
    }

    /// <summary>
    /// Value Pattern情報
    /// </summary>
    public class ValueInfo
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// ExpandCollapse Pattern情報
    /// </summary>
    public class ExpandCollapseInfo
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;  // "Expanded", "Collapsed", "PartiallyExpanded", "LeafNode"
    }

    /// <summary>
    /// Dock Pattern情報
    /// </summary>
    public class DockInfo
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;  // "Top", "Left", "Bottom", "Right", "Fill", "None"
    }

    /// <summary>
    /// MultipleView Pattern情報
    /// </summary>
    public class MultipleViewInfo
    {
        [JsonPropertyName("currentView")]
        public int CurrentView { get; set; }

        [JsonPropertyName("availableViews")]
        public List<PatternViewInfo> AvailableViews { get; set; } = new();

        [JsonPropertyName("supportedViewCount")]
        public int SupportedViewCount { get; set; }

        [JsonPropertyName("viewChangedEventSupported")]
        public bool ViewChangedEventSupported { get; set; }
    }

    /// <summary>
    /// View情報
    /// </summary>
    public class PatternViewInfo
    {
        [JsonPropertyName("viewId")]
        public int ViewId { get; set; }

        [JsonPropertyName("viewName")]
        public string ViewName { get; set; } = string.Empty;
    }

    /// <summary>
    /// GridItem Pattern情報
    /// </summary>
    public class GridItemInfo
    {
        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }

        [JsonPropertyName("rowSpan")]
        public int RowSpan { get; set; }

        [JsonPropertyName("columnSpan")]
        public int ColumnSpan { get; set; }

        [JsonPropertyName("containingGrid")]
        public string? ContainingGrid { get; set; }
    }

    /// <summary>
    /// TableItem Pattern情報
    /// </summary>
    public class TableItemInfo
    {
        [JsonPropertyName("columnHeaders")]
        public List<ElementInfo> ColumnHeaders { get; set; } = new();

        [JsonPropertyName("rowHeaders")]
        public List<ElementInfo> RowHeaders { get; set; } = new();
    }


    /// <summary>
    /// Table Pattern情報
    /// </summary>
    public class TableInfo
    {
        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("columnCount")]
        public int ColumnCount { get; set; }

        [JsonPropertyName("rowOrColumnMajor")]
        public string RowOrColumnMajor { get; set; } = string.Empty;

        [JsonPropertyName("columnHeaders")]
        public List<ElementInfo> ColumnHeaders { get; set; } = new();

        [JsonPropertyName("rowHeaders")]
        public List<ElementInfo> RowHeaders { get; set; } = new();

        [JsonPropertyName("primaryRowHeaderIndex")]
        public int PrimaryRowHeaderIndex { get; set; } = -1;

        [JsonPropertyName("primaryColumnHeaderIndex")]
        public int PrimaryColumnHeaderIndex { get; set; } = -1;

        [JsonPropertyName("selectedCells")]
        public List<GridCellReference> SelectedCells { get; set; } = new();
    }

    /// <summary>
    /// Invoke Pattern情報
    /// </summary>
    public class InvokeInfo
    {
        [JsonPropertyName("isInvokable")]
        public bool IsInvokable { get; set; }
    }

    /// <summary>
    /// ScrollItem Pattern情報
    /// </summary>
    public class ScrollItemInfo
    {
        [JsonPropertyName("isScrollable")]
        public bool IsScrollable { get; set; }
    }

    /// <summary>
    /// VirtualizedItem Pattern情報
    /// </summary>
    public class VirtualizedItemInfo
    {
        [JsonPropertyName("isVirtualized")]
        public bool IsVirtualized { get; set; }
    }

    /// <summary>
    /// ItemContainer Pattern情報
    /// </summary>
    public class ItemContainerInfo
    {
        [JsonPropertyName("isItemContainer")]
        public bool IsItemContainer { get; set; }
    }

    /// <summary>
    /// SynchronizedInput Pattern情報
    /// </summary>
    public class SynchronizedInputInfo
    {
        [JsonPropertyName("supportsSynchronizedInput")]
        public bool SupportsSynchronizedInput { get; set; }
    }

    /// <summary>
    /// Accessibility関連情報
    /// </summary>
    public class AccessibilityInfo
    {
        [JsonPropertyName("labeledBy")]
        public ElementReference? LabeledBy { get; set; }

        [JsonPropertyName("helpText")]
        public string? HelpText { get; set; }

        [JsonPropertyName("accessKey")]
        public string? AccessKey { get; set; }

        [JsonPropertyName("acceleratorKey")]
        public string? AcceleratorKey { get; set; }
    }

    /// <summary>
    /// 要素参照情報
    /// </summary>
    public class ElementReference
    {
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Grid セル参照情報
    /// </summary>
    public class GridCellReference
    {
        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }

        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
