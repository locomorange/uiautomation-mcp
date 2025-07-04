using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ICorePatternService
    {
        Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class CorePatternService : ICorePatternService
    {
        private readonly ILogger<CorePatternService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public CorePatternService(ILogger<CorePatternService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.InvokeElementAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.SetElementValueAsync(elementId, value, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.GetElementValueAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.ToggleElementAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.SelectElementAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        // All pattern execution is now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}