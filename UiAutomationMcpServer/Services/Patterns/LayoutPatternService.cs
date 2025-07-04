using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ILayoutPatternService
    {
        Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null);
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

        public async Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var result = await _uiAutomationWorker.ExpandCollapseElementAsync(elementId, expand, windowTitle, processId);
                return new OperationResult 
                { 
                    Success = result.Success, 
                    Error = result.Error, 
                    Data = result.Data ?? "Element expand/collapse executed successfully" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing expand/collapse on element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var result = await _uiAutomationWorker.ScrollElementAsync(elementId, direction, horizontal, vertical, windowTitle, processId);
                return new OperationResult 
                { 
                    Success = result.Success, 
                    Error = result.Error, 
                    Data = result.Data ?? "Element scrolled successfully" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scrolling element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var result = await _uiAutomationWorker.ScrollElementIntoViewAsync(elementId, windowTitle, processId);
                return new OperationResult 
                { 
                    Success = result.Success, 
                    Error = result.Error, 
                    Data = result.Data ?? "Element scrolled into view successfully" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scrolling element into view {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var result = await _uiAutomationWorker.TransformElementAsync(elementId, action, x, y, width, height, degrees, windowTitle, processId);
                return new OperationResult 
                { 
                    Success = result.Success, 
                    Error = result.Error, 
                    Data = result.Data ?? $"Element {action} executed successfully" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var result = await _uiAutomationWorker.DockElementAsync(elementId, position, windowTitle, processId);
                return new OperationResult 
                { 
                    Success = result.Success, 
                    Error = result.Error, 
                    Data = result.Data ?? "Element docked successfully" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error docking element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }
    }
}