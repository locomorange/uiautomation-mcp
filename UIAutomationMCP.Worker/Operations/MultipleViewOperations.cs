using System.Windows.Automation;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    public class MultipleViewOperations
    {
        public OperationResult GetAvailableViews(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return new OperationResult { Success = false, Error = "MultipleViewPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var supportedViews = multipleViewPattern.Current.GetSupportedViews();
            var viewInfo = new List<object>();

            foreach (var viewId in supportedViews)
            {
                var viewName = multipleViewPattern.GetViewName(viewId);
                viewInfo.Add(new
                {
                    ViewId = viewId,
                    ViewName = viewName
                });
            }

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    CurrentView = multipleViewPattern.Current.CurrentView,
                    SupportedViews = viewInfo
                }
            };
        }

        public OperationResult SetView(string elementId, int viewId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return new OperationResult { Success = false, Error = "MultipleViewPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var supportedViews = multipleViewPattern.Current.GetSupportedViews();
            if (!supportedViews.Contains(viewId))
                return new OperationResult { Success = false, Error = $"View ID {viewId} is not supported" };

            var oldView = multipleViewPattern.Current.CurrentView;
            multipleViewPattern.SetCurrentView(viewId);
            var newView = multipleViewPattern.Current.CurrentView;

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    OldView = oldView,
                    NewView = newView,
                    ViewName = multipleViewPattern.GetViewName(newView)
                }
            };
        }

        public OperationResult GetCurrentView(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return new OperationResult { Success = false, Error = "MultipleViewPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var currentView = multipleViewPattern.Current.CurrentView;
            var viewName = multipleViewPattern.GetViewName(currentView);

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    CurrentView = currentView,
                    ViewName = viewName
                }
            };
        }

        public OperationResult GetViewName(string elementId, int viewId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return new OperationResult { Success = false, Error = "MultipleViewPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var viewName = multipleViewPattern.GetViewName(viewId);

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    ViewId = viewId,
                    ViewName = viewName
                }
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