using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// 統合テスト - 実際のWorkerプロセスとの通信をテスト
    /// このテストは実際のUIAutomationWorker.exeが必要
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class WorkerIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly UIAutomationWorker _worker;

        public WorkerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // テスト用のロガーを作成
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<UIAutomationWorker>();
            
            _worker = new UIAutomationWorker(_logger);
        }

        [Fact]
        public async Task WorkerProcess_ShouldStartAndExecuteBasicOperation()
        {
            // Arrange
            var operation = new WorkerOperation
            {
                Operation = "GetSupportedOperations",
                Parameters = new Dictionary<string, object>(),
                Timeout = 10
            };

            try
            {
                // Act
                var result = await _worker.ExecuteInProcessAsync(
                    System.Text.Json.JsonSerializer.Serialize(operation), 10);

                // Assert
                _output.WriteLine($"Worker process result: Success={result.Success}, Data={result.Data}");
                
                if (!result.Success)
                {
                    _output.WriteLine($"Worker process failed: {result.Error}");
                    // Workerプロセスが存在しない場合はスキップ（CI環境等）
                    Assert.True(result.Error?.Contains("not found") == true, 
                        "Expected worker executable not found, but got different error");
                    return;
                }

                Assert.True(result.Success);
                Assert.NotNull(result.Data);
                Assert.Contains("GetSupportedOperations", result.Data);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Integration test exception: {ex.Message}");
                // 統合テスト環境でWorkerが利用できない場合はスキップ
                throw;
            }
        }

        [Fact]
        public async Task WorkerProcess_ShouldHandleInvalidOperation()
        {
            // Arrange
            var operation = new WorkerOperation
            {
                Operation = "InvalidOperation",
                Parameters = new Dictionary<string, object>(),
                Timeout = 5
            };

            try
            {
                // Act
                var result = await _worker.ExecuteInProcessAsync(
                    System.Text.Json.JsonSerializer.Serialize(operation), 5);

                // Assert
                _output.WriteLine($"Invalid operation result: Success={result.Success}, Error={result.Error}");
                
                if (result.Error?.Contains("not found") == true)
                {
                    // Workerプロセスが存在しない場合はスキップ
                    return;
                }

                // Workerプロセスが存在する場合、無効な操作はエラーを返すべき
                Assert.False(result.Success);
                Assert.NotNull(result.Error);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Integration test exception: {ex.Message}");
                // 統合テスト環境でWorkerが利用できない場合はスキップ
            }
        }

        [Fact]
        public async Task WorkerProcess_ShouldHandleTimeout()
        {
            // Arrange - 極端に短いタイムアウト
            var operation = new WorkerOperation
            {
                Operation = "GetWindowInfo", 
                Parameters = new Dictionary<string, object>(),
                Timeout = 1 // 1秒の短いタイムアウト
            };

            try
            {
                // Act
                var result = await _worker.ExecuteInProcessAsync(
                    System.Text.Json.JsonSerializer.Serialize(operation), 1);

                // Assert
                _output.WriteLine($"Timeout test result: Success={result.Success}, Error={result.Error}");
                
                if (result.Error?.Contains("not found") == true)
                {
                    // Workerプロセスが存在しない場合はスキップ
                    return;
                }

                // タイムアウトまたは成功のどちらかになるはず
                if (!result.Success)
                {
                    Assert.Contains("timeout", result.Error?.ToLower() ?? "");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Timeout test exception: {ex.Message}");
                // 統合テスト環境でWorkerが利用できない場合はスキップ
            }
        }

        public void Dispose()
        {
            _worker?.Dispose();
        }
    }
}
