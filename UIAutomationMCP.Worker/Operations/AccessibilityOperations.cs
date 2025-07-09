using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class AccessibilityOperations
    {
        public OperationResult GetAccessibilityInfo(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            var accessibilityInfo = new
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                LocalizedControlType = element.Current.LocalizedControlType,
                ControlType = element.Current.ControlType.LocalizedControlType,
                HelpText = element.Current.HelpText,
                AcceleratorKey = element.Current.AcceleratorKey,
                AccessKey = element.Current.AccessKey,
                HasKeyboardFocus = element.Current.HasKeyboardFocus,
                IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                IsEnabled = element.Current.IsEnabled,
                IsOffscreen = element.Current.IsOffscreen,
                IsPassword = element.Current.IsPassword,
                IsRequiredForForm = element.Current.IsRequiredForForm,
                ItemType = element.Current.ItemType,
                ItemStatus = element.Current.ItemStatus,
                ClassName = element.Current.ClassName,
                BoundingRectangle = new
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatterns(element),
                LabeledBy = GetLabeledByElement(element),
                DescribedBy = GetDescribedByElements(element)
            };

            return new OperationResult
            {
                Success = true,
                Data = accessibilityInfo
            };
        }

        public OperationResult VerifyAccessibility(string? elementId = null, string windowTitle = "", int processId = 0)
        {
            AutomationElement element;
            if (!string.IsNullOrEmpty(elementId))
            {
                element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return new OperationResult { Success = false, Error = "Element not found" };
            }
            else
            {
                element = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            }

            // Let exceptions flow naturally - no try-catch
            var violations = new List<object>();
            var checkCount = 0;

            // Check accessibility compliance
            VerifyElementAccessibility(element, violations, ref checkCount);

            var complianceResult = new
            {
                IsCompliant = violations.Count == 0,
                ViolationCount = violations.Count,
                ChecksPerformed = checkCount,
                Violations = violations,
                Summary = violations.Count == 0 ? "All accessibility checks passed" : $"Found {violations.Count} accessibility violations"
            };

            return new OperationResult
            {
                Success = true,
                Data = complianceResult
            };
        }

        public OperationResult GetLabeledBy(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            var labeledByElement = GetLabeledByElement(element);

            return new OperationResult
            {
                Success = true,
                Data = labeledByElement
            };
        }

        public OperationResult GetDescribedBy(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            var describedByElements = GetDescribedByElements(element);

            return new OperationResult
            {
                Success = true,
                Data = new { DescribedBy = describedByElements }
            };
        }

        private void VerifyElementAccessibility(AutomationElement element, List<object> violations, ref int checkCount)
        {
            checkCount++;

            // Check for name
            if (string.IsNullOrEmpty(element.Current.Name) && 
                element.Current.ControlType != ControlType.Group &&
                element.Current.ControlType != ControlType.Pane)
            {
                violations.Add(new
                {
                    Type = "MissingName",
                    Element = element.Current.AutomationId ?? "Unknown",
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    Description = "Element is missing a name property"
                });
            }

            // Check for keyboard accessibility
            if (element.Current.IsKeyboardFocusable && !element.Current.IsEnabled)
            {
                violations.Add(new
                {
                    Type = "DisabledKeyboardFocusable",
                    Element = element.Current.AutomationId ?? "Unknown",
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    Description = "Element is keyboard focusable but disabled"
                });
            }

            // Check for contrast (basic check for offscreen elements)
            if (element.Current.IsOffscreen && element.Current.IsEnabled)
            {
                violations.Add(new
                {
                    Type = "OffscreenEnabled",
                    Element = element.Current.AutomationId ?? "Unknown",
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    Description = "Enabled element is offscreen"
                });
            }

            // Check child elements
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            foreach (AutomationElement child in children)
            {
                if (child != null)
                {
                    VerifyElementAccessibility(child, violations, ref checkCount);
                }
            }
        }

        private object? GetLabeledByElement(AutomationElement element)
        {
            var labeledBy = element.Current.LabeledBy;
            if (labeledBy != null)
            {
                return new
                {
                    AutomationId = labeledBy.Current.AutomationId,
                    Name = labeledBy.Current.Name,
                    ControlType = labeledBy.Current.ControlType.LocalizedControlType
                };
            }
            return null;
        }

        private List<object> GetDescribedByElements(AutomationElement element)
        {
            var describedBy = new List<object>();
            
            // UI Automation doesn't have a direct DescribedBy property like LabeledBy
            // This is a simplified implementation that looks for help text
            if (!string.IsNullOrEmpty(element.Current.HelpText))
            {
                describedBy.Add(new
                {
                    Type = "HelpText",
                    Content = element.Current.HelpText
                });
            }

            return describedBy;
        }

        private List<string> GetSupportedPatterns(AutomationElement element)
        {
            var patterns = new List<string>();
            var supportedPatterns = element.GetSupportedPatterns();

            foreach (var pattern in supportedPatterns)
            {
                patterns.Add(pattern.ProgrammaticName);
            }

            return patterns;
        }

        private AutomationElement? FindElementById(string elementId, string windowTitle, int processId)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
            return searchRoot.FindFirst(TreeScope.Descendants, condition);
        }

        private AutomationElement? GetSearchRoot(string windowTitle, int processId)
        {
            if (processId > 0)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            return null;
        }
    }
}
