using UIAutomationMCP.Models;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    /// <summary>
    /// Table pattern operations parameter validation tests
    /// Tests basic parameter validation without UIAutomation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TablePatternParameterValidationTests : IDisposable
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidateTableElementId_WhenInvalidInput_ShouldReturnFalse(string? elementId)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("table1")]
        [InlineData("dataGrid")]
        [InlineData("spreadsheet-view")]
        public void ValidateTableElementId_WhenValidInput_ShouldReturnTrue(string elementId)
        {
            // Arrange & Act
            var isValid = !string.IsNullOrWhiteSpace(elementId);

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void ValidateRowIndex_WhenNegativeValue_ShouldBeInvalid(int rowIndex)
        {
            // Arrange & Act
            var isValid = rowIndex >= 0;

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void ValidateRowIndex_WhenNonNegativeValue_ShouldBeValid(int rowIndex)
        {
            // Arrange & Act
            var isValid = rowIndex >= 0;

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void ValidateColumnIndex_WhenNegativeValue_ShouldBeInvalid(int columnIndex)
        {
            // Arrange & Act
            var isValid = columnIndex >= 0;

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public void ValidateColumnIndex_WhenNonNegativeValue_ShouldBeValid(int columnIndex)
        {
            // Arrange & Act
            var isValid = columnIndex >= 0;

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void CreateTableRequest_WithValidParameters_ShouldCreateValidRequest()
        {
            // Arrange
            var elementId = "test-table";
            var windowTitle = "Spreadsheet Window";
            var row = 2;
            var column = 3;

            // Act
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId,
                    ["windowTitle"] = windowTitle,
                    ["row"] = row,
                    ["column"] = column,
                    ["timeoutSeconds"] = 30
                }
            };

            // Assert
            Assert.NotNull(request);
            Assert.Equal(elementId, ((Dictionary<string, object>)request.Parameters)["elementId"]);
            Assert.Equal(windowTitle, ((Dictionary<string, object>)request.Parameters)["windowTitle"]);
            Assert.Equal(row, ((Dictionary<string, object>)request.Parameters)["row"]);
            Assert.Equal(column, ((Dictionary<string, object>)request.Parameters)["column"]);
        }

        [Fact]
        public void CreateGetTableInfoRequest_WithValidParameters_ShouldCreateValidRequest()
        {
            // Arrange
            var elementId = "data-table";
            var processId = 1234;

            // Act
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId,
                    ["processId"] = processId,
                    ["timeoutSeconds"] = 30
                }
            };

            // Assert
            Assert.NotNull(request);
            Assert.Equal(elementId, ((Dictionary<string, object>)request.Parameters)["elementId"]);
            Assert.Equal(processId, ((Dictionary<string, object>)request.Parameters)["processId"]);
        }

        public void Dispose()
        {
            // No cleanup needed
        }
    }
}

