using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class CustomPropertyOperations
    {
        public OperationResult GetCustomProperties(string elementId, string[] propertyIds, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            var properties = new Dictionary<string, object>();

            foreach (var propertyId in propertyIds)
            {
                var propertyValue = GetCustomProperty(element, propertyId);
                properties[propertyId] = propertyValue;
            }

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    ElementId = elementId,
                    Properties = properties
                }
            };
        }

        public OperationResult SetCustomProperty(string elementId, string propertyId, object value, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            // Note: Setting custom properties is limited in UI Automation
            // This is a placeholder implementation
            return new OperationResult
            {
                Success = false,
                Error = "Setting custom properties is not supported in this UI Automation implementation"
            };
        }

        private object GetCustomProperty(AutomationElement element, string propertyId)
        {
            // Map common custom property requests to standard properties
            switch (propertyId.ToLowerInvariant())
            {
                case "value":
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                    {
                        return vp.Current.Value;
                    }
                    return null;

                case "text":
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern) && textPattern is TextPattern tp)
                    {
                        return tp.DocumentRange.GetText(-1);
                    }
                    return element.Current.Name;

                case "togglestate":
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) && togglePattern is TogglePattern togp)
                    {
                        return togp.Current.ToggleState.ToString();
                    }
                    return null;

                case "expandcollapsestate":
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern ecp)
                    {
                        return ecp.Current.ExpandCollapseState.ToString();
                    }
                    return null;

                case "windowvisualstate":
                    if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPattern) && windowPattern is WindowPattern wp)
                    {
                        return wp.Current.WindowVisualState.ToString();
                    }
                    return null;

                case "rangevalue":
                    if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangePattern) && rangePattern is RangeValuePattern rvp)
                    {
                        return new
                        {
                            Value = rvp.Current.Value,
                            Minimum = rvp.Current.Minimum,
                            Maximum = rvp.Current.Maximum,
                            SmallChange = rvp.Current.SmallChange,
                            LargeChange = rvp.Current.LargeChange
                        };
                    }
                    return null;

                case "selection":
                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern sp)
                    {
                        var selection = sp.Current.GetSelection();
                        return selection.Select(s => new
                        {
                            AutomationId = s.Current.AutomationId,
                            Name = s.Current.Name
                        }).ToArray();
                    }
                    return null;

                case "gridinfo":
                    if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPattern) && gridPattern is GridPattern gp)
                    {
                        return new
                        {
                            RowCount = gp.Current.RowCount,
                            ColumnCount = gp.Current.ColumnCount
                        };
                    }
                    return null;

                case "tableinfo":
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePattern) && tablePattern is TablePattern tbp)
                    {
                        return new
                        {
                            RowOrColumnMajor = tbp.Current.RowOrColumnMajor.ToString(),
                            RowHeaders = tbp.Current.GetRowHeaders().Length,
                            ColumnHeaders = tbp.Current.GetColumnHeaders().Length
                        };
                    }
                    return null;

                case "scrollinfo":
                    if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var scrollPattern) && scrollPattern is ScrollPattern scp)
                    {
                        return new
                        {
                            HorizontalScrollPercent = scp.Current.HorizontalScrollPercent,
                            VerticalScrollPercent = scp.Current.VerticalScrollPercent,
                            HorizontalViewSize = scp.Current.HorizontalViewSize,
                            VerticalViewSize = scp.Current.VerticalViewSize,
                            HorizontallyScrollable = scp.Current.HorizontallyScrollable,
                            VerticallyScrollable = scp.Current.VerticallyScrollable
                        };
                    }
                    return null;

                case "transforminfo":
                    if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var transformPattern) && transformPattern is TransformPattern trp)
                    {
                        return new
                        {
                            CanMove = trp.Current.CanMove,
                            CanResize = trp.Current.CanResize,
                            CanRotate = trp.Current.CanRotate
                        };
                    }
                    return null;

                case "dockposition":
                    if (element.TryGetCurrentPattern(DockPattern.Pattern, out var dockPattern) && dockPattern is DockPattern dp)
                    {
                        return dp.Current.DockPosition.ToString();
                    }
                    return null;

                case "multipleview":
                    if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPattern) && multipleViewPattern is MultipleViewPattern mvp)
                    {
                        return new
                        {
                            CurrentView = mvp.Current.CurrentView,
                            SupportedViews = mvp.Current.GetSupportedViews()
                        };
                    }
                    return null;

                // Standard properties
                case "automationid":
                    return element.Current.AutomationId;

                case "name":
                    return element.Current.Name;

                case "controltype":
                    return element.Current.ControlType.LocalizedControlType;

                case "classname":
                    return element.Current.ClassName;

                case "helptext":
                    return element.Current.HelpText;

                case "acceleratorkey":
                    return element.Current.AcceleratorKey;

                case "accesskey":
                    return element.Current.AccessKey;

                case "isenabled":
                    return element.Current.IsEnabled;

                case "hasKeyboardfocus":
                    return element.Current.HasKeyboardFocus;

                case "iskeyboardfocusable":
                    return element.Current.IsKeyboardFocusable;

                case "isoffscreen":
                    return element.Current.IsOffscreen;

                case "ispassword":
                    return element.Current.IsPassword;

                case "processid":
                    return element.Current.ProcessId;

                case "boundingrectangle":
                    return new
                    {
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    };

                default:
                    return $"Unknown property: {propertyId}";
            }
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
