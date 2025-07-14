using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Operations.Layout;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.UnitTests
{
    /// <summary>
    /// Microsoft ScrollItemPattern仕様準拠テスト - エッジケースとエラーハンドリング
    /// ScrollItemPattern.ScrollIntoView()の完全な仕様準拠を検証
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ScrollItemPatternSpecificationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinder;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;
        private readonly ScrollElementIntoViewOperation _operation;

        public ScrollItemPatternSpecificationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(new UIAutomationOptions());
            _operation = new ScrollElementIntoViewOperation(_mockElementFinder.Object, _mockOptions.Object);
        }

        #region Microsoft ScrollItemPattern Specification Compliance Tests

        /// <summary>
        /// Microsoft仕様準拠：ScrollItemPattern.ScrollIntoView()の基本動作確認
        /// 仕様: "Scrolls the content area of a container object in order to display the control within the visible region (viewport) of the container."
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Comply_With_Microsoft_Specification()
        {
            // Arrange - Microsoft仕様に準拠したScrollItemPatternの動作をテスト
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "scrollable-list-item" },
                    { "windowTitle", "Container Window" },
                    { "processId", "1234" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById("scrollable-list-item", "Container Window", 1234))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.GetCurrentPattern(ScrollItemPattern.Pattern))
                .Returns(mockScrollItemPattern.Object);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert - Microsoft仕様準拠確認
            Assert.True(result.Success);
            Assert.IsType<ScrollActionResult>(result.Data);
            var scrollResult = (ScrollActionResult)result.Data;
            Assert.True(scrollResult.Completed);
            Assert.Equal("ScrollIntoView", scrollResult.ActionName);
            
            // ScrollIntoView()が正確に1回呼ばれることを確認（Microsoft仕様）
            mockScrollItemPattern.Verify(x => x.ScrollIntoView(), Times.Once);
            
            _output.WriteLine("Microsoft ScrollItemPattern.ScrollIntoView() specification compliance verified");
        }

        /// <summary>
        /// Microsoft仕様準拠：InvalidOperationExceptionの適切な処理
        /// 仕様: "If an item cannot be scrolled into view, providers must throw an InvalidOperationException."
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Handle_InvalidOperationException_Per_Microsoft_Specification()
        {
            // Arrange - Microsoft仕様のInvalidOperationException処理をテスト
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "cannot-scroll-element" },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById("cannot-scroll-element", "Test Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.GetCurrentPattern(ScrollItemPattern.Pattern))
                .Returns(mockScrollItemPattern.Object);

            // Microsoft仕様：スクロール不可能な場合のInvalidOperationException
            mockScrollItemPattern
                .Setup(x => x.ScrollIntoView())
                .Throws(new InvalidOperationException("Element cannot be scrolled into view"));

            // Act
            var exception = await Record.ExceptionAsync(() => _operation.ExecuteAsync(request));

            // Assert - Microsoft仕様準拠エラーハンドリング
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Contains("Element cannot be scrolled into view", exception.Message);
            
            _output.WriteLine("Microsoft specification InvalidOperationException handling verified");
        }

        #endregion

        #region Control Type Specific Tests

        /// <summary>
        /// Microsoft仕様：ListItemコントロールタイプでのScrollItemPattern動作確認
        /// 仕様: ListItem controls are expected to support ScrollItemPattern (optional)
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Work_With_ListItem_Control_Type()
        {
            // Arrange - ListItemコントロールタイプでのテスト
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "listview-item-5" },
                    { "windowTitle", "ListView Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            // ListItemコントロールタイプをシミュレート
            mockElement.Setup(x => x.Current.ControlType).Returns(ControlType.ListItem);

            _mockElementFinder
                .Setup(x => x.FindElementById("listview-item-5", "ListView Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.GetCurrentPattern(ScrollItemPattern.Pattern))
                .Returns(mockScrollItemPattern.Object);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            mockScrollItemPattern.Verify(x => x.ScrollIntoView(), Times.Once);
            
            _output.WriteLine("ScrollItemPattern works correctly with ListItem control type");
        }

        /// <summary>
        /// Microsoft仕様：TreeItemコントロールタイプでのScrollItemPattern動作確認
        /// 仕様: TreeItem controls are expected to support ScrollItemPattern (optional)
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Work_With_TreeItem_Control_Type()
        {
            // Arrange - TreeItemコントロールタイプでのテスト
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "tree-node-deep-level" },
                    { "windowTitle", "TreeView Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            // TreeItemコントロールタイプをシミュレート
            mockElement.Setup(x => x.Current.ControlType).Returns(ControlType.TreeItem);

            _mockElementFinder
                .Setup(x => x.FindElementById("tree-node-deep-level", "TreeView Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.GetCurrentPattern(ScrollItemPattern.Pattern))
                .Returns(mockScrollItemPattern.Object);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            mockScrollItemPattern.Verify(x => x.ScrollIntoView(), Times.Once);
            
            _output.WriteLine("ScrollItemPattern works correctly with TreeItem control type");
        }

        /// <summary>
        /// Microsoft仕様：WindowコントロールまたはCanvasコントロール内の要素は
        /// ScrollItemPatternの実装が必須ではない
        /// 仕様: "Items contained within a Window or Canvas control are not required to implement the IScrollItemProvider interface"
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Handle_Window_Canvas_Container_Elements()
        {
            // Arrange - WindowまたはCanvas内の要素でScrollItemPatternが利用できない場合
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "canvas-child-element" },
                    { "windowTitle", "Canvas Container" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();

            _mockElementFinder
                .Setup(x => x.FindElementById("canvas-child-element", "Canvas Container", 0))
                .Returns(mockElement.Object);

            // Canvas内の要素はScrollItemPatternが必須ではない（Microsoft仕様）
            mockElement
                .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                .Returns((AutomationPattern pattern, out object patternObject) =>
                {
                    patternObject = null;
                    return false;
                });

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert - パターンが利用できない場合の適切なエラーメッセージ
            Assert.False(result.Success);
            Assert.Equal("Element does not support ScrollItemPattern", result.Error);
            
            _output.WriteLine("Correctly handled Window/Canvas container elements per Microsoft specification");
        }

        #endregion

        #region Edge Cases and Error Conditions

        /// <summary>
        /// エッジケース：極端に長い要素IDの処理
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Handle_Extremely_Long_Element_Id()
        {
            // Arrange - 極端に長い要素ID
            var veryLongElementId = new string('a', 10000); // 10,000文字の要素ID
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", veryLongElementId },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            _mockElementFinder
                .Setup(x => x.FindElementById(veryLongElementId, "Test Window", 0))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
            
            _output.WriteLine("Handled extremely long element ID correctly");
        }

        /// <summary>
        /// エッジケース：特殊文字を含む要素IDの処理
        /// </summary>
        [Theory]
        [InlineData("element<>&\"'")]
        [InlineData("元素_日本語_テスト")]
        [InlineData("element\u0000\u0001\u0002")]
        [InlineData("element\t\n\r")]
        public async Task ScrollIntoView_Should_Handle_Special_Characters_In_Element_Id(string specialElementId)
        {
            // Arrange - 特殊文字を含む要素ID
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", specialElementId },
                    { "windowTitle", "Test Window" },
                    { "processId", "0" }
                }
            };

            _mockElementFinder
                .Setup(x => x.FindElementById(specialElementId, "Test Window", 0))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
            
            _output.WriteLine($"Handled special characters in element ID correctly: {specialElementId}");
        }

        /// <summary>
        /// エッジケース：concurrent access pattern - 複数の同時ScrollIntoView呼び出し
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Handle_Concurrent_Calls()
        {
            // Arrange - 複数の同時呼び出し
            var tasks = new List<Task<OperationResult>>();
            
            for (int i = 0; i < 10; i++)
            {
                var elementId = $"concurrent-element-{i}";
                var request = new WorkerRequest
                {
                    Parameters = new Dictionary<string, object>
                    {
                        { "elementId", elementId },
                        { "windowTitle", "Test Window" },
                        { "processId", "0" }
                    }
                };

                var mockElement = new Mock<AutomationElement>();
                var mockScrollItemPattern = new Mock<ScrollItemPattern>();

                _mockElementFinder
                    .Setup(x => x.FindElementById(elementId, "Test Window", 0))
                    .Returns(mockElement.Object);

                mockElement
                    .Setup(x => x.TryGetCurrentPattern(ScrollItemPattern.Pattern, out It.Ref<object>.IsAny))
                    .Returns((AutomationPattern pattern, out object patternObject) =>
                    {
                        patternObject = mockScrollItemPattern.Object;
                        return true;
                    });

                tasks.Add(_operation.ExecuteAsync(request));
            }

            // Act - 全ての同時呼び出しを実行
            var results = await Task.WhenAll(tasks);

            // Assert - 全て成功することを確認
            Assert.All(results, result => Assert.True(result.Success));
            
            _output.WriteLine("Handled concurrent ScrollIntoView calls correctly");
        }

        /// <summary>
        /// エッジケース：メモリ制約下でのScrollItemPattern動作
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Handle_Memory_Pressure()
        {
            // Arrange - メモリプレッシャーをシミュレート
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "memory-test-element" },
                    { "windowTitle", "Memory Test Window" },
                    { "processId", "0" }
                }
            };

            var mockElement = new Mock<AutomationElement>();
            var mockScrollItemPattern = new Mock<ScrollItemPattern>();

            _mockElementFinder
                .Setup(x => x.FindElementById("memory-test-element", "Memory Test Window", 0))
                .Returns(mockElement.Object);

            mockElement
                .Setup(x => x.GetCurrentPattern(ScrollItemPattern.Pattern))
                .Returns(mockScrollItemPattern.Object);

            // メモリ不足をシミュレート
            mockScrollItemPattern
                .Setup(x => x.ScrollIntoView())
                .Throws(new OutOfMemoryException("Insufficient memory to complete operation"));

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _operation.ExecuteAsync(request));
            
            Assert.NotNull(exception);
            Assert.IsType<OutOfMemoryException>(exception);
            
            _output.WriteLine("Handled memory pressure condition correctly");
        }

        /// <summary>
        /// エッジケース：非常に大きなプロセスIDの処理
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task ScrollIntoView_Should_Handle_Edge_Case_Process_Ids(int processId)
        {
            // Arrange
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "test-element" },
                    { "windowTitle", "Test Window" },
                    { "processId", processId.ToString() }
                }
            };

            _mockElementFinder
                .Setup(x => x.FindElementById("test-element", "Test Window", processId))
                .Returns((AutomationElement)null);

            // Act
            var result = await _operation.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
            
            _output.WriteLine($"Handled edge case process ID correctly: {processId}");
        }

        #endregion

        #region Performance and Reliability Tests

        /// <summary>
        /// パフォーマンステスト：大量の要素に対するScrollIntoView実行時間測定
        /// </summary>
        [Fact]
        public async Task ScrollIntoView_Should_Complete_Within_Reasonable_Time()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 100;
            
            for (int i = 0; i < iterations; i++)
            {
                var request = new WorkerRequest
                {
                    Parameters = new Dictionary<string, object>
                    {
                        { "elementId", $"perf-test-element-{i}" },
                        { "windowTitle", "Performance Test Window" },
                        { "processId", "0" }
                    }
                };

                var mockElement = new Mock<AutomationElement>();
                var mockScrollItemPattern = new Mock<ScrollItemPattern>();

                _mockElementFinder
                    .Setup(x => x.FindElementById($"perf-test-element-{i}", "Performance Test Window", 0))
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
            }

            stopwatch.Stop();
            
            // パフォーマンス要件: 100回の実行が5秒以内に完了すること
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Performance test failed: {iterations} operations took {stopwatch.ElapsedMilliseconds}ms");
            
            _output.WriteLine($"Performance test passed: {iterations} operations completed in {stopwatch.ElapsedMilliseconds}ms");
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