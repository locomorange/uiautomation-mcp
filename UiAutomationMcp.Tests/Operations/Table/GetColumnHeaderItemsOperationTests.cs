using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Operations.Table
{
    /// <summary>
    /// Tests for GetColumnHeaderItemsOperation - Microsoft TableItemPattern.GetColumnHeaderItems()仕様準拠
    /// 安全性ポリシー準拠: Mock使用でUIAutomation直接実行を回避
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GetColumnHeaderItemsOperationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinderService;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;
        private readonly GetColumnHeaderItemsOperation _operation;

        public GetColumnHeaderItemsOperationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinderService = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(new UIAutomationOptions());
            _operation = new GetColumnHeaderItemsOperation(_mockElementFinderService.Object, _mockOptions.Object);
        }

        public void Dispose()
        {
            // Mockのクリーンアップ
        }

        /// <summary>
        /// GetColumnHeaderItems - 正常系：Microsoft TableItemPattern仕様準拠テスト
        /// Required Members: GetColumnHeaderItems() - テーブル項目の列ヘッダー要素を取得
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ValidTableItem_ReturnsColumnHeaderItems()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetColumnHeaderItems()をテスト
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // 列ヘッダー要素のモック作成
            var mockHeaderElement1 = new Mock<AutomationElement>();
            var mockHeaderElement2 = new Mock<AutomationElement>();
            
            // 列ヘッダー要素のプロパティ設定
            mockHeaderElement1.Setup(e => e.Current.AutomationId).Returns("header_col1");
            mockHeaderElement1.Setup(e => e.Current.Name).Returns("Name");
            mockHeaderElement1.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement1.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement1.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(10, 5, 100, 25));
            
            mockHeaderElement2.Setup(e => e.Current.AutomationId).Returns("header_col2");
            mockHeaderElement2.Setup(e => e.Current.Name).Returns("Age");
            mockHeaderElement2.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement2.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement2.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(110, 5, 80, 25));

            var columnHeaders = new[] { mockHeaderElement1.Object, mockHeaderElement2.Object };
            
            // TableItemPatternのGetColumnHeaderItems()メソッドをモック
            mockTableItemPattern.Setup(p => p.Current.GetColumnHeaderItems()).Returns(columnHeaders);
            
            // AutomationElementのGetCurrentPatternをモック
            mockElement.Setup(e => e.GetCurrentPattern(TableItemPattern.Pattern))
                      .Returns(mockTableItemPattern.Object);

            _mockElementFinderService.Setup(s => s.FindElementById("tableCell1", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "tableCell1" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            Assert.IsType<ElementSearchResult>(result.Data);
            var searchResult = (ElementSearchResult)result.Data;
            Assert.NotNull(searchResult.Elements);
            Assert.Equal(2, searchResult.Elements.Count);
            
            // 最初の列ヘッダーの検証
            Assert.Equal("header_col1", searchResult.Elements[0].AutomationId);
            Assert.Equal("Name", searchResult.Elements[0].Name);
            Assert.Equal("Header", searchResult.Elements[0].ControlType);
            Assert.True(searchResult.Elements[0].IsEnabled);
            
            // 2番目の列ヘッダーの検証
            Assert.Equal("header_col2", searchResult.Elements[1].AutomationId);
            Assert.Equal("Age", searchResult.Elements[1].Name);
            Assert.Equal("Header", searchResult.Elements[1].ControlType);
            Assert.True(searchResult.Elements[1].IsEnabled);

            _output.WriteLine("GetColumnHeaderItemsOperation test passed - TableItemPattern.GetColumnHeaderItems() verified");
        }

        /// <summary>
        /// GetColumnHeaderItems - エラーハンドリング：要素が見つからない場合
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ElementNotFound_ReturnsError()
        {
            // Arrange
            _mockElementFinderService.Setup(s => s.FindElementById("nonExistentElement", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns((AutomationElement?)null);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "nonExistentElement" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Element not found", result.Error);
            _output.WriteLine("GetColumnHeaderItemsOperation element not found test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - エラーハンドリング：TableItemPatternがサポートされていない場合
        /// Microsoft仕様: InvalidOperationException when pattern not supported
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_TableItemPatternNotSupported_ReturnsError()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            
            // TableItemPatternがサポートされていない場合のモック
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = null!;
                          return false;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("nonTableCell", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "nonTableCell" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TableItemPattern not supported", result.Error);
            _output.WriteLine("GetColumnHeaderItemsOperation pattern not supported test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - エラーハンドリング：列ヘッダーが見つからない場合
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_NoColumnHeadersFound_ReturnsError()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // 空の列ヘッダー配列を返すモック
            mockTableItemPattern.Setup(p => p.Current.GetColumnHeaderItems()).Returns(new AutomationElement[0]);
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("emptyTableCell", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "emptyTableCell" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No column header items found", result.Error);
            _output.WriteLine("GetColumnHeaderItemsOperation no headers found test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - エラーハンドリング：例外処理
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ExceptionThrown_ReturnsError()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // GetColumnHeaderItems()で例外をスローするモック
            mockTableItemPattern.Setup(p => p.Current.GetColumnHeaderItems())
                               .Throws(new InvalidOperationException("UI Automation error"));
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("errorCell", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "errorCell" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error getting column header items", result.Error);
            Assert.Contains("UI Automation error", result.Error);
            _output.WriteLine("GetColumnHeaderItemsOperation exception handling test passed");
        }

        /// <summary>
        /// GetColumnHeaderItems - パラメータ処理：プロセスID指定テスト
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_WithProcessId_ParsesCorrectly()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            var mockHeaderElement = new Mock<AutomationElement>();
            
            mockHeaderElement.Setup(e => e.Current.AutomationId).Returns("test_header");
            mockHeaderElement.Setup(e => e.Current.Name).Returns("Test Header");
            mockHeaderElement.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(0, 0, 50, 20));

            var columnHeaders = new[] { mockHeaderElement.Object };
            mockTableItemPattern.Setup(p => p.Current.GetColumnHeaderItems()).Returns(columnHeaders);
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("cell1", "", 1234, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "cell1" },
                    { "windowTitle", "" },
                    { "processId", "1234" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            _mockElementFinderService.Verify(s => s.FindElementById("cell1", "", 1234, TreeScope.Descendants, null), Times.Once);
            _output.WriteLine("GetColumnHeaderItemsOperation processId parameter test passed");
        }
    }
}