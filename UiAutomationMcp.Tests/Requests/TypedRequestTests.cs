using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using System.Text.Json;

namespace UiAutomationMcp.Tests.Requests
{
    /// <summary>
    /// 型安全なリクエストシステムのテスト (新しいTools Level Serializationパターン)
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class TypedRequestTests
    {
        [Fact]
        public void InvokeElementRequest_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var typedRequest = new InvokeElementRequest
            {
                AutomationId = "btn_test",
                WindowTitle = "Test Window",
                ProcessId = 1234
            };

            // Act
            var json = JsonSerializationHelper.Serialize(typedRequest);
            var deserialized = JsonSerializationHelper.Deserialize<InvokeElementRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("btn_test", deserialized.AutomationId);
            Assert.Equal("Test Window", deserialized.WindowTitle);
            Assert.Equal(1234, deserialized.ProcessId);
        }

        [Fact]
        public void JsonSerialization_ShouldHandleComplexRequest()
        {
            // Arrange
            var typedRequest = new SearchElementsRequest
            {
                WindowTitle = "Test Window",
                ProcessId = 1234,
                SearchText = "Button",
                AutomationId = "btn_test",
                ControlType = "Button",
                TimeoutSeconds = 30,
                UseRegex = true
            };

            // Act
            var json = JsonSerializationHelper.Serialize(typedRequest);
            var deserialized = JsonSerializationHelper.Deserialize<SearchElementsRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("Test Window", deserialized.WindowTitle);
            Assert.Equal(1234, deserialized.ProcessId);
            Assert.Equal("Button", deserialized.SearchText);
            Assert.Equal("btn_test", deserialized.AutomationId);
            Assert.Equal("Button", deserialized.ControlType);
            Assert.Equal(30, deserialized.TimeoutSeconds);
            Assert.True(deserialized.UseRegex);
        }

        [Fact]
        public void JsonSerialization_ShouldPreserveAllProperties()
        {
            // Arrange - SearchElements request
            var request = new SearchElementsRequest
            {
                WindowTitle = "Test Window",
                SearchText = "Button",
                UseRegex = true,
                TimeoutSeconds = 15,
                ProcessId = 1234,
                AutomationId = "btn_search",
                ControlType = "Button",
                ClassName = "WinButton"
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<SearchElementsRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("Test Window", deserialized.WindowTitle);
            Assert.Equal("Button", deserialized.SearchText);
            Assert.True(deserialized.UseRegex);
            Assert.Equal(15, deserialized.TimeoutSeconds);
            Assert.Equal(1234, deserialized.ProcessId);
            Assert.Equal("btn_search", deserialized.AutomationId);
            Assert.Equal("Button", deserialized.ControlType);
            Assert.Equal("WinButton", deserialized.ClassName);
        }

        [Fact]
        public void SetValueRequest_ShouldSerializeCorrectly()
        {
            // Arrange
            var request = new SetValueRequest
            {
                AutomationId = "input_field",
                Value = "Test Value",
                WindowTitle = "Test Application",
                ProcessId = 5678
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<SetValueRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("input_field", deserialized.AutomationId);
            Assert.Equal("Test Value", deserialized.Value);
            Assert.Equal("Test Application", deserialized.WindowTitle);
            Assert.Equal(5678, deserialized.ProcessId);
        }

        [Fact]
        public void ComplexRequest_ShouldHandleAllParameters()
        {
            // Arrange
            var complexRequest = new SearchElementsRequest
            {
                WindowTitle = "Complex App",
                ProcessId = 9999,
                SearchText = "Submit.*Button",
                AutomationId = "btn_submit",
                ControlType = "Button",
                ClassName = "WinFormButton",
                Scope = "descendants",
                TimeoutSeconds = 45,
                // UseCache property removed from SearchElementsRequest
                UseRegex = true,
                // UseWildcard property removed from SearchElementsRequest
            };

            // Act
            var json = JsonSerializationHelper.Serialize(complexRequest);
            var roundTrip = JsonSerializationHelper.Deserialize<SearchElementsRequest>(json);

            // Assert
            Assert.NotNull(roundTrip);
            Assert.Equal(complexRequest.WindowTitle, roundTrip.WindowTitle);
            Assert.Equal(complexRequest.ProcessId, roundTrip.ProcessId);
            Assert.Equal(complexRequest.SearchText, roundTrip.SearchText);
            Assert.Equal(complexRequest.AutomationId, roundTrip.AutomationId);
            Assert.Equal(complexRequest.ControlType, roundTrip.ControlType);
            Assert.Equal(complexRequest.ClassName, roundTrip.ClassName);
            Assert.Equal(complexRequest.Scope, roundTrip.Scope);
            Assert.Equal(complexRequest.TimeoutSeconds, roundTrip.TimeoutSeconds);
            // Assert.Equal(complexRequest.UseCache, roundTrip.UseCache); // UseCache property removed
            Assert.Equal(complexRequest.UseRegex, roundTrip.UseRegex);
            // Assert.Equal(complexRequest.UseWildcard, roundTrip.UseWildcard); // UseWildcard property removed
        }

        [Fact]
        public void JsonSerialization_ShouldWorkForSetRangeValueRequest()
        {
            // Arrange
            var request = new SetRangeValueRequest
            {
                AutomationId = "slider1",
                Value = 50,
                WindowTitle = "Test Window"
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<SetRangeValueRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(request.AutomationId, deserialized.AutomationId);
            Assert.Equal(request.Value, deserialized.Value);
            Assert.Equal(request.WindowTitle, deserialized.WindowTitle);
        }

        [Fact]
        public void JsonSerialization_ShouldWorkForGetTextRequest()
        {
            // Arrange
            var request = new GetTextRequest
            {
                AutomationId = "text1",
                WindowTitle = "Test Window"
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<GetTextRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(request.AutomationId, deserialized.AutomationId);
            Assert.Equal(request.WindowTitle, deserialized.WindowTitle);
        }

        [Fact]
        public void JsonSerialization_ShouldWorkForMoveElementRequest()
        {
            // Arrange
            var request = new MoveElementRequest
            {
                AutomationId = "element1",
                X = 100,
                Y = 200,
                WindowTitle = "Test Window"
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<MoveElementRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(request.AutomationId, deserialized.AutomationId);
            Assert.Equal(request.X, deserialized.X);
            Assert.Equal(request.Y, deserialized.Y);
            Assert.Equal(request.WindowTitle, deserialized.WindowTitle);
        }

        [Fact]
        public void JsonSerialization_ShouldHandleEmptyValues()
        {
            // Arrange - Request with empty optional fields
            var request = new SetRangeValueRequest
            {
                AutomationId = "slider1",
                Value = 50,
                WindowTitle = "", // Empty string instead of null
                ProcessId = 0
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<SetRangeValueRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("slider1", deserialized.AutomationId);
            Assert.Equal(50, deserialized.Value);
            Assert.Equal("", deserialized.WindowTitle);
            Assert.Equal(0, deserialized.ProcessId);
        }

        [Fact]
        public void JsonSerialization_ShouldHandleInheritance()
        {
            // Arrange
            var request = new SetValueRequest
            {
                AutomationId = "input1",
                WindowTitle = "Test Window",
                ProcessId = 1234,
                Value = "Test Value"
            };

            // Act
            var json = JsonSerializationHelper.Serialize(request);
            var deserialized = JsonSerializationHelper.Deserialize<SetValueRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsAssignableFrom<ElementTargetRequest>(deserialized);
            Assert.Equal("input1", deserialized.AutomationId);
            Assert.Equal("Test Window", deserialized.WindowTitle);
            Assert.Equal(1234, deserialized.ProcessId);
            Assert.Equal("Test Value", deserialized.Value);
        }
    }
}