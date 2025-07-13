using System.Windows.Automation;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// ControlType関連の共通ヘルパークラス
    /// </summary>
    public static class ControlTypeHelper
    {
        /// <summary>
        /// 文字列からControlTypeを取得（完全版：40種類対応）
        /// </summary>
        public static ControlType? GetControlType(string controlTypeName)
        {
            return controlTypeName?.ToLower() switch
            {
                // 基本コントロール
                "button" => ControlType.Button,
                "calendar" => ControlType.Calendar,
                "checkbox" => ControlType.CheckBox,
                "combobox" => ControlType.ComboBox,
                "edit" => ControlType.Edit,
                "hyperlink" => ControlType.Hyperlink,
                "image" => ControlType.Image,
                "listitem" => ControlType.ListItem,
                "list" or "listbox" => ControlType.List,
                "menu" => ControlType.Menu,
                "menubar" => ControlType.MenuBar,
                "menuitem" => ControlType.MenuItem,
                "progressbar" => ControlType.ProgressBar,
                "radiobutton" => ControlType.RadioButton,
                "scrollbar" => ControlType.ScrollBar,
                "slider" => ControlType.Slider,
                "spinner" => ControlType.Spinner,
                "statusbar" => ControlType.StatusBar,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "text" => ControlType.Text,
                "toolbar" => ControlType.ToolBar,
                "tooltip" => ControlType.ToolTip,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                
                // コンテナコントロール
                "datagrid" => ControlType.DataGrid,
                "dataitem" => ControlType.DataItem,
                "document" => ControlType.Document,
                "splitbutton" => ControlType.SplitButton,
                "window" => ControlType.Window,
                "pane" => ControlType.Pane,
                "header" => ControlType.Header,
                "headeritem" => ControlType.HeaderItem,
                "table" => ControlType.Table,
                "titlebar" => ControlType.TitleBar,
                "separator" => ControlType.Separator,
                
                // その他のコントロール
                "group" => ControlType.Group,
                "thumb" => ControlType.Thumb,
                "custom" => ControlType.Custom,
                
                _ => null
            };
        }

        /// <summary>
        /// サポートされているすべてのControlType名を取得
        /// </summary>
        public static string[] GetAllControlTypeNames()
        {
            return new[]
            {
                "button", "calendar", "checkbox", "combobox", "edit", "hyperlink", "image",
                "listitem", "list", "listbox", "menu", "menubar", "menuitem", "progressbar",
                "radiobutton", "scrollbar", "slider", "spinner", "statusbar", "tab", "tabitem",
                "text", "toolbar", "tooltip", "tree", "treeitem", "datagrid", "dataitem",
                "document", "splitbutton", "window", "pane", "header", "headeritem", "table",
                "titlebar", "separator", "group", "thumb", "custom"
            };
        }

        /// <summary>
        /// ControlTypeに必要なパターンを取得
        /// </summary>
        public static string[] GetRequiredPatterns(ControlType controlType)
        {
            if (controlType == ControlType.Button) return new[] { "InvokePattern" };
            if (controlType == ControlType.CheckBox) return new[] { "TogglePattern" };
            if (controlType == ControlType.RadioButton) return new[] { "SelectionItemPattern" };
            if (controlType == ControlType.ComboBox) return new[] { "ExpandCollapsePattern", "SelectionPattern" };
            if (controlType == ControlType.Edit) return new[] { "ValuePattern" };
            if (controlType == ControlType.List) return new[] { "SelectionPattern" };
            if (controlType == ControlType.ListItem) return new[] { "SelectionItemPattern" };
            if (controlType == ControlType.Menu) return new[] { "ExpandCollapsePattern" };
            if (controlType == ControlType.MenuItem) return new[] { "InvokePattern" };
            if (controlType == ControlType.ProgressBar) return new[] { "RangeValuePattern" };
            if (controlType == ControlType.ScrollBar) return new[] { "RangeValuePattern" };
            if (controlType == ControlType.Slider) return new[] { "RangeValuePattern" };
            if (controlType == ControlType.Spinner) return new[] { "RangeValuePattern" };
            if (controlType == ControlType.Tab) return new[] { "SelectionPattern" };
            if (controlType == ControlType.TabItem) return new[] { "SelectionItemPattern" };
            if (controlType == ControlType.Tree) return new[] { "SelectionPattern" };
            if (controlType == ControlType.TreeItem) return new[] { "ExpandCollapsePattern", "SelectionItemPattern" };
            if (controlType == ControlType.DataGrid) return new[] { "GridPattern", "SelectionPattern", "TablePattern" };
            if (controlType == ControlType.DataItem) return new[] { "SelectionItemPattern" };
            if (controlType == ControlType.Document) return new[] { "TextPattern" };
            if (controlType == ControlType.SplitButton) return new[] { "ExpandCollapsePattern", "InvokePattern" };
            if (controlType == ControlType.Window) return new[] { "TransformPattern", "WindowPattern" };
            if (controlType == ControlType.Hyperlink) return new[] { "InvokePattern" };
            if (controlType == ControlType.Table) return new[] { "GridPattern", "TablePattern" };
            if (controlType == ControlType.Calendar) return new[] { "GridPattern", "TablePattern" };
            
            return Array.Empty<string>();
        }
    }
}