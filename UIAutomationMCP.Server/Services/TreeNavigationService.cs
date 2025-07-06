using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public interface ITreeNavigationService
    {
        Task<object> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60);
    }

    public class TreeNavigationService : ITreeNavigationService
    {
        private readonly ILogger<TreeNavigationService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public TreeNavigationService(
            ILogger<TreeNavigationService> logger,
            UIAutomationExecutor executor,
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree with WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                var tree = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return BuildElementTree(searchRoot, maxDepth, 0);
                }, timeoutSeconds, $"GetElementTree_{windowTitle}");

                _logger.LogInformation("Element tree built successfully");
                return new { Success = true, Data = tree };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new { Success = false, Error = ex.Message };
            }
        }

        private Dictionary<string, object> BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
            var treeNode = new Dictionary<string, object>
            {
                ["ElementInfo"] = elementInfo,
                ["Children"] = new List<Dictionary<string, object>>()
            };

            // 最大深度に達した場合は子要素を探索しない
            if (currentDepth >= maxDepth)
            {
                return treeNode;
            }

            try
            {
                var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                var childList = (List<Dictionary<string, object>>)treeNode["Children"];

                foreach (AutomationElement child in children)
                {
                    try
                    {
                        var childNode = BuildElementTree(child, maxDepth, currentDepth + 1);
                        childList.Add(childNode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to process child element");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get children for element");
            }

            return treeNode;
        }
    }
}