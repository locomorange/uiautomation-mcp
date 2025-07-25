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
        public string ActionPerformed { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
        public bool VerificationSuccessful { get; set; }
        public int PropertiesCount { get; set; }
    }

    /// <summary>
    /// Element search service metadata
    /// </summary>
    public class SearchServiceMetadata : ServiceMetadata
    {
        public int ElementsFound { get; set; }
        public string SearchScope { get; set; } = string.Empty;
        public bool UsedFallback { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public string SearchCriteria { get; set; } = string.Empty;
        public bool SearchResultsLimited { get; set; }
        public int TotalResults { get; set; }
    }

    /// <summary>
    /// Custom property service metadata
    /// </summary>
    public class CustomPropertyServiceMetadata : ServiceMetadata
    {
        public int PropertiesProcessed { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
        public int PropertiesCount { get; set; }
    }

    /// <summary>
    /// Control type service metadata
    /// </summary>
    public class ControlTypeServiceMetadata : ServiceMetadata
    {
        public string ControlType { get; set; } = string.Empty;
        public int PatternsValidated { get; set; }
        public int ElementsFound { get; set; }
        public bool ValidationSuccessful { get; set; }
        public int SupportedPatternsCount { get; set; }
    }

    /// <summary>
    /// Grid service metadata
    /// </summary>
    public class GridServiceMetadata : ServiceMetadata
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string GridOperation { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public bool SupportsRowHeaders { get; set; }
        public bool SupportsColumnHeaders { get; set; }
    }

    /// <summary>
    /// Invoke service metadata
    /// </summary>
    public class InvokeServiceMetadata : ServiceMetadata
    {
        public string InvokeMethod { get; set; } = string.Empty;
        public bool InvokeSuccessful { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
    }

    /// <summary>
    /// Item container service metadata
    /// </summary>
    public class ItemContainerServiceMetadata : ServiceMetadata
    {
        public int ContainerSize { get; set; }
        public string ContainerType { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int ItemsFound { get; set; }
    }

    /// <summary>
    /// Layout service metadata
    /// </summary>
    public class LayoutServiceMetadata : ServiceMetadata
    {
        public string LayoutOperation { get; set; } = string.Empty;
        public string LayoutResult { get; set; } = string.Empty;
        public string ActionPerformed { get; set; } = string.Empty;
        public bool ScrollActionSuccessful { get; set; }
        public double ScrollAmount { get; set; }
        public string ScrollDirection { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
    }

    /// <summary>
    /// Multiple view service metadata
    /// </summary>
    public class MultipleViewServiceMetadata : ServiceMetadata
    {
        public int ViewCount { get; set; }
        public string CurrentView { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int ViewsCount { get; set; }
    }

    /// <summary>
    /// Range service metadata
    /// </summary>
    public class RangeServiceMetadata : ServiceMetadata
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double CurrentValue { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public double RangeValue { get; set; }
        public double MinimumValue { get; set; }
        public double MaximumValue { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Selection service metadata
    /// </summary>
    public class SelectionServiceMetadata : ServiceMetadata
    {
        public int SelectedCount { get; set; }
        public bool MultipleSelection { get; set; }
        public string SelectionType { get; set; } = string.Empty;
        public string ActionPerformed { get; set; } = string.Empty;
        public int SelectedItemsCount { get; set; }
        public bool SupportsMultipleSelection { get; set; }
        public bool IsSelectionRequired { get; set; }
        public bool OperationSuccessful { get; set; }
    }

    /// <summary>
    /// Synchronized input service metadata
    /// </summary>
    public class SynchronizedInputServiceMetadata : ServiceMetadata
    {
        public string InputType { get; set; } = string.Empty;
        public bool InputProcessed { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
    }

    /// <summary>
    /// Table service metadata
    /// </summary>
    public class TableServiceMetadata : ServiceMetadata
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string TableOperation { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
    }

    /// <summary>
    /// Text service metadata
    /// </summary>
    public class TextServiceMetadata : ServiceMetadata
    {
        public int TextLength { get; set; }
        public string TextOperation { get; set; } = string.Empty;
        public bool HasFormatting { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public bool HasAttributes { get; set; }
        public bool TextFound { get; set; }
        public int StartIndex { get; set; }
    }

    /// <summary>
    /// Toggle service metadata
    /// </summary>
    public class ToggleServiceMetadata : ServiceMetadata
    {
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transform service metadata
    /// </summary>
    public class TransformServiceMetadata : ServiceMetadata
    {
        public string TransformType { get; set; } = string.Empty;
        public bool TransformSuccessful { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
    }

    /// <summary>
    /// Value service metadata
    /// </summary>
    public class ValueServiceMetadata : ServiceMetadata
    {
        public string ValueType { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
        public string PreviousValue { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public int ValueLength { get; set; }
        public bool HasValue { get; set; }
    }

    /// <summary>
    /// Virtualized item service metadata
    /// </summary>
    public class VirtualizedItemServiceMetadata : ServiceMetadata
    {
        public bool IsVirtualized { get; set; }
        public int VirtualIndex { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public bool ItemRealized { get; set; }
    }

    /// <summary>
    /// Window service metadata
    /// </summary>
    public class WindowServiceMetadata : ServiceMetadata
    {
        public string WindowState { get; set; } = string.Empty;
        public string WindowOperation { get; set; } = string.Empty;
        public bool IsModal { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public bool InputIdleAchieved { get; set; }
    }

    /// <summary>
    /// Tree navigation service metadata
    /// </summary>
    public class TreeNavigationServiceMetadata : ServiceMetadata
    {
        public int NodesTraversed { get; set; }
        public int MaxDepth { get; set; }
        public string NavigationType { get; set; } = string.Empty;
        public string SourceElementId { get; set; } = string.Empty;
        public string ActionPerformed { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
        public bool NavigationSuccessful { get; set; }
    }

    /// <summary>
    /// Screenshot service metadata
    /// </summary>
    public class ScreenshotServiceMetadata : ServiceMetadata
    {
        public string ImageFormat { get; set; } = string.Empty;
        public int ImageSize { get; set; }
        public string CaptureMethod { get; set; } = string.Empty;
        public bool OperationSuccessful { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public int ScreenshotWidth { get; set; }
        public int ScreenshotHeight { get; set; }
        public long FileSize { get; set; }
        public bool HasBase64Data { get; set; }
        public string ScreenshotTimestamp { get; set; } = string.Empty;
        public string TargetWindowTitle { get; set; } = string.Empty;
        public int? TargetProcessId { get; set; }
        public int? MaxTokensRequested { get; set; }
    }

    /// <summary>
    /// Focus service metadata
    /// </summary>
    public class FocusServiceMetadata : ServiceMetadata
    {
        public string FocusOperation { get; set; } = string.Empty;
        public bool FocusSuccessful { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event monitor service metadata
    /// </summary>
    public class EventMonitorServiceMetadata : ServiceMetadata
    {
        public int EventCount { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
        public TimeSpan MonitoringDuration { get; set; }
        public string ActionPerformed { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public int EventsCount { get; set; }
        public bool MonitoringActive { get; set; }
        public bool OperationSuccessful { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Application launcher metadata
    /// </summary>
    public class ApplicationLauncherMetadata : ServiceMetadata
    {
        public string ApplicationPath { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public bool LaunchSuccessful { get; set; }
        public bool OperationSuccessful { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public bool HasExited { get; set; }
        public string WindowTitle { get; set; } = string.Empty;
        public string ActionPerformed { get; set; } = string.Empty;
    }
}