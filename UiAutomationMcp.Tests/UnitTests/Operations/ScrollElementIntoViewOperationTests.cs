using Microsoft.Extensions.Logging;
using Moq;
using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Operations.Layout;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.UnitTests.Operations
{
    /// <summary>
    /// ScrollElementIntoViewOperation単体テスト - Microsoft ScrollItemPattern仕様準拠テスト
    /// Mock使用により安全にUIAutomation依存を回避
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ScrollElementIntoViewOperationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinder;
        private readonly ScrollElementIntoViewOperation _operation;

        public ScrollElementIntoViewOperationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _operation = new ScrollElementIntoViewOperation(_mockElementFinder.Object);
        }

        #region Microsoft ScrollItemPattern Specification Tests

        /// <summary>
        /// ExecuteAsync - 正常系：ScrollItemPattern.ScrollIntoView()が正常に実行されることをテスト
        /// Microsoft仕様: ScrollItemPattern.ScrollIntoView() method implementation
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Successfully_Scroll_Element_With_ScrollItemPattern()
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "scrollable-element" },
                    { "windowTitle", "Test Window" },
                    { "processId", "1234" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById("scrollable-element", "Test Window", 1234))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = mockScrollItemPattern.Object;
                    return true;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Element scrolled into view successfully", result.Data);

            // Verify ScrollIntoView was called
            mockScrollItemPattern.Verify(x => x.ScrollIntoView(), Times.Once);
            
            _output.WriteLine($"ScrollElementIntoView executed successfully: {result.Data}");
        }

        /// <summary>
        /// ExecuteAsync - 要素が見つからない場合のエラーハンドリング
        /// Microsoft仕様: Element not found handling
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Return_Error_When_Element_Not_Found()
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "non-existent-element" },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            _mockElementFinder
                .Setup(x => x.FindElementById("non-existent-element", "Test Window", 0))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element 'non-existent-element' not found", result.Error);
            
            _output.WriteLine($"Expected error handled correctly: {result.Error}");
        }

        /// <summary>
        /// ExecuteAsync - ScrollItemPatternが利用できない場合のエラーハンドリング
        /// Microsoft仕様: InvalidOperationException when pattern not supported
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Return_Error_When_ScrollItemPattern_Not_Supported()
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "non-scrollable-element" },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();

            _mockElementFinder
                .Setup(x => x.FindElementById("non-scrollable-element", "Test Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = null;
                    return false;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element does not support ScrollItemPattern", result.Error);
            
            _output.WriteLine($"ScrollItemPattern not supported error handled correctly: {result.Error}");
        }

        #endregion

        #region Parameter Validation Tests

        /// <summary>
        /// ExecuteAsync - パラメータ解析：正しいパラメータが適切に解析されることをテスト
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Parse_Parameters_Correctly()
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "test-element" },
                    { "windowTitle", "My Application" },
                    { "processId", "9876" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById("test-element", "My Application", 9876))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = mockScrollItemPattern.Object;
                    return true;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            
            // Verify correct parameters were passed to ElementFinder
            _mockElementFinder.Verify(x => x.FindElementById("test-element", "My Application", 9876), Times.Once);
        }

        /// <summary>
        /// ExecuteAsync - デフォルトパラメータ処理：空文字列やnullパラメータの適切な処理
        /// </summary>
        [Theory]
        [InlineData(null, "", "0", "", "", 0)]
        [InlineData("", null, null, "", "", 0)]
        [InlineData("element1", "window1", "invalid", "element1", "window1", 0)]
        public async Task ExecuteAsync_Should_Handle_Default_And_Invalid_Parameters(
            string elementId, string windowTitle, string processId,
            string expectedElementId, string expectedWindowTitle, int expectedProcessId)
        {
            // Arrange
            var parameters = new Dictionary<string, object>();
            
            if (elementId != null) parameters["elementId"] = elementId;
            if (windowTitle != null) parameters["windowTitle"] = windowTitle;
            if (processId != null) parameters["processId"] = processId;

            var request = new WorkerRequest { Parameters = parameters };

            _mockElementFinder
                .Setup(x => x.FindElementById(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success); // Element not found due to mock setup
            
            // Verify correct parameter defaults were applied
            _mockElementFinder.Verify(x => x.FindElementById(expectedElementId, expectedWindowTitle, expectedProcessId), Times.Once);
        }

        /// <summary>
        /// ExecuteAsync - 空のパラメータディクショナリの処理
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Handle_Empty_Parameters()
        {
            // Arrange
            var request = new WorkerRequest { Parameters = new Dictionary<string, object>() };

            _mockElementFinder
                .Setup(x => x.FindElementById("", "", 0))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
            
            // Verify default parameters were used
            _mockElementFinder.Verify(x => x.FindElementById("", "", 0), Times.Once);
        }

        /// <summary>
        /// ExecuteAsync - nullパラメータディクショナリの処理
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Handle_Null_Parameters()
        {
            // Arrange
            var request = new WorkerRequest { Parameters = null };

            _mockElementFinder
                .Setup(x => x.FindElementById("", "", 0))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
        }

        #endregion

        #region Edge Cases and Error Handling

        /// <summary>
        /// ExecuteAsync - ScrollItemPattern型チェック：間違った型が返された場合の処理
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_Should_Handle_Invalid_Pattern_Type()
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "element-with-wrong-pattern" },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var wrongPatternType = new Mock<InvokePattern>(); // 間違ったパターン型

            _mockElementFinder
                .Setup(x => x.FindElementById("element-with-wrong-pattern", "Test Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = wrongPatternType.Object; // 間違った型を返す
                    return true;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element does not support ScrollItemPattern", result.Error);
        }

        /// <summary>
        /// Microsoft仕様準拠テスト：ListItemとTreeItemコントロールタイプでの動作確認
        /// </summary>
        [Theory]
        [InlineData("list-item-element")]
        [InlineData("tree-item-element")]
        public async Task ExecuteAsync_Should_Work_With_ListItem_And_TreeItem_Controls(string elementId)
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", "Control Test Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById(elementId, "Control Test Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = mockScrollItemPattern.Object;
                    return true;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            mockScrollItemPattern.Verify(x => x.ScrollIntoView(), Times.Once);
            
            _output.WriteLine($"ScrollElementIntoView worked correctly for {elementId}");
        }

        #endregion

        public void Dispose()
        {
            // テストクリーンアップ
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}