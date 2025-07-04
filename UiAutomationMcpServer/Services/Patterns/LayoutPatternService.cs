using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ILayoutPatternService
    {
        Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class LayoutPatternService : ILayoutPatternService
    {
        private readonly ILogger<LayoutPatternService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public LayoutPatternService(ILogger<LayoutPatternService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.ScrollElementAsync(elementId, direction, horizontal, vertical, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.ScrollElementIntoViewAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        // All layout pattern operations are now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}
