using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    public class RangeOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public RangeOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        public OperationResult SetRangeValue(string elementId, double value, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" };

            var currentValue = rangePattern.Current.Value;
            var minimum = rangePattern.Current.Minimum;
            var maximum = rangePattern.Current.Maximum;
            var isReadOnly = rangePattern.Current.IsReadOnly;

            if (isReadOnly)
                return new OperationResult { Success = false, Error = "Range element is read-only" };

            if (value < minimum || value > maximum)
            {
                return new OperationResult 
                { 
                    Success = false, 
                    Error = $"Value {value} is out of range. Valid range: {minimum} - {maximum}" 
                };
            }

            // Let exceptions flow naturally - no try-catch
            rangePattern.SetValue(value);
            var newValue = rangePattern.Current.Value;
            
            return new OperationResult 
            { 
                Success = true, 
                Data = new 
                { 
                    PreviousValue = currentValue, 
                    NewValue = newValue,
                    Minimum = minimum,
                    Maximum = maximum
                }
            };
        }

        public OperationResult GetRangeValue(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" };

            // Let exceptions flow naturally - no try-catch
            var rangeInfo = new
            {
                Value = rangePattern.Current.Value,
                Minimum = rangePattern.Current.Minimum,
                Maximum = rangePattern.Current.Maximum,
                LargeChange = rangePattern.Current.LargeChange,
                SmallChange = rangePattern.Current.SmallChange,
                IsReadOnly = rangePattern.Current.IsReadOnly
            };

            return new OperationResult { Success = true, Data = rangeInfo };
        }

        public OperationResult GetRangeProperties(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" };

            // Let exceptions flow naturally - no try-catch
            var properties = new
            {
                Minimum = rangePattern.Current.Minimum,
                Maximum = rangePattern.Current.Maximum,
                LargeChange = rangePattern.Current.LargeChange,
                SmallChange = rangePattern.Current.SmallChange,
                IsReadOnly = rangePattern.Current.IsReadOnly,
                Range = rangePattern.Current.Maximum - rangePattern.Current.Minimum,
                SupportsLargeChange = rangePattern.Current.LargeChange > 0,
                SupportsSmallChange = rangePattern.Current.SmallChange > 0
            };

            return new OperationResult { Success = true, Data = properties };
        }

    }
}
