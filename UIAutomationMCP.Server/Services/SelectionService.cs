using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Services
{
    public interface ISelectionService
    {
        Task<object> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetSelectionAsync(string containerElementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class SelectionService : ISelectionService
    {
        private readonly ILogger<SelectionService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public SelectionService(
            ILogger<SelectionService> logger, 
            UIAutomationExecutor executor, 
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && pattern is SelectionItemPattern selectionPattern)
                    {
                        selectionPattern.Select();
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support SelectionItemPattern");
                    }
                }, timeoutSeconds, $"SelectElement_{elementId}");

                _logger.LogInformation("Element selected successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetSelectionAsync(string containerElementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting selection from container: {ContainerElementId}", containerElementId);

                var containerElement = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(containerElementId, searchRoot);
                }, timeoutSeconds, $"FindContainer_{containerElementId}");

                if (containerElement == null)
                {
                    return new { Success = false, Error = $"Container element '{containerElementId}' not found" };
                }

                var selectedItems = await _executor.ExecuteAsync(() =>
                {
                    if (containerElement.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) && pattern is SelectionPattern selectionPattern)
                    {
                        var selection = selectionPattern.Current.GetSelection();
                        var selectedInfo = new List<object>();

                        foreach (AutomationElement selectedElement in selection)
                        {
                            try
                            {
                                var elementInfo = _elementInfoExtractor.ExtractElementInfo(selectedElement);
                                selectedInfo.Add(elementInfo);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Failed to extract info for selected element");
                            }
                        }

                        return selectedInfo;
                    }
                    else
                    {
                        throw new InvalidOperationException("Container element does not support SelectionPattern");
                    }
                }, timeoutSeconds, $"GetSelection_{containerElementId}");

                _logger.LogInformation("Selection retrieved successfully from container: {ContainerElementId}", containerElementId);
                return new { Success = true, Data = selectedItems };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get selection from container: {ContainerElementId}", containerElementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}