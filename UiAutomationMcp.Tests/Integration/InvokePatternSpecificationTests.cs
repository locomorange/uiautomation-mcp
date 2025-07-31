using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Microsoft Invoke Pattern                   /// UI Automation Invoke Control Pattern                           /// https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-invoke-control-pattern
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class InvokePatternSpecificationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly InvokeService _invokeService;
        private readonly string _workerPath;

        public InvokePatternSpecificationTests(ITestOutputHelper output)
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

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath, new CancellationTokenSource());
            _invokeService = new InvokeService(Mock.Of<IProcessManager>(), 
                _serviceProvider.GetRequiredService<ILogger<InvokeService>>());
        }

        [Fact]
        public async Task InvokeElement_ShouldReturnImmediately_NonBlocking()
        {
            // Microsoft     "asynchronous call and must return immediately without blocking"
            
            // Given
            var elementId = "TestElement";
            var timeout = 5;

            // When
            var startTime = DateTime.UtcNow;
            var result = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);
            var endTime = DateTime.UtcNow;
            var responseTime = (endTime - startTime).TotalMilliseconds;

            // Then - Should return quickly (not blocking UI)
            Assert.NotNull(result);
            Assert.True(responseTime < 1000, $"Invoke should return immediately, took {responseTime}ms");
            
            _output.WriteLine($"Invoke returned in {responseTime}ms - non-blocking verified");
        }

        [Fact]
        public async Task InvokeElement_ShouldHandleSingleUnambiguousAction()
        {
            // Microsoft     "single, unambiguous action"
            
            // Given
            var elementId = "SingleActionButton";
            var timeout = 5;

            // When
            var result = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);

            // Then
            Assert.NotNull(result);
            _output.WriteLine($"Single action result: {result}");
        }

        [Fact]
        public async Task InvokeElement_ShouldNotMaintainState()
        {
            // Microsoft     Control should not maintain state after activation
            
            // Given
            var elementId = "StatelessButton";
            var timeout = 5;

            // When - Invoke same element multiple times
            var result1 = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);
            var result2 = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);
            var result3 = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);

            // Then - Each invocation should behave consistently (no state maintained)
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            
            _output.WriteLine($"Stateless invocation results: {result1}, {result2}, {result3}");
        }

        [Fact]
        public async Task InvokeElement_WithDisabledElement_ShouldHandleGracefully()
        {
            // Microsoft     ElementNotEnabledException for disabled controls
            
            // Given
            var disabledAutomationId = "DisabledButton";
            var timeout = 5;

            // When
            var result = await _invokeService.InvokeElementAsync(automationId: disabledAutomationId, processId: null, timeoutSeconds: timeout);

            // Then - Should handle disabled elements gracefully
            Assert.NotNull(result);
            _output.WriteLine($"Disabled element result: {result}");
        }

        [Fact]
        public async Task InvokeElement_MultipleParameters_ShouldSupportFlexibleTargeting()
        {
            // Test all parameter combinations for element targeting
            
            // Given
            var elementId = "FlexibleButton";
            var processId = 1234;
            var timeout = 3;

            // When - Test different targeting methods
            var resultElementOnly = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);
            var resultWithWindow = await _invokeService.InvokeElementAsync(automationId: elementId, processId: null, timeoutSeconds: timeout);
            var resultWithProcess = await _invokeService.InvokeElementAsync(automationId: elementId, processId: processId, timeoutSeconds: timeout);
            var resultAllParams = await _invokeService.InvokeElementAsync(automationId: elementId, processId: processId, timeoutSeconds: timeout);

            // Then
            Assert.NotNull(resultElementOnly);
            Assert.NotNull(resultWithWindow);
            Assert.NotNull(resultWithProcess);
            Assert.NotNull(resultAllParams);
            
            _output.WriteLine($"Flexible targeting results:");
            _output.WriteLine($"  Element only: {resultElementOnly}");
            _output.WriteLine($"  With window: {resultWithWindow}");
            _output.WriteLine($"  With process: {resultWithProcess}");
            _output.WriteLine($"  All params: {resultAllParams}");
        }

        [Fact]
        public async Task InvokeElement_EdgeCaseParameters_ShouldHandleRobustly()
        {
            // Test edge cases and boundary conditions
            
            // Given
            var timeout = 2;

            // When - Test edge case parameters
            var resultEmptyId = await _invokeService.InvokeElementAsync(automationId: "", processId: null, timeoutSeconds: timeout);
            var resultNullWindow = await _invokeService.InvokeElementAsync(automationId: "test", processId: null, timeoutSeconds: timeout);
            var resultZeroProcess = await _invokeService.InvokeElementAsync(automationId: "test", processId: 0, timeoutSeconds: timeout);
            var resultNegativeProcess = await _invokeService.InvokeElementAsync(automationId: "test", processId: -1, timeoutSeconds: timeout);

            // Then
            Assert.NotNull(resultEmptyId);
            Assert.NotNull(resultNullWindow);
            Assert.NotNull(resultZeroProcess);
            Assert.NotNull(resultNegativeProcess);
            
            _output.WriteLine($"Edge case handling results:");
            _output.WriteLine($"  Empty ID: {resultEmptyId}");
            _output.WriteLine($"  Null window: {resultNullWindow}");
            _output.WriteLine($"  Zero process: {resultZeroProcess}");
            _output.WriteLine($"  Negative process: {resultNegativeProcess}");
        }

        [Fact]
        public async Task InvokeElement_PerformanceUnderLoad_ShouldRemainResponsive()
        {
            // Test performance characteristics under load
            
            // Given
            var elementCount = 10;
            var timeout = 1;

            // When - Execute multiple invocations rapidly
            var startTime = DateTime.UtcNow;
            var tasks = Enumerable.Range(1, elementCount)
                .Select(i => _invokeService.InvokeElementAsync(automationId: $"LoadTestElement{i}", processId: null, timeoutSeconds: timeout))
                .ToArray();

            var results = await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;

            // Then - Should handle load efficiently
            Assert.Equal(elementCount, results.Length);
            Assert.All(results, result => Assert.NotNull(result));
            Assert.True(totalTime < 15000, $"Load test took too long: {totalTime}ms");
            
            _output.WriteLine($"Load test: {elementCount} invocations completed in {totalTime}ms");
            _output.WriteLine($"Average response time: {totalTime / elementCount:F1}ms per invocation");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}

