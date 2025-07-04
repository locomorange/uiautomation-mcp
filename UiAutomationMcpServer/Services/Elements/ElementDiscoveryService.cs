using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementDiscoveryService
    {
        Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null);
        Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null);
    }

    public class ElementDiscoveryService : IElementDiscoveryService
    {
        private readonly ILogger<ElementDiscoveryService> _logger;
        private readonly IWindowService _windowService;
        private readonly IElementUtilityService _elementUtilityService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public ElementDiscoveryService(ILogger<ElementDiscoveryService> logger, IWindowService windowService, IElementUtilityService elementUtilityService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null)
        {
            try
            {
                var elements = new List<ElementInfo>();

                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                }

                Condition condition = Condition.TrueCondition;
                if (!string.IsNullOrEmpty(controlType))
                {
                    var controlTypeObj = GetControlType(controlType);
                    if (controlTypeObj != null)
                    {
                        condition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj);
                    }
                }

                var findResult = await _uiAutomationHelper.FindAllAsync(searchRoot, TreeScope.Descendants, condition);
                if (!findResult.Success)
                {
                    return new OperationResult { Success = false, Error = findResult.Error };
                }

                var elementCollection = findResult.Data;
                if (elementCollection == null)
                {
                    return new OperationResult { Success = true, Data = elements };
                }

                foreach (AutomationElement element in elementCollection)
                {
                    try
                    {
                        var name = element?.Current.Name ?? "";
                        var automationId = element?.Current.AutomationId ?? "";

                        if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(automationId))
                        {
                            elements.Add(new ElementInfo
                            {
                                Name = name,
                                AutomationId = automationId,
                                ControlType = element?.Current.ControlType.ProgrammaticName ?? "",
                                ClassName = element?.Current.ClassName ?? "",
                                ProcessId = element?.Current.ProcessId ?? 0,
                                IsEnabled = element?.Current.IsEnabled ?? false,
                                IsVisible = !(element?.Current.IsOffscreen ?? true),
                                BoundingRectangle = element != null ? SafeGetBoundingRectangle(element) : new BoundingRectangle(),
                                Value = _elementUtilityService.GetElementValue(element),
                                AvailableActions = _elementUtilityService.GetAvailableActions(element)
                            });
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return new OperationResult { Success = true, Data = elements };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = $"GetElementInfo failed: {ex.Message}" };
            }
        }

        public async Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elements = new List<ElementInfo>();

                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                }

                List<Condition> conditions = new List<Condition>();

                if (!string.IsNullOrEmpty(searchText))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, searchText));
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, searchText));
                }

                if (!string.IsNullOrEmpty(controlType))
                {
                    var controlTypeObj = GetControlType(controlType);
                    if (controlTypeObj != null)
                    {
                        conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj));
                    }
                }

                Condition searchCondition;
                if (conditions.Count == 0)
                {
                    searchCondition = Condition.TrueCondition;
                }
                else if (conditions.Count == 1)
                {
                    searchCondition = conditions[0];
                }
                else
                {
                    // If we have both searchText and controlType, use AND condition
                    // If we have only searchText with multiple conditions (name OR automationId), use OR condition
                    if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(controlType))
                    {
                        var textConditions = conditions.Take(2).ToArray(); // name OR automationId
                        var textCondition = textConditions.Length > 1 ? new OrCondition(textConditions) : textConditions[0];
                        var controlTypeCondition = conditions.Last(); // controlType
                        searchCondition = new AndCondition(textCondition, controlTypeCondition);
                    }
                    else
                    {
                        searchCondition = new OrCondition(conditions.ToArray());
                    }
                }

                var findResult = await _uiAutomationHelper.FindAllAsync(searchRoot, TreeScope.Descendants, searchCondition);
                if (!findResult.Success)
                {
                    return new OperationResult { Success = false, Error = findResult.Error };
                }

                var elementCollection = findResult.Data;
                if (elementCollection != null)
                {

                    foreach (AutomationElement element in elementCollection)
                    {
                        try
                        {
                            var name = element?.Current.Name ?? "";
                            var automationId = element?.Current.AutomationId ?? "";

                            if (string.IsNullOrEmpty(searchText) ||
                                (!string.IsNullOrEmpty(name) && name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(automationId) && automationId.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                            {
                                elements.Add(new ElementInfo
                                {
                                    Name = name,
                                    AutomationId = automationId,
                                    ControlType = element?.Current.ControlType.ProgrammaticName ?? "",
                                    ClassName = element?.Current.ClassName ?? "",
                                    ProcessId = element?.Current.ProcessId ?? 0,
                                    IsEnabled = element?.Current.IsEnabled ?? false,
                                    IsVisible = !(element?.Current.IsOffscreen ?? true),
                                    BoundingRectangle = element != null ? SafeGetBoundingRectangle(element) : new BoundingRectangle(),
                                    Value = _elementUtilityService.GetElementValue(element),
                                    AvailableActions = _elementUtilityService.GetAvailableActions(element)
                                });
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                return new OperationResult { Success = true, Data = elements };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = $"FindElements failed: {ex.Message}" };
            }
        }

        private ControlType? GetControlType(string controlTypeName)
        {
            return controlTypeName.ToLower() switch
            {
                "button" => ControlType.Button,
                "edit" => ControlType.Edit,
                "text" => ControlType.Text,
                "combobox" => ControlType.ComboBox,
                "listbox" => ControlType.List,
                "listitem" => ControlType.ListItem,
                "checkbox" => ControlType.CheckBox,
                "radiobutton" => ControlType.RadioButton,
                "tree" => ControlType.Tree,
                "treeitem" => ControlType.TreeItem,
                "tab" => ControlType.Tab,
                "tabitem" => ControlType.TabItem,
                "menu" => ControlType.Menu,
                "menuitem" => ControlType.MenuItem,
                "window" => ControlType.Window,
                "pane" => ControlType.Pane,
                "group" => ControlType.Group,
                "hyperlink" => ControlType.Hyperlink,
                "image" => ControlType.Image,
                "table" => ControlType.Table,
                "dataitem" => ControlType.DataItem,
                "document" => ControlType.Document,
                "slider" => ControlType.Slider,
                "progressbar" => ControlType.ProgressBar,
                "scrollbar" => ControlType.ScrollBar,
                "spinner" => ControlType.Spinner,
                "statusbar" => ControlType.StatusBar,
                "toolbar" => ControlType.ToolBar,
                "tooltip" => ControlType.ToolTip,
                "calendar" => ControlType.Calendar,
                "datagrid" => ControlType.DataGrid,
                "splitbutton" => ControlType.SplitButton,
                _ => null
            };
        }

        private BoundingRectangle SafeGetBoundingRectangle(AutomationElement element)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                
                // Check for invalid values that would cause JSON serialization issues
                if (double.IsInfinity(rect.Left) || double.IsInfinity(rect.Top) ||
                    double.IsInfinity(rect.Width) || double.IsInfinity(rect.Height) ||
                    double.IsNaN(rect.Left) || double.IsNaN(rect.Top) ||
                    double.IsNaN(rect.Width) || double.IsNaN(rect.Height))
                {
                    return new BoundingRectangle { X = 0, Y = 0, Width = 0, Height = 0 };
                }
                
                return new BoundingRectangle
                {
                    X = rect.Left,
                    Y = rect.Top,
                    Width = rect.Width,
                    Height = rect.Height
                };
            }
            catch (Exception)
            {
                return new BoundingRectangle { X = 0, Y = 0, Width = 0, Height = 0 };
            }
        }
    }
}