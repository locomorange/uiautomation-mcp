using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITableService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> GetRowOrColumnMajorAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TableInfoResult>> GetTableInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
