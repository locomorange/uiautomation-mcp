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
            // 現在はモック実装、includeDetailsパラメータのテスト対応
            await Task.Delay(100);
            
            // includeDetailsパラメータに応じてElementInfoを構築
            var elements = CreateMockElements(request);
            
            return new SearchElementsResult
            {
                Success = true,
                OperationName = "SearchElements",
                Elements = elements,
                Metadata = new SearchMetadata
                {
                    TotalFound = elements.Length,
                    Returned = elements.Length,
                    SearchDuration = TimeSpan.FromMilliseconds(100),
                    SearchCriteria = BuildSearchCriteria(request),
                    WasTruncated = false,
                    SuggestedRefinements = request.IncludeDetails 
                        ? new string[] { "Detailed search completed" }
                        : new string[] { "Use includeDetails=true for more information" },
                    ExecutedAt = DateTime.UtcNow
                }
            };
        }

        private ElementInfo[] CreateMockElements(SearchElementsRequest request)
        {
            // モック要素データ作成
            var elements = new List<ElementInfo>();
            
            // 基本的なモック要素を2つ作成
            for (int i = 1; i <= 2; i++)
            {
                var element = new ElementInfo
                {
                    Name = $"TestElement{i}",
                    AutomationId = $"test-element-{i}",
                    ControlType = request.ControlType ?? "Button",
                    LocalizedControlType = "ボタン",
                    ClassName = "TestClass",
                    ProcessId = 12345,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = 100 + (i * 50),
                        Y = 200 + (i * 30),
                        Width = 80,
                        Height = 25
                    },
                    IsEnabled = true,
                    IsVisible = true,
                    IsOffscreen = false,
                    FrameworkId = "Win32",
                    SupportedPatterns = new string[] { "Invoke", "Toggle" }
                };

                // includeDetails=trueの場合、詳細情報を追加
                if (request.IncludeDetails)
                {
                    element.Details = CreateMockElementDetails(i);
                }

                elements.Add(element);
            }
            
            return elements.ToArray();
        }

        private ElementDetails CreateMockElementDetails(int elementIndex)
        {
            return new ElementDetails
            {
                HelpText = $"This is test element {elementIndex}",
                Value = $"TestValue{elementIndex}",
                HasKeyboardFocus = elementIndex == 1,
                IsKeyboardFocusable = true,
                IsPassword = false,
                
                // モックパターン情報
                Toggle = new ToggleInfo
                {
                    State = elementIndex == 1 ? "On" : "Off",
                    CanToggle = true
                },
                
                // モック階層情報
                Parent = new ElementInfo
                {
                    Name = "ParentContainer",
                    AutomationId = "parent-container",
                    ControlType = "Group",
                    LocalizedControlType = "グループ"
                },
                
                Children = new ElementInfo[]
                {
                    new ElementInfo
                    {
                        Name = $"Child{elementIndex}-1",
                        AutomationId = $"child-{elementIndex}-1",
                        ControlType = "Text",
                        LocalizedControlType = "テキスト"
                    }
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
            if (request.IncludeDetails)
                criteria.Add("IncludeDetails=true");
            
            return string.Join(", ", criteria);
        }
    }
}