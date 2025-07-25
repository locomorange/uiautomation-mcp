using System.Windows.Automation;

namespace UIAutomationMCP.Common.Helpers
{
    /// <summary>
    /// UI Automation condition builder helper
    /// Shared between Worker and Monitor processes
    /// </summary>
    public static class ConditionBuilder
    {
        /// <summary>
        /// Condition by AutomationId
        /// </summary>
        public static PropertyCondition ByAutomationId(string automationId)
        {
            return new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        }

        /// <summary>
        /// Condition by Name
        /// </summary>
        public static PropertyCondition ByName(string name)
        {
            return new PropertyCondition(AutomationElement.NameProperty, name);
        }

        /// <summary>
        /// Condition by ControlType
        /// </summary>
        public static PropertyCondition ByControlType(ControlType controlType)
        {
            return new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
        }

        /// <summary>
        /// Condition by ClassName
        /// </summary>
        public static PropertyCondition ByClassName(string className)
        {
            return new PropertyCondition(AutomationElement.ClassNameProperty, className);
        }

        /// <summary>
        /// Condition by ProcessId
        /// </summary>
        public static PropertyCondition ByProcessId(int processId)
        {
            return new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
        }

        /// <summary>
        /// Condition by IsEnabled
        /// </summary>
        public static PropertyCondition ByIsEnabled(bool isEnabled = true)
        {
            return new PropertyCondition(AutomationElement.IsEnabledProperty, isEnabled);
        }

        /// <summary>
        /// Condition by IsOffscreen
        /// </summary>
        public static PropertyCondition ByIsOffscreen(bool isOffscreen = false)
        {
            return new PropertyCondition(AutomationElement.IsOffscreenProperty, isOffscreen);
        }

        /// <summary>
        /// Combine conditions with AND logic
        /// </summary>
        public static AndCondition And(params Condition[] conditions)
        {
            return new AndCondition(conditions);
        }

        /// <summary>
        /// Combine conditions with OR logic
        /// </summary>
        public static OrCondition Or(params Condition[] conditions)
        {
            return new OrCondition(conditions);
        }

        /// <summary>
        /// Negate condition
        /// </summary>
        public static NotCondition Not(Condition condition)
        {
            return new NotCondition(condition);
        }

        /// <summary>
        /// Build complex condition for element identification
        /// Priority: AutomationId > Name + ControlType > Name only
        /// </summary>
        public static Condition BuildElementCondition(
            string? automationId = null,
            string? name = null,
            string? controlType = null,
            int? processId = null,
            bool enabledOnly = true)
        {
            var conditions = new List<Condition>();

            // Primary identifier: AutomationId (most reliable)
            if (!string.IsNullOrEmpty(automationId))
            {
                conditions.Add(ByAutomationId(automationId));
            }
            // Secondary identifier: Name (less reliable)
            else if (!string.IsNullOrEmpty(name))
            {
                conditions.Add(ByName(name));
            }

            // Additional filters
            if (!string.IsNullOrEmpty(controlType) && TryParseControlType(controlType, out var ct))
            {
                conditions.Add(ByControlType(ct));
            }

            if (processId.HasValue)
            {
                conditions.Add(ByProcessId(processId.Value));
            }

            if (enabledOnly)
            {
                conditions.Add(ByIsEnabled(true));
            }

            // Combine all conditions
            return conditions.Count switch
            {
                0 => Condition.TrueCondition,
                1 => conditions[0],
                _ => And(conditions.ToArray())
            };
        }

        /// <summary>
        /// Try to parse control type string to ControlType
        /// </summary>
        private static bool TryParseControlType(string controlTypeString, out ControlType controlType)
        {
            controlType = controlTypeString.ToLowerInvariant() switch
            {
                "button" => ControlType.Button,
                "text" => ControlType.Text,
                "edit" => ControlType.Edit,
                "combobox" => ControlType.ComboBox,
                "listbox" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "checkbox" => ControlType.CheckBox,
                "radiobutton" => ControlType.RadioButton,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "table" => ControlType.Table,
                "window" => ControlType.Window,
                "pane" => ControlType.Pane,
                "group" => ControlType.Group,
                "menubar" => ControlType.MenuBar,
                "menu" => ControlType.Menu,
                "menuitem" => ControlType.MenuItem,
                "toolbar" => ControlType.ToolBar,
                "statusbar" => ControlType.StatusBar,
                "slider" => ControlType.Slider,
                "progressbar" => ControlType.ProgressBar,
                "scrollbar" => ControlType.ScrollBar,
                "image" => ControlType.Image,
                "calendar" => ControlType.Calendar,
                "hyperlink" => ControlType.Hyperlink,
                _ => ControlType.Custom
            };

            return controlType != ControlType.Custom || controlTypeString.ToLowerInvariant() == "custom";
        }
    }
}