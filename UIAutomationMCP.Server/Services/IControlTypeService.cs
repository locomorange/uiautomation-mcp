using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IControlTypeService
    {
        Task<object> ComboBoxOperationAsync(string elementId, string operation, string? itemToSelect = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> MenuOperationAsync(string menuPath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> TabOperationAsync(string elementId, string operation, string? tabName = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> TreeViewOperationAsync(string elementId, string operation, string? nodePath = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ListViewOperationAsync(string elementId, string operation, string? itemName = null, int? itemIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
