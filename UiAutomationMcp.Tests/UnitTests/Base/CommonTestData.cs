namespace UIAutomationMCP.Tests.UnitTests.Base
{
    /// <summary>
    ///                                   
    /// </summary>
    public static class CommonTestData
    {
        /// <summary>
        ///                    
        /// </summary>
        public static readonly int[] ValidTimeouts = { 0, 1000, 5000, 10000, 30000 };

        /// <summary>
        ///                     
        /// </summary>
        public static readonly int[] InvalidTimeouts = { -1, -100, -5000 };

        /// <summary>
        ///             ID
        /// </summary>
        public static readonly int[] ValidProcessIds = { 0, 1234, 5678, 9999 };

        /// <summary>
        ///              ID
        /// </summary>
        public static readonly int[] InvalidProcessIds = { -1, -100 };

        /// <summary>
        ///                            
        /// </summary>
        public static readonly string?[] EmptyOrInvalidStrings = { "", "   ", "\t", "\n", null };

        /// <summary>
        ///          ID
        /// </summary>
        public static readonly string[] ValidElementIds = { "button1", "textBox1", "listItem1", "menuItem1" };

        /// <summary>
        ///                      
        /// </summary>
        public static readonly string[] ValidWindowTitles = { "Test Window", "Application", "Dialog", "" };

        /// <summary>
        ///                   ockPattern            /// </summary>
        public static readonly string[] DockPositions = { "top", "bottom", "left", "right", "fill", "none" };

        /// <summary>
        ///              ogglePattern            /// </summary>
        public static readonly string[] ToggleStates = { "on", "off", "indeterminate" };

        /// <summary>
        ///    /                xpandCollapsePattern            /// </summary>
        public static readonly string[] ExpandCollapseStates = { "expanded", "collapsed", "partiallyexpanded", "leafnode" };

        /// <summary>
        ///         ransform/Grid Pattern            /// </summary>
        public static readonly double[] ValidCoordinates = { 0.0, 100.0, 500.0, 1920.0 };

        /// <summary>
        ///             
        /// </summary>
        public static readonly double[] InvalidCoordinates = { -1.0, -100.0, double.NaN, double.PositiveInfinity };

        /// <summary>
        ///                      
        /// </summary>
        public static readonly int[] ValidGridIndices = { 0, 1, 5, 10 };

        /// <summary>
        ///                            
        /// </summary>
        public static readonly int[] InvalidGridIndices = { -1, -5, int.MaxValue };

        /// <summary>
        ///                          /// </summary>
        public static readonly string[] ControlTypes = 
        {
            "Button", "CheckBox", "ComboBox", "DataGrid", "Document", "Edit",
            "Group", "Header", "HeaderItem", "Hyperlink", "Image", "List",
            "ListItem", "Menu", "MenuBar", "MenuItem", "Pane", "ProgressBar",
            "RadioButton", "ScrollBar", "Slider", "Spinner", "Tab", "TabItem",
            "Table", "Text", "ToolBar", "ToolTip", "Tree", "TreeItem", "Window"
        };

        /// <summary>
        ///                            
        /// </summary>
        public static class ErrorMessages
        {
            public const string ElementNotFound = "Element not found";
            public const string PatternNotSupported = "Pattern not supported";
            public const string InvalidOperation = "Invalid operation";
            public const string TimeoutError = "Operation could not complete within timeout";
            public const string InvalidParameter = "Invalid parameter";
            public const string AccessDenied = "Access denied";
        }

        /// <summary>
        ///                    
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
