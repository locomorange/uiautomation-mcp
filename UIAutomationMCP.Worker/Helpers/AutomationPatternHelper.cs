using System.Windows.Automation;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// UI Automationパターンの文字列変換を行うヘルパークラス
    /// </summary>
    public static class AutomationPatternHelper
    {
        /// <summary>
        /// 文字列からAutomationPatternに変換する
        /// </summary>
        /// <param name="patternName">パターン名（大文字小文字を区別しない）</param>
        /// <returns>対応するAutomationPattern、見つからない場合はnull</returns>
        public static AutomationPattern? GetAutomationPattern(string? patternName)
        {
            if (string.IsNullOrEmpty(patternName))
                return null;

            return patternName.ToLowerInvariant() switch
            {
                "invoke" => InvokePattern.Pattern,
                "value" => ValuePattern.Pattern,
                "toggle" => TogglePattern.Pattern,
                "grid" => GridPattern.Pattern,
                "griditem" => GridItemPattern.Pattern,
                "table" => TablePattern.Pattern,
                "tableitem" => TableItemPattern.Pattern,
                "text" => TextPattern.Pattern,
                "selection" => SelectionPattern.Pattern,
                "selectionitem" => SelectionItemPattern.Pattern,
                "rangevalue" => RangeValuePattern.Pattern,
                "scroll" => ScrollPattern.Pattern,
                "scrollitem" => ScrollItemPattern.Pattern,
                "expandcollapse" => ExpandCollapsePattern.Pattern,
                "window" => WindowPattern.Pattern,
                "transform" => TransformPattern.Pattern,
                "dock" => DockPattern.Pattern,
                "multipleview" => MultipleViewPattern.Pattern,
                "virtualizeditem" => VirtualizedItemPattern.Pattern,
                "itemcontainer" => ItemContainerPattern.Pattern,
                "synchronizedinput" => SynchronizedInputPattern.Pattern,
                _ => null
            };
        }

        /// <summary>
        /// AutomationPatternから文字列に変換する
        /// </summary>
        /// <param name="pattern">AutomationPattern</param>
        /// <returns>パターン名、不明な場合は"Unknown"</returns>
        public static string GetPatternName(AutomationPattern pattern)
        {
            if (pattern == InvokePattern.Pattern) return "Invoke";
            if (pattern == ValuePattern.Pattern) return "Value";
            if (pattern == TogglePattern.Pattern) return "Toggle";
            if (pattern == GridPattern.Pattern) return "Grid";
            if (pattern == GridItemPattern.Pattern) return "GridItem";
            if (pattern == TablePattern.Pattern) return "Table";
            if (pattern == TableItemPattern.Pattern) return "TableItem";
            if (pattern == TextPattern.Pattern) return "Text";
            if (pattern == SelectionPattern.Pattern) return "Selection";
            if (pattern == SelectionItemPattern.Pattern) return "SelectionItem";
            if (pattern == RangeValuePattern.Pattern) return "RangeValue";
            if (pattern == ScrollPattern.Pattern) return "Scroll";
            if (pattern == ScrollItemPattern.Pattern) return "ScrollItem";
            if (pattern == ExpandCollapsePattern.Pattern) return "ExpandCollapse";
            if (pattern == WindowPattern.Pattern) return "Window";
            if (pattern == TransformPattern.Pattern) return "Transform";
            if (pattern == DockPattern.Pattern) return "Dock";
            if (pattern == MultipleViewPattern.Pattern) return "MultipleView";
            if (pattern == VirtualizedItemPattern.Pattern) return "VirtualizedItem";
            if (pattern == ItemContainerPattern.Pattern) return "ItemContainer";
            if (pattern == SynchronizedInputPattern.Pattern) return "SynchronizedInput";
            
            return "Unknown";
        }
    }
}