using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface IRangePatternService
    {
        Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class RangePatternService : IRangePatternService
    {
        private readonly ILogger<RangePatternService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public RangePatternService(ILogger<RangePatternService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.SetRangeValueAsync(elementId, value, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.GetRangeValueAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        // All range value operations are now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}
