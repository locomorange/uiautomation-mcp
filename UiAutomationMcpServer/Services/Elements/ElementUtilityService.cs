using Microsoft.Extensions.Logging;
using System.Windows.Automation;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementUtilityService
    {
        Dictionary<string, string> GetAvailableActions(AutomationElement? element);
        string GetElementValue(AutomationElement? element);
    }

    public class ElementUtilityService : IElementUtilityService
    {
        private readonly ILogger<ElementUtilityService> _logger;

        public ElementUtilityService(ILogger<ElementUtilityService> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, string> GetAvailableActions(AutomationElement? element)
        {
            var actions = new Dictionary<string, string>();
            if (element == null) return actions;

            try
            {
                // Check for Invoke pattern
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out _))
                {
                    actions["Invoke"] = "Click or activate the element";
                }

                // Check for Value pattern
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                {
                    if (vp.Current.IsReadOnly)
                    {
                        actions["GetValue"] = "Get the current value";
                    }
                    else
                    {
                        actions["SetValue"] = "Set a new value";
                        actions["GetValue"] = "Get the current value";
                    }
                }

                // Check for Toggle pattern
                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out _))
                {
                    actions["Toggle"] = "Toggle the element state";
                }

                // Check for Selection pattern
                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out _))
                {
                    actions["Select"] = "Select this item";
                }

                // Check for ExpandCollapse pattern
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out _))
                {
                    actions["Expand"] = "Expand the element";
                    actions["Collapse"] = "Collapse the element";
                }

                // Check for RangeValue pattern
                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangePattern) && rangePattern is RangeValuePattern rvp)
                {
                    if (!rvp.Current.IsReadOnly)
                    {
                        actions["SetRangeValue"] = "Set a value within the range";
                    }
                    actions["GetRangeValue"] = "Get the current range value";
                }

                // Check for Scroll pattern
                if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out _))
                {
                    actions["Scroll"] = "Scroll the element";
                }

                // Check for ScrollItem pattern
                if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out _))
                {
                    actions["ScrollIntoView"] = "Scroll this item into view";
                }

                // Check for Text pattern
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out _))
                {
                    actions["GetText"] = "Get text content";
                    actions["SelectText"] = "Select text range";
                    actions["FindText"] = "Find text within element";
                }

                // Check for Window pattern
                if (element.TryGetCurrentPattern(WindowPattern.Pattern, out _))
                {
                    actions["WindowAction"] = "Perform window actions (minimize, maximize, close, etc.)";
                }

                // Check for Transform pattern
                if (element.TryGetCurrentPattern(TransformPattern.Pattern, out _))
                {
                    actions["Transform"] = "Move, resize, or rotate the element";
                }

                // Check for Dock pattern
                if (element.TryGetCurrentPattern(DockPattern.Pattern, out _))
                {
                    actions["Dock"] = "Dock the element to a specific position";
                }

                // Check for VirtualizedItem pattern
                if (element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out _))
                {
                    actions["Realize"] = "Realize the virtualized item";
                }

                // Check for ItemContainer pattern
                if (element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out _))
                {
                    actions["FindItemInContainer"] = "Find an item within the container";
                }

                // Check for SynchronizedInput pattern
                if (element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out _))
                {
                    actions["CancelSynchronizedInput"] = "Cancel synchronized input";
                }

                // Check for MultipleView pattern
                if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out _))
                {
                    actions["ChangeView"] = "Change the current view";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining available actions for element");
            }

            return actions;
        }

        public string GetElementValue(AutomationElement? element)
        {
            if (element == null) return "";

            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                {
                    return vp.Current.Value ?? "";
                }

                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangePattern) && rangePattern is RangeValuePattern rvp)
                {
                    var value = double.IsInfinity(rvp.Current.Value) ? 0 : rvp.Current.Value;
                    return value.ToString();
                }

                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) && togglePattern is TogglePattern tp)
                {
                    return tp.Current.ToggleState.ToString();
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionItemPattern sip)
                {
                    return sip.Current.IsSelected.ToString();
                }

                return element.Current.Name ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting element value");
                return element.Current.Name ?? "";
            }
        }
    }
}
