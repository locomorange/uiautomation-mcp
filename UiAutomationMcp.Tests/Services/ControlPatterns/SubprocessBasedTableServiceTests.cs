using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Services.ControlPatterns
{
    /// <summary>
    /// Tests for TableService - Microsoft TableItemPattern仕様準拠
    /// 安全性ポリシー準拠: サブプロセス実行とモック使用でUIAutomation直接実行を回避
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TableServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<TableService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly TableService _service;

        public TableServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<TableService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            _service = new TableService(_mockLogger.Object, _mockExecutor.Object);
        }

        public void Dispose()
        {
            // Mockのクリーンアップ
        }

        #region GetColumnHeaderItemsAsync Tests - Microsoft TableItemPattern仕様準拠

        /// <summary>
        /// GetColumnHeaderItemsAsync - 正常系：Microsoft TableItemPattern.GetColumnHeaderItems()仕様準拠テスト
        /// Required Members: GetColumnHeaderItems() - テーブル項目の列ヘッダー要素を取得
        /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_ValidTableCell_ReturnsColumnHeaders()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetColumnHeaderItems()をテスト
            var expectedColumnHeaders = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "header_col1",
                        Name = "Product Name",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 10, Y = 5, Width = 120, Height = 25 }
                    },
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "header_col2",
                        Name = "Price",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 130, Y = 5, Width = 80, Height = 25 }
                    }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(expectedColumnHeaders));

            // Act
            var result = await _service.GetColumnHeaderItemsAsync("tableCell1", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedColumnHeaders, result.Data);

            // サブプロセス実行の検証
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<Dictionary<string, object>>(d => 
                    d["elementId"].ToString() == "tableCell1" &&
                    d["windowTitle"].ToString() == "TestWindow" &&
                    (int)d["processId"] == 0), 30), Times.Once);

            _output.WriteLine("GetColumnHeaderItemsAsync test passed - TableItemPattern.GetColumnHeaderItems() verified");
        }

        /// <summary>
        /// GetColumnHeaderItemsAsync - プロセスID指定とカスタムタイムアウトのテスト
        /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_WithProcessIdAndCustomTimeout_ExecutesCorrectly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Column Header 1" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 60))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.GetColumnHeaderItemsAsync("cell2_3", null, 1234, 60);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedResult, result.Data);

            // パラメータの検証
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<Dictionary<string, object>>(d => 
                    d["elementId"].ToString() == "cell2_3" &&
                    d["windowTitle"].ToString() == "" &&
                    (int)d["processId"] == 1234), 60), Times.Once);

            _output.WriteLine("GetColumnHeaderItemsAsync with ProcessId and custom timeout test passed");
        }

        /// <summary>
        /// GetColumnHeaderItemsAsync - 例外処理テスト
        /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_SubprocessException_ReturnsError()
        {
            // Arrange
            var expectedException = new InvalidOperationException("TableItemPattern not supported");
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .ThrowsAsync(expectedException);

            // Act
            var result = await _service.GetColumnHeaderItemsAsync("nonTableCell", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("TableItemPattern not supported", result.ErrorMessage);

            _output.WriteLine("GetColumnHeaderItemsAsync exception handling test passed");
        }

        #endregion

        #region GetRowHeaderItemsAsync Tests - Microsoft TableItemPattern仕様準拠

        /// <summary>
        /// GetRowHeaderItemsAsync - 正常系：Microsoft TableItemPattern.GetRowHeaderItems()仕様準拠テスト
        /// Required Members: GetRowHeaderItems() - テーブル項目の行ヘッダー要素を取得
        /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_ValidTableCell_ReturnsRowHeaders()
        {
            // Arrange - Microsoft TableItemPattern仕様のGetRowHeaderItems()をテスト
            var expectedRowHeaders = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "header_row1",
                        Name = "Customer 1",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 5, Y = 30, Width = 90, Height = 20 }
                    },
                    new UIAutomationMCP.Shared.ElementInfo
                    {
                        AutomationId = "header_row2",
                        Name = "Customer 2",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle { X = 5, Y = 50, Width = 90, Height = 20 }
                    }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(expectedRowHeaders));

            // Act
            var result = await _service.GetRowHeaderItemsAsync("tableCell2", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRowHeaders, result.Data);

            // サブプロセス実行の検証
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", 
                It.Is<Dictionary<string, object>>(d => 
                    d["elementId"].ToString() == "tableCell2" &&
                    d["windowTitle"].ToString() == "TestWindow" &&
                    (int)d["processId"] == 0), 30), Times.Once);

            _output.WriteLine("GetRowHeaderItemsAsync test passed - TableItemPattern.GetRowHeaderItems() verified");
        }

        /// <summary>
        /// GetRowHeaderItemsAsync - デフォルトパラメータでの動作テスト
        /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_WithDefaultParameters_ExecutesCorrectly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Default Row Header" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.GetRowHeaderItemsAsync("defaultCell");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedResult, result.Data);

            // デフォルトパラメータの検証
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", 
                It.Is<Dictionary<string, object>>(d => 
                    d["elementId"].ToString() == "defaultCell" &&
                    d["windowTitle"].ToString() == "" &&
                    (int)d["processId"] == 0), 30), Times.Once);

            _output.WriteLine("GetRowHeaderItemsAsync with default parameters test passed");
        }

        /// <summary>
        /// GetRowHeaderItemsAsync - 例外処理テスト
        /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_SubprocessException_ReturnsError()
        {
            // Arrange
            var expectedException = new ArgumentException("Element not found: nonExistentCell");
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .ThrowsAsync(expectedException);

            // Act
            var result = await _service.GetRowHeaderItemsAsync("nonExistentCell", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Element not found: nonExistentCell", result.ErrorMessage);

            _output.WriteLine("GetRowHeaderItemsAsync exception handling test passed");
        }

        #endregion

        #region TableItem Pattern Integration Tests

        /// <summary>
        /// TableItem Pattern Integration - 両方のメソッドが協調動作することをテスト
        /// Microsoft仕様: TableItemPatternは通常GridItemPatternと併用される
        /// </summary>
        [Theory]
        [InlineData("cell_1_1", "Cell at row 1, column 1")]
        [InlineData("cell_2_3", "Cell at row 2, column 3")]
        [InlineData("cell_0_0", "Top-left cell")]
        public async Task TableItem_Pattern_Methods_Should_Work_Together(string cellId, string description)
        {
            // Arrange - 異なるタイプのテーブルセルでの動作確認
            var columnHeadersResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = $"Column Header for {cellId}" }
                }
            };
            
            var rowHeadersResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = $"Row Header for {cellId}" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(columnHeadersResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(rowHeadersResult));

            // Act
            var columnResult = await _service.GetColumnHeaderItemsAsync(cellId, "TestApplication", null, 30);
            var rowResult = await _service.GetRowHeaderItemsAsync(cellId, "TestApplication", null, 30);

            // Assert
            Assert.NotNull(columnResult);
            Assert.NotNull(rowResult);
            
            Assert.True(columnResult.Success);
            Assert.True(rowResult.Success);

            // 両方のメソッドが正しく実行されたことを検証
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<Dictionary<string, object>>(d => d["elementId"].ToString() == cellId), 30), Times.Once);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", 
                It.Is<Dictionary<string, object>>(d => d["elementId"].ToString() == cellId), 30), Times.Once);

            _output.WriteLine($"TableItem pattern integration test passed for {description}");
        }

        /// <summary>
        /// TableItem Pattern - ログ出力の検証
        /// </summary>
        [Fact]
        public async Task TableItem_Pattern_Should_Log_Correctly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult { Elements = new List<UIAutomationMCP.Shared.ElementInfo>() };
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(expectedResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            await _service.GetColumnHeaderItemsAsync("testCell", "TestApp", null, 30);
            await _service.GetRowHeaderItemsAsync("testCell", "TestApp", null, 30);

            // Assert - ログ出力の検証
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting column header items for element: testCell")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting row header items for element: testCell")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _output.WriteLine("TableItem pattern logging verification test passed");
        }

        /// <summary>
        /// TableItem Pattern - タイムアウト処理テスト
        /// </summary>
        [Theory]
        [InlineData(15)]
        [InlineData(45)]
        [InlineData(120)]
        public async Task TableItem_Pattern_Should_Handle_Custom_Timeouts(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ElementSearchResult { Elements = new List<UIAutomationMCP.Shared.ElementInfo>() };
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), timeoutSeconds))
                        .Returns(Task.FromResult(expectedResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), timeoutSeconds))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            await _service.GetColumnHeaderItemsAsync("timeoutTestCell", "TestApp", null, timeoutSeconds);
            await _service.GetRowHeaderItemsAsync("timeoutTestCell", "TestApp", null, timeoutSeconds);

            // Assert
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), timeoutSeconds), Times.Once);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaderItems", It.IsAny<Dictionary<string, object>>(), timeoutSeconds), Times.Once);

            _output.WriteLine($"TableItem pattern custom timeout test passed for {timeoutSeconds} seconds");
        }

        #endregion

        #region Existing Table Service Methods Tests

        /// <summary>
        /// 既存のTableServiceメソッドとの統合確認テスト
        /// Microsoft仕様: TablePatternとTableItemPatternの併用
        /// </summary>
        [Fact]
        public async Task TableService_Should_Support_Both_Table_And_TableItem_Patterns()
        {
            // Arrange - TablePatternとTableItemPatternの両方をサポートすることを確認
            var tableInfoResult = new UIAutomationMCP.Shared.Results.TableInfoResult { RowCount = 5, ColumnCount = 3 };
            var columnHeaderItemsResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Header 1" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<UIAutomationMCP.Shared.Results.TableInfoResult>("GetTableInfo", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(tableInfoResult));
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30))
                        .Returns(Task.FromResult(columnHeaderItemsResult));

            // Act
            var tableInfoResponse = await _service.GetTableInfoAsync("table1", "TestApp", null, 30);
            var columnHeaderItemsResponse = await _service.GetColumnHeaderItemsAsync("cell1_1", "TestApp", null, 30);

            // Assert
            Assert.NotNull(tableInfoResponse);
            Assert.NotNull(columnHeaderItemsResponse);

            // 両方のパターンが正しく動作することを検証
            _mockExecutor.Verify(e => e.ExecuteAsync<UIAutomationMCP.Shared.Results.TableInfoResult>("GetTableInfo", It.IsAny<Dictionary<string, object>>(), 30), Times.Once);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaderItems", It.IsAny<Dictionary<string, object>>(), 30), Times.Once);

            _output.WriteLine("TableService integration test passed - both Table and TableItem patterns supported");
        }

        #endregion
    }
}