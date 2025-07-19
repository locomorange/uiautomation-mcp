using System.Threading.Tasks;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<ServerEnhancedResponse<DesktopWindowsResult>> GetWindowsAsync(int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, string scope = "descendants", bool validatePatterns = true, int maxResults = 100, bool useCache = true, int timeoutSeconds = 60);
    }
}
