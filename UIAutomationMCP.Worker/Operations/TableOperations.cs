using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class TableOperations
    {
        public OperationResult GetTableInfo(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return new OperationResult { Success = false, Error = "TablePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var rowHeaders = tablePattern.Current.GetRowHeaders();
            var columnHeaders = tablePattern.Current.GetColumnHeaders();
            var rowOrColumnMajor = tablePattern.Current.RowOrColumnMajor;

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    RowHeaderCount = rowHeaders.Length,
                    ColumnHeaderCount = columnHeaders.Length,
                    RowOrColumnMajor = rowOrColumnMajor.ToString(),
                    RowHeaders = rowHeaders.Select((header, index) => new
                    {
                        Index = index,
                        AutomationId = header.Current.AutomationId,
                        Name = header.Current.Name
                    }).ToArray(),
                    ColumnHeaders = columnHeaders.Select((header, index) => new
                    {
                        Index = index,
                        AutomationId = header.Current.AutomationId,
                        Name = header.Current.Name
                    }).ToArray()
                }
            };
        }

        public OperationResult GetRowHeaders(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return new OperationResult { Success = false, Error = "TablePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var rowHeaders = tablePattern.Current.GetRowHeaders();
            var headerInfo = new List<object>();

            for (int i = 0; i < rowHeaders.Length; i++)
            {
                var header = rowHeaders[i];
                headerInfo.Add(new
                {
                    Index = i,
                    AutomationId = header.Current.AutomationId,
                    Name = header.Current.Name,
                    Text = header.Current.Name,
                    IsEnabled = header.Current.IsEnabled
                });
            }

            return new OperationResult
            {
                Success = true,
                Data = headerInfo
            };
        }

        public OperationResult GetColumnHeaders(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return new OperationResult { Success = false, Error = "TablePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var columnHeaders = tablePattern.Current.GetColumnHeaders();
            var headerInfo = new List<object>();

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                var header = columnHeaders[i];
                headerInfo.Add(new
                {
                    Index = i,
                    AutomationId = header.Current.AutomationId,
                    Name = header.Current.Name,
                    Text = header.Current.Name,
                    IsEnabled = header.Current.IsEnabled
                });
            }

            return new OperationResult
            {
                Success = true,
                Data = headerInfo
            };
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
