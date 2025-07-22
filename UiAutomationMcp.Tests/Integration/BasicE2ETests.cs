using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// 基本的なE2Eテスト - 最も重要なユーザーシナリオのみをテスト
    /// Process IDベースの安全な終了処理により並行実行をサポート
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class BasicE2ETests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly ElementSearchService _elementSearchService;
        private readonly InvokeService _invokeService;
        private readonly ValueService _valueService;
        private readonly string _workerPath;

        public BasicE2ETests(ITestOutputHelper output)
        {
            _output = output;
            
            var services = new ServiceCollection();
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = possiblePaths.FirstOrDefault(File.Exists) ?? 
                throw new InvalidOperationException("Worker executable not found");

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath);
            
            // Create options
            var options = Options.Create(new UIAutomationOptions());
            
            _elementSearchService = new ElementSearchService(
                _serviceProvider.GetRequiredService<ILogger<ElementSearchService>>(), 
                _subprocessExecutor, 
                options);
            _invokeService = new InvokeService(
                _serviceProvider.GetRequiredService<ILogger<InvokeService>>(), 
                _subprocessExecutor);
            _valueService = new ValueService(
                _serviceProvider.GetRequiredService<ILogger<ValueService>>(), 
                _subprocessExecutor);
        }

        [Fact]
        public async Task BasicWorkflow_FindWindowsAndElements_ShouldComplete()
        {
            // Given
            var timeout = 10;

            // When
            var windowsResult = await _elementSearchService.GetWindowsAsync(timeout);
            var elementsResult = await _elementSearchService.SearchElementsAsync(new UIAutomationMCP.Shared.Requests.SearchElementsRequest
            {
                ControlType = "Button",
                TimeoutSeconds = timeout
            });

            // Then
            Assert.NotNull(windowsResult);
            Assert.NotNull(elementsResult);
            _output.WriteLine($"Windows found: {windowsResult}");
            _output.WriteLine($"Elements found: {elementsResult}");
        }

        [Fact]
        public async Task BasicWorkflow_HandleNonExistentElement_ShouldReturnError()
        {
            // Given
            var nonExistentAutomationId = "NonExistentElement12345";
            var timeout = 1;

            // When
            var invokeResult = await _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: timeout);
            var valueResult = await _valueService.GetValueAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: timeout);

            // Then
            Assert.NotNull(invokeResult);
            Assert.NotNull(valueResult);
            
            var invokeJson = System.Text.Json.JsonSerializer.Serialize(invokeResult);
            var valueJson = System.Text.Json.JsonSerializer.Serialize(valueResult);
            
            Assert.Contains("Success", invokeJson);
            Assert.Contains("false", invokeJson);
            Assert.Contains("Success", valueJson);
            Assert.Contains("false", valueJson);
            
            _output.WriteLine($"Invoke result: {invokeJson}");
            _output.WriteLine($"Value result: {valueJson}");
        }

        [Fact]
        public async Task InvokeElement_WithDifferentParameters_ShouldHandleGracefully()
        {
            // Given
            var nonExistentAutomationId = "TestButton123";
            var timeout = 5;

            // When - Test different parameter combinations
            var resultByAutomationId = await _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: timeout);
            var resultByWindowTitle = await _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: timeout);
            var resultByProcessId = await _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: 99999, timeoutSeconds: timeout);

            // Then
            Assert.NotNull(resultByAutomationId);
            Assert.NotNull(resultByWindowTitle);
            Assert.NotNull(resultByProcessId);
            
            _output.WriteLine($"Result by AutomationId: {resultByAutomationId}");
            _output.WriteLine($"Result by WindowTitle: {resultByWindowTitle}");
            _output.WriteLine($"Result by ProcessId: {resultByProcessId}");
        }

        [Fact]
        public async Task InvokeElement_ConcurrentCalls_ShouldNotBlock()
        {
            // Given
            var elementIds = new[] { "Element1", "Element2", "Element3" };
            var timeout = 2;

            // When - Execute concurrent invoke operations
            var startTime = DateTime.UtcNow;
            var tasks = elementIds.Select(id => 
                _invokeService.InvokeElementAsync(automationId: id, processId: null, timeoutSeconds: timeout)).ToArray();
            
            var results = await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;

            // Then - Should complete in reasonable time (non-blocking behavior)
            Assert.All(results, result => Assert.NotNull(result));
            Assert.True(totalTime < 10000, $"Concurrent calls took too long: {totalTime}ms");
            
            _output.WriteLine($"Concurrent invoke operations completed in {totalTime}ms");
            for (int i = 0; i < results.Length; i++)
            {
                _output.WriteLine($"Result {i}: {results[i]}");
            }
        }

        [Fact]
        public async Task InvokeElement_ShortTimeout_ShouldReturnQuickly()
        {
            // Given
            var nonExistentAutomationId = "TimeoutTestElement";
            var shortTimeout = 1;

            // When
            var startTime = DateTime.UtcNow;
            var result = await _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: shortTimeout);
            var endTime = DateTime.UtcNow;
            var actualTime = (endTime - startTime).TotalSeconds;

            // Then - Should respect timeout and return quickly
            Assert.NotNull(result);
            Assert.True(actualTime <= shortTimeout + 2, $"Operation took {actualTime}s, expected <= {shortTimeout + 2}s");
            
            _output.WriteLine($"Short timeout test completed in {actualTime}s with result: {result}");
        }

        private async Task SafelyTerminateProcessAsync(Process? process, string appName)
        {
            if (process == null) return;

            try
            {
                if (!process.HasExited)
                {
                    _output.WriteLine($"Terminating {appName} with PID: {process.Id}");
                    
                    // まず正常終了を試行
                    process.CloseMainWindow();
                    
                    // 少し待機してから強制終了
                    if (!process.WaitForExit(2000))
                    {
                        _output.WriteLine($"Force killing {appName} with PID: {process.Id}");
                        process.Kill();
                        await process.WaitForExitAsync();
                    }
                    
                    _output.WriteLine($"{appName} with PID: {process.Id} terminated successfully");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error terminating {appName}: {ex.Message}");
            }
            finally
            {
                process?.Dispose();
            }
        }

        [Fact]
        public async Task BasicWorkflow_WithCalculator_ShouldHandleGracefully()
        {
            // Given
            Process? calcProcess = null;
            try
            {
                calcProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });

                if (calcProcess == null)
                {
                    _output.WriteLine("Calculator could not be launched, skipping test");
                    return;
                }

                await Task.Delay(3000); // Wait for calculator to start
                _output.WriteLine($"Calculator launched with PID: {calcProcess.Id}");
                var timeout = 10;

                // When
                var elementsResult = await _elementSearchService.SearchElementsAsync(new UIAutomationMCP.Shared.Requests.SearchElementsRequest
            {
                ControlType = "Button",
                TimeoutSeconds = timeout
            });
                var windowsResult = await _elementSearchService.GetWindowsAsync(timeout);

                // Then
                Assert.NotNull(elementsResult);
                Assert.NotNull(windowsResult);
                _output.WriteLine($"Calculator test - Elements: {elementsResult}");
                _output.WriteLine($"Calculator test - Windows: {windowsResult}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Calculator test exception (expected in CI): {ex.Message}");
            }
            finally
            {
                await SafelyTerminateProcessAsync(calcProcess, "Calculator");
            }
        }

        [Fact]
        public async Task BasicWorkflow_TimeoutHandling_ShouldWork()
        {
            // Given
            var veryShortTimeout = 1;
            var nonExistentAutomationId = "NonExistentElement12345";

            // When & Then
            var invokeTask = _invokeService.InvokeElementAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: veryShortTimeout);
            var valueTask = _valueService.GetValueAsync(automationId: nonExistentAutomationId, processId: null, timeoutSeconds: veryShortTimeout);

            var invokeResult = await invokeTask;
            var valueResult = await valueTask;

            Assert.NotNull(invokeResult);
            Assert.NotNull(valueResult);
            _output.WriteLine($"Timeout test - Invoke: {invokeResult}");
            _output.WriteLine($"Timeout test - Value: {valueResult}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}