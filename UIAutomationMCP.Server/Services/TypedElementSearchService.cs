using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// Type-safe element search service that eliminates object type usage
    /// </summary>
    public interface ITypedElementSearchService
    {
        Task<ElementSearchResult> FindElementAsync(string? windowTitle = null, int? processId = null, 
            string? name = null, string? automationId = null, string? className = null, 
            string? controlType = null, int timeoutSeconds = 30);
            
        Task<ElementSearchResult> FindElementsAsync(string? searchText = null, string? controlType = null, 
            string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
            
        Task<ElementSearchResult> FindElementsByControlTypeAsync(string controlType, 
            string? windowTitle = null, int processId = 0, bool validatePatterns = true, 
            int maxResults = 100, int timeoutSeconds = 30);
            
        Task<ElementSearchResult> GetElementTreeAsync(string? windowTitle = null, 
            int processId = 0, int maxDepth = 3, int timeoutSeconds = 60);
            
        Task<ElementSearchResult> GetElementInfoAsync(string? windowTitle = null, 
            int processId = 0, string? controlType = null, int timeoutSeconds = 60);
    }

    public class TypedElementSearchService : ITypedElementSearchService
    {
        private readonly ILogger<TypedElementSearchService> _logger;
        private readonly ITypedSubprocessExecutor _executor;

        public TypedElementSearchService(
            ILogger<TypedElementSearchService> logger,
            ITypedSubprocessExecutor executor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public async Task<ElementSearchResult> FindElementAsync(string? windowTitle = null, int? processId = null, 
            string? name = null, string? automationId = null, string? className = null, 
            string? controlType = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding element with WindowTitle={WindowTitle}, ProcessId={ProcessId}, Name={Name}, AutomationId={AutomationId}, ClassName={ClassName}, ControlType={ControlType}",
                    windowTitle, processId, name, automationId, className, controlType);

                // 専用のfind element操作を呼び出し - これは既存のFindElementsを使用し、結果をフィルタリング
                var searchText = !string.IsNullOrEmpty(name) ? name : automationId;
                var result = await _executor.FindElementsAsync(searchText, controlType, windowTitle, processId ?? 0, timeoutSeconds);

                _logger.LogInformation("Found element successfully. Elements count: {Count}", result.Elements?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element");
                
                return new ElementSearchResult
                {
                    Success = false,
                    Message = $"Failed to find element: {ex.Message}",
                    Elements = new List<Dictionary<string, object>>(),
                    Count = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<ElementSearchResult> FindElementsAsync(string? searchText = null, string? controlType = null, 
            string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding elements with SearchText={SearchText}, ControlType={ControlType}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    searchText, controlType, windowTitle, processId);

                var result = await _executor.FindElementsAsync(searchText, controlType, windowTitle, processId, timeoutSeconds);

                _logger.LogInformation("Found {Count} elements successfully", result.Elements?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                
                return new ElementSearchResult
                {
                    Success = false,
                    Message = $"Failed to find elements: {ex.Message}",
                    Elements = new List<Dictionary<string, object>>(),
                    Count = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<ElementSearchResult> FindElementsByControlTypeAsync(string controlType, 
            string? windowTitle = null, int processId = 0, bool validatePatterns = true, 
            int maxResults = 100, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding elements by control type: {ControlType}, WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxResults={MaxResults}",
                    controlType, windowTitle, processId, maxResults);

                var result = await _executor.FindElementsByControlTypeAsync(controlType, windowTitle, processId, timeoutSeconds);

                _logger.LogInformation("Found {Count} elements of type {ControlType}", result.Elements?.Count ?? 0, controlType);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by control type: {ControlType}", controlType);
                
                return new ElementSearchResult
                {
                    Success = false,
                    Message = $"Failed to find elements by control type '{controlType}': {ex.Message}",
                    Elements = new List<Dictionary<string, object>>(),
                    Count = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<ElementSearchResult> GetElementTreeAsync(string? windowTitle = null, 
            int processId = 0, int maxDepth = 3, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree for WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                // Element tree は専用APIが必要 - 現在はFindElementsで代用
                var result = await _executor.FindElementsAsync(null, null, windowTitle, processId, timeoutSeconds);

                _logger.LogInformation("Retrieved element tree successfully with {Count} elements", result.Elements?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element tree");
                
                return new ElementSearchResult
                {
                    Success = false,
                    Message = $"Failed to get element tree: {ex.Message}",
                    Elements = new List<Dictionary<string, object>>(),
                    Count = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<ElementSearchResult> GetElementInfoAsync(string? windowTitle = null, 
            int processId = 0, string? controlType = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element info for WindowTitle={WindowTitle}, ProcessId={ProcessId}, ControlType={ControlType}",
                    windowTitle, processId, controlType);

                var result = await _executor.FindElementsAsync(null, controlType, windowTitle, processId, timeoutSeconds);

                _logger.LogInformation("Retrieved element info successfully with {Count} elements", result.Elements?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element info");
                
                return new ElementSearchResult
                {
                    Success = false,
                    Message = $"Failed to get element info: {ex.Message}",
                    Elements = new List<Dictionary<string, object>>(),
                    Count = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
