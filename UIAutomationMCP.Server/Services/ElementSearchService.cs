using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<object> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
        Task<object> GetWindowsAsync(int timeoutSeconds = 60);
    }

    public class ElementSearchService : IElementSearchService
    {
        private readonly ILogger<ElementSearchService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public ElementSearchService(
            ILogger<ElementSearchService> logger,
            UIAutomationExecutor executor,
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

                var elements = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var condition = BuildSearchCondition(searchText, controlType);
                    
                    var foundElements = searchRoot.FindAll(TreeScope.Descendants, condition);
                    var elementInfoList = new List<ElementInfo>();

                    foreach (AutomationElement element in foundElements)
                    {
                        try
                        {
                            var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                            elementInfoList.Add(elementInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to extract info for element");
                        }
                    }

                    return elementInfoList;
                }, timeoutSeconds, $"FindElements_{windowTitle}_{searchText}");

                _logger.LogInformation("Found {Count} elements", elements.Count);
                return new { Success = true, Data = elements };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding elements");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetWindowsAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting all windows");

                var windows = await _executor.ExecuteAsync(() =>
                {
                    var windowList = new List<WindowInfo>();
                    var desktopElement = AutomationElement.RootElement;
                    
                    var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
                    var windowElements = desktopElement.FindAll(TreeScope.Children, condition);
                    
                    foreach (AutomationElement windowElement in windowElements)
                    {
                        try
                        {
                            var windowInfo = new WindowInfo
                            {
                                ProcessId = windowElement.Current.ProcessId,
                                Title = windowElement.Current.Name ?? "",
                                Name = windowElement.Current.Name ?? "",
                                AutomationId = windowElement.Current.AutomationId ?? "",
                                ClassName = windowElement.Current.ClassName ?? "",
                                BoundingRectangle = new BoundingRectangle
                                {
                                    X = (int)windowElement.Current.BoundingRectangle.X,
                                    Y = (int)windowElement.Current.BoundingRectangle.Y,
                                    Width = (int)windowElement.Current.BoundingRectangle.Width,
                                    Height = (int)windowElement.Current.BoundingRectangle.Height
                                },
                                IsVisible = !windowElement.Current.IsOffscreen,
                                IsEnabled = windowElement.Current.IsEnabled
                            };
                            
                            // 空のタイトルやサイズ0のウィンドウは除外
                            if (!string.IsNullOrEmpty(windowInfo.Title) && 
                                windowInfo.BoundingRectangle.Width > 0 && 
                                windowInfo.BoundingRectangle.Height > 0)
                            {
                                windowList.Add(windowInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to extract window info");
                        }
                    }

                    return windowList;
                }, timeoutSeconds, "GetWindows");

                _logger.LogInformation("Found {Count} windows", windows.Count);
                return new { Success = true, Data = windows };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting windows");
                return new { Success = false, Error = ex.Message };
            }
        }

        private Condition BuildSearchCondition(string? searchText, string? controlType)
        {
            var conditions = new List<Condition>();

            // ControlType条件
            if (!string.IsNullOrEmpty(controlType) && _automationHelper.TryParseControlType(controlType, out var parsedControlType))
            {
                conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, parsedControlType));
            }

            // SearchText条件（NameまたはAutomationId）
            if (!string.IsNullOrEmpty(searchText))
            {
                var nameCondition = new PropertyCondition(AutomationElement.NameProperty, searchText);
                var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, searchText);
                conditions.Add(new OrCondition(nameCondition, automationIdCondition));
            }

            // 条件を組み合わせ
            return conditions.Count switch
            {
                0 => Condition.TrueCondition,
                1 => conditions[0],
                _ => new AndCondition(conditions.ToArray())
            };
        }
    }
}