using System.Threading.Tasks;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, string scope = "descendants", bool validatePatterns = true, int maxResults = 100, bool useCache = true, int timeoutSeconds = 60);
        
        // New unified search method
        Task<ServerEnhancedResponse<SearchElementsResult>> SearchElementsAsync(SearchElementsRequest request, int timeoutSeconds = 30);
    }
}
