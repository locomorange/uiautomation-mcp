using Microsoft.Extensions.Logging;
using System.Text.Json;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

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
            try
            {
                _logger.LogInformation("Getting properties for element '{ElementId}' in window '{WindowTitle}'", elementId, windowTitle);

                // Worker経由でプロパティを取得
                var operationParams = new AdvancedOperationParameters
                {
                    Operation = "get_properties",
                    ElementId = elementId,
                    WindowTitle = windowTitle,
                    ProcessId = processId,
                    TimeoutSeconds = 20
                };

                var result = await _uiAutomationWorker.ExecuteAdvancedOperationAsync(operationParams);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to get properties for element '{ElementId}': {Error}", elementId, result.Error);
                    return new OperationResult<Dictionary<string, object>>
                    {
                        Success = false,
                        Error = result.Error ?? "Failed to get element properties"
                    };
                }

                _logger.LogInformation("Successfully retrieved properties for element '{ElementId}'", elementId);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = true,
                    Data = result.Data ?? new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting properties for element '{ElementId}'", elementId);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = $"Error getting element properties: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Getting patterns for element '{ElementId}' in window '{WindowTitle}'", elementId, windowTitle);

                // Worker経由でパターンを取得
                var operationParams = new AdvancedOperationParameters
                {
                    Operation = "get_patterns",
                    ElementId = elementId,
                    WindowTitle = windowTitle,
                    ProcessId = processId,
                    TimeoutSeconds = 20
                };

                var result = await _uiAutomationWorker.ExecuteAdvancedOperationAsync(operationParams);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to get patterns for element '{ElementId}': {Error}", elementId, result.Error);
                    return new OperationResult<Dictionary<string, object>>
                    {
                        Success = false,
                        Error = result.Error ?? "Failed to get element patterns"
                    };
                }

                _logger.LogInformation("Successfully retrieved patterns for element '{ElementId}'", elementId);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = true,
                    Data = result.Data ?? new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patterns for element '{ElementId}'", elementId);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = $"Error getting element patterns: {ex.Message}"
                };
            }
        }
    }
}