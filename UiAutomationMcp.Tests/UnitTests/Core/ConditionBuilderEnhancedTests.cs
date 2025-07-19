using System;
using System.Windows.Automation;
using UIAutomationMCP.Worker.Core;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Core
{
    public class ConditionBuilderEnhancedTests
    {
        [Fact]
        public void IsVisible_ReturnsCorrectPropertyCondition()
        {
            // Act
            var condition = ConditionBuilder.IsVisible();

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.IsOffscreenProperty, propCondition.Property);
            Assert.Equal(false, propCondition.Value);
        }

        [Fact]
        public void IsKeyboardFocusable_ReturnsCorrectPropertyCondition()
        {
            // Act
            var condition = ConditionBuilder.IsKeyboardFocusable();

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.IsKeyboardFocusableProperty, propCondition.Property);
            Assert.Equal(true, propCondition.Value);
        }

        [Fact]
        public void IsControlElement_ReturnsCorrectPropertyCondition()
        {
            // Act
            var condition = ConditionBuilder.IsControlElement();

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.IsControlElementProperty, propCondition.Property);
            Assert.Equal(true, propCondition.Value);
        }

        [Fact]
        public void IsContentElement_ReturnsCorrectPropertyCondition()
        {
            // Act
            var condition = ConditionBuilder.IsContentElement();

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.IsContentElementProperty, propCondition.Property);
            Assert.Equal(true, propCondition.Value);
        }

        [Theory]
        [InlineData("WPF")]
        [InlineData("Win32")]
        [InlineData("CustomFramework")]
        public void ByFrameworkId_ReturnsCorrectPropertyCondition(string frameworkId)
        {
            // Act
            var condition = ConditionBuilder.ByFrameworkId(frameworkId);

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.FrameworkIdProperty, propCondition.Property);
            Assert.Equal(frameworkId, propCondition.Value);
        }

        [Fact]
        public void Win32Elements_ReturnsOrConditionWithCorrectFrameworks()
        {
            // Act
            var condition = ConditionBuilder.Win32Elements();

            // Assert
            Assert.IsType<OrCondition>(condition);
            var orCondition = (OrCondition)condition;
            var conditions = orCondition.GetConditions();
            
            Assert.Equal(3, conditions.Length);
            
            // Verify each condition is a PropertyCondition for FrameworkId
            foreach (var cond in conditions)
            {
                Assert.IsType<PropertyCondition>(cond);
                var propCond = (PropertyCondition)cond;
                Assert.Equal(AutomationElement.FrameworkIdProperty, propCond.Property);
            }
        }

        [Fact]
        public void WpfElements_ReturnsOrConditionWithCorrectFrameworks()
        {
            // Act
            var condition = ConditionBuilder.WpfElements();

            // Assert
            Assert.IsType<OrCondition>(condition);
            var orCondition = (OrCondition)condition;
            var conditions = orCondition.GetConditions();
            
            Assert.Equal(3, conditions.Length);
            
            // Verify each condition is a PropertyCondition for FrameworkId
            foreach (var cond in conditions)
            {
                Assert.IsType<PropertyCondition>(cond);
                var propCond = (PropertyCondition)cond;
                Assert.Equal(AutomationElement.FrameworkIdProperty, propCond.Property);
            }
        }

        [Fact]
        public void EnabledAndVisible_ReturnsAndConditionWithCorrectProperties()
        {
            // Act
            var condition = ConditionBuilder.EnabledAndVisible();

            // Assert
            Assert.IsType<AndCondition>(condition);
            var andCondition = (AndCondition)condition;
            var conditions = andCondition.GetConditions();
            
            Assert.Equal(2, conditions.Length);
            
            // Verify conditions are PropertyConditions for IsEnabled and IsOffscreen
            var properties = new[] { AutomationElement.IsEnabledProperty, AutomationElement.IsOffscreenProperty };
            var conditionProperties = new AutomationProperty[conditions.Length];
            
            for (int i = 0; i < conditions.Length; i++)
            {
                Assert.IsType<PropertyCondition>(conditions[i]);
                var propCond = (PropertyCondition)conditions[i];
                conditionProperties[i] = propCond.Property;
            }
            
            Assert.Contains(AutomationElement.IsEnabledProperty, conditionProperties);
            Assert.Contains(AutomationElement.IsOffscreenProperty, conditionProperties);
        }

        [Fact]
        public void FocusableAndEnabled_ReturnsAndConditionWithCorrectProperties()
        {
            // Act
            var condition = ConditionBuilder.FocusableAndEnabled();

            // Assert
            Assert.IsType<AndCondition>(condition);
            var andCondition = (AndCondition)condition;
            var conditions = andCondition.GetConditions();
            
            Assert.Equal(2, conditions.Length);
            
            // Verify conditions contain both IsKeyboardFocusable and IsEnabled
            var conditionProperties = new AutomationProperty[conditions.Length];
            
            for (int i = 0; i < conditions.Length; i++)
            {
                Assert.IsType<PropertyCondition>(conditions[i]);
                var propCond = (PropertyCondition)conditions[i];
                conditionProperties[i] = propCond.Property;
            }
            
            Assert.Contains(AutomationElement.IsKeyboardFocusableProperty, conditionProperties);
            Assert.Contains(AutomationElement.IsEnabledProperty, conditionProperties);
        }

        [Fact]
        public void Interactable_ReturnsAndConditionWithThreeProperties()
        {
            // Act
            var condition = ConditionBuilder.Interactable();

            // Assert
            Assert.IsType<AndCondition>(condition);
            var andCondition = (AndCondition)condition;
            var conditions = andCondition.GetConditions();
            
            Assert.Equal(3, conditions.Length);
            
            // Verify conditions contain IsEnabled, IsOffscreen, and IsKeyboardFocusable
            var conditionProperties = new AutomationProperty[conditions.Length];
            
            for (int i = 0; i < conditions.Length; i++)
            {
                Assert.IsType<PropertyCondition>(conditions[i]);
                var propCond = (PropertyCondition)conditions[i];
                conditionProperties[i] = propCond.Property;
            }
            
            Assert.Contains(AutomationElement.IsEnabledProperty, conditionProperties);
            Assert.Contains(AutomationElement.IsOffscreenProperty, conditionProperties);
            Assert.Contains(AutomationElement.IsKeyboardFocusableProperty, conditionProperties);
        }

        [Fact]
        public void CombinedConditions_CanBeNestedCorrectly()
        {
            // Act
            var condition = ConditionBuilder.And(
                ConditionBuilder.ByName("TestElement"),
                ConditionBuilder.EnabledAndVisible(),
                ConditionBuilder.Or(
                    ConditionBuilder.Buttons(),
                    ConditionBuilder.TextBoxes()
                )
            );

            // Assert
            Assert.IsType<AndCondition>(condition);
            var andCondition = (AndCondition)condition;
            var conditions = andCondition.GetConditions();
            
            Assert.Equal(3, conditions.Length);
            
            // First condition should be a PropertyCondition for Name
            Assert.IsType<PropertyCondition>(conditions[0]);
            
            // Second condition should be an AndCondition (EnabledAndVisible)
            Assert.IsType<AndCondition>(conditions[1]);
            
            // Third condition should be an OrCondition (Buttons OR TextBoxes)
            Assert.IsType<OrCondition>(conditions[2]);
        }

        [Fact]
        public void FrameworkSpecificConditions_WorkWithControlTypes()
        {
            // Act
            var wpfButtons = ConditionBuilder.And(
                ConditionBuilder.WpfElements(),
                ConditionBuilder.Buttons()
            );
            
            var win32TextBoxes = ConditionBuilder.And(
                ConditionBuilder.Win32Elements(),
                ConditionBuilder.TextBoxes()
            );

            // Assert
            Assert.IsType<AndCondition>(wpfButtons);
            Assert.IsType<AndCondition>(win32TextBoxes);
            
            var wpfConditions = ((AndCondition)wpfButtons).GetConditions();
            var win32Conditions = ((AndCondition)win32TextBoxes).GetConditions();
            
            Assert.Equal(2, wpfConditions.Length);
            Assert.Equal(2, win32Conditions.Length);
        }

        [Fact]
        public void VisibilityConditions_HandleEdgeCases()
        {
            // Act
            var visibleAndInteractable = ConditionBuilder.And(
                ConditionBuilder.IsVisible(),
                ConditionBuilder.Interactable()
            );

            // Assert
            Assert.IsType<AndCondition>(visibleAndInteractable);
            
            // This should result in a flattened condition that doesn't duplicate IsOffscreen checks
            var conditions = ((AndCondition)visibleAndInteractable).GetConditions();
            Assert.Equal(2, conditions.Length);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ByFrameworkId_WithInvalidInput_StillCreatesCondition(string frameworkId)
        {
            // Act
            var condition = ConditionBuilder.ByFrameworkId(frameworkId);

            // Assert
            Assert.IsType<PropertyCondition>(condition);
            var propCondition = (PropertyCondition)condition;
            Assert.Equal(AutomationElement.FrameworkIdProperty, propCondition.Property);
            Assert.Equal(frameworkId, propCondition.Value);
        }

        [Fact]
        public void ComplexNestedConditions_PerformanceTest()
        {
            // Arrange & Act
            var startTime = DateTime.UtcNow;
            
            var complexCondition = ConditionBuilder.Or(
                ConditionBuilder.And(
                    ConditionBuilder.WpfElements(),
                    ConditionBuilder.Interactable(),
                    ConditionBuilder.ByControlType(ControlType.Button)
                ),
                ConditionBuilder.And(
                    ConditionBuilder.Win32Elements(),
                    ConditionBuilder.EnabledAndVisible(),
                    ConditionBuilder.Or(
                        ConditionBuilder.TextBoxes(),
                        ConditionBuilder.MenuItems()
                    )
                )
            );
            
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Assert
            Assert.IsType<OrCondition>(complexCondition);
            Assert.True(duration.TotalMilliseconds < 100, "Complex condition creation should be fast");
        }
    }
}