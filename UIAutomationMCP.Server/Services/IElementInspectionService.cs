using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementInspectionService
    {
        Task<object> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}