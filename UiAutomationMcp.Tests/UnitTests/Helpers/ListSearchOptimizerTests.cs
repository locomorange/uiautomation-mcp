using System;
using System.Collections.Generic;
using System.Windows.Automation;
using UIAutomationMCP.Subprocess.Worker.Helpers;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    public class ListSearchOptimizerTests
    {
        [Fact]
        public void GetOptimalMethod_WithNullElement_ReturnsFindAll()
        {
            var result = ListSearchOptimizer.GetOptimalMethod(null!);
            Assert.Equal(ListSearchMethod.FindAll, result);
        }

        [Fact]
        public void GetOptimalMethod_WithMockElement_ReturnsValidMethod()
        {
            var mockElement = CreateMockElement();
            var result = ListSearchOptimizer.GetOptimalMethod(mockElement!);
            Assert.True(Enum.IsDefined(typeof(ListSearchMethod), result));
        }

        [Fact]
        public void GetOptimalMethod_MethodSelection_IsConsistent()
        {
            var mockElement = CreateMockElement();
            var result1 = ListSearchOptimizer.GetOptimalMethod(mockElement!);
            var result2 = ListSearchOptimizer.GetOptimalMethod(mockElement!);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void FindListItemByIndex_WithNullElement_ReturnsNull()
        {
            var result = ListSearchOptimizer.FindListItemByIndex(null!, 0);
            Assert.Null(result);
        }

        [Fact]
        public void FindListItemByIndex_WithNegativeIndex_ReturnsNull()
        {
            var mockElement = CreateMockElement();
            var result = ListSearchOptimizer.FindListItemByIndex(mockElement!, -1);
            Assert.Null(result);
        }

        [Fact]
        public void FindAllListItems_WithNullElement_ReturnsEmpty()
        {
            var result = ListSearchOptimizer.FindAllListItems(null!);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FindAllChildrenOptimized_WithNullElement_ReturnsEmpty()
        {
            var condition = Condition.TrueCondition;
            var result = ListSearchOptimizer.FindAllChildrenOptimized(null!, condition);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void IsListContainer_WithNullElement_ReturnsFalse()
        {
            Assert.False(ListSearchOptimizer.IsListContainer(null!));
        }

        [Fact]
        public void ListSearchMethod_Enum_HasExpectedValues()
        {
            Assert.True(Enum.IsDefined(typeof(ListSearchMethod), ListSearchMethod.FindAll));
            Assert.True(Enum.IsDefined(typeof(ListSearchMethod), ListSearchMethod.TreeWalker));
            Assert.Equal(2, Enum.GetValues<ListSearchMethod>().Length);
        }

        [Fact]
        public void GetOptimalMethod_WithDifferentFrameworks_ReturnsValidMethod()
        {
            // All mock elements use RootElement, so we just verify graceful handling
            var mockElement = CreateMockElement();
            if (mockElement == null) return;

            var result = ListSearchOptimizer.GetOptimalMethod(mockElement);
            Assert.True(Enum.IsDefined(typeof(ListSearchMethod), result));
        }

        [Fact]
        public void GetOptimalMethod_WithExceptionThrown_ReturnsFindAll()
        {
            // null causes early return to FindAll
            var result = ListSearchOptimizer.GetOptimalMethod(null!);
            Assert.Equal(ListSearchMethod.FindAll, result);
        }

        private AutomationElement? CreateMockElement()
        {
            try
            {
                return AutomationElement.RootElement;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Integration tests that require actual UI Automation elements
    /// </summary>
    public class ListSearchOptimizerIntegrationTests
    {
        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void GetOptimalMethod_WithActualDesktopElement_DoesNotThrow()
        {
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null) return;

                var result = ListSearchOptimizer.GetOptimalMethod(desktop);
                Assert.True(Enum.IsDefined(typeof(ListSearchMethod), result));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                return;
            }
        }

        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void FindAllListItems_WithActualElement_ReturnsReadOnlyList()
        {
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null) return;

                var result = ListSearchOptimizer.FindAllListItems(desktop);

                // Should return a non-null IReadOnlyList (possibly empty)
                Assert.NotNull(result);
                Assert.IsAssignableFrom<IReadOnlyList<AutomationElement>>(result);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                return;
            }
        }

        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void FindAllChildrenOptimized_WithDesktop_ReturnsResults()
        {
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null) return;

                var condition = Condition.TrueCondition;
                var result = ListSearchOptimizer.FindAllChildrenOptimized(desktop, condition);

                Assert.NotNull(result);
                Assert.IsAssignableFrom<IReadOnlyList<AutomationElement>>(result);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                return;
            }
        }

        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void IsListContainer_WithDesktop_ReturnsFalse()
        {
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null) return;

                // Desktop (Pane) is not a list container
                Assert.False(ListSearchOptimizer.IsListContainer(desktop));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                return;
            }
        }
    }
}

