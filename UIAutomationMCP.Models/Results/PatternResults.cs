namespace UIAutomationMCP.Models.Results
{
    // UI Pattern operation results

    /// <summary>
    /// Result for range value operations
    /// </summary>
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

    /// <summary>
    /// Result for toggle state operations
    /// </summary>
    public class SetToggleStateResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousToggleState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for toggle element operations
    /// </summary>
    public class ToggleElementResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousToggleState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for invoke element operations
    /// </summary>
    public class InvokeElementResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? InvokedElement { get; set; }
        public DateTime InvokedAt { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for set value operations
    /// </summary>
    public class SetValueResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string AttemptedValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for set focus operations
    /// </summary>
    public class SetFocusResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? FocusedElement { get; set; }
        public bool HadFocusBefore { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Range operation result
    /// </summary>
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

    /// <summary>
    /// Range value information result
    /// </summary>
    public class RangeValueResult : BaseOperationResult
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Toggle operation result
    /// </summary>
    public class ToggleResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ToggleState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public List<string> SupportedStates { get; set; } = new List<string>();
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Toggle action result
    /// </summary>
    public class ToggleActionResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// General pattern operation result
    /// </summary>
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

    /// <summary>
    /// Multiple view pattern result
    /// </summary>
    public class MultipleViewResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int CurrentView { get; set; }
        public List<int> SupportedViews { get; set; } = new List<int>();
        public string ViewName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Item container pattern result
    /// </summary>
    public class ItemContainerResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? ContainerElement { get; set; }
        public int ItemCount { get; set; }
        public string ContainerType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Virtualized item pattern result
    /// </summary>
    public class VirtualizedItemResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? VirtualizedElement { get; set; }
        public bool IsRealized { get; set; }
        public int ItemIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Synchronized input pattern result
    /// </summary>
    public class SynchronizedInputResult : BaseOperationResult
    {
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string InputType { get; set; } = string.Empty;
        public bool IsSynchronized { get; set; }
        public DateTime InputTime { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Wait for input idle result
    /// </summary>
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

    /// <summary>
    /// Window operation result
    /// </summary>
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

    /// <summary>
    /// Window interaction state result
    /// </summary>
    public class WindowInteractionStateResult : BaseOperationResult
    {
        public string WindowInteractionState { get; set; } = string.Empty;
        public string WindowVisualState { get; set; } = string.Empty;
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
    }

    /// <summary>
    /// Window capabilities result
    /// </summary>
    public class WindowCapabilitiesResult : BaseOperationResult
    {
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public string WindowVisualState { get; set; } = string.Empty;
        public string WindowInteractionState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic state change result
    /// </summary>
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
}
