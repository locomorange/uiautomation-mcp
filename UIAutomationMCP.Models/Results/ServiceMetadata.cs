using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Base class for all service metadata
    /// </summary>
    public class ServiceMetadata
    {
        public string OperationType { get; set; } = string.Empty;
        public bool OperationCompleted { get; set; }
        public double ExecutionTimeMs { get; set; }
        public string MethodName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Accessibility service metadata
    /// </summary>
    public class AccessibilityServiceMetadata : ServiceMetadata
    {
        public string AccessibilityLevel { get; set; } = string.Empty;
        public int PropertyCount { get; set; }
    }

    /// <summary>
    /// Element search service metadata
    /// </summary>
    public class SearchServiceMetadata : ServiceMetadata
    {
        public int ElementsFound { get; set; }
        public string SearchScope { get; set; } = string.Empty;
        public bool UsedFallback { get; set; }
    }

    /// <summary>
    /// Custom property service metadata
    /// </summary>
    public class CustomPropertyServiceMetadata : ServiceMetadata
    {
        public int PropertiesProcessed { get; set; }
        public string PropertyType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Control type service metadata
    /// </summary>
    public class ControlTypeServiceMetadata : ServiceMetadata
    {
        public string ControlType { get; set; } = string.Empty;
        public int PatternsValidated { get; set; }
    }

    /// <summary>
    /// Grid service metadata
    /// </summary>
    public class GridServiceMetadata : ServiceMetadata
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string GridOperation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Invoke service metadata
    /// </summary>
    public class InvokeServiceMetadata : ServiceMetadata
    {
        public string InvokeMethod { get; set; } = string.Empty;
        public bool InvokeSuccessful { get; set; }
    }

    /// <summary>
    /// Item container service metadata
    /// </summary>
    public class ItemContainerServiceMetadata : ServiceMetadata
    {
        public int ContainerSize { get; set; }
        public string ContainerType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Layout service metadata
    /// </summary>
    public class LayoutServiceMetadata : ServiceMetadata
    {
        public string LayoutOperation { get; set; } = string.Empty;
        public string LayoutResult { get; set; } = string.Empty;
    }

    /// <summary>
    /// Multiple view service metadata
    /// </summary>
    public class MultipleViewServiceMetadata : ServiceMetadata
    {
        public int ViewCount { get; set; }
        public string CurrentView { get; set; } = string.Empty;
    }

    /// <summary>
    /// Range service metadata
    /// </summary>
    public class RangeServiceMetadata : ServiceMetadata
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double CurrentValue { get; set; }
    }

    /// <summary>
    /// Selection service metadata
    /// </summary>
    public class SelectionServiceMetadata : ServiceMetadata
    {
        public int SelectedCount { get; set; }
        public bool MultipleSelection { get; set; }
        public string SelectionType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Synchronized input service metadata
    /// </summary>
    public class SynchronizedInputServiceMetadata : ServiceMetadata
    {
        public string InputType { get; set; } = string.Empty;
        public bool InputProcessed { get; set; }
    }

    /// <summary>
    /// Table service metadata
    /// </summary>
    public class TableServiceMetadata : ServiceMetadata
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string TableOperation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Text service metadata
    /// </summary>
    public class TextServiceMetadata : ServiceMetadata
    {
        public int TextLength { get; set; }
        public string TextOperation { get; set; } = string.Empty;
        public bool HasFormatting { get; set; }
    }

    /// <summary>
    /// Toggle service metadata
    /// </summary>
    public class ToggleServiceMetadata : ServiceMetadata
    {
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transform service metadata
    /// </summary>
    public class TransformServiceMetadata : ServiceMetadata
    {
        public string TransformType { get; set; } = string.Empty;
        public bool TransformSuccessful { get; set; }
    }

    /// <summary>
    /// Value service metadata
    /// </summary>
    public class ValueServiceMetadata : ServiceMetadata
    {
        public string ValueType { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
        public string PreviousValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Virtualized item service metadata
    /// </summary>
    public class VirtualizedItemServiceMetadata : ServiceMetadata
    {
        public bool IsVirtualized { get; set; }
        public int VirtualIndex { get; set; }
    }

    /// <summary>
    /// Window service metadata
    /// </summary>
    public class WindowServiceMetadata : ServiceMetadata
    {
        public string WindowState { get; set; } = string.Empty;
        public string WindowOperation { get; set; } = string.Empty;
        public bool IsModal { get; set; }
    }

    /// <summary>
    /// Tree navigation service metadata
    /// </summary>
    public class TreeNavigationServiceMetadata : ServiceMetadata
    {
        public int NodesTraversed { get; set; }
        public int MaxDepth { get; set; }
        public string NavigationType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Screenshot service metadata
    /// </summary>
    public class ScreenshotServiceMetadata : ServiceMetadata
    {
        public string ImageFormat { get; set; } = string.Empty;
        public int ImageSize { get; set; }
        public string CaptureMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Focus service metadata
    /// </summary>
    public class FocusServiceMetadata : ServiceMetadata
    {
        public string FocusOperation { get; set; } = string.Empty;
        public bool FocusSuccessful { get; set; }
    }

    /// <summary>
    /// Event monitor service metadata
    /// </summary>
    public class EventMonitorServiceMetadata : ServiceMetadata
    {
        public int EventCount { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
        public TimeSpan MonitoringDuration { get; set; }
    }

    /// <summary>
    /// Application launcher metadata
    /// </summary>
    public class ApplicationLauncherMetadata : ServiceMetadata
    {
        public string ApplicationPath { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public bool LaunchSuccessful { get; set; }
    }
}