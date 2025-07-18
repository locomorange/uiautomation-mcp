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
    public class GetRowOrColumnMajorOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetRowOrColumnMajorOperation> _logger;

        public GetRowOrColumnMajorOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRowOrColumnMajorOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetRowOrColumnMajorRequest>(parametersJson)!;
                
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
                        Data = new BooleanResult { Value = false, Description = "Element not found" }
                    });
                }

                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "TablePattern not supported",
                        Data = new BooleanResult { Value = false, Description = "TablePattern not supported" }
                    });
                }

                var rowOrColumnMajor = tablePattern.Current.RowOrColumnMajor;
                var isRowMajor = rowOrColumnMajor == RowOrColumnMajor.RowMajor;
                
                var result = new BooleanResult
                {
                    Value = isRowMajor,
                    Description = $"Table is {rowOrColumnMajor} (Row major: {isRowMajor})"
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRowOrColumnMajor operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get row/column major: {ex.Message}",
                    Data = new BooleanResult { Value = false, Description = "Operation failed" }
                });
            }
        }
    }
}