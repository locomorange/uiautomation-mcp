using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetTableInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetTableInfoOperation> _logger;

        public GetTableInfoOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetTableInfoOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetTableInfoRequest>(parametersJson)!;
                
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
                        Data = new TableInfoResult()
                    });
                }

                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "TablePattern not supported",
                        Data = new TableInfoResult()
                    });
                }

                var result = new TableInfoResult
                {
                    RowCount = tablePattern.Current.RowCount,
                    ColumnCount = tablePattern.Current.ColumnCount,
                    RowOrColumnMajor = tablePattern.Current.RowOrColumnMajor.ToString()
                };

                // Check if grid pattern is also supported for additional info
                if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPat) && gridPat is GridPattern gridPattern)
                {
                    // Grid pattern might provide additional selection info
                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selPat) && selPat is SelectionPattern selectionPattern)
                    {
                        result.CanSelectMultiple = selectionPattern.Current.CanSelectMultiple;
                    }
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTableInfo operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get table information: {ex.Message}",
                    Data = new TableInfoResult()
                });
            }
        }
    }
}