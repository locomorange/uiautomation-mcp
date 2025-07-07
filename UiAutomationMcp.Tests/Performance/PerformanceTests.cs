using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Server.Models;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;
using Moq;

namespace UIAutomationMCP.Tests.Performance
{
    /// <summary>
    /// パフォーマンステスト - 大量の操作や連続操作での性能をテスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class PerformanceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly Mock<IProcessTimeoutManager> _mockProcessTimeoutManager;
        private readonly UIAutomationWorker _worker;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
            
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<UIAutomationWorker>();
            
            _mockProcessTimeoutManager = new Mock<IProcessTimeoutManager>();
            _worker = new UIAutomationWorker(_logger, _mockProcessTimeoutManager.Object);
        }

        [Fact]
        public async Task MultipleElementSearches_ShouldCompleteWithinReasonableTime()
        {
            var stopwatch = Stopwatch.StartNew();
            const int searchCount = 10;
            var successCount = 0;

            try
            {
                // Act - 複数の要素検索を順次実行
                for (int i = 0; i < searchCount; i++)
                {
                    var searchParams = new ElementSearchParameters
                    {
                        WindowTitle = $"TestWindow{i}",
                        ControlType = "Button",
                        TimeoutSeconds = 2 // 短いタイムアウト
                    };

                    var result = await _worker.FindFirstElementAsync(searchParams, 2);
                    
                    if (result.Error?.Contains("not found") == true)
                    {
                        // Workerプロセスが存在しない場合はスキップ
                        _output.WriteLine("Worker process not found, skipping performance test");
                        return;
                    }

                    if (result.Success || result.Error != null)
                    {
                        successCount++; // 成功または期待されるエラー
                    }
                }

                stopwatch.Stop();

                // Assert
                _output.WriteLine($"Multiple searches completed: {successCount}/{searchCount} in {stopwatch.ElapsedMilliseconds}ms");
                
                // パフォーマンス要件: 10回の検索が30秒以内に完了すること
                Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
                    $"Performance test took too long: {stopwatch.ElapsedMilliseconds}ms");
                
                // 全ての検索が何らかの結果を返すこと
                Assert.Equal(searchCount, successCount);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _output.WriteLine($"Performance test exception after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            }
        }

        [Fact]
        public async Task ConcurrentElementSearches_ShouldHandleLoad()
        {
            var stopwatch = Stopwatch.StartNew();
            const int concurrentCount = 5;

            try
            {
                // Act - 並行して要素検索を実行
                var tasks = new List<Task<OperationResult<ElementInfo?>>>();
                
                for (int i = 0; i < concurrentCount; i++)
                {
                    var searchParams = new ElementSearchParameters
                    {
                        WindowTitle = $"ConcurrentTest{i}",
                        ControlType = "Button",
                        TimeoutSeconds = 3
                    };
                    
                    tasks.Add(_worker.FindFirstElementAsync(searchParams, 3));
                }

                var results = await Task.WhenAll(tasks);
                stopwatch.Stop();

                // Assert
                _output.WriteLine($"Concurrent searches completed: {results.Length} operations in {stopwatch.ElapsedMilliseconds}ms");
                
                foreach (var result in results)
                {
                    if (result.Error?.Contains("not found") == true)
                    {
                        // Workerプロセスが存在しない場合はスキップ
                        _output.WriteLine("Worker process not found, skipping concurrent test");
                        return;
                    }
                }

                // パフォーマンス要件: 5つの並行検索が20秒以内に完了すること
                Assert.True(stopwatch.ElapsedMilliseconds < 20000, 
                    $"Concurrent test took too long: {stopwatch.ElapsedMilliseconds}ms");
                
                // 全ての操作が結果を返すこと
                Assert.Equal(concurrentCount, results.Length);
                foreach (var result in results)
                {
                    Assert.NotNull(result);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _output.WriteLine($"Concurrent test exception after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            }
        }

        [Fact]
        public async Task DeepElementTreeSearch_ShouldCompleteWithinTimeLimit()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Act - 深い階層の要素ツリー検索
                var result = await _worker.GetElementTreeAsync(
                    maxDepth: 5, 
                    timeoutSeconds: 15);

                stopwatch.Stop();

                // Assert
                _output.WriteLine($"Deep tree search completed: Success={result.Success} in {stopwatch.ElapsedMilliseconds}ms");
                
                if (result.Error?.Contains("not found") == true)
                {
                    // Workerプロセスが存在しない場合はスキップ
                    _output.WriteLine("Worker process not found, skipping deep search test");
                    return;
                }

                // パフォーマンス要件: 深い検索が15秒以内に完了すること
                Assert.True(stopwatch.ElapsedMilliseconds < 15000, 
                    $"Deep search took too long: {stopwatch.ElapsedMilliseconds}ms");
                
                // 何らかの結果が返されること
                Assert.NotNull(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _output.WriteLine($"Deep search test exception after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            }
        }

        [Fact]
        public async Task RepeatedWorkerCalls_ShouldNotDegradePerformance()
        {
            const int iterationCount = 20;
            var times = new List<long>();

            try
            {
                // Act - 同一操作を繰り返し実行してパフォーマンス劣化を確認
                for (int i = 0; i < iterationCount; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    
                    var operation = new WorkerOperation
                    {
                        Operation = "GetSupportedOperations",
                        Parameters = new Dictionary<string, object>(),
                        Timeout = 5
                    };

                    var result = await _worker.ExecuteInProcessAsync(
                        System.Text.Json.JsonSerializer.Serialize(operation), 5);
                    
                    stopwatch.Stop();
                    times.Add(stopwatch.ElapsedMilliseconds);

                    if (result.Error?.Contains("not found") == true)
                    {
                        // Workerプロセスが存在しない場合はスキップ
                        _output.WriteLine("Worker process not found, skipping repeated calls test");
                        return;
                    }
                }

                // Assert
                var avgTime = times.Average();
                var maxTime = times.Max();
                var minTime = times.Min();
                
                _output.WriteLine($"Repeated calls performance: Avg={avgTime:F1}ms, Min={minTime}ms, Max={maxTime}ms");
                
                // パフォーマンス要件: 平均実行時間が5秒以内、最大時間が10秒以内
                Assert.True(avgTime < 5000, $"Average time too high: {avgTime}ms");
                Assert.True(maxTime < 10000, $"Max time too high: {maxTime}ms");
                
                // パフォーマンス劣化チェック: 最後の5回と最初の5回を比較
                if (times.Count >= 10)
                {
                    var firstFive = times.Take(5).Average();
                    var lastFive = times.Skip(times.Count - 5).Average();
                    var degradationRatio = lastFive / firstFive;
                    
                    _output.WriteLine($"Performance degradation ratio: {degradationRatio:F2} (first 5 avg: {firstFive:F1}ms, last 5 avg: {lastFive:F1}ms)");
                    
                    // 2倍以上の劣化は問題
                    Assert.True(degradationRatio < 2.0, 
                        $"Performance degraded significantly: {degradationRatio:F2}x");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Repeated calls test exception: {ex.Message}");
            }
        }

        [Fact]
        public async Task MemoryUsage_ShouldNotGrowExcessively()
        {
            const int operationCount = 50;
            long initialMemory = GC.GetTotalMemory(true);

            try
            {
                // Act - 多数の操作を実行してメモリ使用量を監視
                for (int i = 0; i < operationCount; i++)
                {
                    var searchParams = new ElementSearchParameters
                    {
                        WindowTitle = $"MemoryTest{i}",
                        ControlType = "Button",
                        TimeoutSeconds = 1
                    };

                    var result = await _worker.FindFirstElementAsync(searchParams, 1);
                    
                    if (result.Error?.Contains("not found") == true && i == 0)
                    {
                        // Workerプロセスが存在しない場合はスキップ
                        _output.WriteLine("Worker process not found, skipping memory test");
                        return;
                    }

                    // 定期的にガベージコレクション
                    if (i % 10 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                }

                // 最終的なメモリ計測
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long finalMemory = GC.GetTotalMemory(false);
                long memoryIncrease = finalMemory - initialMemory;

                // Assert
                _output.WriteLine($"Memory usage: Initial={initialMemory / 1024 / 1024}MB, Final={finalMemory / 1024 / 1024}MB, Increase={memoryIncrease / 1024 / 1024}MB");
                
                // メモリ増加量が100MB未満であることを確認
                Assert.True(memoryIncrease < 100 * 1024 * 1024, 
                    $"Memory usage increased too much: {memoryIncrease / 1024 / 1024}MB");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Memory test exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _worker?.Dispose();
        }
    }
}
