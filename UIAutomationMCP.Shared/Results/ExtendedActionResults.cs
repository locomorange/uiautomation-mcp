namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 状態変更を伴うアクションの結果
    /// </summary>
    public class StateChangeResult<T> : ActionResult
    {
        /// <summary>
        /// 変更前の状態
        /// </summary>
        public T? PreviousState { get; set; }

        /// <summary>
        /// 変更後の状態
        /// </summary>
        public T? CurrentState { get; set; }

        /// <summary>
        /// 状態が変更されたか
        /// </summary>
        public bool StateChanged => !EqualityComparer<T>.Default.Equals(PreviousState, CurrentState);
    }

    /// <summary>
    /// 選択アクションの結果
    /// </summary>
    public class SelectionActionResult : ActionResult
    {
        public List<string> SelectedItems { get; set; } = new();
        public int SelectionCount { get; set; }
        public bool CanSelectMultiple { get; set; }
        public string SelectionType { get; set; } = "";
        public UIAutomationMCP.Shared.ElementInfo? SelectedElement { get; set; }
        public int CurrentSelectionCount { get; set; }
    }

    /// <summary>
    /// 展開/折りたたみアクションの結果
    /// </summary>
    public class ExpandCollapseResult : StateChangeResult<string>
    {
        public string ExpandCollapseState { get; set; } = "";
    }

    /// <summary>
    /// レンジ値設定の結果
    /// </summary>
    public class SetRangeValueResult : StateChangeResult<double>
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public double AttemptedValue { get; set; }
        public bool WasClampedToRange { get; set; }
    }

    /// <summary>
    /// 値設定の結果
    /// </summary>
    public class SetValueResult : StateChangeResult<string>
    {
        public string? Value { get; set; }
        public bool IsReadOnly { get; set; }
        public string? AttemptedValue { get; set; }
    }

    /// <summary>
    /// スクロールアクションの結果
    /// </summary>
    public class ScrollActionResult : ActionResult
    {
        public double HorizontalScrollPercent { get; set; }
        public double VerticalScrollPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
    }

    /// <summary>
    /// トグルアクションの結果
    /// </summary>
    public class ToggleActionResult : StateChangeResult<string>
    {
        public string ToggleState { get; set; } = "";
    }

    /// <summary>
    /// 変形アクションの結果
    /// </summary>
    public class TransformActionResult : ActionResult
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public string TransformType { get; set; } = "";
        public UIAutomationMCP.Shared.BoundingRectangle NewBounds { get; set; } = new();
        public double RotationAngle { get; set; }
    }

    /// <summary>
    /// ウィンドウアクションの結果
    /// </summary>
    public class WindowActionResult : StateChangeResult<string>
    {
        public new string WindowTitle { get; set; } = "";
        public string WindowState { get; set; } = "";
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
        public int WindowHandle { get; set; }
    }
}