using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetGridInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetGridInfoOperation> _logger;

        public GetGridInfoOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetGridInfoOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetGridInfoRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new GridInfoResult()
                    });
                }

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "GridPattern not supported",
                        Data = new GridInfoResult()
                    });
                }

                var result = new GridInfoResult
                {
                    RowCount = gridPattern.Current.RowCount,
                    ColumnCount = gridPattern.Current.ColumnCount
                };

                // Check if selection is supported
                if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selPattern) && selPattern is SelectionPattern selectionPattern)
                {
                    result.CanSelectMultiple = selectionPattern.Current.CanSelectMultiple;
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetGridInfo operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get grid information: {ex.Message}",
                    Data = new GridInfoResult()
                });
            }
        }
    }
}