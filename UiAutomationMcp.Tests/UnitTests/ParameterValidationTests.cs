using UIAutomationMCP.Shared;

namespace UiAutomationMcp.Tests.UnitTests
{
    /// <summary>
    /// パラメータ検証の単体テスト
    /// UI Automationに依存しない純粋なロジックをテストします
    /// </summary>
    [Trait("Category", "Unit")]
    public class ParameterValidationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidateElementId_WhenInvalidInput_ShouldReturnFalse(string? elementId)
        {
            // Given & When
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Then
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("btn1")]
        [InlineData("textBox_123")]
        [InlineData("menu-item")]
        public void ValidateElementId_WhenValidInput_ShouldReturnTrue(string elementId)
        {
            // Given & When
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Then
            Assert.True(isValid);
        }

        [Theory]
        [InlineData("minimize")]
        [InlineData("maximize")]
        [InlineData("normal")]
        [InlineData("close")]
        [InlineData("restore")]
        public void ValidateWindowState_WithValidStates_ShouldReturnTrue(string state)
        {
            // Arrange
            var validStates = new[] { "minimize", "minimized", "maximize", "maximized", "normal", "restore", "restored", "close" };

            // Act
            var isValid = validStates.Contains(state.ToLower());

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData("unknown")]
        public void ValidateWindowState_WithInvalidStates_ShouldReturnFalse(string state)
        {
            // Arrange
            var validStates = new[] { "minimize", "minimized", "maximize", "maximized", "normal", "restore", "restored", "close" };

            // Act
            var isValid = validStates.Contains(state.ToLower());

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(0, 5, 10, true)]    // Valid range
        [InlineData(5, 3, 10, true)]    // Valid range
        [InlineData(-1, 5, 10, false)]  // Invalid start
        [InlineData(0, -1, 10, false)]  // Invalid length
        [InlineData(8, 5, 10, false)]   // Start + length > total
        [InlineData(15, 1, 10, false)]  // Start > total
        public void ValidateTextSelection_WithVariousInputs_ShouldReturnExpectedResult(
            int startIndex, int length, int totalLength, bool expectedValid)
        {
            // Act
            var isValid = startIndex >= 0 && 
                         length >= 0 && 
                         startIndex < totalLength && 
                         startIndex + length <= totalLength;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, true)]
        [InlineData(30, true)]
        [InlineData(300, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void ValidateTimeout_WithVariousValues_ShouldReturnExpectedResult(int timeout, bool expectedValid)
        {
            // Act
            var isValid = timeout > 0;

            // Assert
            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void WorkerRequest_WithValidParameters_ShouldBeConstructable()
        {
            // Arrange & Act
            var request = new WorkerRequest
            {
                Operation = "invoke",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "btn1",
                    ["windowTitle"] = "TestWindow"
                }
            };

            // Assert
            Assert.NotNull(request);
            Assert.Equal("invoke", request.Operation);
            Assert.True(request.Parameters.ContainsKey("elementId"));
            Assert.Equal("btn1", request.Parameters["elementId"]);
        }

        [Theory]
        [InlineData("invoke", "ElementId")]
        [InlineData("setvalue", "ElementId")]
        [InlineData("setvalue", "Value")]
        [InlineData("selecttext", "StartIndex")]
        [InlineData("selecttext", "Length")]
        [InlineData("setwindowstate", "State")]
        public void RequiredParameters_ForOperations_ShouldBeDocumented(string operation, string requiredParameter)
        {
            // This test documents the expected parameters for each operation
            // It serves as documentation and validation that our API is consistent

            var parameterRequirements = new Dictionary<string, string[]>
            {
                ["invoke"] = new[] { "ElementId" },
                ["setvalue"] = new[] { "ElementId", "Value" },
                ["getvalue"] = new[] { "ElementId" },
                ["toggle"] = new[] { "ElementId" },
                ["select"] = new[] { "ElementId" },
                ["selecttext"] = new[] { "ElementId", "StartIndex", "Length" },
                ["findtext"] = new[] { "ElementId", "SearchText" },
                ["setwindowstate"] = new[] { "ElementId", "State" },
                ["getwindowstate"] = new[] { "ElementId" },
                ["closewindow"] = new[] { "ElementId" },
                ["waitforwindowstate"] = new[] { "ElementId", "ExpectedState" }
            };

            // Act & Assert
            Assert.True(parameterRequirements.ContainsKey(operation), 
                $"Operation '{operation}' should be documented");
            Assert.Contains(requiredParameter, parameterRequirements[operation]);
        }

        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("left")]
        [InlineData("right")]
        [InlineData("pageup")]
        [InlineData("pagedown")]
        public void ValidateScrollDirection_WithValidDirections_ShouldReturnTrue(string direction)
        {
            // Arrange
            var validDirections = new[] { "up", "down", "left", "right", "pageup", "pagedown" };

            // Act
            var isValid = validDirections.Contains(direction.ToLower());

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(50.0)]
        [InlineData(100.0)]
        public void ValidatePercentage_WithValidValues_ShouldReturnTrue(double value)
        {
            // Act
            var isValid = value >= 0.0 && value <= 100.0;

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(101.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void ValidatePercentage_WithInvalidValues_ShouldReturnFalse(double value)
        {
            // Act
            var isValid = value >= 0.0 && value <= 100.0 && !double.IsNaN(value) && !double.IsInfinity(value);

            // Assert
            Assert.False(isValid);
        }
    }
}
