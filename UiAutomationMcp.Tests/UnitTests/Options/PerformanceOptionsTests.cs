using UIAutomationMCP.Core.Options;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Options
{
    /// <summary>
    /// Unit tests for PerformanceOptions configuration
    /// </summary>
    public class PerformanceOptionsTests
    {
        [Fact]
        public void PerformanceOptions_DefaultValues_ShouldMatchExpectedDefaults()
        {
            // Arrange & Act
            var options = new PerformanceOptions();

            // Assert
            Assert.Equal(55, options.DefaultOperationTimeoutSeconds);
            Assert.Equal(110, options.TreeTraversalTimeoutSeconds);
            Assert.Equal(10, options.ToggleMaxIterations);
            Assert.Equal(10000, options.MaxElementCount);
            Assert.True(options.EnableCacheOptimization);
            Assert.False(options.VerboseCacheLogging);
        }

        [Fact]
        public void PerformanceOptions_CanSetCustomValues()
        {
            // Arrange
            var options = new PerformanceOptions
            {
                DefaultOperationTimeoutSeconds = 30,
                TreeTraversalTimeoutSeconds = 60,
                ToggleMaxIterations = 5,
                MaxElementCount = 5000,
                EnableCacheOptimization = false,
                VerboseCacheLogging = true
            };

            // Assert
            Assert.Equal(30, options.DefaultOperationTimeoutSeconds);
            Assert.Equal(60, options.TreeTraversalTimeoutSeconds);
            Assert.Equal(5, options.ToggleMaxIterations);
            Assert.Equal(5000, options.MaxElementCount);
            Assert.False(options.EnableCacheOptimization);
            Assert.True(options.VerboseCacheLogging);
        }

        [Fact]
        public void UIAutomationOptions_ShouldIncludePerformanceOptions()
        {
            // Arrange & Act
            var options = new UIAutomationOptions();

            // Assert
            Assert.NotNull(options.Performance);
            Assert.IsType<PerformanceOptions>(options.Performance);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(55)]
        [InlineData(300)]
        [InlineData(600)]
        public void PerformanceOptions_DefaultOperationTimeoutSeconds_WithinValidRange(int timeout)
        {
            // Arrange
            var options = new PerformanceOptions
            {
                DefaultOperationTimeoutSeconds = timeout
            };

            // Assert - Should not throw, values within Range(1, 600)
            Assert.Equal(timeout, options.DefaultOperationTimeoutSeconds);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        public void PerformanceOptions_MaxElementCount_WithinValidRange(int maxCount)
        {
            // Arrange
            var options = new PerformanceOptions
            {
                MaxElementCount = maxCount
            };

            // Assert - Should not throw, values within Range(100, 100000)
            Assert.Equal(maxCount, options.MaxElementCount);
        }
    }
}
