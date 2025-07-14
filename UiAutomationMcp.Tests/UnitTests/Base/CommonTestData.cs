namespace UIAutomationMCP.Tests.UnitTests.Base
{
    /// <summary>
    /// テスト間で共通して使用されるテストデータ
    /// </summary>
    public static class CommonTestData
    {
        /// <summary>
        /// 有効なタイムアウト値
        /// </summary>
        public static readonly int[] ValidTimeouts = { 0, 1000, 5000, 10000, 30000 };

        /// <summary>
        /// 無効なタイムアウト値
        /// </summary>
        public static readonly int[] InvalidTimeouts = { -1, -100, -5000 };

        /// <summary>
        /// 有効なプロセスID
        /// </summary>
        public static readonly int[] ValidProcessIds = { 0, 1234, 5678, 9999 };

        /// <summary>
        /// 無効なプロセスID
        /// </summary>
        public static readonly int[] InvalidProcessIds = { -1, -100 };

        /// <summary>
        /// 空または無効な文字列パラメータ
        /// </summary>
        public static readonly string[] EmptyOrInvalidStrings = { "", "   ", "\t", "\n", null };

        /// <summary>
        /// 有効な要素ID
        /// </summary>
        public static readonly string[] ValidElementIds = { "button1", "textBox1", "listItem1", "menuItem1" };

        /// <summary>
        /// 有効なウィンドウタイトル
        /// </summary>
        public static readonly string[] ValidWindowTitles = { "Test Window", "Application", "Dialog", "" };

        /// <summary>
        /// ドックポジション値（DockPattern用）
        /// </summary>
        public static readonly string[] DockPositions = { "top", "bottom", "left", "right", "fill", "none" };

        /// <summary>
        /// トグル状態値（TogglePattern用）
        /// </summary>
        public static readonly string[] ToggleStates = { "on", "off", "indeterminate" };

        /// <summary>
        /// 展開/折りたたみ状態値（ExpandCollapsePattern用）
        /// </summary>
        public static readonly string[] ExpandCollapseStates = { "expanded", "collapsed", "partiallyexpanded", "leafnode" };

        /// <summary>
        /// 座標値（Transform/Grid Pattern用）
        /// </summary>
        public static readonly double[] ValidCoordinates = { 0.0, 100.0, 500.0, 1920.0 };

        /// <summary>
        /// 無効な座標値
        /// </summary>
        public static readonly double[] InvalidCoordinates = { -1.0, -100.0, double.NaN, double.PositiveInfinity };

        /// <summary>
        /// グリッド行/列インデックス
        /// </summary>
        public static readonly int[] ValidGridIndices = { 0, 1, 5, 10 };

        /// <summary>
        /// 無効なグリッド行/列インデックス
        /// </summary>
        public static readonly int[] InvalidGridIndices = { -1, -5, int.MaxValue };

        /// <summary>
        /// コントロールタイプ
        /// </summary>
        public static readonly string[] ControlTypes = 
        {
            "Button", "CheckBox", "ComboBox", "DataGrid", "Document", "Edit",
            "Group", "Header", "HeaderItem", "Hyperlink", "Image", "List",
            "ListItem", "Menu", "MenuBar", "MenuItem", "Pane", "ProgressBar",
            "RadioButton", "ScrollBar", "Slider", "Spinner", "Tab", "TabItem",
            "Table", "Text", "ToolBar", "ToolTip", "Tree", "TreeItem", "Window"
        };

        /// <summary>
        /// 共通のエラーメッセージパターン
        /// </summary>
        public static class ErrorMessages
        {
            public const string ElementNotFound = "Element not found";
            public const string PatternNotSupported = "Pattern not supported";
            public const string InvalidOperation = "Invalid operation";
            public const string TimeoutError = "Operation timed out";
            public const string InvalidParameter = "Invalid parameter";
            public const string AccessDenied = "Access denied";
        }

        /// <summary>
        /// 成功メッセージパターン
        /// </summary>
        public static class SuccessMessages
        {
            public const string OperationCompleted = "Operation completed successfully";
            public const string ElementFound = "Element found";
            public const string PatternSupported = "Pattern supported";
            public const string StateChanged = "State changed successfully";
        }
    }
}