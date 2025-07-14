using UIAutomationMCP.Shared;
using System.Text.Json;

namespace UiAutomationMcp.Tests.Models
{
    /// <summary>
    /// SharedModelsのユニットテスト
    /// シリアライゼーション、デシリアライゼーション、バリデーションをテスト
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class SharedModelsTests
    {
        [Fact]
        public void ElementInfo_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var original = new ElementInfo
            {
                Name = "Test Button",
                AutomationId = "btn_test",
                ControlType = "Button",
                ClassName = "Button",
                ProcessId = 1234,
                BoundingRectangle = new BoundingRectangle { X = 10, Y = 20, Width = 100, Height = 30 },
                IsEnabled = true,
                IsVisible = true,
                HelpText = "Click to test"
            };

            // Act
            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<ElementInfo>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.AutomationId, deserialized.AutomationId);
            Assert.Equal(original.ControlType, deserialized.ControlType);
            Assert.Equal(original.ClassName, deserialized.ClassName);
            Assert.Equal(original.ProcessId, deserialized.ProcessId);
            Assert.Equal(original.IsEnabled, deserialized.IsEnabled);
            Assert.Equal(original.IsVisible, deserialized.IsVisible);
            Assert.Equal(original.HelpText, deserialized.HelpText);
            
            Assert.Equal(original.BoundingRectangle.X, deserialized.BoundingRectangle.X);
            Assert.Equal(original.BoundingRectangle.Y, deserialized.BoundingRectangle.Y);
            Assert.Equal(original.BoundingRectangle.Width, deserialized.BoundingRectangle.Width);
            Assert.Equal(original.BoundingRectangle.Height, deserialized.BoundingRectangle.Height);
        }

        [Fact]
        public void BoundingRectangle_ShouldHandleDefaultValues()
        {
            // Arrange & Act
            var rect = new BoundingRectangle();

            // Assert
            Assert.Equal(0, rect.X);
            Assert.Equal(0, rect.Y);
            Assert.Equal(0, rect.Width);
            Assert.Equal(0, rect.Height);
        }

        [Fact]
        public void WindowInfo_ShouldSerializeCorrectly()
        {
            // Arrange
            var original = new WindowInfo
            {
                Title = "Test Window",
                ProcessId = 1234,
                ProcessName = "TestApp",
                Handle = new IntPtr(12345),
                ClassName = "TestWindowClass",
                IsVisible = true,
                BoundingRectangle = new BoundingRectangle { X = 0, Y = 0, Width = 800, Height = 600 }
            };

            // Act
            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<WindowInfo>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Title, deserialized.Title);
            Assert.Equal(original.ProcessId, deserialized.ProcessId);
            Assert.Equal(original.ProcessName, deserialized.ProcessName);
            Assert.Equal(original.ClassName, deserialized.ClassName);
            Assert.Equal(original.IsVisible, deserialized.IsVisible);
        }

        [Fact]
        public void ElementSearchParameters_ShouldHandleOptionalFields()
        {
            // Arrange & Act
            var searchParams = new ElementSearchParameters
            {
                WindowTitle = "TestWindow",
                SearchText = "Button"
                // Other fields left as null/default
            };

            // Assert
            Assert.Equal("TestWindow", searchParams.WindowTitle);
            Assert.Equal("Button", searchParams.SearchText);
            Assert.Null(searchParams.AutomationId);
            Assert.Null(searchParams.ControlType);
            Assert.Null(searchParams.ProcessId);
            Assert.Equal(30, searchParams.TimeoutSeconds); // Default value is 30
            Assert.Equal("descendants", searchParams.Scope); // Default value is "descendants"
        }

        [Fact]
        public void AdvancedOperationParameters_ShouldHandleParametersDictionary()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["Value"] = "test value",
                ["Action"] = "click",
                ["Count"] = 5
            };

            var operationParams = new AdvancedOperationParameters
            {
                Operation = "setvalue",
                ElementId = "input1",
                WindowTitle = "TestWindow",
                ProcessId = 1234,
                TimeoutSeconds = 30,
                Parameters = parameters
            };

            // Act
            var json = JsonSerializer.Serialize(operationParams);
            var deserialized = JsonSerializer.Deserialize<AdvancedOperationParameters>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(operationParams.Operation, deserialized.Operation);
            Assert.Equal(operationParams.ElementId, deserialized.ElementId);
            Assert.Equal(operationParams.WindowTitle, deserialized.WindowTitle);
            Assert.Equal(operationParams.ProcessId, deserialized.ProcessId);
            Assert.Equal(operationParams.TimeoutSeconds, deserialized.TimeoutSeconds);
            
            Assert.NotNull(deserialized.Parameters);
            Assert.Equal(3, deserialized.Parameters.Count);
            Assert.True(deserialized.Parameters.ContainsKey("Value"));
            Assert.True(deserialized.Parameters.ContainsKey("Action"));
            Assert.True(deserialized.Parameters.ContainsKey("Count"));
        }

        [Fact]
        public void WorkerOperation_ShouldSerializeCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["ElementId"] = "btn1",
                ["WindowTitle"] = "TestWindow"
            };

            var request = new WorkerRequest
            {
                Operation = "invoke",
                Parameters = parameters
            };

            // Act
            var json = JsonSerializer.Serialize(request, JsonSerializationConfig.Options);
            var deserialized = JsonSerializer.Deserialize<WorkerRequest>(json, JsonSerializationConfig.Options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(request.Operation, deserialized.Operation);
            Assert.NotNull(deserialized.Parameters);
            Assert.Equal(2, deserialized.Parameters.Count);
        }

        [Fact]
        public void WorkerResponse_ShouldHandleSuccessAndFailureStates()
        {
            // Arrange & Act - Success case with generic version
            var successResponse = WorkerResponse<object>.CreateSuccess(new { Message = "Operation completed" });

            // Assert - Success case
            Assert.True(successResponse.Success);
            Assert.NotNull(successResponse.Data);
            Assert.Null(successResponse.Error);

            // Arrange & Act - Failure case
            var failureResponse = WorkerResponse<object>.CreateError("Element not found");

            // Assert - Failure case
            Assert.False(failureResponse.Success);
            Assert.Null(failureResponse.Data);
            Assert.Equal("Element not found", failureResponse.Error);
        }

        [Theory]
        [InlineData("", true)] // Empty string is valid
        [InlineData("test", true)]
        [InlineData(null, true)] // Null is handled by converting to empty string
        public void ElementInfo_ShouldHandleStringFields(string? value, bool isValid)
        {
            // Arrange & Act
            var elementInfo = new ElementInfo
            {
                Name = value ?? string.Empty,
                AutomationId = value ?? string.Empty,
                ControlType = value ?? "Unknown",
                ClassName = value ?? string.Empty,
                HelpText = value ?? string.Empty
            };

            // Assert
            if (isValid)
            {
                Assert.Equal(value ?? string.Empty, elementInfo.Name);
                Assert.Equal(value ?? string.Empty, elementInfo.AutomationId);
                Assert.Equal(value ?? string.Empty, elementInfo.HelpText);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1234)]
        [InlineData(int.MaxValue)]
        public void ElementInfo_ShouldHandleProcessIdValues(int processId)
        {
            // Arrange & Act
            var elementInfo = new ElementInfo
            {
                ProcessId = processId
            };

            // Assert
            Assert.Equal(processId, elementInfo.ProcessId);
        }

        [Fact]
        public void OperationResult_Generic_ShouldWorkWithDifferentTypes()
        {
            // Test with string
            var stringResult = new OperationResult<string>
            {
                Success = true,
                Data = "Test result"
            };
            Assert.Equal("Test result", stringResult.Data);

            // Test with List<ElementInfo>
            var listResult = new OperationResult<List<ElementInfo>>
            {
                Success = true,
                Data = new List<ElementInfo> { new ElementInfo { Name = "Test" } }
            };
            Assert.Single(listResult.Data);

            // Test with Dictionary
            var dictResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = new Dictionary<string, object> { ["key"] = "value" }
            };
            Assert.Contains("key", dictResult.Data);
        }
    }
}
