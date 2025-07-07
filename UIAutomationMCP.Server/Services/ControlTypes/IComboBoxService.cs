using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public interface IComboBoxService
    {
        Task<object> ComboBoxOperationAsync(string elementId, string operation, string? itemToSelect = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}