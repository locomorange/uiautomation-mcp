using System.Windows.Automation;

namespace UIAutomationMCP.Subprocess.Core.Helpers
{
    /// <summary>
    /// ControlType related common helper class
    /// Shared between Worker and Monitor processes
    /// </summary>
    public static class ControlTypeHelper
    {
        /// <summary>
        /// Unified ControlType conversion dictionary (case insensitive)
        /// </summary>
        private static readonly Dictionary<string, ControlType> ControlTypeMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            // Basic controls
            ["Button"] = ControlType.Button,
            ["Calendar"] = ControlType.Calendar,
            ["CheckBox"] = ControlType.CheckBox,
            ["ComboBox"] = ControlType.ComboBox,
            ["Edit"] = ControlType.Edit,
            ["Hyperlink"] = ControlType.Hyperlink,
            ["Image"] = ControlType.Image,
            ["ListItem"] = ControlType.ListItem,
            ["List"] = ControlType.List,
            ["ListBox"] = ControlType.List, // Alias
            ["Menu"] = ControlType.Menu,
            ["MenuBar"] = ControlType.MenuBar,
            ["MenuItem"] = ControlType.MenuItem,
            ["ProgressBar"] = ControlType.ProgressBar,
            ["RadioButton"] = ControlType.RadioButton,
            ["ScrollBar"] = ControlType.ScrollBar,
            ["Slider"] = ControlType.Slider,
            ["Spinner"] = ControlType.Spinner,
            ["StatusBar"] = ControlType.StatusBar,
            ["Tab"] = ControlType.Tab,
            ["TabItem"] = ControlType.TabItem,
            ["Text"] = ControlType.Text,
            ["ToolBar"] = ControlType.ToolBar,
            ["ToolTip"] = ControlType.ToolTip,
            ["Tree"] = ControlType.Tree,
            ["TreeItem"] = ControlType.TreeItem,

            // Container controls
            ["DataGrid"] = ControlType.DataGrid,
            ["DataItem"] = ControlType.DataItem,
            ["Document"] = ControlType.Document,
            ["SplitButton"] = ControlType.SplitButton,
            ["Window"] = ControlType.Window,
            ["Pane"] = ControlType.Pane,
            ["Header"] = ControlType.Header,
            ["HeaderItem"] = ControlType.HeaderItem,
            ["Table"] = ControlType.Table,
            ["TitleBar"] = ControlType.TitleBar,
            ["Separator"] = ControlType.Separator,

            // Other controls
            ["Group"] = ControlType.Group,
            ["Thumb"] = ControlType.Thumb,
            ["Custom"] = ControlType.Custom
        };

        /// <summary>
        /// Get ControlType from string (nullable version)
        /// </summary>
        public static ControlType? GetControlType(string? controlTypeName)
        {
            if (string.IsNullOrEmpty(controlTypeName))
                return null;

            return ControlTypeMappings.TryGetValue(controlTypeName, out var controlType) ? controlType : null;
        }

        /// <summary>
        /// Get ControlType from string (out parameter version for existing code compatibility)
        /// Supports both direct names ("Button") and with "ControlType" suffix ("ButtonControlType")
        /// </summary>
        public static bool TryGetControlType(string? controlTypeName, out ControlType controlType)
        {
            controlType = ControlType.Custom;

            if (string.IsNullOrEmpty(controlTypeName))
                return false;

            // Try direct lookup first
            if (ControlTypeMappings.TryGetValue(controlTypeName, out var foundType))
            {
                controlType = foundType;
                return true;
            }

            // Try removing "ControlType" suffix if present
            if (controlTypeName.EndsWith("ControlType", StringComparison.OrdinalIgnoreCase))
            {
                var withoutSuffix = controlTypeName.Substring(0, controlTypeName.Length - "ControlType".Length);
                if (ControlTypeMappings.TryGetValue(withoutSuffix, out foundType))
                {
                    controlType = foundType;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get string name from ControlType
        /// </summary>
        public static string? GetControlTypeName(ControlType controlType)
        {
            var entry = ControlTypeMappings.FirstOrDefault(kvp => kvp.Value.Equals(controlType));
            return entry.Key; // Returns null by default
        }

        /// <summary>
        /// Get all supported ControlType names
        /// </summary>
        public static string[] GetAllControlTypeNames()
        {
            return ControlTypeMappings.Keys.ToArray();
        }

        /// <summary>
        /// ControlType pattern information
        /// </summary>
        public class ControlTypePatternInfo
        {
            public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
            public string[] OptionalPatterns { get; set; } = Array.Empty<string>();
        }

        /// <summary>
        /// ControlType and pattern mapping (Microsoft Documentation compliant)
        /// </summary>
        private static readonly Dictionary<ControlType, ControlTypePatternInfo> ControlTypePatterns = new()
        {
            [ControlType.Button] = new() { RequiredPatterns = new[] { "Invoke" }, OptionalPatterns = new[] { "ExpandCollapse", "Toggle" } },
            [ControlType.CheckBox] = new() { RequiredPatterns = new[] { "Toggle" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.ComboBox] = new() { RequiredPatterns = new[] { "ExpandCollapse" }, OptionalPatterns = new[] { "Value", "Selection" } },
            [ControlType.Edit] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Value", "Text", "RangeValue" } },
            [ControlType.List] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Grid", "MultipleView", "Scroll" } },
            [ControlType.ListItem] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "ExpandCollapse", "GridItem", "Invoke", "ScrollItem", "Toggle", "Value" } },
            [ControlType.Menu] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse" } },
            [ControlType.MenuItem] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "Toggle", "SelectionItem" } },
            [ControlType.RadioButton] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Toggle" } },
            [ControlType.ScrollBar] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Slider] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = new[] { "Selection", "Value" } },
            [ControlType.TabItem] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Invoke" } },
            [ControlType.Table] = new() { RequiredPatterns = new[] { "Grid", "Table" }, OptionalPatterns = new[] { "Selection", "Sort" } },
            [ControlType.Tree] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Scroll" } },
            [ControlType.TreeItem] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "ScrollItem", "SelectionItem", "Toggle" } },
            [ControlType.Window] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Transform", "Window" } },
            // Additional ControlTypes (from legacy GetRequiredPatterns)
            [ControlType.ProgressBar] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Spinner] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Tab] = new() { RequiredPatterns = new[] { "Selection" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.DataGrid] = new() { RequiredPatterns = new[] { "Grid", "Selection", "Table" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.DataItem] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Document] = new() { RequiredPatterns = new[] { "Text" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.SplitButton] = new() { RequiredPatterns = new[] { "ExpandCollapse", "Invoke" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Hyperlink] = new() { RequiredPatterns = new[] { "Invoke" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Calendar] = new() { RequiredPatterns = new[] { "Grid", "Table" }, OptionalPatterns = Array.Empty<string>() }
        };

        /// <summary>
        /// Get ControlType pattern information
        /// </summary>
        public static ControlTypePatternInfo? GetPatternInfo(ControlType controlType)
        {
            return ControlTypePatterns.TryGetValue(controlType, out var info) ? info : null;
        }

        /// <summary>
        /// Get required patterns for ControlType (for backward compatibility)
        /// </summary>
        public static string[] GetRequiredPatterns(ControlType controlType)
        {
            return GetPatternInfo(controlType)?.RequiredPatterns ?? Array.Empty<string>();
        }

        /// <summary>
        /// Get optional patterns for ControlType
        /// </summary>
        public static string[] GetOptionalPatterns(ControlType controlType)
        {
            return GetPatternInfo(controlType)?.OptionalPatterns ?? Array.Empty<string>();
        }
    }
}

