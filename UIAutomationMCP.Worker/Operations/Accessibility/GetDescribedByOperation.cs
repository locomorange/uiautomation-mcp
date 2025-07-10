using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Accessibility
{
    public class GetDescribedByOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetDescribedByOperation(ElementFinderService elementFinderService)
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
                var describedBy = element.Current.DescribedBy;
                if (describedBy == null || describedBy.Length == 0)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element is not described by any other elements" });

                var descriptionInfos = new List<Dictionary<string, object>>();
                foreach (var describingElement in describedBy)
                {
                    var descriptionInfo = new Dictionary<string, object>
                    {
                        ["AutomationId"] = describingElement.Current.AutomationId,
                        ["Name"] = describingElement.Current.Name,
                        ["ControlType"] = describingElement.Current.ControlType.LocalizedControlType,
                        ["LocalizedControlType"] = describingElement.Current.LocalizedControlType,
                        ["ClassName"] = describingElement.Current.ClassName,
                        ["IsEnabled"] = describingElement.Current.IsEnabled,
                        ["IsVisible"] = !describingElement.Current.IsOffscreen,
                        ["BoundingRectangle"] = new BoundingRectangle
                        {
                            X = describingElement.Current.BoundingRectangle.X,
                            Y = describingElement.Current.BoundingRectangle.Y,
                            Width = describingElement.Current.BoundingRectangle.Width,
                            Height = describingElement.Current.BoundingRectangle.Height
                        },
                        ["HelpText"] = describingElement.Current.HelpText,
                        ["AccessKey"] = describingElement.Current.AccessKey,
                        ["AcceleratorKey"] = describingElement.Current.AcceleratorKey
                    };

                    // Try to get the text content if it's a text element
                    if (describingElement.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern value)
                    {
                        descriptionInfo["Value"] = value.Current.Value;
                    }
                    else if (describingElement.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern) && textPattern is TextPattern text)
                    {
                        try
                        {
                            var textRange = text.DocumentRange;
                            descriptionInfo["Text"] = textRange.GetText(-1);
                        }
                        catch (Exception)
                        {
                            // Text pattern may not be fully accessible
                        }
                    }

                    descriptionInfos.Add(descriptionInfo);
                }

                return Task.FromResult(new OperationResult { Success = true, Data = descriptionInfos });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting described by elements: {ex.Message}" });
            }
        }
    }
}