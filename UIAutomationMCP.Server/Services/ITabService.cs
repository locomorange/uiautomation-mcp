using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface ITabService
    {
        Task<object> TabOperationAsync(string elementId, string operation, string? tabName = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}