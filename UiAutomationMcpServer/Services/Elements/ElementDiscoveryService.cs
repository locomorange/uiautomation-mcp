using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcp.Models;
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
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public ElementDiscoveryService(ILogger<ElementDiscoveryService> logger, IWindowService windowService, IElementUtilityService elementUtilityService, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null)
        {
            try
            {
                // Use UIAutomationWorker directly
                var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, null, controlType, processId);
                
                if (!findResult.Success)
                {
                    return new OperationResult { Success = false, Error = findResult.Error };
                }

                var elements = findResult.Data ?? new List<ElementInfo>();
                return new OperationResult { Success = true, Data = elements };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = $"GetElementInfo failed: {ex.Message}" };
            }
        }

        public async Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("FindElementsAsync started - SearchText: '{SearchText}', ControlType: '{ControlType}', WindowTitle: '{WindowTitle}', ProcessId: {ProcessId}",
                searchText, controlType, windowTitle, processId);

            try
            {
                // Use UIAutomationWorker directly
                var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, searchText, controlType, processId);
                
                stopwatch.Stop();
                if (findResult.Success)
                {
                    var elements = findResult.Data ?? new List<ElementInfo>();
                    _logger.LogInformation("FindElementsAsync completed in {ElapsedMs}ms, returning {ResultCount} elements", 
                        stopwatch.ElapsedMilliseconds, elements.Count);
                    return new OperationResult { Success = true, Data = elements };
                }
                else
                {
                    _logger.LogError("FindElementsAsync failed after {ElapsedMs}ms: {Error}", stopwatch.ElapsedMilliseconds, findResult.Error);
                    return new OperationResult { Success = false, Error = findResult.Error };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "FindElementsAsync failed after {ElapsedMs}ms: {Error}", stopwatch.ElapsedMilliseconds, ex.Message);
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