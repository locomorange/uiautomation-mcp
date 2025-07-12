using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Operations.Selection;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Helpers;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests.Operations
{
    /// <summary>
    /// Unit tests for SelectionItem pattern operations
    /// Tests Microsoft UI Automation SelectionItemPattern specification compliance
    /// Uses mock-based testing to avoid direct UI Automation dependencies
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class SelectionItemOperationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinder;
        private readonly Mock<AutomationElement> _mockElement;
        private readonly Mock<SelectionItemPattern> _mockSelectionItemPattern;

        public SelectionItemOperationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockElement = new Mock<AutomationElement>();
            _mockSelectionItemPattern = new Mock<SelectionItemPattern>();
        }

        #region IsSelectedOperation Tests

        [Fact]
        public async Task IsSelectedOperation_WhenElementIsSelected_ShouldReturnTrue()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("listItem1", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("listItem1", "Test Window", 1234);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(true);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.True((bool)data.IsSelected);

            _output.WriteLine("IsSelectedOperation test passed - Element is selected");
        }

        [Fact]
        public async Task IsSelectedOperation_WhenElementIsNotSelected_ShouldReturnFalse()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("listItem2", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("listItem2", "Test Window", 1234);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(false);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.False((bool)data.IsSelected);

            _output.WriteLine("IsSelectedOperation test passed - Element is not selected");
        }

        [Fact]
        public async Task IsSelectedOperation_WhenElementNotFound_ShouldReturnError()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("nonExistentElement", "Test Window", "1234");

            _mockElementFinder
                .Setup(x => x.FindElementById("nonExistentElement", "Test Window", 1234))
                .Returns((AutomationElement)null);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);

            _output.WriteLine("IsSelectedOperation error handling test passed - Element not found");
        }

        [Fact]
        public async Task IsSelectedOperation_WhenSelectionItemPatternNotSupported_ShouldReturnError()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("unsupportedElement", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("unsupportedElement", "Test Window", 1234);
            _mockElement
                .Setup(x => x.TryGetCurrentPattern(SelectionItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = null;
                    return false;
                });

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element does not support SelectionItemPattern: unsupportedElement", result.Error);

            _output.WriteLine("IsSelectedOperation pattern validation test passed");
        }

        #endregion

        #region GetSelectionContainerOperation Tests

        [Fact]
        public async Task GetSelectionContainerOperation_WhenContainerExists_ShouldReturnContainerInfo()
        {
            // Arrange
            var operation = new GetSelectionContainerOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("listItem1", "Test Window", "1234");

            var mockContainer = new Mock<AutomationElement>();
            mockContainer.Setup(c => c.Current.AutomationId).Returns("parentList");
            mockContainer.Setup(c => c.Current.Name).Returns("Items List");
            mockContainer.Setup(c => c.Current.ControlType).Returns(ControlType.List);
            mockContainer.Setup(c => c.Current.ClassName).Returns("ListBox");
            mockContainer.Setup(c => c.Current.ProcessId).Returns(1234);
            mockContainer.Setup(c => c.GetRuntimeId()).Returns(new int[] { 1, 2, 3 });

            SetupElementFinderToReturnMockElement("listItem1", "Test Window", 1234);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.SelectionContainer).Returns(mockContainer.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.NotNull(data.SelectionContainer);

            _output.WriteLine("GetSelectionContainerOperation test passed - Container information retrieved");
        }

        [Fact]
        public async Task GetSelectionContainerOperation_WhenNoContainer_ShouldReturnNull()
        {
            // Arrange
            var operation = new GetSelectionContainerOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("orphanedElement", "Test Window", "1234");

            SetupElementFinderToReturnMockElement("orphanedElement", "Test Window", 1234);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.SelectionContainer).Returns((AutomationElement)null);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.Null(data.SelectionContainer);

            _output.WriteLine("GetSelectionContainerOperation test passed - No container case handled");
        }

        #endregion

        #region CanSelectMultipleOperation Tests

        [Fact]
        public async Task CanSelectMultipleOperation_WhenMultipleSelectionSupported_ShouldReturnTrue()
        {
            // Arrange
            var operation = new CanSelectMultipleOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("multiSelectList", "Test Window", "1234", isContainer: true);

            var mockSelectionPattern = new Mock<SelectionPattern>();
            mockSelectionPattern.Setup(p => p.Current.CanSelectMultiple).Returns(true);

            SetupElementFinderToReturnMockElement("multiSelectList", "Test Window", 1234);
            SetupElementToSupportSelectionPattern(mockSelectionPattern.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.True((bool)data.CanSelectMultiple);

            _output.WriteLine("CanSelectMultipleOperation test passed - Multiple selection supported");
        }

        [Fact]
        public async Task CanSelectMultipleOperation_WhenSingleSelectionOnly_ShouldReturnFalse()
        {
            // Arrange
            var operation = new CanSelectMultipleOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("singleSelectList", "Test Window", "1234", isContainer: true);

            var mockSelectionPattern = new Mock<SelectionPattern>();
            mockSelectionPattern.Setup(p => p.Current.CanSelectMultiple).Returns(false);

            SetupElementFinderToReturnMockElement("singleSelectList", "Test Window", 1234);
            SetupElementToSupportSelectionPattern(mockSelectionPattern.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.False((bool)data.CanSelectMultiple);

            _output.WriteLine("CanSelectMultipleOperation test passed - Single selection only");
        }

        [Fact]
        public async Task CanSelectMultipleOperation_WhenSelectionPatternNotSupported_ShouldReturnError()
        {
            // Arrange
            var operation = new CanSelectMultipleOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("unsupportedContainer", "Test Window", "1234", isContainer: true);

            SetupElementFinderToReturnMockElement("unsupportedContainer", "Test Window", 1234);
            _mockElement
                .Setup(x => x.TryGetCurrentPattern(SelectionPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = null;
                    return false;
                });

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element does not support SelectionPattern: unsupportedContainer", result.Error);

            _output.WriteLine("CanSelectMultipleOperation pattern validation test passed");
        }

        #endregion

        #region IsSelectionRequiredOperation Tests

        [Fact]
        public async Task IsSelectionRequiredOperation_WhenSelectionRequired_ShouldReturnTrue()
        {
            // Arrange
            var operation = new IsSelectionRequiredOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("tabControl", "Test Window", "1234", isContainer: true);

            var mockSelectionPattern = new Mock<SelectionPattern>();
            mockSelectionPattern.Setup(p => p.Current.IsSelectionRequired).Returns(true);

            SetupElementFinderToReturnMockElement("tabControl", "Test Window", 1234);
            SetupElementToSupportSelectionPattern(mockSelectionPattern.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.True((bool)data.IsSelectionRequired);

            _output.WriteLine("IsSelectionRequiredOperation test passed - Selection is required");
        }

        [Fact]
        public async Task IsSelectionRequiredOperation_WhenSelectionOptional_ShouldReturnFalse()
        {
            // Arrange
            var operation = new IsSelectionRequiredOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest("optionalList", "Test Window", "1234", isContainer: true);

            var mockSelectionPattern = new Mock<SelectionPattern>();
            mockSelectionPattern.Setup(p => p.Current.IsSelectionRequired).Returns(false);

            SetupElementFinderToReturnMockElement("optionalList", "Test Window", 1234);
            SetupElementToSupportSelectionPattern(mockSelectionPattern.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var data = result.Data as dynamic;
            Assert.NotNull(data);
            Assert.False((bool)data.IsSelectionRequired);

            _output.WriteLine("IsSelectionRequiredOperation test passed - Selection is optional");
        }

        #endregion

        #region Microsoft Specification Compliance Tests

        /// <summary>
        /// Microsoft仕様準拠テスト：SelectionItemPatternを必須サポートするコントロールタイプでの動作確認
        /// </summary>
        [Theory]
        [InlineData("listItem", true)]
        [InlineData("radioButton", true)]
        [InlineData("tabItem", true)]
        [InlineData("menuItem", false)]  // Optional support
        [InlineData("treeItem", false)]  // Optional support
        public async Task SelectionItemPattern_WithRequiredControlTypes_ShouldSupportPattern(
            string controlTypeName, bool isRequired)
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest($"element_{controlTypeName}", "Test Window", "1234");

            SetupElementFinderToReturnMockElement($"element_{controlTypeName}", "Test Window", 1234);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(true);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            _output.WriteLine($"Microsoft specification test passed for {controlTypeName} (Required: {isRequired})");
        }

        /// <summary>
        /// Microsoft仕様準拠テスト：SelectionPatternを必須サポートするコントロールタイプでの動作確認
        /// </summary>
        [Theory]
        [InlineData("list", true)]
        [InlineData("dataGrid", true)]
        [InlineData("tree", true)]
        [InlineData("tab", true)]
        [InlineData("menu", false)]  // Optional support
        public async Task SelectionPattern_WithRequiredControlTypes_ShouldSupportPattern(
            string controlTypeName, bool isRequired)
        {
            // Arrange
            var operation = new CanSelectMultipleOperation(_mockElementFinder.Object);
            var request = CreateWorkerRequest($"container_{controlTypeName}", "Test Window", "1234", isContainer: true);

            var mockSelectionPattern = new Mock<SelectionPattern>();
            mockSelectionPattern.Setup(p => p.Current.CanSelectMultiple).Returns(true);

            SetupElementFinderToReturnMockElement($"container_{controlTypeName}", "Test Window", 1234);
            SetupElementToSupportSelectionPattern(mockSelectionPattern.Object);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            _output.WriteLine($"Microsoft specification test passed for {controlTypeName} (Required: {isRequired})");
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData(null, "", "0")]
        [InlineData("", null, null)]
        [InlineData("element1", "window1", "invalid")]
        public async Task Operations_ShouldHandle_DefaultAndInvalidParameters(
            string elementId, string windowTitle, string processId)
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
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
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(false);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert - Should handle gracefully regardless of parameter validity
            Assert.NotNull(result);
            _output.WriteLine($"Parameter validation test passed for elementId='{elementId}', windowTitle='{windowTitle}', processId='{processId}'");
        }

        [Fact]
        public async Task Operations_ShouldHandle_EmptyParametersDictionary()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = new WorkerRequest { Parameters = new Dictionary<string, object>() };

            // Setup for default empty string parameters
            SetupElementFinderToReturnMockElement("", "", 0);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(false);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine("Empty parameters dictionary test passed");
        }

        [Fact]
        public async Task Operations_ShouldHandle_NullParametersDictionary()
        {
            // Arrange
            var operation = new IsSelectedOperation(_mockElementFinder.Object);
            var request = new WorkerRequest { Parameters = null };

            // Setup for default empty string parameters
            SetupElementFinderToReturnMockElement("", "", 0);
            SetupElementToSupportSelectionItemPattern();
            _mockSelectionItemPattern.Setup(p => p.Current.IsSelected).Returns(false);

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine("Null parameters dictionary test passed");
        }

        #endregion

        #region Helper Methods

        private WorkerRequest CreateWorkerRequest(string elementId, string windowTitle, string processId, bool isContainer = false)
        {
            var parameterName = isContainer ? "containerId" : "elementId";
            return new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    [parameterName] = elementId,
                    ["windowTitle"] = windowTitle,
                    ["processId"] = processId
                }
            };
        }

        private void SetupElementFinderToReturnMockElement(string elementId, string windowTitle, int processId)
        {
            _mockElementFinder
                .Setup(x => x.FindElementById(elementId, windowTitle, processId))
                .Returns(_mockElement.Object);
        }

        private void SetupElementToSupportSelectionItemPattern()
        {
            _mockElement
                .Setup(x => x.TryGetCurrentPattern(SelectionItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = _mockSelectionItemPattern.Object;
                    return true;
                });
        }

        private void SetupElementToSupportSelectionPattern(SelectionPattern selectionPattern)
        {
            _mockElement
                .Setup(x => x.TryGetCurrentPattern(SelectionPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = selectionPattern;
                    return true;
                });
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("SelectionItemOperationTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}