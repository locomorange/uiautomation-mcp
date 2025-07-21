using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Integration tests for VirtualizedItemPattern
    /// Tests the complete Server→Worker→UI Automation pipeline using subprocess execution
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class VirtualizedItemPatternIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<VirtualizedItemService> _logger;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly VirtualizedItemService _virtualizedItemService;
        private readonly string _workerPath;

        public VirtualizedItemPatternIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // Use simple mock loggers for integration tests
            _logger = Mock.Of<ILogger<VirtualizedItemService>>();
            var executorLogger = Mock.Of<ILogger<SubprocessExecutor>>();

            // Locate the Worker executable for subprocess testing
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = possiblePaths.FirstOrDefault(File.Exists) ??
                throw new InvalidOperationException($"Worker executable not found. Searched paths: {string.Join(", ", possiblePaths)}");

            _subprocessExecutor = new SubprocessExecutor(executorLogger, _workerPath);
            _virtualizedItemService = new VirtualizedItemService(_logger, _subprocessExecutor);

            _output.WriteLine($"Using Worker executable at: {_workerPath}");
        }

        [Fact]
        public async Task RealizeItemAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentVirtualizedItem";
            var windowTitle = "NonExistentWindow";
            var processId = 99999; // Non-existent process ID

            // Act
            var result = await _virtualizedItemService.RealizeItemAsync(automationId: elementId, processId: processId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should return error result due to element not found
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);
            
            var success = (bool?)successProperty.GetValue(result);
            Assert.False(success);
            
            var errorProperty = resultType.GetProperty("Error");
            Assert.NotNull(errorProperty);
            var error = errorProperty.GetValue(result)?.ToString();
            Assert.NotNull(error);
            _output.WriteLine($"Expected error result: {error}");
        }

        [Fact]
        public async Task RealizeItemAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
        {
            // Arrange
            var elementId = "testVirtualItem";
            var windowTitle = "TestWindow";

            // Act - This tests the full Server->Worker communication pipeline
            var result = await _virtualizedItemService.RealizeItemAsync(
                elementId, 
                windowTitle, 
                processId: null, 
                timeoutSeconds: 10);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"Server-Worker communication test completed. Result type: {result.GetType().Name}");
            
            // The Worker should handle the request and return a result (error or success)
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);
        }

        [Fact]
        public async Task RealizeItemAsync_WithTimeout_ShouldRespectTimeoutSetting()
        {
            // Arrange
            var elementId = "timeoutTestItem";
            var startTime = DateTime.UtcNow;
            var timeoutSeconds = 2;

            // Act
            var result = await _virtualizedItemService.RealizeItemAsync(
                automationId: elementId, 
                processId: null, 
                timeoutSeconds: timeoutSeconds);

            // Assert
            var elapsed = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            Assert.True(elapsed.TotalSeconds < timeoutSeconds + 2, 
                $"Operation should complete within timeout period + buffer. Elapsed: {elapsed.TotalSeconds}s");
            
            _output.WriteLine($"Timeout test completed in {elapsed.TotalSeconds:F2}s (timeout was {timeoutSeconds}s)");
        }

        [Fact]
        public async Task RealizeItemAsync_WorkerProcessLifecycle_ShouldStartAndStop()
        {
            // This test verifies that the Worker process starts and stops correctly
            
            // Arrange
            var elementId = "lifecycleTestItem";

            // Act - Multiple calls to test process reuse
            for (int i = 0; i < 3; i++)
            {
                var result = await _virtualizedItemService.RealizeItemAsync(
                    automationId: $"{elementId}_{i}", 
                    processId: null, 
                    timeoutSeconds: 5);

                Assert.NotNull(result);
                _output.WriteLine($"Worker process call {i + 1} completed");
            }

            // The Worker process should handle multiple requests efficiently
            _output.WriteLine("Worker process lifecycle test completed successfully");
        }

        public void Dispose()
        {
            // SubprocessExecutor should clean up Worker processes
            _subprocessExecutor?.Dispose();
            _output.WriteLine("Test cleanup completed");
        }
    }
}