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
            try
            {
                var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, null, controlType, processId);
                
                if (!findResult.Success)
                {
                    return new OperationResult { Success = false, Error = findResult.Error };
                }

                var elements = findResult.Data ?? new List<ElementInfo>();
                return new OperationResult { Success = true, Data = elements };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = $"GetElementInfo failed: {ex.Message}" };
            }
        }

        public async Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("FindElementsAsync started - SearchText: '{SearchText}', ControlType: '{ControlType}', WindowTitle: '{WindowTitle}', ProcessId: {ProcessId}",
                searchText, controlType, windowTitle, processId);

            try
            {
                var findResult = await _uiAutomationWorker.FindAllAsync(windowTitle, searchText, controlType, processId);
                
                stopwatch.Stop();
                if (findResult.Success)
                {
                    var elements = findResult.Data ?? new List<ElementInfo>();
                    _logger.LogInformation("FindElementsAsync completed in {ElapsedMs}ms, returning {ResultCount} elements", 
                        stopwatch.ElapsedMilliseconds, elements.Count);
                    return new OperationResult { Success = true, Data = elements };
                }
                else
                {
                    _logger.LogError("FindElementsAsync failed after {ElapsedMs}ms: {Error}", stopwatch.ElapsedMilliseconds, findResult.Error);
                    return new OperationResult { Success = false, Error = findResult.Error };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "FindElementsAsync failed after {ElapsedMs}ms: {Error}", stopwatch.ElapsedMilliseconds, ex.Message);
                return new OperationResult { Success = false, Error = $"FindElements failed: {ex.Message}" };
            }
        }
    }
}