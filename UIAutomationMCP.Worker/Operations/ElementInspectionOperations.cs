using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// Element inspection operations with minimal UIAutomation API granularity
    /// </summary>
    public class ElementInspectionOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public ElementInspectionOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        /// <summary>
        /// Get comprehensive properties of an element
        /// </summary>
        public OperationResult GetElementProperties(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var properties = new
            {
                AutomationId = element.Current.AutomationId ?? "",
                Name = element.Current.Name ?? "",
                ClassName = element.Current.ClassName ?? "",
                ControlType = element.Current.ControlType.LocalizedControlType,
                LocalizedControlType = element.Current.LocalizedControlType ?? "",
                IsEnabled = element.Current.IsEnabled,
                IsOffscreen = element.Current.IsOffscreen,
                IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                HasKeyboardFocus = element.Current.HasKeyboardFocus,
                IsPassword = element.Current.IsPassword,
                IsRequiredForForm = element.Current.IsRequiredForForm,
                IsContentElement = element.Current.IsContentElement,
                IsControlElement = element.Current.IsControlElement,
                ProcessId = element.Current.ProcessId,
                FrameworkId = element.Current.FrameworkId ?? "",
                HelpText = element.Current.HelpText ?? "",
                AcceleratorKey = element.Current.AcceleratorKey ?? "",
                AccessKey = element.Current.AccessKey ?? "",
                ItemType = element.Current.ItemType ?? "",
                ItemStatus = element.Current.ItemStatus ?? "",
                Orientation = element.Current.Orientation.ToString(),
                BoundingRectangle = new
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                RuntimeId = element.GetRuntimeId()
            };

            return new OperationResult { Success = true, Data = properties };
        }

        /// <summary>
        /// Get supported patterns of an element
        /// </summary>
        public OperationResult GetElementPatterns(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var supportedPatterns = element.GetSupportedPatterns();
            var patternInfo = new List<object>();

            foreach (var pattern in supportedPatterns)
            {
                var patternName = pattern.ProgrammaticName.Replace("PatternIdentifiers.Pattern", "");
                var patternDetails = GetPatternDetails(element, pattern);
                
                patternInfo.Add(new
                {
                    Name = patternName,
                    ProgrammaticName = pattern.ProgrammaticName,
                    Details = patternDetails
                });
            }

            return new OperationResult { Success = true, Data = patternInfo };
        }

        private object? GetPatternDetails(AutomationElement element, AutomationPattern pattern)
        {
            if (!element.TryGetCurrentPattern(pattern, out var patternInstance))
                return null;

            // Let exceptions flow naturally - no try-catch
            return pattern.ProgrammaticName switch
            {
                "InvokePatternIdentifiers.Pattern" => new { Supported = true },
                
                "ValuePatternIdentifiers.Pattern" when patternInstance is ValuePattern valuePattern => new
                {
                    Value = valuePattern.Current.Value,
                    IsReadOnly = valuePattern.Current.IsReadOnly
                },
                
                "SelectionItemPatternIdentifiers.Pattern" when patternInstance is SelectionItemPattern selectionPattern => new
                {
                    IsSelected = selectionPattern.Current.IsSelected,
                    SelectionContainer = selectionPattern.Current.SelectionContainer?.Current.Name ?? ""
                },
                
                "TogglePatternIdentifiers.Pattern" when patternInstance is TogglePattern togglePattern => new
                {
                    ToggleState = togglePattern.Current.ToggleState.ToString()
                },
                
                "ExpandCollapsePatternIdentifiers.Pattern" when patternInstance is ExpandCollapsePattern expandPattern => new
                {
                    ExpandCollapseState = expandPattern.Current.ExpandCollapseState.ToString()
                },
                
                "RangeValuePatternIdentifiers.Pattern" when patternInstance is RangeValuePattern rangePattern => new
                {
                    Value = rangePattern.Current.Value,
                    Minimum = rangePattern.Current.Minimum,
                    Maximum = rangePattern.Current.Maximum,
                    SmallChange = rangePattern.Current.SmallChange,
                    LargeChange = rangePattern.Current.LargeChange,
                    IsReadOnly = rangePattern.Current.IsReadOnly
                },
                
                "WindowPatternIdentifiers.Pattern" when patternInstance is WindowPattern windowPattern => new
                {
                    WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                    WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString(),
                    IsModal = windowPattern.Current.IsModal,
                    IsTopmost = windowPattern.Current.IsTopmost,
                    CanMaximize = windowPattern.Current.CanMaximize,
                    CanMinimize = windowPattern.Current.CanMinimize
                },
                
                "GridPatternIdentifiers.Pattern" when patternInstance is GridPattern gridPattern => new
                {
                    RowCount = gridPattern.Current.RowCount,
                    ColumnCount = gridPattern.Current.ColumnCount
                },
                
                "GridItemPatternIdentifiers.Pattern" when patternInstance is GridItemPattern gridItemPattern => new
                {
                    Row = gridItemPattern.Current.Row,
                    Column = gridItemPattern.Current.Column,
                    RowSpan = gridItemPattern.Current.RowSpan,
                    ColumnSpan = gridItemPattern.Current.ColumnSpan,
                    ContainingGrid = gridItemPattern.Current.ContainingGrid?.Current.Name ?? ""
                },
                
                "TablePatternIdentifiers.Pattern" when patternInstance is TablePattern tablePattern => new
                {
                    RowCount = tablePattern.Current.RowCount,
                    ColumnCount = tablePattern.Current.ColumnCount,
                    RowOrColumnMajor = tablePattern.Current.RowOrColumnMajor.ToString()
                },
                
                "TableItemPatternIdentifiers.Pattern" when patternInstance is TableItemPattern tableItemPattern => new
                {
                    Row = tableItemPattern.Current.Row,
                    Column = tableItemPattern.Current.Column,
                    RowHeaderItems = tableItemPattern.Current.GetRowHeaderItems()?.Select(h => h.Current.Name).ToArray() ?? new string[0],
                    ColumnHeaderItems = tableItemPattern.Current.GetColumnHeaderItems()?.Select(h => h.Current.Name).ToArray() ?? new string[0]
                },
                
                "ScrollPatternIdentifiers.Pattern" when patternInstance is ScrollPattern scrollPattern => new
                {
                    HorizontalScrollPercent = scrollPattern.Current.HorizontalScrollPercent,
                    VerticalScrollPercent = scrollPattern.Current.VerticalScrollPercent,
                    HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                    VerticalViewSize = scrollPattern.Current.VerticalViewSize,
                    HorizontallyScrollable = scrollPattern.Current.HorizontallyScrollable,
                    VerticallyScrollable = scrollPattern.Current.VerticallyScrollable
                },
                
                "SelectionPatternIdentifiers.Pattern" when patternInstance is SelectionPattern selectionPattern => new
                {
                    CanSelectMultiple = selectionPattern.Current.CanSelectMultiple,
                    IsSelectionRequired = selectionPattern.Current.IsSelectionRequired,
                    SelectedItems = selectionPattern.Current.GetSelection()?.Select(s => s.Current.Name).ToArray() ?? new string[0]
                },
                
                "TextPatternIdentifiers.Pattern" => new
                {
                    SupportedTextSelection = "TextPattern is supported"
                },
                
                _ => new { Supported = true }
            };
        }

    }
}
