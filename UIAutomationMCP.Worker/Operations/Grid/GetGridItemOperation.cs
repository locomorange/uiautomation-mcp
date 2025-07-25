using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetGridItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetGridItemOperation> _logger;

        public GetGridItemOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetGridItemOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetGridItemRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId,
                    requiredPattern: GridPattern.Pattern);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new GridItemResult()
                    });
                }

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "GridPattern not supported",
                        Data = new GridItemResult()
                    });
                }

                var gridItem = gridPattern.GetItem(typedRequest.Row, typedRequest.Column);
                if (gridItem == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Grid item not found",
                        Data = new GridItemResult()
                    });
                }

                var result = new GridItemResult
                {
                    Row = typedRequest.Row,
                    Column = typedRequest.Column,
                    Element = ElementInfoBuilder.CreateElementInfo(gridItem, includeDetails: true, _logger)
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetGridItem operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get grid item: {ex.Message}",
                    Data = new GridItemResult()
                });
            }
        }

    }
}