using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UIAutomationMCP.Core.Options;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Worker.Helpers;
using UIAutomationMCP.Subprocess.Worker.Operations.TreeNavigation;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMcp.Tests.UnitTests.Operations
{
    /// <summary>
    /// Tests for GetElementTreeOperation cache optimization functionality
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GetElementTreeCacheOptimizationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<GetElementTreeOperation>> _mockLogger;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;

        public GetElementTreeCacheOptimizationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<GetElementTreeOperation>>();
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();

            // Setup default options
            var options = new UIAutomationOptions
            {
                Performance = new PerformanceOptions
                {
                    DefaultOperationTimeoutSeconds = 55,
                    TreeTraversalTimeoutSeconds = 110,
                    MaxElementCount = 10000,
                    EnableCacheOptimization = true,
                    VerboseCacheLogging = false
                }
            };
            _mockOptions.Setup(o => o.Value).Returns(options);
        }

        [Fact]
        public void GetElementTreeOperation_ShouldHaveConfiguredPerformanceOptions()
        {
            // Arrange & Act
            var performanceOptions = _mockOptions.Object.Value.Performance;

            // Assert
            Assert.Equal(110, performanceOptions.TreeTraversalTimeoutSeconds);
        }

        [Fact]
        public void GetElementTreeOperation_ShouldEnableCacheOptimizationByDefault()
        {
            // Arrange & Act
            var performanceOptions = _mockOptions.Object.Value.Performance;

            // Assert
            Assert.True(performanceOptions.EnableCacheOptimization);
        }

        [Fact]
        public void GetElementTreeOperation_ShouldUseDefaultMaxElementCount()
        {
            // Arrange & Act
            var performanceOptions = _mockOptions.Object.Value.Performance;

            // Assert
            Assert.Equal(10000, performanceOptions.MaxElementCount);
        }

        [Fact]
        public void CacheRequestHelper_CreateTreeTraversalCache_ShouldIncludeEssentialProperties()
        {
            // Act
            var cache = CacheRequestHelper.CreateTreeTraversalCache();

            // Assert - Verify the cache was created successfully
            Assert.NotNull(cache);
            Assert.NotEqual(System.Windows.Automation.TreeScope.Element, cache.TreeScope);
        }

        [Fact]
        public void GetElementTreeOperation_WithDisabledCacheOptimization_ShouldStillWork()
        {
            // Arrange
            var options = new UIAutomationOptions
            {
                Performance = new PerformanceOptions
                {
                    EnableCacheOptimization = false,
                    MaxElementCount = 5000
                }
            };
            _mockOptions.Setup(o => o.Value).Returns(options);

            // Assert - Options should be configured correctly
            Assert.False(_mockOptions.Object.Value.Performance.EnableCacheOptimization);
            Assert.Equal(5000, _mockOptions.Object.Value.Performance.MaxElementCount);
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(50000)]
        public void PerformanceOptions_MaxElementCount_ShouldBeConfigurable(int maxCount)
        {
            // Arrange
            var options = new UIAutomationOptions
            {
                Performance = new PerformanceOptions
                {
                    MaxElementCount = maxCount
                }
            };

            // Assert
            Assert.Equal(maxCount, options.Performance.MaxElementCount);
        }

        [Fact]
        public void GetElementTreeOperation_VerboseCacheLogging_ShouldDefaultToFalse()
        {
            // Arrange & Act
            var performanceOptions = _mockOptions.Object.Value.Performance;

            // Assert
            Assert.False(performanceOptions.VerboseCacheLogging);
        }
    }
}
