namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 単一値を返す操作の結果
    /// </summary>
    public class ValueResult<T>
    {
        /// <summary>
        /// 実際の値
        /// </summary>
        public T Value { get; set; } = default!;

        /// <summary>
        /// 表示用のテキスト
        /// </summary>
        public string DisplayText { get; set; } = "";

        /// <summary>
        /// 値の追加情報
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// トグル状態の結果
    /// </summary>
    public class ToggleStateResult
    {
        public string State { get; set; } = "";
        public bool IsOn => State == "On";
        public bool IsOff => State == "Off";
        public bool IsIndeterminate => State == "Indeterminate";
    }

    /// <summary>
    /// 要素の値の結果
    /// </summary>
    public class ElementValueResult
    {
        public string Value { get; set; } = "";
        public bool IsReadOnly { get; set; }
        public bool HasValue => !string.IsNullOrEmpty(Value);
    }

    /// <summary>
    /// 範囲値の結果
    /// </summary>
    public class RangeValueResult
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SmallChange { get; set; }
        public double LargeChange { get; set; }
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// テキスト検索の結果
    /// </summary>
    public class TextSearchResult
    {
        public bool Found { get; set; }
        public string Text { get; set; } = "";
        public BoundingRectangle? BoundingRectangle { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
    }

    /// <summary>
    /// 選択されたテキストの結果
    /// </summary>
    public class SelectedTextResult
    {
        public List<string> SelectedTexts { get; set; } = new();
        public bool HasSelection => SelectedTexts.Count > 0;
        public bool MultipleSelections => SelectedTexts.Count > 1;
        public string FirstSelection => SelectedTexts.FirstOrDefault() ?? "";
    }

    /// <summary>
    /// ウィンドウ情報の結果
    /// </summary>
    public class WindowInfoResult
    {
        public string Title { get; set; } = "";
        public string ClassName { get; set; } = "";
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = "";
        public long Handle { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public string WindowState { get; set; } = "";
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
    }

    /// <summary>
    /// ブール値の結果
    /// </summary>
    public class BooleanResult
    {
        public bool Value { get; set; }
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// ウィンドウ相互作用状態の結果
    /// </summary>
    public class WindowInteractionStateResult
    {
        public string InteractionState { get; set; } = "";
        public int InteractionStateValue { get; set; }
        public string Description { get; set; } = "";
        public bool IsRunning => InteractionState == "Running";
        public bool IsClosing => InteractionState == "Closing";
        public bool IsReady => InteractionState == "ReadyForUserInteraction";
        public bool IsBlocked => InteractionState == "BlockedByModalWindow";
        public bool IsNotResponding => InteractionState == "NotResponding";
    }

    /// <summary>
    /// ビューの結果
    /// </summary>
    public class ViewResult
    {
        public int ViewId { get; set; }
        public string ViewName { get; set; } = "";
    }
}