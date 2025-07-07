using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IButtonService
    {
        Task<object> ButtonOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}