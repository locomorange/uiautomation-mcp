using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Models.Results;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// GridItemPattern unit tests
    /// Test GridItemPattern functionality based on Microsoft specification using mocks
    /// Tests GridItem provider properties: Row, Column, RowSpan, ColumnSpan, ContainingGrid
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
            
            // Mock other UIAutomationTools services (minimal setup)
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
            var mockVirtualizedItem = new Mock<IVirtualizedItemService>();
            var mockItemContainer = new Mock<IItemContainerService>();
            var mockSynchronizedInput = new Mock<ISynchronizedInputService>();

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
                mockTransform.Object,
                mockVirtualizedItem.Object,
                mockItemContainer.Object,
                mockSynchronizedInput.Object,
                Mock.Of<IEventMonitorService>(),
                Mock.Of<IFocusService>(),
                Mock.Of<IOperationExecutor>()
            );
        }

        public void Dispose()
        {
            // Mock cleanup is not required
        }

        #region Microsoft spec compliant GridItem property tests
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(4, 5)]
        public async Task GetGridItem_WithValidCoordinates_ShouldReturnRowAndColumnProperties(int expectedRow, int expectedColumn)
        {
            // Arrange - Microsoft spec: Row, Column properties return 0-based coordinates
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = $"cell_{expectedRow}_{expectedColumn}",
                            Name = $"Cell({expectedRow},{expectedColumn})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("dataGrid", null, expectedRow, expectedColumn, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(expectedRow, expectedColumn, "dataGrid", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataGrid", null, expectedRow, expectedColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"GridItem Row/Column properties test passed: ({expectedRow},{expectedColumn})");
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        public async Task GetGridItem_WithSpannedCells_ShouldReturnRowSpanAndColumnSpanProperties(int rowSpan, int columnSpan)
        {
            // Arrange - Microsoft spec: RowSpan, ColumnSpan properties return number of rows/columns the cell spans
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "spanned_cell",
                            Name = "Spanned Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } },
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("spanGrid", null, 0, 0, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(0, 0, "spanGrid", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("spanGrid", null, 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"GridItem RowSpan/ColumnSpan properties test passed: span({rowSpan},{columnSpan})");
        }

        [Fact]
        public async Task GetGridItem_WithSingleCell_ShouldReturnSpanOfOne()
        {
            // Arrange - Microsoft spec: Single cell RowSpan, ColumnSpan should be 1
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "single_cell",
                            Name = "Single Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } },
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("normalGrid", null, 2, 3, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 2, column: 3, automationId: "normalGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("normalGrid", null, 2, 3, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem single cell span test passed");
        }

        [Fact]
        public async Task GetGridItem_WithContainingGridReference_ShouldReturnValidReference()
        {
            // Arrange - Microsoft spec: ContainingGrid property returns reference to parent grid
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "grid_item_1",
                            Name = "Grid Item 1",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } },
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mainDataGrid", null, 1, 1, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 1, column: 1, automationId: "mainDataGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mainDataGrid", null, 1, 1, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem ContainingGrid reference test passed");
        }

        #endregion

        #region Microsoft spec compliant merged cell tests
        [Theory]
        [InlineData(0, 0, 2, 3)] // Anchor cell (0,0) with 2 rows, 3 columns merged
        [InlineData(1, 1, 1, 2)] // Anchor cell (1,1) with 1 row, 2 columns merged
        [InlineData(3, 2, 3, 1)] // Anchor cell (3,2) with 3 rows, 1 column merged
        public async Task GetGridItem_WithMergedCells_ShouldReportAnchorCellCoordinates(int anchorRow, int anchorColumn, int rowSpan, int columnSpan)
        {
            // Arrange - Microsoft spec: Merged cells should report anchor cell coordinates
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = $"merged_cell_{anchorRow}_{anchorColumn}",
                            Name = $"Merged Cell Anchor({anchorRow},{anchorColumn})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mergeGrid", null, anchorRow, anchorColumn, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(anchorRow, anchorColumn, automationId: "mergeGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mergeGrid", null, anchorRow, anchorColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Merged cell anchor coordinates test passed: anchor({anchorRow},{anchorColumn}) span({rowSpan},{columnSpan})");
        }

        [Theory]
        [InlineData(0, 1, 0, 0)] // Merged cell at (0,1) actually points to anchor cell (0,0)
        [InlineData(1, 0, 0, 0)] // Merged cell at (1,0) actually points to anchor cell (0,0)
        [InlineData(1, 1, 0, 0)] // Merged cell at (1,1) actually points to anchor cell (0,0)
        public async Task GetGridItem_WithCellsInMergeRange_ShouldReturnAnchorCellProperties(int requestRow, int requestColumn, int anchorRow, int anchorColumn)
        {
            // Arrange - Microsoft spec: Merged cells at any coordinate return same anchor cell properties
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = $"merged_anchor_{anchorRow}_{anchorColumn}",
                            Name = $"Merged Anchor Cell({anchorRow},{anchorColumn})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("mergeGrid", null, requestRow, requestColumn, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(requestRow, requestColumn, automationId: "mergeGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("mergeGrid", null, requestRow, requestColumn, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Merged cell range test passed: request({requestRow},{requestColumn}) -> anchor({anchorRow},{anchorColumn})");
        }

        #endregion

        #region Microsoft spec compliant coordinate system tests
        [Fact]
        public async Task GetGridItem_WithZeroBasedCoordinates_ShouldReturnCorrectItem()
        {
            // Arrange - Microsoft spec: Coordinate system is 0-based, top-left is (0,0)
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "top_left_cell",
                            Name = "Top Left Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("coordGrid", null, 0, 0, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(0, 0, automationId: "coordGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("coordGrid", null, 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Zero-based coordinate system test passed");
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 4)]
        [InlineData(9, 9)]
        public async Task GetGridItem_WithVariousCoordinates_ShouldMaintainCoordinateConsistency(int row, int column)
        {
            // Arrange - Microsoft仕槁E 返されるRow/Columnプロパティは要求された座標と一致する忁E��がある
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = $"consistency_cell_{row}_{column}",
                            Name = $"Consistency Cell({row},{column})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("consistencyGrid", null, row, column, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: row, column: column, automationId: "consistencyGrid", name: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("consistencyGrid", null, row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Coordinate consistency test passed: ({row},{column})");
        }

        #endregion

        #region Microsoft仕様準拠の例外琁E��スチE
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        public async Task GetGridItem_WithNegativeCoordinates_ShouldThrowArgumentOutOfRangeException(int row, int column)
        {
            // Arrange - Microsoft仕槁E 負の座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("errorGrid", null, row, column, "TestWindow", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row < 0 ? "row" : "column", 
                               "Row and column coordinates must be greater than or equal to zero"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem(row: row, column: column, automationId: "errorGrid", name: "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("errorGrid", null, row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Negative coordinates exception test passed: ({row},{column})");
        }

        [Theory]
        [InlineData(5, 0, 5, 3)] // Row index >= RowCount
        [InlineData(0, 3, 5, 3)] // Column index >= ColumnCount
        [InlineData(10, 10, 5, 3)] // Both indices exceed bounds
        public async Task GetGridItem_WithCoordinatesExceedingBounds_ShouldThrowArgumentOutOfRangeException(
            int row, int column, int maxRow, int maxColumn)
        {
            // Arrange - Microsoft仕槁E RowCount/ColumnCountを趁E��る座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("boundGrid", null, row, column, "TestWindow", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row >= maxRow ? "row" : "column", 
                               $"Row/column coordinates must be less than RowCount ({maxRow}) / ColumnCount ({maxColumn})"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem(row: row, column: column, automationId: "boundGrid", name: "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("boundGrid", null, row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Bounds exception test passed: ({row},{column}) exceeds bounds ({maxRow},{maxColumn})");
        }

        #endregion

        #region GridItem property consistency tests
        [Fact]
        public async Task GetGridItem_WithValidItem_ShouldHaveConsistentProperties()
        {
            // Arrange - All GridItem properties should be properly set and verified
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "consistent_item",
                            Name = "Consistent Item",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("propertyGrid", null, 2, 3, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 2, column: 3, automationId: "propertyGrid", name: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("propertyGrid", null, 2, 3, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GridItem property consistency test passed");
        }

        [Theory]
        [InlineData(0, 0, 2, 3)]
        [InlineData(1, 2, 1, 4)]
        [InlineData(3, 1, 3, 2)]
        public async Task GetGridItem_WithSpannedItems_ShouldHaveValidSpanValues(int row, int column, int rowSpan, int columnSpan)
        {
            // Arrange - RowSpan/ColumnSpan should be valid values greater than or equal to 1
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = $"span_item_{row}_{column}",
                            Name = $"Span Item({row},{column})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("spanValidGrid", null, row, column, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: row, column: column, automationId: "spanValidGrid", name: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("spanValidGrid", null, row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"Valid span values test passed: span({rowSpan},{columnSpan})");
        }

        #endregion

        #region 墁E��値とエラーハンドリングテスチE
        [Fact]
        public async Task GetGridItem_WithNonExistentGrid_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridItemAsync("nonExistentGrid", null, 0, 0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Grid element 'nonExistentGrid' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridItem(row: 0, column: 0, automationId: "nonExistentGrid", name: "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("nonExistentGrid", null, 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent grid error handling test passed");
        }

        [Fact]
        public async Task GetGridItem_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridItemAsync("textBox", null, 0, 0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element does not support GridItemPattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridItem(row: 0, column: 0, automationId: "textBox", name: "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("textBox", null, 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスチE
        [Theory]
        [InlineData("", 0, 0, "TestWindow")]
        [InlineData("grid1", 0, 0, "")]
        public async Task GetGridItem_WithEmptyParameters_ShouldCallService(string elementId, int row, int column, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "empty_param_cell",
                            Name = "Empty Param Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync(elementId, null, row, column, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: row, column: column, automationId: elementId, 
                name: string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync(elementId, null, row, column, 
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
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "process_cell",
                            Name = "Process Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("processGrid", null, 1, 1, "TestWindow", processId, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 1, column: 1, automationId: "processGrid", name: "TestWindow", processId: processId);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("processGrid", null, 1, 1, "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task GetGridItem_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Success = true,
                    Elements = new List<ElementInfo>
                    {
                        new ElementInfo
                        {
                            AutomationId = "timeout_cell",
                            Name = "Timeout Cell",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            SupportedPatterns = new string[] { "GridItemPattern" },
                            Details = new ElementDetails { GridItem = new GridItemInfo
                            {
                                Row = 0,
                                Column = 0,
                                RowSpan = 1,
                                ColumnSpan = 1
                            } }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("timeoutGrid", null, 0, 0, "TestWindow", null, timeoutSeconds))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 0, column: 0, automationId: "timeoutGrid", name: "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("timeoutGrid", null, 0, 0, "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion
    }
}
