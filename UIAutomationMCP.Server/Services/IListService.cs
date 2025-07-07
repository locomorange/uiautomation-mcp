using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IListService
    {
        Task<object> ListOperationAsync(string elementId, string operation, string? itemName = null, int? itemIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}