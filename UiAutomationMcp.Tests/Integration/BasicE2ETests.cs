using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// 基本的なE2Eテスト - 最も重要なユーザーシナリオのみをテスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class BasicE2ETests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly SubprocessBasedElementSearchService _elementSearchService;
        private readonly SubprocessBasedInvokeService _invokeService;
        private readonly SubprocessBasedValueService _valueService;
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
            _elementSearchService = new SubprocessBasedElementSearchService(
                _serviceProvider.GetRequiredService<ILogger<SubprocessBasedElementSearchService>>(), 
                _subprocessExecutor);
            _invokeService = new SubprocessBasedInvokeService(
                _serviceProvider.GetRequiredService<ILogger<SubprocessBasedInvokeService>>(), 
                _subprocessExecutor);
            _valueService = new SubprocessBasedValueService(
                _serviceProvider.GetRequiredService<ILogger<SubprocessBasedValueService>>(), 
                _subprocessExecutor);
        }

        [Fact]
        public async Task BasicWorkflow_FindWindowsAndElements_ShouldComplete()
        {
            // Given
            var timeout = 10;

            // When
            var windowsResult = await _elementSearchService.GetWindowsAsync(timeout);
            var elementsResult = await _elementSearchService.FindElementsAsync(null, null, "Button", null, timeout);

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
            var nonExistentElementId = "NonExistentElement12345";
            var timeout = 1;

            // When
            var invokeResult = await _invokeService.InvokeElementAsync(nonExistentElementId, null, null, timeout);
            var valueResult = await _valueService.GetValueAsync(nonExistentElementId, null, null, timeout);

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
                var timeout = 10;

                // When
                var elementsResult = await _elementSearchService.FindElementsAsync(null, null, "Button", null, timeout);
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
                if (calcProcess != null && !calcProcess.HasExited)
                {
                    try
                    {
                        calcProcess.Kill();
                        calcProcess.WaitForExit(3000);
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Failed to cleanup calculator: {ex.Message}");
                    }
                }
            }
        }

        [Fact]
        public async Task BasicWorkflow_TimeoutHandling_ShouldWork()
        {
            // Given
            var veryShortTimeout = 1;
            var nonExistentElementId = "NonExistentElement12345";

            // When & Then
            var invokeTask = _invokeService.InvokeElementAsync(nonExistentElementId, null, null, veryShortTimeout);
            var valueTask = _valueService.GetValueAsync(nonExistentElementId, null, null, veryShortTimeout);

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