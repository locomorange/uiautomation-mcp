using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetRowOrColumnMajorOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetRowOrColumnMajorOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public async Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new BooleanResult();
            
            try
            {
                // Try to get typed request first, fall back to legacy parameter extraction
                var typedRequest = request.GetTypedRequest<GetRowOrColumnMajorRequest>(_options);
                
                var elementId = typedRequest?.ElementId ?? request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = typedRequest?.WindowTitle ?? request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = typedRequest?.ProcessId ?? (request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0);

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