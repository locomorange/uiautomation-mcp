using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    // Test request and result types
    public class TestTimeoutRequest
    {
        public int DelayMs { get; set; }
    }

    public class TestTimeoutResult
    {
        public bool Completed { get; set; }
    }

    /// <summary>
    /// Test operation that simulates a long-running operation
    /// </summary>
    public class SlowTestOperation : BaseUIAutomationOperation<TestTimeoutRequest, TestTimeoutResult>
    {
        /// <summary>
        /// Override to use a very short timeout for testing
        /// </summary>
        protected override int OperationTimeoutSeconds => 2;

        public SlowTestOperation(ElementFinderService elementFinderService, ILogger<SlowTestOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<TestTimeoutResult> ExecuteOperationAsync(TestTimeoutRequest request)
        {
            await Task.Delay(request.DelayMs);
            return new TestTimeoutResult { Completed = true };
        }
    }

    /// <summary>
    /// Test operation with default timeout
    /// </summary>
    public class DefaultTimeoutTestOperation : BaseUIAutomationOperation<TestTimeoutRequest, TestTimeoutResult>
    {
        public DefaultTimeoutTestOperation(ElementFinderService elementFinderService, ILogger<DefaultTimeoutTestOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<TestTimeoutResult> ExecuteOperationAsync(TestTimeoutRequest request)
        {
            await Task.Delay(request.DelayMs);
            return new TestTimeoutResult { Completed = true };
        }
    }

    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class BaseUIAutomationOperationTimeoutTests
    {
        private readonly Mock<ILogger<SlowTestOperation>> _slowLogger;
        private readonly Mock<ILogger<DefaultTimeoutTestOperation>> _defaultLogger;
        private readonly Mock<ElementFinderService> _mockElementFinder;

        public BaseUIAutomationOperationTimeoutTests()
        {
            _slowLogger = new Mock<ILogger<SlowTestOperation>>();
            _defaultLogger = new Mock<ILogger<DefaultTimeoutTestOperation>>();

            // ElementFinderService requires a logger in its constructor
            var elementFinderLogger = new Mock<ILogger<ElementFinderService>>();
            _mockElementFinder = new Mock<ElementFinderService>(elementFinderLogger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_FastOperation_ShouldSucceed()
        {
            // Arrange
            var operation = new SlowTestOperation(_mockElementFinder.Object, _slowLogger.Object);
            var request = new TestTimeoutRequest { DelayMs = 100 }; // 100ms - well within 2s timeout

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data!.Completed);
        }

        [Fact]
        public async Task ExecuteAsync_SlowOperation_ShouldTimeoutWithErrorMessage()
        {
            // Arrange
            var operation = new SlowTestOperation(_mockElementFinder.Object, _slowLogger.Object);
            var request = new TestTimeoutRequest { DelayMs = 10000 }; // 10s - exceeds 2s timeout

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await operation.ExecuteAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("timed out", result.Error!, StringComparison.OrdinalIgnoreCase);
            // Should timeout reasonably close to the 2s limit, not hang for 10s
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected less than 5000ms");
        }

        [Fact]
        public async Task ExecuteAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var operation = new DefaultTimeoutTestOperation(_mockElementFinder.Object, _defaultLogger.Object);
            var request = new TestTimeoutRequest { DelayMs = 60000 }; // 60s - would take long
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Cancel after 1s

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await operation.ExecuteAsync(request, cts.Token);
            stopwatch.Stop();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("cancelled", result.Error!, StringComparison.OrdinalIgnoreCase);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected less than 5000ms");
        }

        [Fact]
        public void DefaultTimeoutOperation_ShouldHave55SecondDefault()
        {
            // Arrange
            var operation = new DefaultTimeoutTestOperation(_mockElementFinder.Object, _defaultLogger.Object);

            // Assert - verify the default timeout via reflection since it's protected
            var property = typeof(BaseUIAutomationOperation<TestTimeoutRequest, TestTimeoutResult>)
                .GetProperty("OperationTimeoutSeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(property);
            var timeout = (int)property!.GetValue(operation)!;
            Assert.Equal(55, timeout);
        }

        [Fact]
        public void SlowTestOperation_ShouldHaveOverriddenTimeout()
        {
            // Arrange
            var operation = new SlowTestOperation(_mockElementFinder.Object, _slowLogger.Object);

            // Assert - verify the overridden timeout
            var property = typeof(BaseUIAutomationOperation<TestTimeoutRequest, TestTimeoutResult>)
                .GetProperty("OperationTimeoutSeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(property);
            var timeout = (int)property!.GetValue(operation)!;
            Assert.Equal(2, timeout);
        }

        [Fact]
        public async Task ExecuteAsync_WithNullRequest_ShouldReturnValidationError()
        {
            // Arrange
            var operation = new SlowTestOperation(_mockElementFinder.Object, _slowLogger.Object);

            // Act
            var result = await operation.ExecuteAsync((TestTimeoutRequest)null!);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("null", result.Error!, StringComparison.OrdinalIgnoreCase);
        }
    }
}
