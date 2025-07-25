using System.Windows.Automation;

namespace UIAutomationMCP.UIAutomation.Helpers
{
    /// <summary>
    /// UI Automation pattern string conversion helper
    /// Shared between Worker and Monitor processes
    /// </summary>
    public static class AutomationPatternHelper
    {
        /// <summary>
        /// Convert string to AutomationPattern
        /// </summary>
        /// <param name="patternName">Pattern name (case insensitive)</param>
        /// <returns>Corresponding AutomationPattern, or null if not found</returns>
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
        /// Convert AutomationPattern to string
        /// </summary>
        /// <param name="pattern">AutomationPattern</param>
        /// <returns>Pattern name string, or "Unknown" if not found</returns>
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

        /// <summary>
        /// Get all supported pattern names
        /// </summary>
        /// <returns>Array of supported pattern names</returns>
        public static string[] GetSupportedPatterns()
        {
            return new[]
            {
                "Invoke", "Value", "Toggle", "Grid", "GridItem", "Table", "TableItem",
                "Text", "Selection", "SelectionItem", "RangeValue", "Scroll", "ScrollItem",
                "ExpandCollapse", "Window", "Transform", "Dock", "MultipleView",
                "VirtualizedItem", "ItemContainer", "SynchronizedInput"
            };
        }

        /// <summary>
        /// Get the pattern availability property for a given AutomationPattern
        /// </summary>
        /// <param name="pattern">The AutomationPattern to get the availability property for</param>
        /// <returns>The corresponding availability property, or null if not found</returns>
        public static AutomationProperty? GetPatternAvailableProperty(AutomationPattern pattern)
        {
            if (pattern == InvokePattern.Pattern)
                return InvokePattern.IsInvokePatternAvailableProperty;
            else if (pattern == ValuePattern.Pattern)
                return ValuePattern.IsValuePatternAvailableProperty;
            else if (pattern == TogglePattern.Pattern)
                return TogglePattern.IsTogglePatternAvailableProperty;
            else if (pattern == GridPattern.Pattern)
                return GridPattern.IsGridPatternAvailableProperty;
            else if (pattern == GridItemPattern.Pattern)
                return GridItemPattern.IsGridItemPatternAvailableProperty;
            else if (pattern == TablePattern.Pattern)
                return TablePattern.IsTablePatternAvailableProperty;
            else if (pattern == TableItemPattern.Pattern)
                return TableItemPattern.IsTableItemPatternAvailableProperty;
            else if (pattern == TextPattern.Pattern)
                return TextPattern.IsTextPatternAvailableProperty;
            else if (pattern == SelectionPattern.Pattern)
                return SelectionPattern.IsSelectionPatternAvailableProperty;
            else if (pattern == SelectionItemPattern.Pattern)
                return SelectionItemPattern.IsSelectionItemPatternAvailableProperty;
            else if (pattern == RangeValuePattern.Pattern)
                return RangeValuePattern.IsRangeValuePatternAvailableProperty;
            else if (pattern == ScrollPattern.Pattern)
                return ScrollPattern.IsScrollPatternAvailableProperty;
            else if (pattern == ScrollItemPattern.Pattern)
                return ScrollItemPattern.IsScrollItemPatternAvailableProperty;
            else if (pattern == ExpandCollapsePattern.Pattern)
                return ExpandCollapsePattern.IsExpandCollapsePatternAvailableProperty;
            else if (pattern == WindowPattern.Pattern)
                return WindowPattern.IsWindowPatternAvailableProperty;
            else if (pattern == TransformPattern.Pattern)
                return TransformPattern.IsTransformPatternAvailableProperty;
            else if (pattern == DockPattern.Pattern)
                return DockPattern.IsDockPatternAvailableProperty;
            else if (pattern == MultipleViewPattern.Pattern)
                return MultipleViewPattern.IsMultipleViewPatternAvailableProperty;
            else if (pattern == VirtualizedItemPattern.Pattern)
                return VirtualizedItemPattern.IsVirtualizedItemPatternAvailableProperty;
            else if (pattern == ItemContainerPattern.Pattern)
                return ItemContainerPattern.IsItemContainerPatternAvailableProperty;
            else if (pattern == SynchronizedInputPattern.Pattern)
                return SynchronizedInputPattern.IsSynchronizedInputPatternAvailableProperty;
            
            return null;
        }
    }
}