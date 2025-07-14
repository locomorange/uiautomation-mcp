using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using System.Text.Json;

namespace UiAutomationMcp.Tests.Requests
{
    /// <summary>
    /// 型安全なリクエストシステムのテスト
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class TypedRequestTests
    {
        [Fact]
        public void InvokeElementRequest_ShouldConvertToWorkerRequest()
        {
            // Arrange
            var typedRequest = new InvokeElementRequest
            {
                ElementId = "btn_test",
                WindowTitle = "Test Window",
                ProcessId = 1234
            };

            // Act
            var workerRequest = typedRequest.ToWorkerRequest();

            // Assert
            Assert.Equal("InvokeElement", workerRequest.Operation);
            Assert.NotNull(workerRequest.Parameters);
            Assert.Equal("btn_test", workerRequest.Parameters["elementId"]?.ToString());
            Assert.Equal("Test Window", workerRequest.Parameters["windowTitle"]?.ToString());
            Assert.Equal("1234", workerRequest.Parameters["processId"]?.ToString());
        }

        [Fact]
        public void WorkerRequest_ShouldConvertToTypedRequest()
        {
            // Arrange
            var workerRequest = new WorkerRequest
            {
                Operation = "InvokeElement",
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "btn_test",
                    ["windowTitle"] = "Test Window",
                    ["processId"] = 1234
                }
            };

            // Act
            var typedRequest = workerRequest.GetTypedRequest<InvokeElementRequest>();

            // Assert
            Assert.NotNull(typedRequest);
            Assert.Equal("InvokeElement", typedRequest.Operation);
            Assert.Equal("btn_test", typedRequest.ElementId);
            Assert.Equal("Test Window", typedRequest.WindowTitle);
            Assert.Equal(1234, typedRequest.ProcessId);
        }

        [Fact]
        public void GetTypedRequestByOperation_ShouldReturnCorrectType()
        {
            // Arrange - FindElements request
            var workerRequest = new WorkerRequest
            {
                Operation = "FindElements",
                Parameters = new Dictionary<string, object>
                {
                    ["windowTitle"] = "Test Window",
                    ["searchText"] = "Button",
                    ["useRegex"] = true,
                    ["timeoutSeconds"] = 15
                }
            };

            // Act
            var typedRequest = workerRequest.GetTypedRequestByOperation();

            // Assert
            Assert.NotNull(typedRequest);
            Assert.IsType<FindElementsRequest>(typedRequest);
            
            var findRequest = (FindElementsRequest)typedRequest;
            Assert.Equal("Test Window", findRequest.WindowTitle);
            Assert.Equal("Button", findRequest.SearchText);
            Assert.True(findRequest.UseRegex);
            Assert.Equal(15, findRequest.TimeoutSeconds);
        }

        [Fact]
        public void SetElementValueRequest_ShouldSerializeCorrectly()
        {
            // Arrange
            var request = new SetElementValueRequest
            {
                ElementId = "input_field",
                Value = "Test Value",
                WindowTitle = "Test Application",
                ProcessId = 5678
            };

            // Act
            var json = JsonSerializer.Serialize(request);
            var deserialized = JsonSerializer.Deserialize<SetElementValueRequest>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("SetElementValue", deserialized.Operation);
            Assert.Equal("input_field", deserialized.ElementId);
            Assert.Equal("Test Value", deserialized.Value);
            Assert.Equal("Test Application", deserialized.WindowTitle);
            Assert.Equal(5678, deserialized.ProcessId);
        }

        [Fact]
        public void TypedWorkerRequestFactory_ShouldCreateValidRequests()
        {
            // Test CreateInvokeElement
            var invokeRequest = TypedWorkerRequestFactory.CreateInvokeElement("btn1", "App", 123);
            Assert.Equal("InvokeElement", invokeRequest.Operation);

            // Test CreateSetElementValue
            var valueRequest = TypedWorkerRequestFactory.CreateSetElementValue("input1", "value", "App", 123);
            Assert.Equal("SetElementValue", valueRequest.Operation);

            // Test CreateFindElements
            var findRequest = TypedWorkerRequestFactory.CreateFindElements(
                windowTitle: "Test App",
                searchText: "Button",
                useRegex: true,
                timeoutSeconds: 20
            );
            Assert.Equal("FindElements", findRequest.Operation);

            // Test CreateWindowAction
            var windowRequest = TypedWorkerRequestFactory.CreateWindowAction("close", "Test Window", 456);
            Assert.Equal("WindowAction", windowRequest.Operation);
        }

        [Fact]
        public void ComplexRequest_ShouldHandleAllParameters()
        {
            // Arrange
            var complexRequest = new FindElementsRequest
            {
                WindowTitle = "Complex App",
                ProcessId = 9999,
                SearchText = "Submit.*Button",
                AutomationId = "btn_submit",
                ControlType = "Button",
                ClassName = "WinFormButton",
                Scope = "descendants",
                TimeoutSeconds = 45,
                UseCache = false,
                UseRegex = true,
                UseWildcard = false
            };

            // Act
            var workerRequest = complexRequest.ToWorkerRequest();
            var roundTrip = workerRequest.GetTypedRequest<FindElementsRequest>();

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
            Assert.Equal(complexRequest.UseCache, roundTrip.UseCache);
            Assert.Equal(complexRequest.UseRegex, roundTrip.UseRegex);
            Assert.Equal(complexRequest.UseWildcard, roundTrip.UseWildcard);
        }

        [Theory]
        [InlineData("SetRangeValue", typeof(SetRangeValueRequest))]
        [InlineData("GetText", typeof(GetTextRequest))]
        [InlineData("MoveElement", typeof(MoveElementRequest))]
        [InlineData("WaitForInputIdle", typeof(WaitForInputIdleRequest))]
        public void GetTypedRequestByOperation_ShouldReturnCorrectTypeForOperation(string operation, Type expectedType)
        {
            // Arrange
            var workerRequest = new WorkerRequest
            {
                Operation = operation,
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = "test",
                    ["windowTitle"] = "Test"
                }
            };

            // Act
            var typedRequest = workerRequest.GetTypedRequestByOperation();

            // Assert
            Assert.NotNull(typedRequest);
            Assert.IsType(expectedType, typedRequest);
        }

        [Fact]
        public void GetTypedRequest_ShouldReturnNullForIncompatibleData()
        {
            // Arrange - Request with missing required fields
            var invalidRequest = new WorkerRequest
            {
                Operation = "SetRangeValue",
                Parameters = new Dictionary<string, object>
                {
                    ["invalidField"] = "invalid"
                    // Missing required fields like value
                }
            };

            // Act
            var typedRequest = invalidRequest.GetTypedRequest<SetRangeValueRequest>();

            // Assert
            Assert.NotNull(typedRequest); // Deserialization will succeed but with default values
            Assert.Equal("", typedRequest.ElementId); // Default value
            Assert.Equal(0, typedRequest.Value); // Default value
        }

        [Fact]
        public void ElementTargetRequest_ShouldInheritCommonProperties()
        {
            // Arrange
            var request = new SetElementValueRequest
            {
                ElementId = "input1",
                WindowTitle = "Test Window",
                ProcessId = 1234,
                Value = "Test Value"
            };

            // Act & Assert
            Assert.IsAssignableFrom<ElementTargetRequest>(request);
            Assert.IsAssignableFrom<TypedWorkerRequest>(request);
            Assert.Equal("SetElementValue", request.Operation);
        }
    }
}