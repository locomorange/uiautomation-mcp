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

        public ElementDiscoveryService(ILogger<ElementDiscoveryService> logger, IWindowService windowService, IElementUtilityService elementUtilityService)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
        }

        public Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null)
        {
            try
            {
                var elements = new List<ElementInfo>();
                AutomationElementCollection? elementCollection = null;

                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
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

                try
                {
                    elementCollection = searchRoot.FindAll(TreeScope.Descendants, condition);
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Failed to find elements: {ex.Message}" });
                }

                if (elementCollection == null)
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = elements });
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
                                BoundingRectangle = element != null ? new BoundingRectangle
                                {
                                    X = double.IsInfinity(element.Current.BoundingRectangle.X) ? 0 : element.Current.BoundingRectangle.X,
                                    Y = double.IsInfinity(element.Current.BoundingRectangle.Y) ? 0 : element.Current.BoundingRectangle.Y,
                                    Width = double.IsInfinity(element.Current.BoundingRectangle.Width) ? 0 : element.Current.BoundingRectangle.Width,
                                    Height = double.IsInfinity(element.Current.BoundingRectangle.Height) ? 0 : element.Current.BoundingRectangle.Height
                                } : new BoundingRectangle(),
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

                return Task.FromResult(new OperationResult { Success = true, Data = elements });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"GetElementInfo failed: {ex.Message}" });
            }
        }

        public Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
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
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
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

                Condition searchCondition = conditions.Count > 0 ? new OrCondition(conditions.ToArray()) : Condition.TrueCondition;

                try
                {
                    var elementCollection = searchRoot.FindAll(TreeScope.Descendants, searchCondition);

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
                                    BoundingRectangle = element != null ? new BoundingRectangle
                                    {
                                        X = double.IsInfinity(element.Current.BoundingRectangle.X) ? 0 : element.Current.BoundingRectangle.X,
                                        Y = double.IsInfinity(element.Current.BoundingRectangle.Y) ? 0 : element.Current.BoundingRectangle.Y,
                                        Width = double.IsInfinity(element.Current.BoundingRectangle.Width) ? 0 : element.Current.BoundingRectangle.Width,
                                        Height = double.IsInfinity(element.Current.BoundingRectangle.Height) ? 0 : element.Current.BoundingRectangle.Height
                                    } : new BoundingRectangle(),
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
                catch (Exception ex)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Failed to search elements: {ex.Message}" });
                }

                return Task.FromResult(new OperationResult { Success = true, Data = elements });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"FindElements failed: {ex.Message}" });
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
    }
}