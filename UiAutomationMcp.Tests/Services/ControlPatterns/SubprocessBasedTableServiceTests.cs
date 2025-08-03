using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Abstractions;

namespace UIAutomationMCP.Tests.Services.ControlPatterns
{
    /// <summary>
    /// Tests for TableService - Microsoft TableItemPattern 
    ///  :  UIAutomation 
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TableServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<TableService>> _mockLogger;
        private readonly Mock<IProcessManager> _mockProcessManager;
        private readonly TableService _service;

        public TableServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<TableService>>();
            _mockProcessManager = new Mock<IProcessManager>();
            _service = new TableService(_mockProcessManager.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            // Mock 
        }

        #region GetColumnHeaderItemsAsync Tests - Microsoft TableItemPattern 

        /// <summary>
        /// GetColumnHeaderItemsAsync - Microsoft TableItemPattern.GetColumnHeaderItems()の実装テスト
        /// Required Members: GetColumnHeaderItems() - テーブルアイテムのカラムヘッダーを取得
        /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_ValidTableCell_ReturnsColumnHeaders()
        {
            // Arrange - Microsoft TableItemPattern GetColumnHeaderItems()のテスト用データ
            var expectedColumnHeaders = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = "header_col1",
                        Name = "Product Name",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle { X = 10, Y = 5, Width = 120, Height = 25 }
                    },
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = "header_col2",
                        Name = "Price",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle { X = 130, Y = 5, Width = 80, Height = 25 }
                    }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(expectedColumnHeaders));

            // Act
            var result = await _service.GetColumnHeaderItemsAsync("tableCell1", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedColumnHeaders, result.Data);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<GetColumnHeaderItemsRequest>(r => 
                    r.AutomationId == "tableCell1" &&
                    r.WindowTitle == "TestWindow"), 30), Times.Once);

            _output.WriteLine("GetColumnHeaderItemsAsync test passed - TableItemPattern.GetColumnHeaderItems() verified");
        }

        /// <summary>
        /// GetColumnHeaderItemsAsync -  ID         /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_WithProcessIdAndCustomTimeout_ExecutesCorrectly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Column Header 1" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 60))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.GetColumnHeaderItemsAsync("cell2_3", null, null, 1234, 60);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedResult, result.Data);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<GetColumnHeaderItemsRequest>(r => 
                    r.AutomationId == "cell2_3" &&
                    r.WindowTitle == "" &&
                    r.ProcessId == 1234), 60), Times.Once);

            _output.WriteLine("GetColumnHeaderItemsAsync with ProcessId and custom timeout test passed");
        }

        /// <summary>
        /// GetColumnHeaderItemsAsync -          /// </summary>
        [Fact]
        public async Task GetColumnHeaderItemsAsync_SubprocessException_ReturnsError()
        {
            // Arrange
            var expectedException = new InvalidOperationException("TableItemPattern not supported");
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
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

        #region GetRowHeaderItemsAsync Tests - Microsoft TableItemPattern 

        /// <summary>
        /// GetRowHeaderItemsAsync -  icrosoft TableItemPattern.GetRowHeaderItems()         /// Required Members: GetRowHeaderItems() -          /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_ValidTableCell_ReturnsRowHeaders()
        {
            // Arrange - Microsoft TableItemPattern GetRowHeaderItems()のテスト用データ
            var expectedRowHeaders = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = "header_row1",
                        Name = "Customer 1",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle { X = 5, Y = 30, Width = 90, Height = 20 }
                    },
                    new UIAutomationMCP.Models.ElementInfo
                    {
                        AutomationId = "header_row2",
                        Name = "Customer 2",
                        ControlType = "Header",
                        IsEnabled = true,
                        BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle { X = 5, Y = 50, Width = 90, Height = 20 }
                    }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(expectedRowHeaders));

            // Act
            var result = await _service.GetRowHeaderItemsAsync("tableCell2", "TestWindow", null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRowHeaders, result.Data);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", 
                It.Is<GetRowHeaderItemsRequest>(r => 
                    r.AutomationId == "tableCell2" &&
                    r.WindowTitle == "TestWindow" &&
                    r.ProcessId == 0), 30), Times.Once);

            _output.WriteLine("GetRowHeaderItemsAsync test passed - TableItemPattern.GetRowHeaderItems() verified");
        }

        /// <summary>
        /// GetRowHeaderItemsAsync -          /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_WithDefaultParameters_ExecutesCorrectly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Default Row Header" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.GetRowHeaderItemsAsync("defaultCell");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedResult, result.Data);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", 
                It.Is<GetRowHeaderItemsRequest>(r => 
                    r.AutomationId == "defaultCell" &&
                    r.WindowTitle == "" &&
                    r.ProcessId == 0), 30), Times.Once);

            _output.WriteLine("GetRowHeaderItemsAsync with default parameters test passed");
        }

        /// <summary>
        /// GetRowHeaderItemsAsync -          /// </summary>
        [Fact]
        public async Task GetRowHeaderItemsAsync_SubprocessException_ReturnsError()
        {
            // Arrange
            var expectedException = new ArgumentException("Element not found: nonExistentCell");
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
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
        /// TableItem Pattern Integration -          /// Microsoft  TableItemPattern GridItemPattern         /// </summary>
        [Theory]
        [InlineData("cell_1_1", "Cell at row 1, column 1")]
        [InlineData("cell_2_3", "Cell at row 2, column 3")]
        [InlineData("cell_0_0", "Top-left cell")]
        public async Task TableItem_Pattern_Methods_Should_Work_Together(string cellId, string description)
        {
            // Arrange - 統合テスト用のテーブルアイテムパターンデータ
            var columnHeadersResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = $"Column Header for {cellId}" }
                }
            };
            
            var rowHeadersResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = $"Row Header for {cellId}" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(columnHeadersResult));
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(rowHeadersResult));

            // Act
            var columnResult = await _service.GetColumnHeaderItemsAsync(cellId, "TestApplication", null, 30);
            var rowResult = await _service.GetRowHeaderItemsAsync(cellId, "TestApplication", null, 30);

            // Assert
            Assert.NotNull(columnResult);
            Assert.NotNull(rowResult);
            
            Assert.True(columnResult.Success);
            Assert.True(rowResult.Success);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", 
                It.Is<GetColumnHeaderItemsRequest>(r => r.AutomationId == cellId), 30), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", 
                It.Is<GetRowHeaderItemsRequest>(r => r.AutomationId == cellId), 30), Times.Once);

            _output.WriteLine($"TableItem pattern integration test passed for {description}");
        }

        /// <summary>
        /// TableItem Pattern -  
        /// </summary>
        [Fact]
        public async Task TableItem_Pattern_Should_Log_Correctly()
        {
            // Arrange
            var expectedResult = new ElementSearchResult { Elements = new List<UIAutomationMCP.Models.ElementInfo>() };
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(expectedResult));
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            await _service.GetColumnHeaderItemsAsync("testCell", "TestApp", null, 30);
            await _service.GetRowHeaderItemsAsync("testCell", "TestApp", null, 30);

            // Assert -  
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
        /// TableItem Pattern -          /// </summary>
        [Theory]
        [InlineData(15)]
        [InlineData(45)]
        [InlineData(120)]
        public async Task TableItem_Pattern_Should_Handle_Custom_Timeouts(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ElementSearchResult { Elements = new List<UIAutomationMCP.Models.ElementInfo>() };
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), timeoutSeconds))
                        .Returns(Task.FromResult(expectedResult));
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), timeoutSeconds))
                        .Returns(Task.FromResult(expectedResult));

            // Act
            await _service.GetColumnHeaderItemsAsync("timeoutTestCell", "TestApp", null, timeoutSeconds);
            await _service.GetRowHeaderItemsAsync("timeoutTestCell", "TestApp", null, timeoutSeconds);

            // Assert
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), timeoutSeconds), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), timeoutSeconds), Times.Once);

            _output.WriteLine($"TableItem pattern custom timeout test passed for {timeoutSeconds} seconds");
        }

        #endregion

        #region Existing Table Service Methods Tests

        /// <summary>
        ///  TableService         /// Microsoft  TablePattern TableItemPattern 
        /// </summary>
        [Fact]
        public async Task TableService_Should_Support_Both_Table_And_TableItem_Patterns()
        {
            // Arrange - TablePatternとTableItemPatternの統合テスト
            var rowHeadersResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Row Header 1" }
                }
            };
            var columnHeaderItemsResult = new ElementSearchResult
            {
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Header 1" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), 30))
                        .Returns(Task.FromResult(rowHeadersResult));
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
                        .Returns(Task.FromResult(columnHeaderItemsResult));

            // Act
            var rowHeadersResponse = await _service.GetRowHeadersAsync("table1", "TestApp", null, 30);
            var columnHeaderItemsResponse = await _service.GetColumnHeaderItemsAsync("cell1_1", "TestApp", null, 30);

            // Assert
            Assert.NotNull(rowHeadersResponse);
            Assert.NotNull(columnHeaderItemsResponse);

            //  
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), 30), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30), Times.Once);

            _output.WriteLine("TableService integration test passed - both Table and TableItem patterns supported");
        }

        #endregion
    }
}
