using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementDiscoveryService
    {
        Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null);
        Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null);
    }

    public class ElementDiscoveryService : IElementDiscoveryService
    {
        private readonly ILogger<ElementDiscoveryService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public ElementDiscoveryService(ILogger<ElementDiscoveryService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null)
        {
            var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, null, controlType, processId);
            return new OperationResult { Success = findResult.Success, Data = findResult.Data, Error = findResult.Error };
        }

        public async Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
        {
            var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, searchText, controlType, processId);
            return new OperationResult { Success = findResult.Success, Data = findResult.Data, Error = findResult.Error };
        }
    }
}