using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface ICustomPropertyService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> SetCustomPropertyAsync(string? automationId = null, string? name = null, string propertyId = "", object? value = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
