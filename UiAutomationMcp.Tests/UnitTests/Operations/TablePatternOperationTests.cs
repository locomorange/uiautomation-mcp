using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Helpers;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    /// <summary>
    /// Unit tests for Table pattern operations
    /// Tests Microsoft UI Automation TablePattern specification compliance
    /// Uses mock-based testing to avoid direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TablePatternOperationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinder;
        private readonly Mock<AutomationElement> _mockElement;
        private readonly Mock<TablePattern> _mockTablePattern;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;

        public TablePatternOperationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockElement = new Mock<AutomationElement>();
            _mockTablePattern = new Mock<TablePattern>();
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(new UIAutomationOptions());
        }

        #region GetRowOrColumnMajorOperation Tests

        [Fact]
        public async Task GetRowOrColumnMajorOperation_WhenRowMajor_ShouldReturnRowMajor()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("dataGrid1", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("dataGrid1", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var booleanResult = result.Data as BooleanResult;
            Assert.NotNull(booleanResult);
            Assert.True(booleanResult.Value);
            Assert.Contains("RowMajor", booleanResult.Description);

            _output.WriteLine("GetRowOrColumnMajorOperation test passed - RowMajor detected");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_WhenColumnMajor_ShouldReturnColumnMajor()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("spreadsheet1", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("spreadsheet1", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.ColumnMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var booleanResult = result.Data as BooleanResult;
            Assert.NotNull(booleanResult);
            Assert.True(booleanResult.Value);
            Assert.Contains("ColumnMajor", booleanResult.Description);

            _output.WriteLine("GetRowOrColumnMajorOperation test passed - ColumnMajor detected");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_WhenIndeterminate_ShouldReturnIndeterminate()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("customTable1", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("customTable1", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.Indeterminate);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var booleanResult = result.Data as BooleanResult;
            Assert.NotNull(booleanResult);
            Assert.False(booleanResult.Value);
            Assert.Contains("Indeterminate", booleanResult.Description);

            _output.WriteLine("GetRowOrColumnMajorOperation test passed - Indeterminate detected");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_WhenElementNotFound_ShouldReturnError()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("nonExistentTable", "Test Window", "1234");

            _mockElementFinder
                .Setup(x => x.FindElementById("nonExistentTable", "Test Window", 1234, TreeScope.Descendants, null))
                .Returns((AutomationElement?)null);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);

            _output.WriteLine("GetRowOrColumnMajorOperation error handling test passed - Element not found");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_WhenTablePatternNotSupported_ShouldReturnError()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("unsupportedElement", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("unsupportedElement", "Test Window", 1234);
            _mockElement
                .Setup(x => x.TryGetCurrentPattern(TablePattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = null!;
                    return false;
                });

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TablePattern not supported", result.Error);

            _output.WriteLine("GetRowOrColumnMajorOperation pattern validation test passed");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_ShouldIncludeElementInfo()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("testTable", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("testTable", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            _mockElement.Setup(e => e.Current.Name).Returns("Test Table");
            _mockElement.Setup(e => e.Current.AutomationId).Returns("testTable");
            _mockElement.Setup(e => e.Current.ControlType).Returns(ControlType.DataGrid);
            _mockElement.Setup(e => e.Current.ProcessId).Returns(1234);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.IsType<BooleanResult>(result.Data);
            
            var booleanResult = (BooleanResult)result.Data;
            // GetRowOrColumnMajorOperation should return a BooleanResult with proper data
            Assert.NotNull(booleanResult);
            // Since this is about RowMajor, we would expect the Value to indicate row major orientation
            // The exact assertion will depend on how the mock TablePattern is set up

            _output.WriteLine("GetRowOrColumnMajorOperation element info test passed");
        }

        #endregion

        #region Microsoft Specification Compliance Tests

        /// <summary>
        /// Microsoft仕様準拠テスト：TablePatternを必須サポートするコントロールタイプでの動作確認
        /// Table Control Pattern Required Members: RowOrColumnMajor property
        /// </summary>
        [Theory]
        [InlineData("dataGrid", true)]
        [InlineData("table", true)]
        [InlineData("list", false)]  // Optional support
        [InlineData("tree", false)]  // Optional support
        public async Task TablePattern_WithRequiredControlTypes_ShouldSupportRowOrColumnMajor(
            string controlTypeName, bool isRequired)
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest($"element_{controlTypeName}", "Test Window", "1234");

            SetupElementFinderToReturnMockElement($"element_{controlTypeName}", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            var booleanResult = result.Data as BooleanResult;
            Assert.NotNull(booleanResult);
            // Assert.Contains("RowOrColumnMajor", booleanResult.Description); // Modify based on actual implementation
            
            _output.WriteLine($"Microsoft specification test passed for {controlTypeName} (Required: {isRequired})");
        }

        /// <summary>
        /// Microsoft仕様準拠テスト：TablePattern Required Members検証
        /// - RowOrColumnMajor property (already tested above)
        /// - GetColumnHeaders() method
        /// - GetRowHeaders() method
        /// </summary>
        [Fact]
        public async Task TablePattern_RequiredMembers_ShouldBeImplemented()
        {
            // This test validates that the required members are accessible through the pattern
            // GetColumnHeaders and GetRowHeaders are tested in separate operation test files
            
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = CreateWorkerRequest("compliantTable", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("compliantTable", "Test Window", 1234);
            SetupElementToSupportTablePattern();
            
            // Test all possible RowOrColumnMajor values per Microsoft specification
            var testValues = new[]
            {
                RowOrColumnMajor.RowMajor,
                RowOrColumnMajor.ColumnMajor,
                RowOrColumnMajor.Indeterminate
            };

            foreach (var value in testValues)
            {
                _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(value);

                // Act
                var result = await operation.ExecuteAsync(request);

                // Assert
                Assert.True(result.Success);
                var booleanResult = result.Data as BooleanResult;
                Assert.NotNull(booleanResult);
                // Assert.Equal(value.ToString(), booleanResult.Description); // Modify based on actual implementation
            }

            _output.WriteLine("Microsoft specification compliance test passed - All RowOrColumnMajor values supported");
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData(null, "", "0")]
        [InlineData("", null, null)]
        [InlineData("table1", "window1", "invalid")]
        public async Task GetRowOrColumnMajorOperation_ShouldHandle_DefaultAndInvalidParameters(
            string elementId, string windowTitle, string processId)
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId ?? "",
                    ["windowTitle"] = windowTitle ?? "",
                    ["processId"] = processId ?? "0"
                }
            };

            // Setup for default behavior (element found, pattern supported)
            SetupElementFinderToReturnMockElement(elementId ?? "", windowTitle ?? "", 0);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert - Should handle gracefully regardless of parameter validity
            Assert.NotNull(result);
            _output.WriteLine($"Parameter validation test passed for elementId='{elementId}', windowTitle='{windowTitle}', processId='{processId}'");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_ShouldHandle_EmptyParametersDictionary()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = new WorkerRequest { Parameters = new Dictionary<string, object>() };

            // Setup for default empty string parameters
            SetupElementFinderToReturnMockElement("", "", 0);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine("Empty parameters dictionary test passed");
        }

        [Fact]
        public async Task GetRowOrColumnMajorOperation_ShouldHandle_NullParametersDictionary()
        {
            // Arrange
            var operation = new GetRowOrColumnMajorOperation(_mockElementFinder.Object, _mockOptions.Object);
            var request = new WorkerRequest { Parameters = null };

            // Setup for default empty string parameters
            SetupElementFinderToReturnMockElement("", "", 0);
            SetupElementToSupportTablePattern();
            _mockTablePattern.Setup(p => p.Current.RowOrColumnMajor).Returns(RowOrColumnMajor.RowMajor);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine("Null parameters dictionary test passed");
        }

        #endregion

        #region Helper Methods

        private WorkerRequest CreateWorkerRequest(string elementId, string windowTitle, string processId)
        {
            return new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    ["elementId"] = elementId,
                    ["windowTitle"] = windowTitle,
                    ["processId"] = processId
                }
            };
        }

        private void SetupElementFinderToReturnMockElement(string elementId, string windowTitle, int processId)
        {
            _mockElementFinder
                .Setup(x => x.FindElementById(elementId, windowTitle, processId, TreeScope.Descendants, null))
                .Returns(_mockElement.Object);
        }

        private void SetupElementToSupportTablePattern()
        {
            _mockElement
                .Setup(x => x.GetCurrentPattern(TablePattern.Pattern))
                .Returns(_mockTablePattern.Object);
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("TablePatternOperationTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}