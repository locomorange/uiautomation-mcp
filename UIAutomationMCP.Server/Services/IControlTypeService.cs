using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IControlTypeService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetControlTypeInfoAsync(
            string elementId, 
            bool validatePatterns = true, 
            bool includeDefaultProperties = true, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30);

        Task<ServerEnhancedResponse<ElementSearchResult>> ValidateControlTypePatternsAsync(
            string elementId, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30);

        Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsByControlTypeAsync(
            string controlType, 
            bool validatePatterns = true, 
            string scope = "descendants", 
            string? windowTitle = null, 
            int? processId = null, 
            int maxResults = 100, 
            int timeoutSeconds = 30);
    }
}