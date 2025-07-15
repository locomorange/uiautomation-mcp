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
    /// Tests for GetRowHeaderItemsOperation - Microsoft TableItemPattern.GetRowHeaderItems()仕様準拠
    /// 安全性ポリシー準拠: Mock使用でUIAutomation直接実行を回避
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GetRowHeaderItemsOperationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinderService;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;
        private readonly GetRowHeaderItemsOperation _operation;

        public GetRowHeaderItemsOperationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinderService = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(new UIAutomationOptions());
            _operation = new GetRowHeaderItemsOperation(_mockElementFinderService.Object, _mockOptions.Object);
        }

        public void Dispose()
        {
            // Mockのクリーンアップ
        }

        /// <summary>
        /// GetRowHeaderItems - 正常系：Microsoft TableItemPattern仕様準拠テスト
        /// Required Members: GetRowHeaderItems() - テーブル項目の行ヘッダー要素を取得
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ValidTableItem_ReturnsRowHeaderItems()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetRowHeaderItems()をテスト
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // 行ヘッダー要素のモック作成
            var mockHeaderElement1 = new Mock<AutomationElement>();
            var mockHeaderElement2 = new Mock<AutomationElement>();
            
            // 行ヘッダー要素のプロパティ設定
            mockHeaderElement1.Setup(e => e.Current.AutomationId).Returns("header_row1");
            mockHeaderElement1.Setup(e => e.Current.Name).Returns("Person 1");
            mockHeaderElement1.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement1.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement1.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(5, 30, 80, 20));
            
            mockHeaderElement2.Setup(e => e.Current.AutomationId).Returns("header_row2");
            mockHeaderElement2.Setup(e => e.Current.Name).Returns("Person 2");
            mockHeaderElement2.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement2.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement2.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(5, 50, 80, 20));

            var rowHeaders = new[] { mockHeaderElement1.Object, mockHeaderElement2.Object };
            
            // TableItemPatternのGetRowHeaderItems()メソッドをモック
            mockTableItemPattern.Setup(p => p.Current.GetRowHeaderItems()).Returns(rowHeaders);
            
            // AutomationElementのGetCurrentPatternをモック
            mockElement.Setup(e => e.GetCurrentPattern(TableItemPattern.Pattern))
                      .Returns(mockTableItemPattern.Object);

            _mockElementFinderService.Setup(s => s.FindElementById("tableCell2", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "tableCell2" },
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
            
            // 最初の行ヘッダーの検証
            Assert.Equal("header_row1", searchResult.Elements[0].AutomationId);
            Assert.Equal("Person 1", searchResult.Elements[0].Name);
            Assert.Equal("Header", searchResult.Elements[0].ControlType);
            Assert.True(searchResult.Elements[0].IsEnabled);
            
            // 2番目の行ヘッダーの検証
            Assert.Equal("header_row2", searchResult.Elements[1].AutomationId);
            Assert.Equal("Person 2", searchResult.Elements[1].Name);
            Assert.Equal("Header", searchResult.Elements[1].ControlType);
            Assert.True(searchResult.Elements[1].IsEnabled);

            _output.WriteLine("GetRowHeaderItemsOperation test passed - TableItemPattern.GetRowHeaderItems() verified");
        }

        /// <summary>
        /// GetRowHeaderItems - エラーハンドリング：要素が見つからない場合
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
            _output.WriteLine("GetRowHeaderItemsOperation element not found test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - エラーハンドリング：TableItemPatternがサポートされていない場合
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
            _output.WriteLine("GetRowHeaderItemsOperation pattern not supported test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - エラーハンドリング：行ヘッダーが見つからない場合
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_NoRowHeadersFound_ReturnsError()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // 空の行ヘッダー配列を返すモック
            mockTableItemPattern.Setup(p => p.Current.GetRowHeaderItems()).Returns(new AutomationElement[0]);
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("emptyRowTableCell", "TestWindow", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "emptyRowTableCell" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No row header items found", result.Error);
            _output.WriteLine("GetRowHeaderItemsOperation no headers found test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - エラーハンドリング：例外処理
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ExceptionThrown_ReturnsError()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            
            // GetRowHeaderItems()で例外をスローするモック
            mockTableItemPattern.Setup(p => p.Current.GetRowHeaderItems())
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
            Assert.Contains("Error getting row header items", result.Error);
            Assert.Contains("UI Automation error", result.Error);
            _output.WriteLine("GetRowHeaderItemsOperation exception handling test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - パラメータ処理：プロセスID指定テスト
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_WithProcessId_ParsesCorrectly()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            var mockHeaderElement = new Mock<AutomationElement>();
            
            mockHeaderElement.Setup(e => e.Current.AutomationId).Returns("test_row_header");
            mockHeaderElement.Setup(e => e.Current.Name).Returns("Test Row Header");
            mockHeaderElement.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement.Setup(e => e.Current.IsEnabled).Returns(true);
            mockHeaderElement.Setup(e => e.Current.BoundingRectangle).Returns(new System.Windows.Rect(0, 0, 60, 25));

            var rowHeaders = new[] { mockHeaderElement.Object };
            mockTableItemPattern.Setup(p => p.Current.GetRowHeaderItems()).Returns(rowHeaders);
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("cell2", "", 5678, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "cell2" },
                    { "windowTitle", "" },
                    { "processId", "5678" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            _mockElementFinderService.Verify(s => s.FindElementById("cell2", "", 5678, TreeScope.Descendants, null), Times.Once);
            _output.WriteLine("GetRowHeaderItemsOperation processId parameter test passed");
        }

        /// <summary>
        /// GetRowHeaderItems - BoundingRectangle情報の正確性テスト
        /// Microsoft仕様: BoundingRectangleプロパティの検証
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ValidTableItem_ReturnsBoundingRectangleInfo()
        {
            // Arrange
            var mockElement = new Mock<AutomationElement>();
            var mockTableItemPattern = new Mock<TableItemPattern>();
            var mockHeaderElement = new Mock<AutomationElement>();
            
            var expectedRect = new System.Windows.Rect(15, 35, 120, 30);
            mockHeaderElement.Setup(e => e.Current.AutomationId).Returns("precise_header");
            mockHeaderElement.Setup(e => e.Current.Name).Returns("Precise Header");
            mockHeaderElement.Setup(e => e.Current.ControlType).Returns(ControlType.Header);
            mockHeaderElement.Setup(e => e.Current.IsEnabled).Returns(false); // 無効状態もテスト
            mockHeaderElement.Setup(e => e.Current.BoundingRectangle).Returns(expectedRect);

            var rowHeaders = new[] { mockHeaderElement.Object };
            mockTableItemPattern.Setup(p => p.Current.GetRowHeaderItems()).Returns(rowHeaders);
            
            mockElement.Setup(e => e.TryGetCurrentPattern(TableItemPattern.Pattern, out It.Ref<object>.IsAny))
                      .Returns((AutomationPattern pattern, out object patternObject) =>
                      {
                          patternObject = mockTableItemPattern.Object;
                          return true;
                      });

            _mockElementFinderService.Setup(s => s.FindElementById("preciseCell", "TestApp", 0, TreeScope.Descendants, null))
                                   .Returns(mockElement.Object);

            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "preciseCell" },
                    { "windowTitle", "TestApp" },
                    { "processId", "0" }
                }
            };

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.IsType<ElementSearchResult>(result.Data);
            var searchResult = (ElementSearchResult)result.Data;
            Assert.NotNull(searchResult.Elements);
            Assert.Single(searchResult.Elements);
            
            var element = searchResult.Elements[0];
            Assert.NotNull(element.BoundingRectangle);
            Assert.Equal(15, element.BoundingRectangle.X);
            Assert.Equal(35, element.BoundingRectangle.Y);
            Assert.Equal(120, element.BoundingRectangle.Width);
            Assert.Equal(30, element.BoundingRectangle.Height);
            
            Assert.False(element.IsEnabled); // 無効状態の確認
            _output.WriteLine("GetRowHeaderItemsOperation BoundingRectangle accuracy test passed");
        }
    }
}