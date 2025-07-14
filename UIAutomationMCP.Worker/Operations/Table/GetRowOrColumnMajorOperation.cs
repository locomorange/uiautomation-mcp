using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetRowOrColumnMajorOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRowOrColumnMajorOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public async Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new BooleanResult();
            
            try
            {
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    result.Value = false;
                    result.Description = "Element not found";
                    return new OperationResult<BooleanResult> { Success = false, Error = "Element not found", Data = result };
                }

                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                {
                    result.Value = false;
                    result.Description = "TablePattern not supported";
                    return new OperationResult<BooleanResult> { Success = false, Error = "TablePattern not supported", Data = result };
                }

                var rowOrColumnMajor = tablePattern.Current.RowOrColumnMajor;
                result.Value = rowOrColumnMajor == RowOrColumnMajor.RowMajor;
                result.Description = $"Table is {rowOrColumnMajor} (Row major: {result.Value})";

                return new OperationResult<BooleanResult> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                result.Value = false;
                result.Description = $"Error getting row/column major: {ex.Message}";
                return new OperationResult<BooleanResult> { Success = false, Error = $"Error getting row/column major: {ex.Message}", Data = result };
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }
    }
}