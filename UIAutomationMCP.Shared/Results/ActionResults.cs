namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// アクション実行の基本結果
    /// </summary>
    public class ActionResult
    {
        /// <summary>
        /// 実行したアクション名
        /// </summary>
        public string ActionName { get; set; } = "";

        /// <summary>
        /// アクションが正常に実行されたか
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// 実行時刻
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 追加情報
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

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
    /// ウィンドウアクションの結果
    /// </summary>
    public class WindowActionResult : StateChangeResult<string>
    {
        /// <summary>
        /// ウィンドウハンドル
        /// </summary>
        public long WindowHandle { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string WindowTitle { get; set; } = "";
    }

    /// <summary>
    /// 展開/折りたたみアクションの結果
    /// </summary>
    public class ExpandCollapseResult : StateChangeResult<string>
    {
        public bool IsExpanded => CurrentState == "Expanded";
        public bool IsCollapsed => CurrentState == "Collapsed";
        public bool IsPartiallyExpanded => CurrentState == "PartiallyExpanded";
    }

    /// <summary>
    /// トグルアクションの結果
    /// </summary>
    public class ToggleActionResult : StateChangeResult<string>
    {
        public bool IsOn => CurrentState == "On";
        public bool IsOff => CurrentState == "Off";
        public bool IsIndeterminate => CurrentState == "Indeterminate";
    }

    /// <summary>
    /// 値設定アクションの結果
    /// </summary>
    public class SetValueResult : StateChangeResult<string>
    {
        /// <summary>
        /// 設定を試みた値
        /// </summary>
        public string AttemptedValue { get; set; } = "";

        /// <summary>
        /// 値が正しく設定されたか
        /// </summary>
        public bool ValueSet => CurrentState == AttemptedValue;
    }

    /// <summary>
    /// 範囲値設定アクションの結果
    /// </summary>
    public class SetRangeValueResult : StateChangeResult<double>
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double AttemptedValue { get; set; }
        public bool WasClampedToRange { get; set; }
    }

    /// <summary>
    /// 選択アクションの結果
    /// </summary>
    public class SelectionActionResult : ActionResult
    {
        /// <summary>
        /// 選択された要素
        /// </summary>
        public ElementInfo? SelectedElement { get; set; }

        /// <summary>
        /// 選択操作のタイプ
        /// </summary>
        public string SelectionType { get; set; } = ""; // "Add", "Remove", "Select", "Clear"

        /// <summary>
        /// 現在選択されている要素の数
        /// </summary>
        public int CurrentSelectionCount { get; set; }
    }

    /// <summary>
    /// スクロールアクションの結果
    /// </summary>
    public class ScrollActionResult : ActionResult
    {
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
    }

    /// <summary>
    /// 変形（Transform）アクションの結果
    /// </summary>
    public class TransformActionResult : ActionResult
    {
        public string TransformType { get; set; } = ""; // "Move", "Resize", "Rotate"
        public BoundingRectangle? NewBounds { get; set; }
        public double? RotationAngle { get; set; }
    }
}