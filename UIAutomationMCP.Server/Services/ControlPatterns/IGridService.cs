using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IGridService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderAsync(string gridElementId, int row, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderAsync(string gridElementId, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
