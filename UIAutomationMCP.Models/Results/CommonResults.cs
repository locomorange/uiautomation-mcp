namespace UIAutomationMCP.Models.Results
{
    // Common operation results
    public class UniversalResponse : BaseOperationResult { }
    public class TreeNavigationResult : BaseOperationResult { }
    public class FindItemResult : BaseOperationResult { }
    public class ElementDetailResult : BaseOperationResult { }
    public class EventMonitoringResult : BaseOperationResult 
    { 
        public string EventType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ElementId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public List<TypedEventData> CapturedEvents { get; set; } = new List<TypedEventData>();
    }
    public class TableInfoResult : BaseOperationResult { }
    
    // Support classes
    public class DetailMetadata { }
    public class ElementDetail { }
    public class EventTimeRange { }
    public class ServerExecutionInfo { }
    public class RequestMetadata { }
    public class ServerEnhancedResponse<T> { }

    // Pattern operation results
    public class SetRangeValueResult : BaseOperationResult { }
    public class SetScrollPercentResult : BaseOperationResult { }
    public class DockElementResult : BaseOperationResult { }
    public class ExpandCollapseElementResult : BaseOperationResult { }
    public class MoveElementResult : BaseOperationResult { }
    public class ResizeElementResult : BaseOperationResult { }
    public class RotateElementResult : BaseOperationResult { }
    public class SetToggleStateResult : BaseOperationResult { }
    public class ToggleElementResult : BaseOperationResult { }
    public class SelectElementResult : BaseOperationResult { }
    public class AddToSelectionResult : BaseOperationResult { }
    public class RemoveFromSelectionResult : BaseOperationResult { }
    public class ClearSelectionResult : BaseOperationResult { }
    public class SelectItemResult : BaseOperationResult { }
    public class InvokeElementResult : BaseOperationResult { }
    public class SetValueResult : BaseOperationResult { }
    public class SetFocusResult : BaseOperationResult { }
    public class GetGridItemResult : BaseOperationResult { }
    public class GridItemResult : BaseOperationResult 
    { 
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string ContainingGridId { get; set; } = string.Empty;
        public ElementInfo? Element { get; set; }
    }
    public class GetRowHeaderResult : BaseOperationResult { }
    public class GetColumnHeaderResult : BaseOperationResult { }
    public class WaitForInputIdleResult : BaseOperationResult { }
    public class ScrollElementIntoViewResult : BaseOperationResult { }
    public class ScrollElementResult : BaseOperationResult { }

    // Missing result types
    public class ExpandCollapseResult : BaseOperationResult { }
    public class SelectionActionResult : BaseOperationResult { }
    public class RangeResult : BaseOperationResult { }
    public class TransformResult : BaseOperationResult { }
    public class DockResult : BaseOperationResult { }
    public class ScrollResult : BaseOperationResult { }
    public class WindowResult : BaseOperationResult { }
    public class ToggleResult : BaseOperationResult { }
    public class PatternResult : BaseOperationResult { }
    public class MultipleViewResult : BaseOperationResult { }
    public class ItemContainerResult : BaseOperationResult { }
    public class VirtualizedItemResult : BaseOperationResult { }
    public class SynchronizedInputResult : BaseOperationResult { }
    public class StateChangeResult<T> : BaseOperationResult where T : class { }
    public class ScrollActionResult : BaseOperationResult { }

    /// <summary>
    /// Error handler registry for operations
    /// </summary>
    public static class ErrorHandlerRegistry
    {
        public static void RegisterHandlers() { }
        public static T Handle<T>(Func<T> operation) where T : BaseOperationResult, new()
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                var result = new T();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
    }
}