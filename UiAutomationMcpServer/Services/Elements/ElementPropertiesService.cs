using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementPropertiesService
    {
        Task<OperationResult<Dictionary<string, object>>> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult<Dictionary<string, object>>> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class ElementPropertiesService : IElementPropertiesService
    {
        private readonly ILogger<ElementPropertiesService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public ElementPropertiesService(ILogger<ElementPropertiesService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var operationParams = new AdvancedOperationParameters
            {
                Operation = "get_properties",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = 20
            };

            var result = await _uiAutomationWorker.ExecuteAdvancedOperationAsync(operationParams);
            return new OperationResult<Dictionary<string, object>>
            {
                Success = result.Success,
                Data = result.Data ?? new Dictionary<string, object>(),
                Error = result.Error
            };
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var operationParams = new AdvancedOperationParameters
            {
                Operation = "get_patterns",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = 20
            };

            var result = await _uiAutomationWorker.ExecuteAdvancedOperationAsync(operationParams);
            return new OperationResult<Dictionary<string, object>>
            {
                Success = result.Success,
                Data = result.Data ?? new Dictionary<string, object>(),
                Error = result.Error
            };
        }
    }
}