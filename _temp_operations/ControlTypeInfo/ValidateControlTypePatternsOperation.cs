using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class ValidateControlTypePatternsOperation : IUIAutomationOperation
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

        public ValidateControlTypePatternsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            try
            {
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<BooleanResult> { Success = false, Error = "Element not found" });

                var controlType = element.Current.ControlType;
                var availablePatterns = element.GetSupportedPatterns()
                    .Select(pattern => pattern.ProgrammaticName)
                    .ToArray();

                var validationDetails = new Dictionary<string, object>
                {
                    ["ElementId"] = elementId,
                    ["ControlType"] = controlType.LocalizedControlType,
                    ["AvailablePatterns"] = availablePatterns
                };

                if (ControlTypePatterns.TryGetValue(controlType, out var expectedPatterns))
                {
                    var missingRequired = expectedPatterns.RequiredPatterns
                        .Where(p => !availablePatterns.Any(ap => ap.Contains(p)))
                        .ToArray();

                    var presentOptional = expectedPatterns.OptionalPatterns
                        .Where(p => availablePatterns.Any(ap => ap.Contains(p)))
                        .ToArray();

                    var unexpectedPatterns = availablePatterns
                        .Where(ap => !expectedPatterns.RequiredPatterns.Concat(expectedPatterns.OptionalPatterns)
                            .Any(ep => ap.Contains(ep)))
                        .ToArray();

                    var isValid = missingRequired.Length == 0;

                    validationDetails["ValidationDetails"] = new
                    {
                        IsValid = isValid,
                        ExpectedRequiredPatterns = expectedPatterns.RequiredPatterns,
                        ExpectedOptionalPatterns = expectedPatterns.OptionalPatterns,
                        MissingRequiredPatterns = missingRequired,
                        PresentOptionalPatterns = presentOptional,
                        UnexpectedPatterns = unexpectedPatterns,
                        Summary = isValid ? "All required patterns are supported" : $"Missing {missingRequired.Length} required pattern(s)"
                    };

                    // Add pattern-specific validation details
                    var patternDetails = new List<object>();
                    foreach (var pattern in availablePatterns)
                    {
                        patternDetails.Add(new
                        {
                            PatternName = pattern,
                            IsRequired = expectedPatterns.RequiredPatterns.Any(rp => pattern.Contains(rp)),
                            IsOptional = expectedPatterns.OptionalPatterns.Any(op => pattern.Contains(op)),
                            IsUnexpected = !expectedPatterns.RequiredPatterns.Concat(expectedPatterns.OptionalPatterns)
                                .Any(ep => pattern.Contains(ep))
                        });
                    }
                    validationDetails["PatternDetails"] = patternDetails;

                    var result = new BooleanResult
                    {
                        Value = isValid,
                        Description = isValid ? "All required patterns are supported" : $"Missing {missingRequired.Length} required pattern(s)"
                    };

                    return Task.FromResult(new OperationResult<BooleanResult> { Success = true, Data = result });
                }
                else
                {
                    validationDetails["ValidationDetails"] = new
                    {
                        IsValid = true,
                        Message = $"No specific pattern requirements defined for control type: {controlType.LocalizedControlType}",
                        Note = "This control type may have custom or framework-specific pattern requirements"
                    };

                    var result = new BooleanResult
                    {
                        Value = true,
                        Description = $"No specific pattern requirements defined for control type: {controlType.LocalizedControlType}"
                    };

                    return Task.FromResult(new OperationResult<BooleanResult> { Success = true, Data = result });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<BooleanResult> { Success = false, Error = ex.Message });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(string parametersJson)
        {
            var typedResult = await ExecuteAsync(parametersJson);
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