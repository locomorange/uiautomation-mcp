using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITableService
    {
        Task<object> GetTableInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetRowHeadersAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetColumnHeadersAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetRowOrColumnMajorAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetColumnHeaderItemsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetRowHeaderItemsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
