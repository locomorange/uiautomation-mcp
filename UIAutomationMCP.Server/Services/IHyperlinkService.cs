using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IHyperlinkService
    {
        Task<object> HyperlinkOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}