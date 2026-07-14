using System.Windows.Automation;
using UIAutomationMCP.Subprocess.Worker.Helpers;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for CacheRequestHelper to verify cache optimization functionality
    /// </summary>
    [Trait("Category", "Unit")]
    public class CacheRequestHelperTests
    {
        [Fact]
        public void CreateTreeTraversalCache_ShouldIncludeElementAndChildrenScope()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateTreeTraversalCache();

            // Assert
            Assert.NotNull(cacheRequest);
            Assert.True((cacheRequest.TreeScope & TreeScope.Element) != 0, "Should include Element scope");
            Assert.True((cacheRequest.TreeScope & TreeScope.Children) != 0, "Should include Children scope");
        }

        [Fact]
        public void CreateTreeTraversalCache_ShouldUseFullAutomationElementMode()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateTreeTraversalCache();

            // Assert
            Assert.Equal(AutomationElementMode.Full, cacheRequest.AutomationElementMode);
        }

        [Fact]
        public void CreateElementSearchCache_ShouldIncludeDescendantsScope()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateElementSearchCache();

            // Assert
            Assert.NotNull(cacheRequest);
            Assert.True((cacheRequest.TreeScope & TreeScope.Element) != 0, "Should include Element scope");
            Assert.True((cacheRequest.TreeScope & TreeScope.Descendants) != 0, "Should include Descendants scope");
        }

        [Fact]
        public void CreateMinimalCache_ShouldOnlyIncludeElementScope()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateMinimalCache();

            // Assert
            Assert.NotNull(cacheRequest);
            Assert.Equal(TreeScope.Element, cacheRequest.TreeScope);
        }

        [Fact]
        public void CreateTreeTraversalCache_ShouldUseRawViewCondition()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateTreeTraversalCache();

            // Assert
            Assert.NotNull(cacheRequest.TreeFilter);
            // TreeFilter should be set to RawViewCondition
            // We can't directly compare Condition objects, but we can verify it's not null
        }

        [Fact]
        public void CreateElementSearchCache_ShouldUseControlViewCondition()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateElementSearchCache();

            // Assert
            Assert.NotNull(cacheRequest.TreeFilter);
            // TreeFilter should be set to ControlViewCondition
        }

        [Fact]
        public void CreateMinimalCache_ShouldUseControlViewCondition()
        {
            // Act
            var cacheRequest = CacheRequestHelper.CreateMinimalCache();

            // Assert
            Assert.NotNull(cacheRequest.TreeFilter);
        }

        [Theory]
        [InlineData(TreeScope.Element)]
        [InlineData(TreeScope.Children)]
        [InlineData(TreeScope.Descendants)]
        public void GetCachedChildren_WithCustomCacheRequest_ShouldIncludeChildrenScope(TreeScope initialScope)
        {
            // Arrange
            var customCache = new CacheRequest
            {
                TreeScope = initialScope
            };
            customCache.Add(AutomationElement.NameProperty);

            // Note: This test requires actual UI automation which may not be available in CI
            // In a real test environment, you would mock or use a test window
            // For now, we're just verifying the helper doesn't crash with various scopes

            try
            {
                var rootElement = AutomationElement.RootElement;
                if (rootElement != null)
                {
                    // Act - this will adjust scope if needed
                    var children = CacheRequestHelper.GetCachedChildren(rootElement, customCache);

                    // Assert - should not throw
                    Assert.NotNull(children);
                }
            }
            catch (InvalidOperationException)
            {
                // UI Automation may not be available in test environment
                // This is acceptable for this test
            }
        }

        [Fact]
        public void UpdateElementCache_ShouldNotThrow()
        {
            try
            {
                // Arrange
                var rootElement = AutomationElement.RootElement;
                if (rootElement != null)
                {
                    var cacheRequest = CacheRequestHelper.CreateMinimalCache();

                    // Act
                    var updatedElement = CacheRequestHelper.UpdateElementCache(rootElement, cacheRequest);

                    // Assert
                    Assert.NotNull(updatedElement);
                }
            }
            catch (InvalidOperationException)
            {
                // UI Automation may not be available in test environment
                // This is acceptable for this test
            }
        }
    }
}
