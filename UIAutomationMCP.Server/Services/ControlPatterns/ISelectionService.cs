using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ISelectionService
    {
        Task<object> SelectItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> AddToSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> RemoveFromSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ClearSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        
        // SelectionPattern properties
        Task<object> CanSelectMultipleAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> IsSelectionRequiredAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        
        // SelectionItemPattern properties
        Task<object> IsSelectedAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetSelectionContainerAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
