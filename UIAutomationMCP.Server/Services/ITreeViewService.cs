using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface ITreeViewService
    {
        Task<object> TreeViewOperationAsync(string elementId, string operation, string? nodePath = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}