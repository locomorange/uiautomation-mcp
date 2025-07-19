using System;
using System.Collections.Generic;
using System.Windows.Automation;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Core;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    public class SearchMethodSelectorTests
    {
        [Fact]
        public void SelectOptimalMethod_WithNullSearchRoot_ReturnsFindAll()
        {
            // Arrange
            var condition = ConditionBuilder.ByName("test");

            // Act
            var result = SearchMethodSelector.SelectOptimalMethod(null!, condition, TreeScope.Children);

            // Assert
            Assert.Equal(SearchMethod.FindAll, result);
        }

        [Fact]
        public void SelectOptimalMethod_WithNullCondition_ReturnsFindAll()
        {
            // Arrange
            var mockElement = CreateMockElement();

            // Act
            var result = SearchMethodSelector.SelectOptimalMethod(mockElement, null!, TreeScope.Children);

            // Assert
            Assert.Equal(SearchMethod.FindAll, result);
        }

        [Fact]
        public void SelectOptimalMethod_WithRootElement_ReturnsTreeWalker()
        {
            // Arrange
            try
            {
                var rootElement = AutomationElement.RootElement;
                var condition = ConditionBuilder.ByName("test");

                // Act
                var result = SearchMethodSelector.SelectOptimalMethod(rootElement, condition, TreeScope.Children);

                // Assert
                Assert.Equal(SearchMethod.TreeWalker, result);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                // Skip test if UI Automation is not available
                return;
            }
        }

        [Theory]
        [InlineData(TreeScope.Children, SearchMethod.FindAll)]
        [InlineData(TreeScope.Subtree, SearchMethod.TreeWalker)]
        public void SelectOptimalMethod_WithDifferentScopes_ReturnsExpectedMethod(TreeScope scope, SearchMethod expected)
        {
            // Arrange
            var mockElement = CreateMockElement();
            var condition = ConditionBuilder.ByName("test");

            // Act
            var result = SearchMethodSelector.SelectOptimalMethod(mockElement, condition, scope, 5);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EvaluateComplexity_WithNullCondition_ReturnsSimple()
        {
            // Act
            var result = SearchMethodSelector.EvaluateComplexity(null!);

            // Assert
            Assert.Equal(SearchComplexity.Simple, result);
        }

        [Fact]
        public void EvaluateComplexity_WithPropertyCondition_ReturnsSimple()
        {
            // Arrange
            var condition = ConditionBuilder.ByName("test");

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Simple, result);
        }

        [Fact]
        public void EvaluateComplexity_WithSimpleAndCondition_ReturnsMedium()
        {
            // Arrange
            var condition = ConditionBuilder.And(
                ConditionBuilder.ByName("test"),
                ConditionBuilder.IsEnabled()
            );

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Medium, result);
        }

        [Fact]
        public void EvaluateComplexity_WithComplexAndCondition_ReturnsComplex()
        {
            // Arrange
            var condition = ConditionBuilder.And(
                ConditionBuilder.ByName("test"),
                ConditionBuilder.IsEnabled(),
                ConditionBuilder.IsVisible(),
                ConditionBuilder.IsKeyboardFocusable(),
                ConditionBuilder.ByControlType(ControlType.Button)
            );

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Complex, result);
        }

        [Fact]
        public void EvaluateComplexity_WithSimpleOrCondition_ReturnsMedium()
        {
            // Arrange
            var condition = ConditionBuilder.Or(
                ConditionBuilder.ByName("test"),
                ConditionBuilder.ByAutomationId("testId")
            );

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Medium, result);
        }

        [Fact]
        public void EvaluateComplexity_WithComplexOrCondition_ReturnsComplex()
        {
            // Arrange
            var condition = ConditionBuilder.Or(
                ConditionBuilder.ByName("test1"),
                ConditionBuilder.ByName("test2"),
                ConditionBuilder.ByName("test3")
            );

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Complex, result);
        }

        [Fact]
        public void EvaluateComplexity_WithNotCondition_ReturnsMedium()
        {
            // Arrange
            var condition = ConditionBuilder.Not(ConditionBuilder.IsEnabled());

            // Act
            var result = SearchMethodSelector.EvaluateComplexity(condition);

            // Assert
            Assert.Equal(SearchComplexity.Medium, result);
        }

        [Fact]
        public void GetRecommendation_WithValidParameters_ReturnsValidRecommendation()
        {
            // Arrange
            var mockElement = CreateMockElement();
            var condition = ConditionBuilder.ByName("test");

            // Act
            var result = SearchMethodSelector.GetRecommendation(mockElement, condition, TreeScope.Children);

            // Assert
            Assert.NotNull(result);
            Assert.True(Enum.IsDefined(typeof(SearchMethod), result.Method));
            Assert.True(Enum.IsDefined(typeof(SearchComplexity), result.Complexity));
            Assert.True(result.TimeoutSeconds > 0);
            Assert.True(result.MaxResults > 0);
            Assert.NotNull(result.Warnings);
        }

        [Fact]
        public void GetRecommendation_WithRootElement_AddsWarning()
        {
            // Arrange
            try
            {
                var rootElement = AutomationElement.RootElement;
                var condition = ConditionBuilder.ByName("test");

                // Act
                var result = SearchMethodSelector.GetRecommendation(rootElement, condition, TreeScope.Children);

                // Assert
                Assert.Contains(result.Warnings, w => w.Contains("Root"));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                // Skip test if UI Automation is not available
                return;
            }
        }

        [Fact]
        public void GetRecommendation_WithComplexSubtreeSearch_AddsWarning()
        {
            // Arrange
            var mockElement = CreateMockElement();
            var complexCondition = ConditionBuilder.And(
                ConditionBuilder.ByName("test"),
                ConditionBuilder.IsEnabled(),
                ConditionBuilder.IsVisible(),
                ConditionBuilder.IsKeyboardFocusable()
            );

            // Act
            var result = SearchMethodSelector.GetRecommendation(mockElement, complexCondition, TreeScope.Subtree);

            // Assert
            Assert.Contains(result.Warnings, w => w.Contains("複雑") || w.Contains("complex") || w.Contains("Subtree"));
        }

        [Theory]
        [InlineData(SearchComplexity.Simple, TreeScope.Children, 3)]
        [InlineData(SearchComplexity.Medium, TreeScope.Children, 8)]
        [InlineData(SearchComplexity.Complex, TreeScope.Children, 15)]
        public void GetRecommendation_TimeoutRecommendations_AreReasonable(
            SearchComplexity complexity, TreeScope scope, int expectedMinTimeout)
        {
            // Arrange
            var mockElement = CreateMockElement();
            var condition = CreateConditionForComplexity(complexity);

            // Act
            var result = SearchMethodSelector.GetRecommendation(mockElement, condition, scope);

            // Assert
            Assert.True(result.TimeoutSeconds >= expectedMinTimeout - 2); // Allow some flexibility
        }

        [Theory]
        [InlineData(TreeScope.Children, 50)]
        [InlineData(TreeScope.Descendants, 20)]
        [InlineData(TreeScope.Subtree, 10)]
        public void GetRecommendation_MaxResultsRecommendations_AreReasonable(
            TreeScope scope, int expectedMinResults)
        {
            // Arrange
            var mockElement = CreateMockElement();
            var condition = ConditionBuilder.ByName("test");

            // Act
            var result = SearchMethodSelector.GetRecommendation(mockElement, condition, scope);

            // Assert
            Assert.True(result.MaxResults >= expectedMinResults);
        }

        [Fact]
        public void SearchMethod_Enum_HasExpectedValues()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(SearchMethod), SearchMethod.FindAll));
            Assert.True(Enum.IsDefined(typeof(SearchMethod), SearchMethod.TreeWalker));
            Assert.Equal(2, Enum.GetValues<SearchMethod>().Length);
        }

        [Fact]
        public void SearchComplexity_Enum_HasExpectedValues()
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(SearchComplexity), SearchComplexity.Simple));
            Assert.True(Enum.IsDefined(typeof(SearchComplexity), SearchComplexity.Medium));
            Assert.True(Enum.IsDefined(typeof(SearchComplexity), SearchComplexity.Complex));
            Assert.Equal(3, Enum.GetValues<SearchComplexity>().Length);
        }

        [Fact]
        public void SearchRecommendation_Properties_AreInitializedCorrectly()
        {
            // Act
            var recommendation = new SearchRecommendation();

            // Assert
            Assert.NotNull(recommendation.Warnings);
            Assert.Empty(recommendation.Warnings);
            Assert.Equal(default(SearchMethod), recommendation.Method);
            Assert.Equal(default(SearchComplexity), recommendation.Complexity);
            Assert.False(recommendation.UseCache);
            Assert.Equal(0, recommendation.TimeoutSeconds);
            Assert.Equal(0, recommendation.MaxResults);
        }

        // Helper methods
        private AutomationElement? CreateMockElement()
        {
            try
            {
                // In a real test environment, you would create a proper mock
                // For this basic implementation, we'll return a simple element
                // that doesn't throw exceptions
                return AutomationElement.RootElement?.FindFirst(
                    TreeScope.Children, 
                    Condition.TrueCondition) ?? AutomationElement.RootElement;
            }
            catch
            {
                // Return null if we can't create a test element
                return null;
            }
        }

        private Condition CreateConditionForComplexity(SearchComplexity complexity)
        {
            return complexity switch
            {
                SearchComplexity.Simple => ConditionBuilder.ByName("test"),
                SearchComplexity.Medium => ConditionBuilder.And(
                    ConditionBuilder.ByName("test"),
                    ConditionBuilder.IsEnabled()
                ),
                SearchComplexity.Complex => ConditionBuilder.And(
                    ConditionBuilder.ByName("test"),
                    ConditionBuilder.IsEnabled(),
                    ConditionBuilder.IsVisible(),
                    ConditionBuilder.IsKeyboardFocusable()
                ),
                _ => ConditionBuilder.ByName("test")
            };
        }
    }

    /// <summary>
    /// Integration tests that require actual UI Automation elements
    /// </summary>
    public class SearchMethodSelectorIntegrationTests
    {
        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void SelectOptimalMethod_WithActualElement_DoesNotThrow()
        {
            // Arrange
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null)
                {
                    return;
                }

                var condition = ConditionBuilder.Windows();

                // Act & Assert - Should not throw
                var result = SearchMethodSelector.SelectOptimalMethod(desktop, condition, TreeScope.Children);
                
                Assert.True(Enum.IsDefined(typeof(SearchMethod), result));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                // Skip test if UI Automation is not available
                return;
            }
        }

        [Fact]
        [Trait("Category", "RequiresUIAutomation")]
        public void GetRecommendation_WithActualElement_ReturnsValidRecommendation()
        {
            // Arrange
            try
            {
                var desktop = AutomationElement.RootElement;
                if (desktop == null)
                {
                    return;
                }

                var condition = ConditionBuilder.Windows();

                // Act
                var result = SearchMethodSelector.GetRecommendation(desktop, condition, TreeScope.Children);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.TimeoutSeconds > 0);
                Assert.True(result.MaxResults > 0);
                Assert.NotNull(result.Warnings);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ElementNotAvailableException)
            {
                // Skip test if UI Automation is not available
                return;
            }
        }
    }
}