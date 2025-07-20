using System.Diagnostics;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class SearchElementsOperation : IUIAutomationOperation
    {
        public async Task<OperationResult<SearchElementsResult>> ExecuteAsync(string parametersJson)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var request = JsonSerializationHelper.Deserialize<SearchElementsRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult<SearchElementsResult>
                    {
                        Success = false,
                        Error = "Failed to deserialize SearchElementsRequest"
                    };
                }

                // 既存のUIAutomationEnvironment.ExecuteWithTimeoutAsyncを使用（非同期操作用）
                SearchElementsResult result;
                try
                {
                    result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(
                        () => PerformSearchAsync(request), 
                        "SearchElements", 
                        request.TimeoutSeconds);
                }
                catch (TimeoutException ex)
                {
                    return new OperationResult<SearchElementsResult>
                    {
                        Success = false,
                        Error = ex.Message,
                        ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                    };
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                return new OperationResult<SearchElementsResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new OperationResult<SearchElementsResult>
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

        private async Task<SearchElementsResult> PerformSearchAsync(SearchElementsRequest request)
        {
            // 現在はモック実装、後で実際のUI Automation検索に置き換え
            await Task.Delay(100); // 正常系テスト用に100msに戻す
            
            return new SearchElementsResult
            {
                Success = true,
                OperationName = "SearchElements",
                Elements = new BasicElementInfo[0], // Empty for now
                Metadata = new SearchMetadata
                {
                    TotalFound = 0,
                    Returned = 0,
                    SearchDuration = TimeSpan.FromMilliseconds(100),
                    SearchCriteria = BuildSearchCriteria(request),
                    WasTruncated = false,
                    SuggestedRefinements = new string[] { "Try broader search criteria" },
                    ExecutedAt = DateTime.UtcNow
                }
            };
        }

        private string BuildSearchCriteria(SearchElementsRequest request)
        {
            var criteria = new List<string>();
            
            if (!string.IsNullOrEmpty(request.SearchText))
                criteria.Add($"SearchText='{request.SearchText}'");
            if (!string.IsNullOrEmpty(request.AutomationId))
                criteria.Add($"AutomationId='{request.AutomationId}'");
            if (!string.IsNullOrEmpty(request.Name))
                criteria.Add($"Name='{request.Name}'");
            if (!string.IsNullOrEmpty(request.ControlType))
                criteria.Add($"ControlType='{request.ControlType}'");
            if (request.VisibleOnly)
                criteria.Add("VisibleOnly=true");
            
            return string.Join(", ", criteria);
        }
    }
}