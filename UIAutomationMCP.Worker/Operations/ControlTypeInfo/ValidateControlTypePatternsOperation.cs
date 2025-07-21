using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class ValidateControlTypePatternsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ValidateControlTypePatternsOperation> _logger;

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

        public ValidateControlTypePatternsOperation(
            ElementFinderService elementFinderService,
            ILogger<ValidateControlTypePatternsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ValidateControlTypePatternsRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new BooleanResult { Value = false, Description = "Element not found" }
                    });
                }

                var controlType = element.Current.ControlType;
                var availablePatterns = element.GetSupportedPatterns()
                    .Select(pattern => pattern.ProgrammaticName)
                    .ToArray();

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

                    var result = new BooleanResult
                    {
                        Value = isValid,
                        Description = isValid ? "All required patterns are supported" : $"Missing {missingRequired.Length} required pattern(s)"
                    };

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = result 
                    });
                }
                else
                {
                    var result = new BooleanResult
                    {
                        Value = true,
                        Description = $"No specific pattern requirements defined for control type: {controlType.LocalizedControlType}"
                    };

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = result 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateControlTypePatterns operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to validate control type patterns: {ex.Message}",
                    Data = new BooleanResult { Value = false, Description = "Operation failed" }
                });
            }
        }

        private class ControlTypePatternInfo
        {
            public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
            public string[] OptionalPatterns { get; set; } = Array.Empty<string>();
        }
    }
}