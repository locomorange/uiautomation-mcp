using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IGridService
    {
        Task<object> GetGridInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetRowHeaderAsync(string gridElementId, int row, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetColumnHeaderAsync(string gridElementId, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
