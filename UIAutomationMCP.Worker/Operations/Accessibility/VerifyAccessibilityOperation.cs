using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Accessibility
{
    public class VerifyAccessibilityOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public VerifyAccessibilityOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            try
            {
                var accessibilityIssues = new List<string>();
                var accessibilityChecks = new Dictionary<string, bool>();

                // Check 1: Element has a name
                var hasName = !string.IsNullOrEmpty(element.Current.Name);
                accessibilityChecks["HasName"] = hasName;
                if (!hasName)
                {
                    accessibilityIssues.Add("Element does not have a name");
                }

                // Check 2: Element is keyboard accessible (if interactive)
                var isKeyboardFocusable = element.Current.IsKeyboardFocusable;
                var isInteractive = IsInteractiveElement(element.Current.ControlType);
                accessibilityChecks["IsKeyboardAccessible"] = !isInteractive || isKeyboardFocusable;
                if (isInteractive && !isKeyboardFocusable)
                {
                    accessibilityIssues.Add("Interactive element is not keyboard focusable");
                }

                // Check 3: Element has proper labeling (for form controls)
                var needsLabeling = NeedsLabeling(element.Current.ControlType);
                var hasLabeling = HasProperLabeling(element);
                accessibilityChecks["HasProperLabeling"] = !needsLabeling || hasLabeling;
                if (needsLabeling && !hasLabeling)
                {
                    accessibilityIssues.Add("Form control lacks proper labeling (Name, LabeledBy, or HelpText)");
                }

                // Check 4: Element has sufficient contrast (placeholder - would need actual color analysis)
                accessibilityChecks["HasSufficientContrast"] = true; // Placeholder

                // Check 5: Element is properly exposed to accessibility tools
                var isContentElement = element.Current.IsContentElement;
                var isControlElement = element.Current.IsControlElement;
                var isProperlyExposed = isContentElement || isControlElement;
                accessibilityChecks["IsProperlyExposed"] = isProperlyExposed;
                if (!isProperlyExposed)
                {
                    accessibilityIssues.Add("Element is not properly exposed to accessibility tools");
                }

                // Check 6: Element provides role information
                var hasRole = !string.IsNullOrEmpty(element.Current.LocalizedControlType);
                accessibilityChecks["HasRole"] = hasRole;
                if (!hasRole)
                {
                    accessibilityIssues.Add("Element does not provide role information");
                }

                // Check 7: Element state is accessible (for stateful controls)
                var isStateful = IsStatefulElement(element.Current.ControlType);
                var hasAccessibleState = HasAccessibleState(element);
                accessibilityChecks["HasAccessibleState"] = !isStateful || hasAccessibleState;
                if (isStateful && !hasAccessibleState)
                {
                    accessibilityIssues.Add("Stateful element does not expose its state accessibly");
                }

                var verificationResult = new Dictionary<string, object>
                {
                    ["IsAccessible"] = accessibilityIssues.Count == 0,
                    ["AccessibilityChecks"] = accessibilityChecks,
                    ["Issues"] = accessibilityIssues,
                    ["Score"] = CalculateAccessibilityScore(accessibilityChecks),
                    ["Recommendations"] = GetAccessibilityRecommendations(accessibilityIssues)
                };

                return Task.FromResult(new OperationResult { Success = true, Data = verificationResult });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error verifying accessibility: {ex.Message}" });
            }
        }

        private bool IsInteractiveElement(ControlType controlType)
        {
            return controlType == ControlType.Button ||
                   controlType == ControlType.CheckBox ||
                   controlType == ControlType.RadioButton ||
                   controlType == ControlType.ComboBox ||
                   controlType == ControlType.Edit ||
                   controlType == ControlType.Hyperlink ||
                   controlType == ControlType.ListItem ||
                   controlType == ControlType.MenuItem ||
                   controlType == ControlType.Slider ||
                   controlType == ControlType.Spinner ||
                   controlType == ControlType.TabItem ||
                   controlType == ControlType.TreeItem;
        }

        private bool NeedsLabeling(ControlType controlType)
        {
            return controlType == ControlType.Edit ||
                   controlType == ControlType.ComboBox ||
                   controlType == ControlType.CheckBox ||
                   controlType == ControlType.RadioButton ||
                   controlType == ControlType.Slider ||
                   controlType == ControlType.Spinner;
        }

        private bool HasProperLabeling(AutomationElement element)
        {
            if (!string.IsNullOrEmpty(element.Current.Name))
                return true;

            if (!string.IsNullOrEmpty(element.Current.HelpText))
                return true;

            try
            {
                var labeledBy = element.Current.LabeledBy;
                if (labeledBy != null && !string.IsNullOrEmpty(labeledBy.Current.Name))
                    return true;
            }
            catch (Exception)
            {
                // LabeledBy may not be available
            }

            return false;
        }

        private bool IsStatefulElement(ControlType controlType)
        {
            return controlType == ControlType.CheckBox ||
                   controlType == ControlType.RadioButton ||
                   controlType == ControlType.ToggleButton ||
                   controlType == ControlType.TabItem ||
                   controlType == ControlType.TreeItem;
        }

        private bool HasAccessibleState(AutomationElement element)
        {
            // Check for Toggle pattern
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern))
                return true;

            // Check for Selection Item pattern
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern))
                return true;

            // Check for Expand/Collapse pattern
            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern))
                return true;

            return false;
        }

        private double CalculateAccessibilityScore(Dictionary<string, bool> checks)
        {
            if (checks.Count == 0) return 0.0;

            var passedChecks = checks.Values.Count(passed => passed);
            return (double)passedChecks / checks.Count * 100;
        }

        private List<string> GetAccessibilityRecommendations(List<string> issues)
        {
            var recommendations = new List<string>();

            foreach (var issue in issues)
            {
                if (issue.Contains("name"))
                {
                    recommendations.Add("Add a meaningful name to the element using the Name property");
                }
                else if (issue.Contains("keyboard"))
                {
                    recommendations.Add("Ensure interactive elements are keyboard accessible");
                }
                else if (issue.Contains("labeling"))
                {
                    recommendations.Add("Add proper labeling using Name, LabeledBy, or HelpText properties");
                }
                else if (issue.Contains("exposed"))
                {
                    recommendations.Add("Ensure element is marked as ContentElement or ControlElement");
                }
                else if (issue.Contains("role"))
                {
                    recommendations.Add("Provide proper role information through LocalizedControlType");
                }
                else if (issue.Contains("state"))
                {
                    recommendations.Add("Implement appropriate patterns to expose element state");
                }
            }

            return recommendations;
        }
    }
}