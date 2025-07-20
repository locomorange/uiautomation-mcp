using System.Threading.Tasks;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<ServerEnhancedResponse<DesktopWindowsResult>> GetWindowsAsync(int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, string scope = "descendants", bool validatePatterns = true, int maxResults = 100, bool useCache = true, int timeoutSeconds = 60);
        
        // New lightweight search methods
        Task<ServerEnhancedResponse<SearchElementsResult>> SearchElementsAsync(SearchElementsRequest request);
        Task<ServerEnhancedResponse<ElementDetailResult>> GetElementDetailsAsync(GetElementDetailsRequest request);
    }
}
