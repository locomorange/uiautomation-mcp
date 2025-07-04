using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ITextPatternService
    {
        Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null);
        Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class TextPatternService : ITextPatternService
    {
        private readonly ILogger<TextPatternService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public TextPatternService(ILogger<TextPatternService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.GetTextAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null)
        {
            var result = await _uiAutomationWorker.SelectTextAsync(elementId, startIndex, length, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null)
        {
            // Note: FindText functionality needs to be implemented in Worker if needed
            return Task.FromResult(new OperationResult { Success = false, Error = "FindText functionality not yet implemented in Worker" });
        }

        public Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            // Note: GetTextSelection functionality needs to be implemented in Worker if needed
            return Task.FromResult(new OperationResult { Success = false, Error = "GetTextSelection functionality not yet implemented in Worker" });
        }

        // All text operations are now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}