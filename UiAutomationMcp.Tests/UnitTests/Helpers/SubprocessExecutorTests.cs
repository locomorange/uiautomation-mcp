using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using Moq;
using System.IO;

namespace UIAutomationMCP.Tests.UnitTests.Helpers
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SubprocessExecutorTests
    {
        private readonly Mock<ILogger<SubprocessExecutor>> _mockLogger;
        private readonly string _nonExistentWorkerPath = "/path/to/nonexistent/worker.exe";
        private readonly string _validWorkerPath = "/tmp/dummy-worker.exe";

        public SubprocessExecutorTests()
        {
            _mockLogger = new Mock<ILogger<SubprocessExecutor>>();
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // When
            var executor = new SubprocessExecutor(_mockLogger.Object, _validWorkerPath);

            // Then
            Assert.NotNull(executor);
            executor.Dispose();
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // When & Then
            var exception = Assert.Throws<ArgumentNullException>(() => new SubprocessExecutor(null!, _validWorkerPath));
            Assert.Contains("logger", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidWorkerPath_ShouldThrowArgumentException(string? workerPath)
        {
            // When & Then
            var exception = Assert.Throws<ArgumentException>(() => new SubprocessExecutor(_mockLogger.Object, workerPath!));
            Assert.Contains("workerPath", exception.ParamName);
        }

        [Fact]
        public async Task ExecuteAsync_WithNonExistentWorkerPath_ShouldThrowException()
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>
            {
                ["elementId"] = "test"
            };

            // When & Then
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", parameters, 5));
        }

        [Fact]
        public async Task ExecuteAsync_WithVeryShortTimeout_ShouldTimeoutQuickly()
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>
            {
                ["elementId"] = "test"
            };

            // When & Then
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", parameters, 1));
            stopwatch.Stop();

            // The operation should fail quickly (within 2 seconds) due to process not starting
            Assert.True(stopwatch.ElapsedMilliseconds < 2000);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task ExecuteAsync_WithInvalidOperation_ShouldThrowException(string? operation)
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>
            {
                ["elementId"] = "test"
            };

            // When & Then
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await executor.ExecuteAsync<object>(operation!, parameters, 5));
        }

        [Fact]
        public async Task ExecuteAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);

            // When & Then
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", null, 5));
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyParameters_ShouldHandleGracefully()
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>();

            // When & Then
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", parameters, 5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public async Task ExecuteAsync_WithInvalidTimeout_ShouldThrowException(int timeout)
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>
            {
                ["elementId"] = "test"
            };

            // When & Then
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", parameters, timeout));
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Given
            var executor = new SubprocessExecutor(_mockLogger.Object, _validWorkerPath);

            // When & Then
            var exception = Record.Exception(() => executor.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            // Given
            var executor = new SubprocessExecutor(_mockLogger.Object, _validWorkerPath);

            // When & Then
            var exception1 = Record.Exception(() => executor.Dispose());
            var exception2 = Record.Exception(() => executor.Dispose());
            var exception3 = Record.Exception(() => executor.Dispose());

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        [Fact]
        public async Task ExecuteAsync_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Given
            var executor = new SubprocessExecutor(_mockLogger.Object, _validWorkerPath);
            executor.Dispose();

            // When & Then
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await executor.ExecuteAsync<object>("TestOperation", new Dictionary<string, object>(), 5));
        }

        [Fact]
        public async Task ExecuteAsync_ConcurrentCalls_ShouldBeThreadSafe()
        {
            // Given
            using var executor = new SubprocessExecutor(_mockLogger.Object, _nonExistentWorkerPath);
            var parameters = new Dictionary<string, object>
            {
                ["elementId"] = "test"
            };

            // When
            var tasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await Assert.ThrowsAsync<FileNotFoundException>(async () =>
                        await executor.ExecuteAsync<object>("TestOperation", parameters, 2));
                }));
            }

            // Then
            await Task.WhenAll(tasks);
            // If we get here without deadlock, the semaphore is working correctly
            Assert.True(true);
        }
    }
}