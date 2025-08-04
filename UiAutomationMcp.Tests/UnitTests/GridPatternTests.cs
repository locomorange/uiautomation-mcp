using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// GridPatternの単体テスト
    /// Microsoft仕様に基づいたGridPatternの機能をモックベースでテストします
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class GridPatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IGridService> _mockGridService;

        public GridPatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockGridService = new Mock<IGridService>();

            // UIAutomationToolsの他サービスもモック化（最小限の設定）
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
                Mock.Of<IMcpLogService>()
            );
        }

        public void Dispose()
        {
            // Mock cleanup is not required
        }

        #region Microsoft spec compliant GridPattern property tests
        /* DISABLED - GetGridInfo method no longer exists
        [Fact]
        public async Task GetGridInfo_WithValidGrid_ShouldReturnRowAndColumnCount()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult
                {
                    RowCount = 5,
                    ColumnCount = 3
                }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("dataGrid", "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo("dataGrid", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("dataGrid", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("GetGridInfo test passed");
        }
        */

        /* DISABLED - GetGridInfo method no longer exists
        [Theory]
        [InlineData(1, 1)]
        [InlineData(10, 5)]
        [InlineData(100, 20)]
        [InlineData(1, 100)]
        public async Task GetGridInfo_WithVariousGridSizes_ShouldReturnCorrectDimensions(int rowCount, int columnCount)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult
                {
                    RowCount = rowCount,
                    ColumnCount = columnCount
                }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("grid", "TestApp", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo("grid", "TestApp");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("grid", "TestApp", null, 30), Times.Once);
            _output.WriteLine($"Grid size test passed: {rowCount}x{columnCount}");
        }
        */

        /* DISABLED - GetGridInfo method no longer exists
        [Fact]
        public async Task GetGridInfo_WithSingleItemGrid_ShouldStillBeValidGrid()
        {
            // Arrange - Microsoft spec: Single item should still be valid as a grid
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult
                {
                    RowCount = 1,
                    ColumnCount = 1
                }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("singleItemGrid", "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo("singleItemGrid", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("singleItemGrid", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Single item grid test passed");
        }
        */

        #endregion

        #region Microsoft spec compliant GetItem method tests
        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 1)]
        [InlineData(4, 2)]
        public async Task GetGridItem_WithValidCoordinates_ShouldReturnItem(int row, int column)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo
                        {
                            AutomationId = $"cell_{row}_{column}",
                            Name = $"Cell({row},{column})",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle
                            {
                                X = 100 + column * 80,
                                Y = 50 + row * 25,
                                Width = 80,
                                Height = 25
                            }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("dataGrid", null, row, column, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row, column, "dataGrid", "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataGrid", null, row, column, "TestWindow", null, 30), Times.Once);
            _output.WriteLine($"GetGridItem test passed for coordinates ({row},{column})");
        }

        [Fact]
        public async Task GetGridItem_WithZeroBasedCoordinates_ShouldReturnFirstItem()
        {
            // Arrange - Microsoft仕槁E グリテス座標0ベス
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo
                        {
                            AutomationId = "cell_0_0",
                            Name = "First Cell",
                            ControlType = "DataItem",
                            IsEnabled = true
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("grid", null, 0, 0, null, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 0, column: 0, automationId: "grid");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("grid", null, 0, 0, null, null, 30), Times.Once);
            _output.WriteLine("Zero-based coordinates test passed");
        }

        [Fact]
        public async Task GetGridItem_WithEmptyCell_ShouldStillReturnElement()
        {
            // Arrange - Microsoft仕槁E 空のセルでもUI Automation要素を返す忁E��がある
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo
                        {
                            AutomationId = "empty_cell_1_2",
                            Name = "",
                            ControlType = "DataItem",
                            IsEnabled = true,
                            BoundingRectangle = new UIAutomationMCP.Models.BoundingRectangle
                            {
                                X = 260,
                                Y = 75,
                                Width = 80,
                                Height = 25
                            }
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("dataGrid", null, 1, 2, "TestWindow", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row: 1, column: 2, automationId: "dataGrid", controlType: "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataGrid", null, 1, 2, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Empty cell test passed");
        }

        #endregion

        #region Microsoft仕様準拠のArgumentOutOfRangeException テスチE
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        public async Task GetGridItem_WithNegativeCoordinates_ShouldThrowArgumentOutOfRangeException(int row, int column)
        {
            // Arrange - Microsoft仕槁E 負の座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("grid", null, row, column, "TestApp", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row < 0 ? "row" : "column",
                               $"Row/column coordinates must be greater than or equal to zero"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem(row: row, column: column, automationId: "grid", controlType: "TestApp"));

            _mockGridService.Verify(s => s.GetGridItemAsync("grid", null, row, column, "TestApp", null, 30), Times.Once);
            _output.WriteLine($"Negative coordinates test passed: ({row},{column})");
        }

        [Theory]
        [InlineData(5, 0, 5, 3)]
        [InlineData(0, 3, 5, 3)]
        [InlineData(10, 5, 5, 3)]
        public async Task GetGridItem_WithCoordinatesExceedingBounds_ShouldThrowArgumentOutOfRangeException(
            int row, int column, int maxRow, int maxColumn)
        {
            // Arrange - Microsoft仕槁E RowCount/ColumnCountを趁E��る座標でArgumentOutOfRangeExceptionをスロー
            _mockGridService.Setup(s => s.GetGridItemAsync("grid", null, row, column, "TestApp", null, 30))
                           .ThrowsAsync(new ArgumentOutOfRangeException(
                               row >= maxRow ? "row" : "column",
                               $"Row/column coordinates must be less than RowCount/ColumnCount"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _tools.GetGridItem(row: row, column: column, automationId: "grid", controlType: "TestApp"));

            _mockGridService.Verify(s => s.GetGridItemAsync("grid", null, row, column, "TestApp", null, 30), Times.Once);
            _output.WriteLine($"Out of bounds test passed: ({row},{column}) exceeds ({maxRow},{maxColumn})");
        }

        #endregion

        #region エラーハンドリングテスチE
        /* DISABLED - GetGridInfo method no longer exists
        [Fact]
        public async Task GetGridInfo_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridInfoAsync("nonExistentGrid", "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element 'nonExistentGrid' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridInfo("nonExistentGrid", "TestWindow"));

            _mockGridService.Verify(s => s.GetGridInfoAsync("nonExistentGrid", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }
        */

        /* DISABLED - GetGridInfo method no longer exists
        [Fact]
        public async Task GetGridInfo_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridInfoAsync("textBox", "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Element does not support GridPattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridInfo("textBox", "TestWindow"));

            _mockGridService.Verify(s => s.GetGridInfoAsync("textBox", "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }
        */

        [Fact]
        public async Task GetGridItem_WithNonExistentGrid_ShouldHandleError()
        {
            // Arrange
            _mockGridService.Setup(s => s.GetGridItemAsync("invalidGrid", null, 0, 0, "TestWindow", null, 30))
                           .ThrowsAsync(new InvalidOperationException("Grid element 'invalidGrid' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.GetGridItem(0, 0, automationId: "invalidGrid", controlType: "TestWindow"));

            _mockGridService.Verify(s => s.GetGridItemAsync("invalidGrid", null, 0, 0, "TestWindow", null, 30), Times.Once);
            _output.WriteLine("Invalid grid element error handling test passed");
        }

        #endregion

        #region パラメータ検証テスチE
        /* DISABLED - GetGridInfo method no longer exists
        [Theory]
        [InlineData("", "TestWindow")]
        [InlineData("grid1", "")]
        public async Task GetGridInfo_WithEmptyParameters_ShouldCallService(string elementId, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult { RowCount = 3, ColumnCount = 2 }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync(elementId, 
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', window='{windowTitle}'");
        }
        */

        /* DISABLED - GetGridInfo method no longer exists
        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task GetGridInfo_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult { RowCount = 4, ColumnCount = 6 }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("grid1", "TestWindow", processId, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo("grid1", "TestWindow", processId);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("grid1", "TestWindow", processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }
        */

        /* DISABLED - GetGridInfo method no longer exists
        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task GetGridInfo_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult { RowCount = 2, ColumnCount = 4 }
            };
            _mockGridService.Setup(s => s.GetGridInfoAsync("grid1", "TestWindow", null, timeoutSeconds))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridInfo("grid1", "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridInfoAsync("grid1", "TestWindow", null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }
        */

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
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { AutomationId = "cell", Name = "Test Cell" }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync(
                elementId,
                null,
                row,
                column,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle,
                null,
                30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(
                row, column,
                automationId: elementId,
                controlType: string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync(
                elementId,
                null,
                row,
                column,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle,
                null,
                30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: elementId='{elementId}', coordinates=({row},{column}), window='{windowTitle}'");
        }

        #endregion

        #region Grid操作シナリオテスチE
        /* DISABLED - GetGridInfo method no longer exists
        [Fact]
        public async Task GridOperations_FullWorkflow_ShouldExecuteCorrectly()
        {
            // Arrange
            var gridInfoResult = new ServerEnhancedResponse<GridInfoResult>
            {
                Success = true,
                Data = new GridInfoResult { RowCount = 3, ColumnCount = 2 }
            };
            var cell00Result = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { AutomationId = "cell_0_0", Name = "Header 1" }
                    }
                }
            };
            var cell01Result = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { AutomationId = "cell_0_1", Name = "Header 2" }
                    }
                }
            };
            var cell10Result = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { AutomationId = "cell_1_0", Name = "Data 1" }
                    }
                }
            };
            var cell11Result = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { AutomationId = "cell_1_1", Name = "Data 2" }
                    }
                }
            };

            _mockGridService.Setup(s => s.GetGridInfoAsync("dataTable", "TestApp", null, 30))
                           .Returns(Task.FromResult(gridInfoResult));
            _mockGridService.Setup(s => s.GetGridItemAsync("dataTable", null, 0, 0, "TestApp", null, 30))
                           .Returns(Task.FromResult(cell00Result));
            _mockGridService.Setup(s => s.GetGridItemAsync("dataTable", null, 0, 1, "TestApp", null, 30))
                           .Returns(Task.FromResult(cell01Result));
            _mockGridService.Setup(s => s.GetGridItemAsync("dataTable", null, 1, 0, "TestApp", null, 30))
                           .Returns(Task.FromResult(cell10Result));
            _mockGridService.Setup(s => s.GetGridItemAsync("dataTable", null, 1, 1, "TestApp", null, 30))
                           .Returns(Task.FromResult(cell11Result));

            // Act
            var gridInfo = await _tools.GetGridInfo("dataTable", "TestApp");
            var headerCell1 = await _tools.GetGridItem("dataTable", 0, 0, "TestApp");
            var headerCell2 = await _tools.GetGridItem("dataTable", 0, 1, "TestApp");
            var dataCell1 = await _tools.GetGridItem("dataTable", 1, 0, "TestApp");
            var dataCell2 = await _tools.GetGridItem("dataTable", 1, 1, "TestApp");

            // Assert
            Assert.NotNull(gridInfo);
            Assert.NotNull(headerCell1);
            Assert.NotNull(headerCell2);
            Assert.NotNull(dataCell1);
            Assert.NotNull(dataCell2);

            _mockGridService.Verify(s => s.GetGridInfoAsync("dataTable", "TestApp", null, 30), Times.Once);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataTable", null, 0, 0, "TestApp", null, 30), Times.Once);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataTable", null, 0, 1, "TestApp", null, 30), Times.Once);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataTable", null, 1, 0, "TestApp", null, 30), Times.Once);
            _mockGridService.Verify(s => s.GetGridItemAsync("dataTable", null, 1, 1, "TestApp", null, 30), Times.Once);

            _output.WriteLine("Full grid workflow test passed");
        }
        */

        #endregion

        #region 墁E��値テスチE
        [Theory]
        [InlineData(0, 0, 1, 1)]
        [InlineData(0, 0, 5, 3)]
        [InlineData(4, 2, 5, 3)]
        public async Task GetGridItem_WithBoundaryCoordinates_ShouldSucceed(int row, int column, int maxRow, int maxColumn)
        {
            // Arrange - Test normal operation with boundary values
            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
            {
                Success = true,
                Data = new ElementSearchResult
                {
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo
                        {
                            AutomationId = $"boundary_cell_{row}_{column}",
                            Name = $"Boundary Cell ({row},{column})",
                            ControlType = "DataItem",
                            IsEnabled = true
                        }
                    }
                }
            };
            _mockGridService.Setup(s => s.GetGridItemAsync("boundaryGrid", null, row, column, "TestApp", null, 30))
                           .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.GetGridItem(row, column, automationId: "boundaryGrid", controlType: "TestApp");

            // Assert
            Assert.NotNull(result);
            _mockGridService.Verify(s => s.GetGridItemAsync("boundaryGrid", null, row, column, "TestApp", null, 30), Times.Once);
            _output.WriteLine($"Boundary coordinates test passed: ({row},{column}) within ({maxRow},{maxColumn})");
        }

        #endregion
    }
}
