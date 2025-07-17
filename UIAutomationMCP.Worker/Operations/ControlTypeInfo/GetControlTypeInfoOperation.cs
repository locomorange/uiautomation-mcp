using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class GetControlTypeInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        // Control Type and Pattern mapping based on Microsoft Documentation
        private static readonly Dictionary<ControlType, ControlTypePatternInfo> ControlTypePatterns = new()
        {
            [ControlType.Button] = new() { RequiredPatterns = new[] { "Invoke" }, OptionalPatterns = new[] { "ExpandCollapse", "Toggle" } },
            [ControlType.CheckBox] = new() { RequiredPatterns = new[] { "Toggle" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.ComboBox] = new() { RequiredPatterns = new[] { "ExpandCollapse" }, OptionalPatterns = new[] { "Value", "Selection" } },
            [ControlType.Edit] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Value", "Text", "RangeValue" } },
            [ControlType.List] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Grid", "MultipleView", "Scroll" } },
            [ControlType.ListItem] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "ExpandCollapse", "GridItem", "Invoke", "ScrollItem", "Toggle", "Value" } },
            [ControlType.Menu] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse" } },
            [ControlType.MenuItem] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "Toggle", "SelectionItem" } },
            [ControlType.RadioButton] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Toggle" } },
            [ControlType.ScrollBar] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = Array.Empty<string>() },
            [ControlType.Slider] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = new[] { "Selection", "Value" } },
            [ControlType.TabItem] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Invoke" } },
            [ControlType.Table] = new() { RequiredPatterns = new[] { "Grid", "Table" }, OptionalPatterns = new[] { "Selection", "Sort" } },
            [ControlType.Tree] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Scroll" } },
            [ControlType.TreeItem] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "ScrollItem", "SelectionItem", "Toggle" } },
            [ControlType.Window] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Transform", "Window" } }
        };

        public GetControlTypeInfoOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ControlTypeInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            try
            {
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                var validatePatterns = request.Parameters?.GetValueOrDefault("validatePatterns")?.ToString() == "True";
                var includeDefaultProperties = request.Parameters?.GetValueOrDefault("includeDefaultProperties")?.ToString() == "True";

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<ControlTypeInfoResult> { Success = false, Error = "Element not found" });

                var controlType = element.Current.ControlType;
                var controlTypeInfo = new ControlTypeInfoResult
                {
                    ControlType = controlType.LocalizedControlType,
                    ControlTypeName = controlType.ProgrammaticName,
                    LocalizedControlType = element.Current.LocalizedControlType,
                    AutomationId = element.Current.AutomationId,
                    Name = element.Current.Name
                };

                // Get available patterns
                var availablePatterns = element.GetSupportedPatterns()
                    .Select(pattern => pattern.ProgrammaticName)
                    .ToList();

                controlTypeInfo.AvailablePatterns = availablePatterns;

                // Pattern validation if requested
                if (validatePatterns && ControlTypePatterns.TryGetValue(controlType, out var expectedPatterns))
                {
                    var missingRequired = expectedPatterns.RequiredPatterns
                        .Where(p => !availablePatterns.Any(ap => ap.Contains(p)))
                        .ToArray();

                    var unexpectedPatterns = availablePatterns
                        .Where(ap => !expectedPatterns.RequiredPatterns.Concat(expectedPatterns.OptionalPatterns)
                            .Any(ep => ap.Contains(ep)))
                        .ToArray();

                    controlTypeInfo.PatternValidation = new Dictionary<string, bool>
                    {
                        ["HasAllRequiredPatterns"] = missingRequired.Length == 0,
                        ["HasValidPatterns"] = unexpectedPatterns.Length == 0
                    };
                }

                // Include default properties if requested
                if (includeDefaultProperties)
                {
                    controlTypeInfo.DefaultProperties = new Dictionary<string, object>
                    {
                        ["IsEnabled"] = element.Current.IsEnabled,
                        ["IsVisible"] = !element.Current.IsOffscreen,
                        ["HasKeyboardFocus"] = element.Current.HasKeyboardFocus,
                        ["IsKeyboardFocusable"] = element.Current.IsKeyboardFocusable,
                        ["ClassName"] = element.Current.ClassName ?? "",
                        ["FrameworkId"] = element.Current.FrameworkId ?? ""
                    };
                }

                return Task.FromResult(new OperationResult<ControlTypeInfoResult> { Success = true, Data = controlTypeInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ControlTypeInfoResult> { Success = false, Error = ex.Message });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }

        private class ControlTypePatternInfo
        {
            public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
            public string[] OptionalPatterns { get; set; } = Array.Empty<string>();
        }
    }
}