using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class IsReadOnlyOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<IsReadOnlyOperation> _logger;

        public IsReadOnlyOperation(
            ElementFinderService elementFinderService, 
            ILogger<IsReadOnlyOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<IsReadOnlyRequest>(parametersJson)!;
                
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

                if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "ValuePattern not supported",
                        Data = new BooleanResult { Value = false, Description = "ValuePattern not supported" }
                    });
                }

                var isReadOnly = valuePattern.Current.IsReadOnly;
                var result = new BooleanResult 
                { 
                    Value = isReadOnly, 
                    Description = isReadOnly ? "Element is read-only" : "Element is editable"
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsReadOnly operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to check read-only status: {ex.Message}",
                    Data = new BooleanResult { Value = false, Description = "Operation failed" }
                });
            }
        }
    }
}