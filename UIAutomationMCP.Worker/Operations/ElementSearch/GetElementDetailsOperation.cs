using System.Diagnostics;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class GetElementDetailsOperation : IUIAutomationOperation
    {
        public async Task<OperationResult<ElementDetailResult>> ExecuteAsync(string parametersJson)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var request = JsonSerializationHelper.Deserialize<GetElementDetailsRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult<ElementDetailResult>
                    {
                        Success = false,
                        Error = "Failed to deserialize GetElementDetailsRequest"
                    };
                }

                // 既存のUIAutomationEnvironment.ExecuteWithTimeoutAsyncを使用
                ElementDetailResult result;
                try
                {
                    result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(
                        () => PerformElementDetailRetrievalAsync(request), 
                        "GetElementDetails", 
                        request.TimeoutSeconds);
                }
                catch (TimeoutException ex)
                {
                    return new OperationResult<ElementDetailResult>
                    {
                        Success = false,
                        Error = ex.Message,
                        ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                    };
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                return new OperationResult<ElementDetailResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new OperationResult<ElementDetailResult>
                {
                    Success = false,
                    Error = $"Operation failed: {ex.Message}",
                    ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(string parametersJson)
        {
            var typedResult = await ExecuteAsync(parametersJson);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }

        private async Task<ElementDetailResult> PerformElementDetailRetrievalAsync(GetElementDetailsRequest request)
        {
            // 現在はモック実装、後で実際のUI Automation要素取得に置き換え
            await Task.Delay(50); // シミュレート取得時間
            
            return new ElementDetailResult
            {
                Success = true,
                OperationName = "GetElementDetails",
                Element = new ElementDetail
                {
                    AutomationId = request.AutomationId ?? "",
                    Name = request.Name ?? "",
                    ControlType = "Unknown",
                    IsEnabled = true,
                    IsVisible = true,
                    HasKeyboardFocus = false,
                    IsKeyboardFocusable = false,
                    IsPassword = false,
                    IsOffscreen = false,
                    LocalizedControlType = "Unknown",
                    FrameworkId = "Unknown"
                },
                Metadata = new DetailMetadata
                {
                    RetrievalDuration = TimeSpan.FromMilliseconds(50),
                    PatternsRetrieved = 0,
                    IncludesHierarchy = request.IncludeChildren || request.IncludeParent,
                    ChildrenCount = 0,
                    ExecutedAt = DateTime.UtcNow,
                    IdentificationCriteria = BuildIdentificationCriteria(request)
                }
            };
        }

        private string BuildIdentificationCriteria(GetElementDetailsRequest request)
        {
            var criteria = new List<string>();
            
            if (!string.IsNullOrEmpty(request.AutomationId))
                criteria.Add($"AutomationId='{request.AutomationId}'");
            if (!string.IsNullOrEmpty(request.Name))
                criteria.Add($"Name='{request.Name}'");
            if (!string.IsNullOrEmpty(request.WindowTitle))
                criteria.Add($"WindowTitle='{request.WindowTitle}'");
            if (request.ProcessId.HasValue)
                criteria.Add($"ProcessId={request.ProcessId.Value}");
            
            return string.Join(", ", criteria);
        }
    }
}