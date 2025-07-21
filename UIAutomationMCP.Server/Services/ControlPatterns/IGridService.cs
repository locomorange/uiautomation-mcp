using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IGridService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetGridItemAsync(string? automationId = null, string? name = null, int row = 0, int column = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderAsync(string? automationId = null, string? name = null, int row = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderAsync(string? automationId = null, string? name = null, int column = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<GridInfoResult>> GetGridInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
