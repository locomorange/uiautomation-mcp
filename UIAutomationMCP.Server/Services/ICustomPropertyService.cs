using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface ICustomPropertyService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetCustomPropertiesAsync(string elementId, string[] propertyIds, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> SetCustomPropertyAsync(string elementId, string propertyId, object value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
