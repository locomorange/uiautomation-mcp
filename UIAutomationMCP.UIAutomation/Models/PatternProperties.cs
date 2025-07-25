using System.Text.Json.Serialization;

namespace UIAutomationMCP.UIAutomation.Models
{
    /// <summary>
    /// Base class for all pattern properties
    /// </summary>
    [JsonDerivedType(typeof(ValuePatternProperties), "Value")]
    [JsonDerivedType(typeof(RangeValuePatternProperties), "RangeValue")]
    [JsonDerivedType(typeof(TogglePatternProperties), "Toggle")]
    [JsonDerivedType(typeof(SelectionPatternProperties), "Selection")]
    [JsonDerivedType(typeof(GridPatternProperties), "Grid")]
    [JsonDerivedType(typeof(WindowPatternProperties), "Window")]
    [JsonDerivedType(typeof(TransformPatternProperties), "Transform")]
    public abstract class PatternProperties
    {
        public string PatternType { get; protected set; } = string.Empty;
    }

    /// <summary>
    /// Value pattern properties
    /// </summary>
    public class ValuePatternProperties : PatternProperties
    {
        public string Value { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }

        public ValuePatternProperties()
        {
            PatternType = "Value";
        }
    }

    /// <summary>
    /// RangeValue pattern properties
    /// </summary>
    public class RangeValuePatternProperties : PatternProperties
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public bool IsReadOnly { get; set; }

        public RangeValuePatternProperties()
        {
            PatternType = "RangeValue";
        }
    }

    /// <summary>
    /// Toggle pattern properties
    /// </summary>
    public class TogglePatternProperties : PatternProperties
    {
        public string ToggleState { get; set; } = string.Empty;

        public TogglePatternProperties()
        {
            PatternType = "Toggle";
        }
    }

    /// <summary>
    /// Selection pattern properties
    /// </summary>
    public class SelectionPatternProperties : PatternProperties
    {
        public bool CanSelectMultiple { get; set; }
        public bool IsSelectionRequired { get; set; }
        public int SelectionCount { get; set; }

        public SelectionPatternProperties()
        {
            PatternType = "Selection";
        }
    }

    /// <summary>
    /// Grid pattern properties
    /// </summary>
    public class GridPatternProperties : PatternProperties
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }

        public GridPatternProperties()
        {
            PatternType = "Grid";
        }
    }

    /// <summary>
    /// Window pattern properties
    /// </summary>
    public class WindowPatternProperties : PatternProperties
    {
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
        public string WindowVisualState { get; set; } = string.Empty;
        public string WindowInteractionState { get; set; } = string.Empty;

        public WindowPatternProperties()
        {
            PatternType = "Window";
        }
    }

    /// <summary>
    /// Transform pattern properties
    /// </summary>
    public class TransformPatternProperties : PatternProperties
    {
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }

        public TransformPatternProperties()
        {
            PatternType = "Transform";
        }
    }

    /// <summary>
    /// Type-safe pattern information
    /// </summary>
    public class TypedPatternInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSupported { get; set; }
        public PatternProperties? Properties { get; set; }
    }
}