using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// UIAutomationワーカーの実装
    /// サブプロセスとの通信とタイムアウト管理のみを担当
    /// </summary>
    public class UIAutomationWorker : IUIAutomationWorker
    {
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly IProcessTimeoutManager _processTimeoutManager;
        private readonly string _workerExecutablePath;
        private bool _disposed;

        public UIAutomationWorker(
            ILogger<UIAutomationWorker> logger,
            IProcessTimeoutManager processTimeoutManager)
        {
            _logger = logger;
            _processTimeoutManager = processTimeoutManager;
            _workerExecutablePath = Path.Combine(
                AppContext.BaseDirectory,
                "UIAutomationMCP.Worker.exe");
        }

        public async Task<OperationResult<T>> ExecuteOperationAsync<T>(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            var result = await ExecuteOperationAsync(operation, parameters, timeoutSeconds, cancellationToken);
            
            if (!result.Success)
            {
                return new OperationResult<T>
                {
                    Success = false,
                    Error = result.Error
                };
            }

            try
            {
                if (result.Data == null)
                {
                    return new OperationResult<T>
                    {
                        Success = true,
                        Data = default
                    };
                }

                // JSONとして再シリアライズしてから型変換
                var json = JsonSerializer.Serialize(result.Data);
                var typedData = JsonSerializer.Deserialize<T>(json);
                
                return new OperationResult<T>
                {
                    Success = true,
                    Data = typedData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert operation result to type {Type}", typeof(T).Name);
                return new OperationResult<T>
                {
                    Success = false,
                    Error = $"Failed to convert result: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<object>> ExecuteOperationAsync(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[Worker] Executing operation '{Operation}' with timeout {Timeout}s",
                operation, timeoutSeconds);

            try
            {
                // 操作リクエストを構築
                var request = new
                {
                    Operation = operation,
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var requestJson = JsonSerializer.Serialize(request);
                
                // プロセス実行
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _workerExecutablePath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // タイムアウト管理付きで実行
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
                
                var processResult = await _processTimeoutManager.ExecuteWithTimeoutAsync(
                    processStartInfo,
                    requestJson,
                    timeoutSeconds,
                    $"UIAutomation-{operation}",
                    cts.Token);

                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (!processResult.Success)
                {
                    _logger.LogWarning(
                        "[Worker] Operation '{Operation}' failed after {Elapsed}ms: {Error}",
                        operation, elapsed, processResult.Error);
                    
                    return new OperationResult<object>
                    {
                        Success = false,
                        Error = processResult.Error ?? "Worker process execution failed"
                    };
                }

                // 結果をパース
                var result = ParseWorkerResponse(processResult.Output);
                
                _logger.LogInformation(
                    "[Worker] Operation '{Operation}' completed successfully in {Elapsed}ms",
                    operation, elapsed);
                
                return result;
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<object>
                {
                    Success = false,
                    Error = $"Operation '{operation}' was cancelled after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Worker] Operation '{Operation}' failed with exception", operation);
                return new OperationResult<object>
                {
                    Success = false,
                    Error = $"Operation failed: {ex.Message}"
                };
            }
        }

        private OperationResult<object> ParseWorkerResponse(string? output)
        {
            if (string.IsNullOrEmpty(output))
            {
                return new OperationResult<object>
                {
                    Success = false,
                    Error = "Worker returned empty response"
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(output);
                var root = doc.RootElement;
                
                if (!root.TryGetProperty("Success", out var successProp) || !successProp.GetBoolean())
                {
                    var error = root.TryGetProperty("Error", out var errorProp)
                        ? errorProp.GetString()
                        : "Unknown error";
                    
                    return new OperationResult<object>
                    {
                        Success = false,
                        Error = error
                    };
                }

                // データを抽出
                object? data = null;
                if (root.TryGetProperty("Data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
                {
                    data = JsonSerializer.Deserialize<object>(dataProp.GetRawText());
                }

                return new OperationResult<object>
                {
                    Success = true,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse worker response");
                return new OperationResult<object>
                {
                    Success = false,
                    Error = $"Failed to parse worker response: {ex.Message}"
                };
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger?.LogDebug("[Worker] Disposing");
                    // ProcessTimeoutManagerはDIコンテナが管理
                }
                _disposed = true;
            }
        }
    }
}
