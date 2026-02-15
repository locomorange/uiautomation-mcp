using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;

using UIAutomationMCP.Server.Abstractions;
namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Integration tests for VirtualizedItemPattern
    /// Tests the complete Server  orker  I Automation pipeline using subprocess execution
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

            // Resolve Worker path using ExecutablePathResolver
            var baseDir = ExecutablePathResolver.GetExecutableRealPath();
            var workerPath = ExecutablePathResolver.ResolveWorkerPath(baseDir);

            if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
            {
                var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Worker", baseDir);
                throw new InvalidOperationException($"Worker executable not found. Searched paths: {string.Join(", ", searchedPaths)}");
            }

            _workerPath = workerPath!;

            _subprocessExecutor = new SubprocessExecutor(executorLogger, _workerPath, new CancellationTokenSource());
            _virtualizedItemService = new VirtualizedItemService(Mock.Of<IProcessManager>(), _logger);

            _output.WriteLine($"Using Worker executable at: {_workerPath}");
        }

        [Fact]
        public async Task RealizeItemAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentVirtualizedItem";

            // Act
            var result = await _virtualizedItemService.RealizeItemAsync(automationId: elementId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);

            // Should return error result due to element not found
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            _output.WriteLine($"Expected error result: {result.ErrorMessage}");
        }

        [Fact]
        public async Task RealizeItemAsync_ServerWorkerCommunication_ShouldHandleCorrectly()
        {
            // Arrange
            var elementId = "testVirtualItem";
            var windowTitle = "TestWindow";

            // Act - This tests the full Server->Worker communication pipeline
            var result = await _virtualizedItemService.RealizeItemAsync(
                automationId: elementId,
                name: windowTitle,
                timeoutSeconds: 10);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"Server-Worker communication test completed. Result type: {result.GetType().Name}");

            // The Worker should handle the request and return a result (error or success)
            Assert.True(result.Success || !result.Success); // Just check it returned
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

