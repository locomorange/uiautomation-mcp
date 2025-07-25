namespace UIAutomationMCP.Models.Results
{
    // Common operation results
    public class UniversalResponse : BaseOperationResult 
    { 
        public object? Data { get; set; }
        public string ResponseType { get; set; } = string.Empty;
        public new string Metadata { get; set; } = string.Empty;
    }
    public class TreeNavigationResult : BaseOperationResult 
    { 
        public List<ElementInfo> Elements { get; set; } = new List<ElementInfo>();
        public string NavigationType { get; set; } = string.Empty;
        public string SourceElementId { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
    }
    public class FindItemResult : BaseOperationResult 
    { 
        public ElementInfo? FoundElement { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public int TotalMatches { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
    public class ElementDetailResult : BaseOperationResult 
    { 
        public ElementInfo? Element { get; set; }
        public List<string> SupportedPatterns { get; set; } = new List<string>();
        public List<string> AvailableProperties { get; set; } = new List<string>();
        public ElementState? ElementState { get; set; }
        public BoundingRectangle? BoundingRectangle { get; set; }
    }
    public class EventMonitoringResult : BaseOperationResult 
    { 
        public string EventType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ElementId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public List<TypedEventData> CapturedEvents { get; set; } = new List<TypedEventData>();
    }
    public class TableInfoResult : BaseOperationResult 
    { 
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string RowOrColumnMajor { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<ElementInfo> Headers { get; set; } = new List<ElementInfo>();
        public bool HasRowHeaders { get; set; }
        public bool HasColumnHeaders { get; set; }
    }
    
    // Support classes
    public class DetailMetadata 
    { 
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
    public class ElementDetail 
    { 
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
    }
    public class EventTimeRange 
    { 
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }
    public class ServerExecutionInfo 
    { 
        public string ExecutionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string ServerVersion { get; set; } = string.Empty;
        public string ServerProcessingTime { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public DateTime ServerExecutedAt { get; set; }
        public List<string> ServerLogs { get; set; } = new List<string>();
        public object? Metadata { get; set; }
    }
    public class RequestMetadata 
    { 
        public string RequestId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string RequestedMethod { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
    }
    public class ServerEnhancedResponse<T> 
    { 
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ServerExecutionInfo ExecutionInfo { get; set; } = new ServerExecutionInfo();
        public RequestMetadata Metadata { get; set; } = new RequestMetadata();
        public RequestMetadata RequestMetadata { get; set; } = new RequestMetadata();
    }

    // Pattern operation results
    public class SetRangeValueResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double PreviousValue { get; set; }
        public double CurrentValue { get; set; }
        public double AttemptedValue { get; set; }
        public bool WasClampedToRange { get; set; }
        public string? PreviousState { get; set; }
        public string? CurrentState { get; set; }
    }
    public class SetScrollPercentResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double PreviousHorizontalPercent { get; set; }
        public double PreviousVerticalPercent { get; set; }
    }
    public class DockElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string DockPosition { get; set; } = string.Empty;
        public string PreviousDockPosition { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class ExpandCollapseElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ExpandCollapseState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class MoveElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double PreviousX { get; set; }
        public double PreviousY { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class ResizeElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double PreviousWidth { get; set; }
        public double PreviousHeight { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class RotateElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double RotationAngle { get; set; }
        public double PreviousRotationAngle { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class SetToggleStateResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousToggleState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class ToggleElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousToggleState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class SelectElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? SelectedElement { get; set; }
        public bool WasAlreadySelected { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class AddToSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? AddedElement { get; set; }
        public int TotalSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class RemoveFromSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? RemovedElement { get; set; }
        public int TotalSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class ClearSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int PreviousSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class SelectItemResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? SelectedItem { get; set; }
        public bool WasAlreadySelected { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class InvokeElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? InvokedElement { get; set; }
        public DateTime InvokedAt { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class SetValueResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string AttemptedValue { get; set; } = string.Empty;
    }
    public class SetFocusResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? FocusedElement { get; set; }
        public bool HadFocusBefore { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class GetGridItemResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public GridItemResult? GridItem { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class GridItemResult : BaseOperationResult 
    { 
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string ContainingGridId { get; set; } = string.Empty;
        public ElementInfo? Element { get; set; }
    }
    public class GetRowHeaderResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public List<ElementInfo> RowHeaders { get; set; } = new List<ElementInfo>();
        public int RowIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class GetColumnHeaderResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public List<ElementInfo> ColumnHeaders { get; set; } = new List<ElementInfo>();
        public int ColumnIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class WaitForInputIdleResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int TimeoutMilliseconds { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public bool TimedOut { get; set; }
        public string Message { get; set; } = string.Empty;
        public string WindowInteractionState { get; set; } = string.Empty;
    }
    public class ScrollElementIntoViewResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? ScrolledElement { get; set; }
        public bool WasAlreadyVisible { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class ScrollElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ScrollDirection { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double PreviousHorizontalPercent { get; set; }
        public double PreviousVerticalPercent { get; set; }
        public double CurrentHorizontalPercent { get; set; }
        public double CurrentVerticalPercent { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    // Missing result types
    public class ExpandCollapseResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class SelectionActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string SelectionType { get; set; } = string.Empty;
        public int CurrentSelectionCount { get; set; }
        public string Details { get; set; } = string.Empty;
        public ElementInfo? SelectedElement { get; set; }
    }
    public class TransformActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string TransformType { get; set; } = string.Empty;
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Rotation { get; set; }
        public string? PreviousState { get; set; }
        public string? CurrentState { get; set; }
        public string Details { get; set; } = string.Empty;
        public string NewBounds { get; set; } = string.Empty;
        public double? RotationAngle { get; set; }
    }
    public class RangeResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double CurrentValue { get; set; }
        public double LargeChange { get; set; }
        public double SmallChange { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class TransformResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string TransformType { get; set; } = string.Empty;
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class DockResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string DockPosition { get; set; } = string.Empty;
        public string PreviousPosition { get; set; } = string.Empty;
        public List<string> SupportedDockPositions { get; set; } = new List<string>();
        public string Details { get; set; } = string.Empty;
    }
    public class ScrollResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class WindowResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string WindowState { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class ToggleResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public List<string> SupportedStates { get; set; } = new List<string>();
        public string Details { get; set; } = string.Empty;
    }
    public class ToggleActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class PatternResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PatternName { get; set; } = string.Empty;
        public bool IsSupported { get; set; }
        public List<string> AvailableProperties { get; set; } = new List<string>();
        public List<string> AvailableMethods { get; set; } = new List<string>();
        public string Details { get; set; } = string.Empty;
    }
    public class MultipleViewResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int CurrentView { get; set; }
        public List<int> SupportedViews { get; set; } = new List<int>();
        public string ViewName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class ItemContainerResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? ContainerElement { get; set; }
        public int ItemCount { get; set; }
        public string ContainerType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
    public class VirtualizedItemResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? VirtualizedElement { get; set; }
        public bool IsRealized { get; set; }
        public int ItemIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class SynchronizedInputResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string InputType { get; set; } = string.Empty;
        public bool IsSynchronized { get; set; }
        public DateTime InputTime { get; set; }
        public string Details { get; set; } = string.Empty;
    }
    public class StateChangeResult<T> : BaseOperationResult where T : class 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public T? PreviousState { get; set; }
        public T? CurrentState { get; set; }
        public string Details { get; set; } = string.Empty;
        public new DateTime ExecutedAt { get; set; }
        public bool StateChanged { get; set; }
    }
    public class ScrollActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public string Details { get; set; } = string.Empty;
    }

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
        
        public static bool ValidateElementId(string? elementId)
        {
            return !string.IsNullOrWhiteSpace(elementId);
        }
        
        public static bool ValidateElementId(string? elementId, string context)
        {
            return !string.IsNullOrWhiteSpace(elementId);
        }
    }

    // Additional missing result types for Server interfaces
    public class GridInfoResult : BaseOperationResult
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string RowOrColumnMajor { get; set; } = string.Empty;
        public bool HasRowHeaders { get; set; }
        public bool HasColumnHeaders { get; set; }
    }

    public class ScrollInfoResult : BaseOperationResult
    {
        public double HorizontalScrollPercent { get; set; }
        public double VerticalScrollPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
    }

    public class RangeValueResult : BaseOperationResult
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class SelectionInfoResult : BaseOperationResult
    {
        public bool CanSelectMultiple { get; set; }
        public bool IsSelectionRequired { get; set; }
        public List<ElementInfo> SelectedItems { get; set; } = new List<ElementInfo>();
        public int SelectionCount { get; set; }
    }

    public class TransformCapabilitiesResult : BaseOperationResult
    {
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public string TransformType { get; set; } = string.Empty;
    }

    public class WindowInteractionStateResult : BaseOperationResult
    {
        public string WindowInteractionState { get; set; } = string.Empty;
        public string WindowVisualState { get; set; } = string.Empty;
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
    }

    public class WindowCapabilitiesResult : BaseOperationResult
    {
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public string WindowVisualState { get; set; } = string.Empty;
        public string WindowInteractionState { get; set; } = string.Empty;
    }
}