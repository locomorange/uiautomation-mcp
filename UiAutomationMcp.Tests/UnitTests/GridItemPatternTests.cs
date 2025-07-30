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
    /// GridItemPattern縺ｮ蜊倅ｽ薙ユ繧ｹ繝・    /// Microsoft莉墓ｧ倥↓蝓ｺ縺･縺・◆GridItemPattern縺ｮ讖溯・繧偵Δ繝・け繝吶・繧ｹ縺ｧ繝・せ繝医＠縺ｾ縺・    /// GridItem繝励Ο繝舌う繝繝ｼ縺ｮ蠢・医・繝ｭ繝代ユ繧｣・・ow, Column, RowSpan, ColumnSpan, ContainingGrid・峨ｒ繝・せ繝・    /// </summary>
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
            
            // UIAutomationTools縺ｮ莉悶・繧ｵ繝ｼ繝薙せ繧ゅΔ繝・け蛹厄ｼ域怙蟆城剞縺ｮ險ｭ螳夲ｼ・            var mockAppLauncher = new Mock<IApplicationLauncher>();
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
            // 繝｢繝・け縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺ｯ荳崎ｦ・        }

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮGridItem蠢・医・繝ｭ繝代ユ繧｣繝・せ繝・
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(4, 5)]
        public async Task GetGridItem_WithValidCoordinates_ShouldReturnRowAndColumnProperties(int expectedRow, int expectedColumn)
        {
            // Arrange - Microsoft莉墓ｧ・ Row, Column繝励Ο繝代ユ繧｣縺ｯ0繝吶・繧ｹ縺ｮ蠎ｧ讓吶ｒ霑斐☆
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
            // Arrange - Microsoft莉墓ｧ・ RowSpan, ColumnSpan繝励Ο繝代ユ繧｣縺ｯ繧ｻ繝ｫ縺瑚ｷｨ縺占｡梧焚/蛻玲焚繧定ｿ斐☆
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
            // Arrange - Microsoft莉墓ｧ・ 蜊倅ｸ繧ｻ繝ｫ縺ｮRowSpan, ColumnSpan縺ｯ1
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
            // Arrange - Microsoft莉墓ｧ・ ContainingGrid繝励Ο繝代ユ繧｣縺ｯ隕ｪ繧ｰ繝ｪ繝・ラ縺ｸ縺ｮ蜿ら・繧定ｿ斐☆
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

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮ繝槭・繧ｸ縺輔ｌ縺溘そ繝ｫ繝・せ繝・
        [Theory]
        [InlineData(0, 0, 2, 3)] // 繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ(0,0)縺九ｉ2陦・蛻励・繝槭・繧ｸ
        [InlineData(1, 1, 1, 2)] // 繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ(1,1)縺九ｉ1陦・蛻励・繝槭・繧ｸ
        [InlineData(3, 2, 3, 1)] // 繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ(3,2)縺九ｉ3陦・蛻励・繝槭・繧ｸ
        public async Task GetGridItem_WithMergedCells_ShouldReportAnchorCellCoordinates(int anchorRow, int anchorColumn, int rowSpan, int columnSpan)
        {
            // Arrange - Microsoft莉墓ｧ・ 繝槭・繧ｸ縺輔ｌ縺溘そ繝ｫ縺ｯ繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ縺ｮ蠎ｧ讓吶ｒ蝣ｱ蜻翫☆繧・            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
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
        [InlineData(0, 1, 0, 0)] // 繝槭・繧ｸ遽・峇蜀・・(0,1)縺ｯ螳滄圀縺ｫ縺ｯ(0,0)縺ｮ繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ繧呈欠縺・        [InlineData(1, 0, 0, 0)] // 繝槭・繧ｸ遽・峇蜀・・(1,0)縺ｯ螳滄圀縺ｫ縺ｯ(0,0)縺ｮ繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ繧呈欠縺・        [InlineData(1, 1, 0, 0)] // 繝槭・繧ｸ遽・峇蜀・・(1,1)縺ｯ螳滄圀縺ｫ縺ｯ(0,0)縺ｮ繧｢繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ繧呈欠縺・        public async Task GetGridItem_WithCellsInMergeRange_ShouldReturnAnchorCellProperties(int requestRow, int requestColumn, int anchorRow, int anchorColumn)
        {
            // Arrange - Microsoft莉墓ｧ・ 繝槭・繧ｸ遽・峇蜀・・縺ｩ縺ｮ蠎ｧ讓吶〒繧ょ酔縺倥い繝ｳ繧ｫ繝ｼ繧ｻ繝ｫ縺ｮ繝励Ο繝代ユ繧｣繧定ｿ斐☆
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

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮ蠎ｧ讓咏ｳｻ繝・せ繝・
        [Fact]
        public async Task GetGridItem_WithZeroBasedCoordinates_ShouldReturnCorrectItem()
        {
            // Arrange - Microsoft莉墓ｧ・ 蠎ｧ讓咏ｳｻ縺ｯ0繝吶・繧ｹ縲∝ｷｦ荳翫′(0,0)
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
            // Arrange - Microsoft莉墓ｧ・ 霑斐＆繧後ｋRow/Column繝励Ο繝代ユ繧｣縺ｯ隕∵ｱゅ＆繧後◆蠎ｧ讓吶→荳閾ｴ縺吶ｋ蠢・ｦ√′縺ゅｋ
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

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮ萓句､門・逅・ユ繧ｹ繝・
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        public async Task GetGridItem_WithNegativeCoordinates_ShouldThrowArgumentOutOfRangeException(int row, int column)
        {
            // Arrange - Microsoft莉墓ｧ・ 雋縺ｮ蠎ｧ讓吶〒ArgumentOutOfRangeException繧偵せ繝ｭ繝ｼ
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
            // Arrange - Microsoft莉墓ｧ・ RowCount/ColumnCount繧定ｶ・∴繧句ｺｧ讓吶〒ArgumentOutOfRangeException繧偵せ繝ｭ繝ｼ
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

        #region GridItem繝励Ο繝代ユ繧｣縺ｮ謨ｴ蜷域ｧ繝・せ繝・
        [Fact]
        public async Task GetGridItem_WithValidItem_ShouldHaveConsistentProperties()
        {
            // Arrange - 縺吶∋縺ｦ縺ｮGridItem蠢・医・繝ｭ繝代ユ繧｣縺碁←蛻・↓險ｭ螳壹＆繧後※縺・ｋ縺薙→繧堤｢ｺ隱・            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
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
            // Arrange - RowSpan/ColumnSpan縺・莉･荳翫・譛牙柑縺ｪ蛟､縺ｧ縺ゅｋ縺薙→繧堤｢ｺ隱・            var expectedResult = new ServerEnhancedResponse<ElementSearchResult>
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

        #region 蠅・阜蛟､縺ｨ繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ繝・せ繝・
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

        #region 繝代Λ繝｡繝ｼ繧ｿ讀懆ｨｼ繝・せ繝・
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