using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IObjectModelService
    {
        Task<object> GetUnderlyingObjectModelAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}