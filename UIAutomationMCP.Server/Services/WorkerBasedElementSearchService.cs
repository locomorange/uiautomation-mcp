using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Models;
using System.Windows.Automation;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// Worker APIを使用した要素検索サービス
    /// </summary>
    public class WorkerBasedElementSearchService : IElementSearchService
    {
        private readonly ILogger<WorkerBasedElementSearchService> _logger;
        private readonly WorkerExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public WorkerBasedElementSearchService(
            ILogger<WorkerBasedElementSearchService> logger,
            WorkerExecutor executor,
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Finding elements with WindowTitle={WindowTitle}, SearchText={SearchText}, ControlType={ControlType}, ProcessId={ProcessId}",
                    windowTitle, searchText, controlType, processId);

                var result = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        var rootResult = _executor.Search.GetRootElement();
                        if (!rootResult.Success || rootResult.Data == null)
                        {
                            throw new InvalidOperationException($"Failed to get root element: {rootResult.Error}");
                        }
                        searchRoot = rootResult.Data;
                    }

                    var condition = BuildSearchCondition(searchText, controlType);
                    var elementsResult = _executor.Search.FindElements(searchRoot, TreeScope.Descendants, condition);
                    
                    if (!elementsResult.Success || elementsResult.Data == null)
                    {
                        throw new InvalidOperationException($"Failed to find elements: {elementsResult.Error}");
                    }

                    var elementInfoList = new List<UIAutomationMCP.Models.ElementInfo>();
                    foreach (AutomationElement element in elementsResult.Data)
                    {
                        try
                        {
                            var serverElementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                            // Convert Server.Models.ElementInfo to Models.ElementInfo
                            var elementInfo = new UIAutomationMCP.Models.ElementInfo
                            {
                                Name = serverElementInfo.Name,
                                AutomationId = serverElementInfo.AutomationId,
                                ControlType = serverElementInfo.ControlType,
                                ClassName = serverElementInfo.ClassName,
                                ProcessId = serverElementInfo.ProcessId,
                                BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle
                                {
                                    X = serverElementInfo.BoundingRectangle.X,
                                    Y = serverElementInfo.BoundingRectangle.Y,
                                    Width = serverElementInfo.BoundingRectangle.Width,
                                    Height = serverElementInfo.BoundingRectangle.Height
                                },
                                IsEnabled = serverElementInfo.IsEnabled,
                                IsVisible = serverElementInfo.IsVisible,
                                HelpText = serverElementInfo.HelpText,
                                Value = serverElementInfo.Value,
                                AvailableActions = serverElementInfo.AvailableActions
                            };
                            elementInfoList.Add(elementInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to extract info for element");
                        }
                    }

                    return elementInfoList;
                }, timeoutSeconds, "FindElements");

                _logger.LogInformation("Found {Count} elements", result.Count);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetWindowsAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting desktop windows");

                var result = await _executor.ExecuteAsync(() =>
                {
                    var windowsResult = _executor.Search.GetDesktopWindows();
                    if (!windowsResult.Success || windowsResult.Data == null)
                    {
                        throw new InvalidOperationException($"Failed to get desktop windows: {windowsResult.Error}");
                    }

                    var windowInfoList = new List<UIAutomationMCP.Models.WindowInfo>();
                    foreach (AutomationElement window in windowsResult.Data)
                    {
                        try
                        {
                            var windowInfo = ExtractWindowInfo(window);
                            windowInfoList.Add(windowInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to extract window info");
                        }
                    }

                    return windowInfoList;
                }, timeoutSeconds, "GetWindows");

                _logger.LogInformation("Found {Count} windows", result.Count);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get windows");
                return new { Success = false, Error = ex.Message };
            }
        }

        private Condition BuildSearchCondition(string? searchText, string? controlType)
        {
            var conditions = new List<Condition>();

            if (!string.IsNullOrEmpty(searchText))
            {
                var nameCondition = new PropertyCondition(AutomationElement.NameProperty, searchText);
                var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, searchText);
                conditions.Add(new OrCondition(nameCondition, automationIdCondition));
            }

            if (!string.IsNullOrEmpty(controlType))
            {
                if (TryParseControlType(controlType, out var parsedControlType))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, parsedControlType));
                }
            }

            return conditions.Count switch
            {
                0 => Condition.TrueCondition,
                1 => conditions[0],
                _ => new AndCondition(conditions.ToArray())
            };
        }

        private bool TryParseControlType(string controlTypeString, out ControlType controlType)
        {
            controlType = controlTypeString.ToLowerInvariant() switch
            {
                "button" => ControlType.Button,
                "edit" => ControlType.Edit,
                "text" => ControlType.Text,
                "list" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "menu" => ControlType.Menu,
                "menuitem" => ControlType.MenuItem,
                "window" => ControlType.Window,
                "combobox" => ControlType.ComboBox,
                "checkbox" => ControlType.CheckBox,
                "radiobutton" => ControlType.RadioButton,
                "image" => ControlType.Image,
                "hyperlink" => ControlType.Hyperlink,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                _ => null
            };

            return controlType != null;
        }

        private UIAutomationMCP.Models.WindowInfo ExtractWindowInfo(AutomationElement window)
        {
            var nameResult = _executor.Property.GetName(window);
            var automationIdResult = _executor.Property.GetAutomationId(window);
            var processIdResult = _executor.Property.GetProcessId(window);
            var classNameResult = _executor.Property.GetClassName(window);
            var boundingRectResult = _executor.Property.GetBoundingRectangle(window);
            var isEnabledResult = _executor.Property.IsEnabled(window);
            var isVisibleResult = _executor.Property.IsVisible(window);

            var boundingRect = boundingRectResult.Success ? boundingRectResult.Data : new System.Windows.Rect();

            return new UIAutomationMCP.Models.WindowInfo
            {
                Name = nameResult.Success ? nameResult.Data ?? "" : "",
                Title = nameResult.Success ? nameResult.Data ?? "" : "",
                AutomationId = automationIdResult.Success ? automationIdResult.Data ?? "" : "",
                ProcessId = processIdResult.Success ? processIdResult.Data : 0,
                ProcessName = GetProcessName(processIdResult.Success ? processIdResult.Data : 0),
                ClassName = classNameResult.Success ? classNameResult.Data ?? "" : "",
                Handle = 0, // Not available through UI Automation
                BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle
                {
                    X = boundingRect.X,
                    Y = boundingRect.Y,
                    Width = boundingRect.Width,
                    Height = boundingRect.Height
                },
                IsEnabled = isEnabledResult.Success && isEnabledResult.Data,
                IsVisible = isVisibleResult.Success && isVisibleResult.Data
            };
        }

        private string GetProcessName(int processId)
        {
            try
            {
                if (processId <= 0) return "";
                using var process = System.Diagnostics.Process.GetProcessById(processId);
                return process.ProcessName;
            }
            catch
            {
                return "";
            }
        }
    }
}