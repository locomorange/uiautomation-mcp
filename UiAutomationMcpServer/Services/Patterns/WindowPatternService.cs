using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface IWindowPatternService
    {
        Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null);
    }

    public class WindowPatternService : IWindowPatternService
    {
        private readonly ILogger<WindowPatternService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public WindowPatternService(ILogger<WindowPatternService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null)
        {
            _logger.LogInformation("WindowAction called - ElementId: {ElementId}, Action: {Action}, WindowTitle: {WindowTitle}, ProcessId: {ProcessId}", 
                elementId, action, windowTitle, processId);

            var result = await _uiAutomationWorker.SetWindowStateAsync(elementId, action, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        // All window pattern operations are now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}
