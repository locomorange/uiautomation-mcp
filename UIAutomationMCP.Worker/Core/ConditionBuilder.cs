using System.Windows.Automation;

namespace UIAutomationMCP.Worker.Core
{
    /// <summary>
    /// UI Automation条件を構築するヘルパー
    /// </summary>
    public static class ConditionBuilder
    {
        /// <summary>
        /// AutomationIdによる条件
        /// </summary>
        public static PropertyCondition ByAutomationId(string automationId)
        {
            return new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        }

        /// <summary>
        /// 名前による条件
        /// </summary>
        public static PropertyCondition ByName(string name)
        {
            return new PropertyCondition(AutomationElement.NameProperty, name);
        }

        /// <summary>
        /// コントロールタイプによる条件
        /// </summary>
        public static PropertyCondition ByControlType(ControlType controlType)
        {
            return new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
        }

        /// <summary>
        /// クラス名による条件
        /// </summary>
        public static PropertyCondition ByClassName(string className)
        {
            return new PropertyCondition(AutomationElement.ClassNameProperty, className);
        }

        /// <summary>
        /// プロセスIDによる条件
        /// </summary>
        public static PropertyCondition ByProcessId(int processId)
        {
            return new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
        }

        /// <summary>
        /// 有効な要素による条件
        /// </summary>
        public static PropertyCondition IsEnabled()
        {
            return new PropertyCondition(AutomationElement.IsEnabledProperty, true);
        }

        /// <summary>
        /// 複数条件のAND結合
        /// </summary>
        public static AndCondition And(params Condition[] conditions)
        {
            return new AndCondition(conditions);
        }

        /// <summary>
        /// 複数条件のOR結合
        /// </summary>
        public static OrCondition Or(params Condition[] conditions)
        {
            return new OrCondition(conditions);
        }

        /// <summary>
        /// 条件の否定
        /// </summary>
        public static NotCondition Not(Condition condition)
        {
            return new NotCondition(condition);
        }

        /// <summary>
        /// ウィンドウを検索する条件
        /// </summary>
        public static PropertyCondition Windows()
        {
            return ByControlType(ControlType.Window);
        }

        /// <summary>
        /// ボタンを検索する条件
        /// </summary>
        public static PropertyCondition Buttons()
        {
            return ByControlType(ControlType.Button);
        }

        /// <summary>
        /// テキストボックスを検索する条件
        /// </summary>
        public static PropertyCondition TextBoxes()
        {
            return ByControlType(ControlType.Edit);
        }

        /// <summary>
        /// リストアイテムを検索する条件
        /// </summary>
        public static PropertyCondition ListItems()
        {
            return ByControlType(ControlType.ListItem);
        }

        /// <summary>
        /// メニューアイテムを検索する条件
        /// </summary>
        public static PropertyCondition MenuItems()
        {
            return ByControlType(ControlType.MenuItem);
        }
    }
}
