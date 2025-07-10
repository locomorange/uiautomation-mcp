using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Accessibility
{
    public class GetAccessibilityInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetAccessibilityInfoOperation(ElementFinderService elementFinderService)
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
                var accessibilityInfo = new Dictionary<string, object>
                {
                    ["Name"] = element.Current.Name,
                    ["AutomationId"] = element.Current.AutomationId,
                    ["ControlType"] = element.Current.ControlType.LocalizedControlType,
                    ["LocalizedControlType"] = element.Current.LocalizedControlType,
                    ["HelpText"] = element.Current.HelpText,
                    ["AcceleratorKey"] = element.Current.AcceleratorKey,
                    ["AccessKey"] = element.Current.AccessKey,
                    ["IsKeyboardFocusable"] = element.Current.IsKeyboardFocusable,
                    ["HasKeyboardFocus"] = element.Current.HasKeyboardFocus,
                    ["IsEnabled"] = element.Current.IsEnabled,
                    ["IsVisible"] = !element.Current.IsOffscreen,
                    ["IsContentElement"] = element.Current.IsContentElement,
                    ["IsControlElement"] = element.Current.IsControlElement,
                    ["ItemStatus"] = element.Current.ItemStatus,
                    ["ItemType"] = element.Current.ItemType,
                    ["ClassName"] = element.Current.ClassName,
                    ["FrameworkId"] = element.Current.FrameworkId
                };

                // Check for LabeledBy relationship
                try
                {
                    var labeledByElement = element.Current.LabeledBy;
                    if (labeledByElement != null)
                    {
                        accessibilityInfo["LabeledBy"] = new Dictionary<string, object>
                        {
                            ["Name"] = labeledByElement.Current.Name,
                            ["AutomationId"] = labeledByElement.Current.AutomationId,
                            ["ControlType"] = labeledByElement.Current.ControlType.LocalizedControlType
                        };
                    }
                }
                catch (Exception)
                {
                    // LabeledBy may not be available, continue without it
                }

                // Check for DescribedBy relationship
                try
                {
                    var describedByElements = element.Current.DescribedBy;
                    if (describedByElements != null && describedByElements.Length > 0)
                    {
                        var describedByInfo = new List<Dictionary<string, object>>();
                        foreach (var describedBy in describedByElements)
                        {
                            describedByInfo.Add(new Dictionary<string, object>
                            {
                                ["Name"] = describedBy.Current.Name,
                                ["AutomationId"] = describedBy.Current.AutomationId,
                                ["ControlType"] = describedBy.Current.ControlType.LocalizedControlType
                            });
                        }
                        accessibilityInfo["DescribedBy"] = describedByInfo;
                    }
                }
                catch (Exception)
                {
                    // DescribedBy may not be available, continue without it
                }

                // Get supported patterns for accessibility context
                var supportedPatterns = element.GetSupportedPatterns();
                accessibilityInfo["SupportedPatterns"] = supportedPatterns.Select(p => p.ProgrammaticName).ToList();

                return Task.FromResult(new OperationResult { Success = true, Data = accessibilityInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting accessibility info: {ex.Message}" });
            }
        }
    }
}