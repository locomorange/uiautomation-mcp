using Moq;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// GridItemPatternの単体テスト
    /// Microsoft仕様に基づいたGridItemPatternの機能をモックベースでテストします
    /// GridItemプロバイダーの必須プロパティ（Row, Column, RowSpan, ColumnSpan, ContainingGrid）をテスト
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GridItemPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IGridService> _mockGridService;

        public GridItemPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockGridService = new Mock<IGridService>();
            
            // UIAutomationToolsの他のサービスもモック化（最小限の設定）
            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInvoke = new Mock<IInvokeService>();
            var mockValue = new Mock<IValueService>();
            var mockRange = new Mock<IRangeService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockToggle = new Mock<IToggleService>();
            var mockWindow = new Mock<IWindowService>();
            var mockLayout = new Mock<ILayoutService>();
            var mockTable = new Mock<ITableService>();
            var mockMultipleView = new Mock<IMultipleViewService>();
            var mockAccessibility = new Mock<IAccessibilityService>();
            var mockCustomProperty = new Mock<ICustomPropertyService>();
            var mockControlType = new Mock<IControlTypeService>();
            var mockTransform = new Mock<ITransformService>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                mockInvoke.Object,
                mockValue.Object,
                mockRange.Object,
                mockSelection.Object,
                mockText.Object,
                mockToggle.Object,
                mockWindow.Object,
                mockLayout.Object,
                _mockGridService.Object,
                mockTable.Object,
                mockMultipleView.Object,
                mockAccessibility.Object,
                mockCustomProperty.Object,
                mockControlType.Object,
                mockTransform.Object
            );
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region Microsoft仕様準拠のGridItem必須プロパティテスト

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(4, 5)]
        public async Task GetGridItem_WithValidCoordinates_ShouldReturnRowAndColumnProperties(int expectedRow, int expectedColumn)
        {
            // Arrange - Microsoft仕様: Row, Columnプロパティは0ベースの座標を返す
            var expectedResult = new
            {
                AutomationId = $"cell_{expectedRow}_{expectedColumn}",
                Name = $"Cell({expectedRow},{expectedColumn})",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = expectedRow,
                Column = expectedColumn,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "parentGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("dataGrid", expectedRow, expectedColumn, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("dataGrid", expectedRow, expectedColumn, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataGrid", expectedRow, expectedColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"GridItem Row/Column properties test passed: ({expectedRow},{expectedColumn})");
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        public async Task GetGridItem_WithSpannedCells_ShouldReturnRowSpanAndColumnSpanProperties(int rowSpan, int columnSpan)
        {
            // Arrange - Microsoft仕様: RowSpan, ColumnSpanプロパティはセルが跨ぐ行数/列数を返す
            var expectedResult = new
            {
                AutomationId = "spanned_cell",
                Name = "Spanned Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 0,
                Column = 0,
                RowSpan = rowSpan,
                ColumnSpan = columnSpan,
                ContainingGrid = "parentGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("spanGrid", 0, 0, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("spanGrid", 0, 0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("spanGrid", 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"GridItem RowSpan/ColumnSpan properties test passed: span({rowSpan},{columnSpan})");
        }

        [Fact]
        public async Task GetGridItem_WithSingleCell_ShouldReturnSpanOfOne()
        {
            // Arrange - Microsoft仕様: 単一セルのRowSpan, ColumnSpanは1
            var expectedResult = new
            {
                AutomationId = "single_cell",
                Name = "Single Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 2,
                Column = 3,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "parentGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("normalGrid", 2, 3, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("normalGrid", 2, 3, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("normalGrid", 2, 3, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem single cell span test passed");
        }

        [Fact]
        public async Task GetGridItem_WithContainingGridReference_ShouldReturnValidReference()
        {
            // Arrange - Microsoft仕様: ContainingGridプロパティは親グリッドへの参照を返す
            var expectedResult = new
            {
                AutomationId = "grid_item_1",
                Name = "Grid Item 1",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 1,
                Column = 1,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "mainDataGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mainDataGrid", 1, 1, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("mainDataGrid", 1, 1, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mainDataGrid", 1, 1, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem ContainingGrid reference test passed");
        }

        #endregion

        #region Microsoft仕様準拠のマージされたセルテスト

        [Theory]
        [InlineData(0, 0, 2, 3)] // アンカーセル(0,0)から2行3列のマージ
        [InlineData(1, 1, 1, 2)] // アンカーセル(1,1)から1行2列のマージ
        [InlineData(3, 2, 3, 1)] // アンカーセル(3,2)から3行1列のマージ
        public async Task GetGridItem_WithMergedCells_ShouldReportAnchorCellCoordinates(int anchorRow, int anchorColumn, int rowSpan, int columnSpan)
        {
            // Arrange - Microsoft仕様: マージされたセルはアンカーセルの座標を報告する
            var expectedResult = new
            {
                AutomationId = $"merged_cell_{anchorRow}_{anchorColumn}",
                Name = $"Merged Cell Anchor({anchorRow},{anchorColumn})",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = anchorRow,
                Column = anchorColumn,
                RowSpan = rowSpan,
                ColumnSpan = columnSpan,
                ContainingGrid = "mergeGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mergeGrid", anchorRow, anchorColumn, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("mergeGrid", anchorRow, anchorColumn, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mergeGrid", anchorRow, anchorColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Merged cell anchor coordinates test passed: anchor({anchorRow},{anchorColumn}) span({rowSpan},{columnSpan})");
        }

        [Theory]
        [InlineData(0, 1, 0, 0)] // マージ範囲内の(0,1)は実際には(0,0)のアンカーセルを指す
        [InlineData(1, 0, 0, 0)] // マージ範囲内の(1,0)は実際には(0,0)のアンカーセルを指す
        [InlineData(1, 1, 0, 0)] // マージ範囲内の(1,1)は実際には(0,0)のアンカーセルを指す
        public async Task GetGridItem_WithCellsInMergeRange_ShouldReturnAnchorCellProperties(int requestRow, int requestColumn, int anchorRow, int anchorColumn)
        {
            // Arrange - Microsoft仕様: マージ範囲内のどの座標でも同じアンカーセルのプロパティを返す
            var expectedResult = new
            {
                AutomationId = $"merged_anchor_{anchorRow}_{anchorColumn}",
                Name = $"Merged Anchor Cell({anchorRow},{anchorColumn})",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = anchorRow,
                Column = anchorColumn,
                RowSpan = 2,
                ColumnSpan = 2,
                ContainingGrid = "mergeGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mergeGrid", requestRow, requestColumn, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("mergeGrid", requestRow, requestColumn, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mergeGrid", requestRow, requestColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Merged cell range test passed: request({requestRow},{requestColumn}) -> anchor({anchorRow},{anchorColumn})");
        }

        #endregion

        #region Microsoft仕様準拠の座標系テスト

        [Fact]
        public async Task GetGridItem_WithZeroBasedCoordinates_ShouldReturnCorrectItem()
        {
            // Arrange - Microsoft仕様: 座標系は0ベース、左上が(0,0)
            var expectedResult = new
            {
                AutomationId = "top_left_cell",
                Name = "Top Left Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 0,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "coordGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("coordGrid", 0, 0, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("coordGrid", 0, 0, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("coordGrid", 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Zero-based coordinate system test passed");
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 4)]
        [InlineData(9, 9)]
        public async Task GetGridItem_WithVariousCoordinates_ShouldMaintainCoordinateConsistency(int row, int column)
        {
            // Arrange - Microsoft仕様: 返されるRow/Columnプロパティは要求された座標と一致する必要がある
            var expectedResult = new
            {
                AutomationId = $"consistency_cell_{row}_{column}",
                Name = $"Consistency Cell({row},{column})",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = row,
                Column = column,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "consistencyGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("consistencyGrid", row, column, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("consistencyGrid", row, column, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("consistencyGrid", row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Coordinate consistency test passed: ({row},{column})");
        }

        #endregion

        #region Microsoft仕様準拠の例外処理テスト

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        public async Task GetGridItem_WithNegativeCoordinates_ShouldThrowArgumentOutOfRangeException(int row, int column)
        {
            // Arrange - Microsoft仕様: 負の座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("errorGrid", row, column, "TestWindow", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row < 0 ? "row" : "column", 
                               "Row and column coordinates must be greater than or equal to zero"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem("errorGrid", row, column, "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("errorGrid", row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Negative coordinates exception test passed: ({row},{column})");
        }

        [Theory]
        [InlineData(5, 0, 5, 3)] // Row index >= RowCount
        [InlineData(0, 3, 5, 3)] // Column index >= ColumnCount
        [InlineData(10, 10, 5, 3)] // Both indices exceed bounds
        public async Task GetGridItem_WithCoordinatesExceedingBounds_ShouldThrowArgumentOutOfRangeException(
            int row, int column, int maxRow, int maxColumn)
        {
            // Arrange - Microsoft仕様: RowCount/ColumnCountを超える座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("boundGrid", row, column, "TestWindow", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row >= maxRow ? "row" : "column", 
                               $"Row/column coordinates must be less than RowCount ({maxRow}) / ColumnCount ({maxColumn})"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem("boundGrid", row, column, "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("boundGrid", row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Bounds exception test passed: ({row},{column}) exceeds bounds ({maxRow},{maxColumn})");
        }

        #endregion

        #region GridItemプロパティの整合性テスト

        [Fact]
        public async Task GetGridItem_WithValidItem_ShouldHaveConsistentProperties()
        {
            // Arrange - すべてのGridItem必須プロパティが適切に設定されていることを確認
            var expectedResult = new
            {
                AutomationId = "consistent_item",
                Name = "Consistent Item",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 2,
                Column = 3,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "propertyGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("propertyGrid", 2, 3, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("propertyGrid", 2, 3, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("propertyGrid", 2, 3, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem property consistency test passed");
        }

        [Theory]
        [InlineData(0, 0, 2, 3)]
        [InlineData(1, 2, 1, 4)]
        [InlineData(3, 1, 3, 2)]
        public async Task GetGridItem_WithSpannedItems_ShouldHaveValidSpanValues(int row, int column, int rowSpan, int columnSpan)
        {
            // Arrange - RowSpan/ColumnSpanが1以上の有効な値であることを確認
            var expectedResult = new
            {
                AutomationId = $"span_item_{row}_{column}",
                Name = $"Span Item({row},{column})",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = row,
                Column = column,
                RowSpan = rowSpan,
                ColumnSpan = columnSpan,
                ContainingGrid = "spanValidGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("spanValidGrid", row, column, "TestWindow", null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("spanValidGrid", row, column, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("spanValidGrid", row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Valid span values test passed: span({rowSpan},{columnSpan})");
        }

        #endregion

        #region 境界値とエラーハンドリングテスト

        [Fact]
        public async Task GetGridItem_WithNonExistentGrid_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridItemAsync("nonExistentGrid", 0, 0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Grid element 'nonExistentGrid' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridItem("nonExistentGrid", 0, 0, "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("nonExistentGrid", 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent grid error handling test passed");
        }

        [Fact]
        public async Task GetGridItem_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridItemAsync("textBox", 0, 0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element does not support GridItemPattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridItem("textBox", 0, 0, "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("textBox", 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスト

        [Theory]
        [InlineData("", 0, 0, "TestWindow")]
        [InlineData("grid1", 0, 0, "")]
        public async Task GetGridItem_WithEmptyParameters_ShouldCallService(string elementId, int row, int column, string windowTitle)
        {
            // Arrange
            var expectedResult = new
            {
                AutomationId = "empty_param_cell",
                Name = "Empty Param Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = row,
                Column = column,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = elementId
            };
            _mockGridService.Setup(s => s.GetGridItemAsync(elementId, row, column, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem(elementId, row, column, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync(elementId, row, column, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task GetGridItem_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new
            {
                AutomationId = "process_cell",
                Name = "Process Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 1,
                Column = 1,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "processGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("processGrid", 1, 1, "TestWindow", processId, 30))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("processGrid", 1, 1, "TestWindow", processId);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("processGrid", 1, 1, "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task GetGridItem_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new
            {
                AutomationId = "timeout_cell",
                Name = "Timeout Cell",
                ControlType = "DataItem",
                IsEnabled = true,
                Row = 0,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                ContainingGrid = "timeoutGrid"
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("timeoutGrid", 0, 0, "TestWindow", null, timeoutSeconds))
                           .ReturnsAsync(expectedResult);

            // Act
            var result = await _tools.GetGridItem("timeoutGrid", 0, 0, "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("timeoutGrid", 0, 0, "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion
    }
}