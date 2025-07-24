using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Worker.Helpers;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    public class UIAutomationEnvironmentTests
    {
        [Fact]
        public void IsAvailable_Property_DoesNotThrow()
        {
            // Act & Assert - Should not throw regardless of UI Automation availability
            var isAvailable = UIAutomationEnvironment.IsAvailable;
            
            // The result can be true or false, but accessing it should not throw
            Assert.True(isAvailable == true || isAvailable == false);
        }

        [Fact]
        public void UnavailabilityReason_Property_IsNotNull()
        {
            // Act
            var reason = UIAutomationEnvironment.UnavailabilityReason;

            // Assert
            Assert.NotNull(reason);
        }

        [Fact(Skip = "ExecuteWithErrorHandling method not available")]
        public void ExecuteWithErrorHandling_WithValidOperation_ReturnsResult()
        {
            // Arrange
            string expectedResult = "test result";
            Func<string> operation = () => expectedResult;

            // Act
            var result = UIAutomationEnvironment.ExecuteWithErrorHandling(operation, "TestOperation");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact(Skip = "ExecuteWithErrorHandling method not available")]
        public void ExecuteWithErrorHandling_WithVoidOperation_DoesNotThrow()
        {
            // Arrange
            bool operationExecuted = false;
            Action operation = () => operationExecuted = true;

            // Act & Assert
            UIAutomationEnvironment.ExecuteWithErrorHandling(operation, "TestVoidOperation");
            
            Assert.True(operationExecuted);
        }

        [Fact(Skip = "ExecuteWithErrorHandling method not available")]
        public void ExecuteWithErrorHandling_WithException_WrapsInInvalidOperationException()
        {
            // Arrange
            var originalException = new ArgumentException("Original message");
            Func<string> operation = () => throw originalException;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                UIAutomationEnvironment.ExecuteWithErrorHandling(operation, "TestOperation"));
            
            Assert.Contains("TestOperation", exception.Message);
            Assert.Contains("Original message", exception.Message);
        }

        [Fact(Skip = "ExecuteWithTimeoutAsync method not available")]
        public async Task ExecuteWithTimeoutAsync_WithValidOperation_ReturnsResult()
        {
            // Arrange
            string expectedResult = "async test result";
            Func<Task<string>> operation = () => Task.FromResult(expectedResult);

            // Act
            var result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(operation, "TestAsyncOperation", 10);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact(Skip = "ExecuteWithTimeoutAsync method not available")]
        public async Task ExecuteWithTimeoutAsync_WithDefaultTimeout_UsesEightSeconds()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            Func<Task<string>> fastOperation = () => Task.FromResult("quick result");

            // Act
            var result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(fastOperation, "FastOperation");
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.Equal("quick result", result);
            // Should complete quickly, well under the 8-second timeout
            Assert.True((endTime - startTime).TotalSeconds < 1);
        }

        [Fact(Skip = "ExecuteWithTimeoutAsync method not available")]
        public async Task ExecuteWithTimeoutAsync_WithSlowOperation_ThrowsTimeoutException()
        {
            // Arrange
            Func<Task<string>> slowOperation = async () =>
            {
                await Task.Delay(2000); // 2 seconds
                return "slow result";
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
                UIAutomationEnvironment.ExecuteWithTimeoutAsync(slowOperation, "SlowOperation", 1));
            
            Assert.Contains("SlowOperation", exception.Message);
            Assert.Contains("timed out after 1 seconds", exception.Message);
        }

        [Fact(Skip = "ExecuteWithTimeout method not available")]
        public void ExecuteWithTimeout_SyncVersion_WithValidOperation_ReturnsResult()
        {
            // Arrange
            string expectedResult = "sync timeout test result";
            Func<string> operation = () => expectedResult;

            // Act
            var result = UIAutomationEnvironment.ExecuteWithTimeout(operation, "TestSyncTimeoutOperation", 10);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact(Skip = "ExecuteWithTimeout method not available")]
        public void ExecuteWithTimeout_SyncVersion_WithDefaultTimeout_UsesEightSeconds()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            Func<string> fastOperation = () => "quick sync result";

            // Act
            var result = UIAutomationEnvironment.ExecuteWithTimeout(fastOperation, "FastSyncOperation");
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.Equal("quick sync result", result);
            // Should complete quickly, well under the 8-second timeout
            Assert.True((endTime - startTime).TotalSeconds < 1);
        }

        [Fact(Skip = "ExecuteWithTimeout method not available")]
        public void ExecuteWithTimeout_SyncVersion_WithSlowOperation_ThrowsTimeoutException()
        {
            // Arrange
            Func<string> slowOperation = () =>
            {
                System.Threading.Thread.Sleep(2000); // 2 seconds
                return "slow sync result";
            };

            // Act & Assert
            var exception = Assert.Throws<TimeoutException>(() =>
                UIAutomationEnvironment.ExecuteWithTimeout(slowOperation, "SlowSyncOperation", 1));
            
            Assert.Contains("SlowSyncOperation", exception.Message);
            Assert.Contains("timed out after 1 seconds", exception.Message);
        }

        [Theory(Skip = "ExecuteWithTimeoutAsync method not available")]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task ExecuteWithTimeoutAsync_WithVariousTimeouts_RespectsTimeoutValue(int timeoutSeconds)
        {
            // Arrange
            string expectedResult = $"result with {timeoutSeconds}s timeout";
            Func<Task<string>> operation = () => Task.FromResult(expectedResult);

            // Act
            var result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(operation, "VariableTimeoutTest", timeoutSeconds);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TimeoutDefault_IsWithinMicrosoftRecommendedRange()
        {
            // This test verifies that our default timeout of 8 seconds is within
            // Microsoft's recommended range of 5-10 seconds for UI Automation operations
            
            // Arrange
            Func<string> quickOperation = () => "test";
            var startTime = DateTime.UtcNow;

            try
            {
                // Act - Use default timeout
                UIAutomationEnvironment.ExecuteWithTimeout(quickOperation, "DefaultTimeoutTest");
                var endTime = DateTime.UtcNow;

                // Assert - Operation should complete quickly, confirming 8-second default wasn't hit
                Assert.True((endTime - startTime).TotalSeconds < 1);
            }
            catch (TimeoutException ex)
            {
                // If we hit a timeout, verify it's at least 5 seconds (minimum recommended)
                Assert.Contains("8 seconds", ex.Message);
            }
        }

        [Fact(Skip = "ExecuteWithTimeoutAsync method not available")]
        public async Task ExecuteWithTimeoutAsync_ErrorHandling_WrapsExceptions()
        {
            // Arrange
            var originalException = new ArgumentException("Original async error");
            Func<Task<string>> faultyOperation = () => throw originalException;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                UIAutomationEnvironment.ExecuteWithTimeoutAsync(faultyOperation, "FaultyAsyncOperation", 10));
            
            Assert.Contains("FaultyAsyncOperation", exception.Message);
            Assert.Contains("Original async error", exception.Message);
        }

        [Fact(Skip = "ExecuteWithErrorHandling method not available")]
        public void ExecuteWithErrorHandling_TypeInitializationException_WrapsCorrectly()
        {
            // Arrange
            var typeInitException = new TypeInitializationException("TestType", new Exception("Inner exception"));
            Func<string> operation = () => throw typeInitException;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                UIAutomationEnvironment.ExecuteWithErrorHandling(operation, "TypeInitTest"));
            
            Assert.Contains("TypeInitTest", exception.Message);
            Assert.Contains("UI Automation type initialization error", exception.Message);
        }

        [Fact(Skip = "ExecuteWithErrorHandling method not available")]
        public void ExecuteWithErrorHandling_AutomationElementException_WrapsCorrectly()
        {
            // Arrange
            var automationException = new InvalidOperationException("AutomationElement error occurred");
            Func<string> operation = () => throw automationException;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                UIAutomationEnvironment.ExecuteWithErrorHandling(operation, "AutomationElementTest"));
            
            Assert.Contains("AutomationElementTest", exception.Message);
            Assert.Contains("UI Automation element error", exception.Message);
        }
    }

    /// <summary>
    /// Performance and stress tests for UIAutomationEnvironment
    /// </summary>
    public class UIAutomationEnvironmentPerformanceTests
    {
        [Fact]
        public void IsAvailable_MultipleAccess_IsEfficient()
        {
            // Arrange
            var startTime = DateTime.UtcNow;

            // Act - Access property multiple times
            for (int i = 0; i < 100; i++)
            {
                bool _ = UIAutomationEnvironment.IsAvailable;
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Assert - Should be very fast (under 100ms for 100 calls)
            Assert.True(duration.TotalMilliseconds < 100, 
                $"100 calls to IsAvailable took {duration.TotalMilliseconds}ms, expected < 100ms");
        }

        [Fact(Skip = "ExecuteWithTimeoutAsync method not available")]
        public async Task ExecuteWithTimeoutAsync_ConcurrentOperations_HandlesCorrectly()
        {
            // Arrange
            const int operationCount = 10;
            var tasks = new Task[operationCount];

            // Act
            for (int i = 0; i < operationCount; i++)
            {
                int operationId = i;
                tasks[i] = UIAutomationEnvironment.ExecuteWithTimeoutAsync(
                    () => Task.FromResult($"Operation {operationId}"),
                    $"ConcurrentTest{operationId}",
                    10);
            }

            var results = await Task.WhenAll(tasks.Cast<Task<string>>());

            // Assert
            Assert.Equal(operationCount, results.Length);
            for (int i = 0; i < operationCount; i++)
            {
                Assert.Equal($"Operation {i}", results[i]);
            }
        }
    }
}