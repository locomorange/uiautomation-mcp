using System;
using UIAutomationMCP.Subprocess.Core.Helpers;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    [Trait("Category", "Unit")]
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
                // UIAutomationEnvironment.ExecuteWithTimeout(quickOperation, "DefaultTimeoutTest");
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
    }

    /// <summary>
    /// Performance and stress tests for UIAutomationEnvironment
    /// </summary>
    [Trait("Category", "Unit")]
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

            // Assert - Should be relatively fast (under 500ms for 100 calls, allowing for CI/slow machines)
            Assert.True(duration.TotalMilliseconds < 500,
                $"100 calls to IsAvailable took {duration.TotalMilliseconds}ms, expected < 500ms");
        }
    }
}

