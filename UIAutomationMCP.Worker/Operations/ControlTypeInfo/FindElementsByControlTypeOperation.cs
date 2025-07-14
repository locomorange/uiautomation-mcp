using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class FindElementsByControlTypeOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        // Control Type and Pattern mapping for validation
        private static readonly Dictionary<string, ControlTypePatternInfo> ControlTypePatterns = new()
        {
            ["Button"] = new() { RequiredPatterns = new[] { "Invoke" }, OptionalPatterns = new[] { "ExpandCollapse", "Toggle" } },
            ["CheckBox"] = new() { RequiredPatterns = new[] { "Toggle" }, OptionalPatterns = Array.Empty<string>() },
            ["ComboBox"] = new() { RequiredPatterns = new[] { "ExpandCollapse" }, OptionalPatterns = new[] { "Value", "Selection" } },
            ["Edit"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Value", "Text", "RangeValue" } },
            ["List"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Grid", "MultipleView", "Scroll" } },
            ["ListItem"] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "ExpandCollapse", "GridItem", "Invoke", "ScrollItem", "Toggle", "Value" } },
            ["Menu"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse" } },
            ["MenuItem"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "Toggle", "SelectionItem" } },
            ["RadioButton"] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Toggle" } },
            ["ScrollBar"] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = Array.Empty<string>() },
            ["Slider"] = new() { RequiredPatterns = new[] { "RangeValue" }, OptionalPatterns = new[] { "Selection", "Value" } },
            ["TabItem"] = new() { RequiredPatterns = new[] { "SelectionItem" }, OptionalPatterns = new[] { "Invoke" } },
            ["Table"] = new() { RequiredPatterns = new[] { "Grid", "Table" }, OptionalPatterns = new[] { "Selection", "Sort" } },
            ["Tree"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Selection", "Scroll" } },
            ["TreeItem"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "ExpandCollapse", "Invoke", "ScrollItem", "SelectionItem", "Toggle" } },
            ["Window"] = new() { RequiredPatterns = Array.Empty<string>(), OptionalPatterns = new[] { "Transform", "Window" } }
        };

        public FindElementsByControlTypeOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ControlTypeSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = ExecuteInternalAsync(request);
            return result;
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }

        private Task<OperationResult<ControlTypeSearchResult>> ExecuteInternalAsync(WorkerRequest request)
        {
            try
            {
                var typedRequest = request.GetTypedRequest<FindElementsByControlTypeRequest>(_options);
                if (typedRequest == null)
                {
                    return Task.FromResult(new OperationResult<ControlTypeSearchResult>
                    {
                        Success = false,
                        Error = "Invalid request format. Expected FindElementsByControlTypeRequest.",
                        Data = new ControlTypeSearchResult()
                    });
                }
                
                var controlType = typedRequest.ControlType;
                var scope = typedRequest.Scope;
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;
                var maxResults = _options.Value.ElementSearch.MaxResults;
                var validatePatterns = _options.Value.ElementSearch.ValidatePatterns;

                if (string.IsNullOrEmpty(controlType))
                    return Task.FromResult(new OperationResult<ControlTypeSearchResult> 
                    { 
                        Success = false, 
                        Error = "ControlType parameter is required",
                        Data = new ControlTypeSearchResult()
                    });

                var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                
                var controlTypeObj = GetControlTypeFromString(controlType);
                if (controlTypeObj == null)
                    return Task.FromResult(new OperationResult<ControlTypeSearchResult> 
                    { 
                        Success = false, 
                        Error = $"Unknown control type: {controlType}",
                        Data = new ControlTypeSearchResult()
                    });

                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj);
                
                // Determine search scope
                TreeScope treeScope = scope.ToLower() switch
                {
                    "children" => TreeScope.Children,
                    "subtree" => TreeScope.Subtree,
                    _ => TreeScope.Descendants
                };

                var elements = searchRoot.FindAll(treeScope, condition);
                var results = new List<ElementInfo>();

                var elementCount = Math.Min(elements.Count, maxResults);
                for (int i = 0; i < elementCount; i++)
                {
                    var element = elements[i];
                    var elementInfo = new ElementInfo
                    {
                        AutomationId = element.Current.AutomationId,
                        Name = element.Current.Name,
                        ControlType = element.Current.ControlType.LocalizedControlType,
                        ClassName = element.Current.ClassName,
                        IsEnabled = element.Current.IsEnabled,
                        IsVisible = !element.Current.IsOffscreen,
                        ProcessId = element.Current.ProcessId,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = element.Current.BoundingRectangle.X,
                            Y = element.Current.BoundingRectangle.Y,
                            Width = element.Current.BoundingRectangle.Width,
                            Height = element.Current.BoundingRectangle.Height
                        }
                    };

                    // Add pattern validation if requested
                    if (validatePatterns)
                    {
                        var availablePatterns = element.GetSupportedPatterns()
                            .Select(pattern => pattern.ProgrammaticName)
                            .ToArray();

                        var availableActions = new Dictionary<string, string>();
                        foreach (var pattern in availablePatterns)
                        {
                            availableActions[pattern] = "Pattern available";
                        }
                        elementInfo.AvailableActions = availableActions;

                        if (ControlTypePatterns.TryGetValue(controlType, out var expectedPatterns))
                        {
                            var missingRequired = expectedPatterns.RequiredPatterns
                                .Where(p => !availablePatterns.Any(ap => ap.Contains(p)))
                                .ToArray();

                            if (missingRequired.Length > 0)
                            {
                                elementInfo.HelpText = $"Missing {missingRequired.Length} required pattern(s): {string.Join(", ", missingRequired)}";
                            }
                            else
                            {
                                elementInfo.HelpText = "All required patterns available";
                            }
                        }
                    }

                    results.Add(elementInfo);
                }

                var searchSummary = new ControlTypeSearchSummary
                {
                    ControlType = controlType,
                    Scope = scope,
                    TotalFound = elements.Count,
                    ValidElements = validatePatterns ? results.Count(r => string.IsNullOrEmpty(r.HelpText) || r.HelpText.Contains("All required")) : results.Count,
                    InvalidElements = validatePatterns ? results.Count(r => !string.IsNullOrEmpty(r.HelpText) && r.HelpText.Contains("Missing")) : 0,
                    MaxResults = maxResults,
                    ValidationEnabled = validatePatterns,
                    SearchDuration = TimeSpan.Zero
                };

                var searchResult = new ControlTypeSearchResult
                {
                    Elements = results,
                    SearchSummary = searchSummary,
                    SearchCriteria = new SearchCriteria
                    {
                        ControlType = controlType,
                        WindowTitle = windowTitle,
                        ProcessId = processId > 0 ? processId : null,
                        Scope = scope,
                        AdditionalCriteria = new Dictionary<string, object>
                        {
                            ["validatePatterns"] = validatePatterns,
                            ["maxResults"] = maxResults
                        }
                    }
                };

                return Task.FromResult(new OperationResult<ControlTypeSearchResult> 
                { 
                    Success = true, 
                    Data = searchResult
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ControlTypeSearchResult> 
                { 
                    Success = false, 
                    Error = ex.Message,
                    Data = new ControlTypeSearchResult()
                });
            }
        }

        private ControlType? GetControlTypeFromString(string controlTypeString)
        {
            return controlTypeString.ToLower() switch
            {
                "button" => ControlType.Button,
                "calendar" => ControlType.Calendar,
                "checkbox" => ControlType.CheckBox,
                "combobox" => ControlType.ComboBox,
                "custom" => ControlType.Custom,
                "datagrid" => ControlType.DataGrid,
                "dataitem" => ControlType.DataItem,
                "document" => ControlType.Document,
                "edit" => ControlType.Edit,
                "group" => ControlType.Group,
                "header" => ControlType.Header,
                "headeritem" => ControlType.HeaderItem,
                "hyperlink" => ControlType.Hyperlink,
                "image" => ControlType.Image,
                "list" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "menu" => ControlType.Menu,
                "menubar" => ControlType.MenuBar,
                "menuitem" => ControlType.MenuItem,
                "pane" => ControlType.Pane,
                "progressbar" => ControlType.ProgressBar,
                "radiobutton" => ControlType.RadioButton,
                "scrollbar" => ControlType.ScrollBar,
                "separator" => ControlType.Separator,
                "slider" => ControlType.Slider,
                "spinner" => ControlType.Spinner,
                "splitbutton" => ControlType.SplitButton,
                "statusbar" => ControlType.StatusBar,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "table" => ControlType.Table,
                "text" => ControlType.Text,
                "thumb" => ControlType.Thumb,
                "titlebar" => ControlType.TitleBar,
                "toolbar" => ControlType.ToolBar,
                "tooltip" => ControlType.ToolTip,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "window" => ControlType.Window,
                _ => null
            };
        }

        private class ControlTypePatternInfo
        {
            public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
            public string[] OptionalPatterns { get; set; } = Array.Empty<string>();
        }
    }
}