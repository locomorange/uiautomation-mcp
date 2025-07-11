using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// Server-Worker統合テスト - 実際のプロセス間通信をテスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class ServerWorkerIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly string _workerPath;

        public ServerWorkerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // テスト用のサービスコンテナをセットアップ
            var services = new ServiceCollection();
            
            // ロガーを追加
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            // Worker.exeのパスを取得
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = null!;
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    _workerPath = fullPath;
                    break;
                }
            }

            if (_workerPath == null)
            {
                throw new InvalidOperationException($"Worker executable not found in any of these locations: {string.Join(", ", possiblePaths.Select(Path.GetFullPath))}");
            }

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath);
            _output.WriteLine($"Worker path: {_workerPath}");
        }

        [Fact]
        public void WorkerExecutable_ShouldExist()
        {
            // Assert
            Assert.True(File.Exists(_workerPath), $"Worker executable should exist at: {_workerPath}");
            _output.WriteLine($"Worker executable verified at: {_workerPath}");
        }

        [Fact]
        public async Task SubprocessExecutor_WhenInvalidOperation_ShouldThrowException()
        {
            // Given
            var parameters = new Dictionary<string, object>
            {
                { "elementId", "test" },
                { "windowTitle", "" },
                { "processId", 0 }
            };

            // When & Then
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _subprocessExecutor.ExecuteAsync<object>("InvalidOperation", parameters, 10));

            Assert.NotNull(exception);
            _output.WriteLine($"Invalid operation correctly threw exception: {exception.Message}");
        }

        [Fact]
        public async Task SubprocessExecutor_WhenValidOperationWithMissingElement_ShouldThrowException()
        {
            // Given
            var parameters = new Dictionary<string, object>
            {
                { "elementId", "NonExistentElement" },
                { "windowTitle", "" },
                { "processId", 0 }
            };

            // When & Then
            var exception = await Assert.ThrowsAsync<TimeoutException>(async () =>
                await _subprocessExecutor.ExecuteAsync<object>("InvokeElement", parameters, 10));

            Assert.NotNull(exception);
            _output.WriteLine($"Valid operation with missing element correctly threw exception: {exception.Message}");
        }

        [Fact]
        public async Task InvokeService_WhenCalledWithNonExistentElement_ShouldHandleTimeout()
        {
            // Given
            var invokeService = new SubprocessBasedInvokeService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedInvokeService>>(), _subprocessExecutor);

            // When
            var result = await invokeService.InvokeElementAsync("NonExistentElement", null, null, 5);

            // Then
            Assert.NotNull(result);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("Success", resultJson);
            Assert.Contains("false", resultJson);
            Assert.Contains("timed out", resultJson);
            _output.WriteLine($"InvokeService timeout test completed: {resultJson}");
        }

        [Fact]
        public async Task ValueService_ShouldCommunicateWithWorker()
        {
            // Arrange
            var valueService = new SubprocessBasedValueService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedValueService>>(), _subprocessExecutor);

            // Act
            var result = await valueService.SetValueAsync("NonExistentElement", "test value", null, null, 5);

            // Assert
            Assert.NotNull(result);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("Success", resultJson);
            Assert.Contains("false", resultJson);
            Assert.Contains("timed out", resultJson);
            _output.WriteLine($"ValueService timeout test completed: {resultJson}");
        }

        [Fact]
        public async Task ToggleService_ShouldCommunicateWithWorker()
        {
            // Arrange
            var toggleService = new SubprocessBasedToggleService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedToggleService>>(), _subprocessExecutor);

            // Act
            var result = await toggleService.ToggleElementAsync("NonExistentElement", null, null, 5);

            // Assert
            Assert.NotNull(result);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("Success", resultJson);
            Assert.Contains("false", resultJson);
            Assert.Contains("timed out", resultJson);
            _output.WriteLine($"ToggleService timeout test completed: {resultJson}");
        }

        [Fact]
        public async Task MultipleOperations_ShouldWorkSequentially()
        {
            // Arrange
            var invokeService = new SubprocessBasedInvokeService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedInvokeService>>(), _subprocessExecutor);
            var valueService = new SubprocessBasedValueService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedValueService>>(), _subprocessExecutor);

            // Act
            var invokeResult = await invokeService.InvokeElementAsync("NonExistentElement1", null, null, 5);
            var valueResult = await valueService.SetValueAsync("NonExistentElement2", "test", null, null, 5);

            // Assert
            Assert.NotNull(invokeResult);
            Assert.NotNull(valueResult);
            var invokeJson = System.Text.Json.JsonSerializer.Serialize(invokeResult);
            var valueJson = System.Text.Json.JsonSerializer.Serialize(valueResult);
            Assert.Contains("Success", invokeJson);
            Assert.Contains("false", invokeJson);
            Assert.Contains("Success", valueJson);
            Assert.Contains("false", valueJson);
            _output.WriteLine("Multiple sequential timeout operations completed successfully");
        }

        [Fact]
        public async Task WorkerProcess_ShouldHandleTimeouts()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                { "elementId", "test" },
                { "windowTitle", "" },
                { "processId", 0 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TimeoutException>(async () =>
                await _subprocessExecutor.ExecuteAsync<object>("InvokeElement", parameters, 1)); // 1秒タイムアウト
            _output.WriteLine($"Timeout handling test completed: {exception.Message}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}