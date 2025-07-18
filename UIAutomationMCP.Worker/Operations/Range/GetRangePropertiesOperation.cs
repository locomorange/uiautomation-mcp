using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class GetRangePropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetRangePropertiesOperation> _logger;

        public GetRangePropertiesOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRangePropertiesOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetRangePropertiesRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element '{typedRequest.ElementId}' not found",
                        Data = new RangeValueResult()
                    });
                }

                if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support RangeValuePattern",
                        Data = new RangeValueResult()
                    });
                }

                var result = new RangeValueResult
                {
                    Value = rangePattern.Current.Value,
                    Minimum = rangePattern.Current.Minimum,
                    Maximum = rangePattern.Current.Maximum,
                    LargeChange = rangePattern.Current.LargeChange,
                    SmallChange = rangePattern.Current.SmallChange,
                    IsReadOnly = rangePattern.Current.IsReadOnly
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRangeProperties operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get range properties: {ex.Message}",
                    Data = new RangeValueResult()
                });
            }
        }
    }
}