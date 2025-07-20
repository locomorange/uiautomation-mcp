using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ISelectionService
    {
        Task<ServerEnhancedResponse<ActionResult>> SelectItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> AddToSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> RemoveFromSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ClearSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        
        
        // SelectionItemPattern properties
        Task<ServerEnhancedResponse<BooleanResult>> IsSelectedAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> GetSelectionContainerAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        
        // SelectionPattern properties
        Task<ServerEnhancedResponse<BooleanResult>> CanSelectMultipleAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<BooleanResult>> IsSelectionRequiredAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<SelectionInfoResult>> GetSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
