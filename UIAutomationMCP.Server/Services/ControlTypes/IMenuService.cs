using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public interface IMenuService
    {
        Task<object> MenuOperationAsync(string menuPath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}