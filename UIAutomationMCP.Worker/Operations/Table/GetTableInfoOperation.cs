using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetTableInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetTableInfoOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.TableInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.TableInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new UIAutomationMCP.Shared.Results.TableInfoResult()
                });

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.TableInfoResult> 
                { 
                    Success = false, 
                    Error = "TablePattern not supported",
                    Data = new UIAutomationMCP.Shared.Results.TableInfoResult()
                });

            var result = new UIAutomationMCP.Shared.Results.TableInfoResult
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

            return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.TableInfoResult> 
            { 
                Success = true, 
                Data = result 
            });
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}