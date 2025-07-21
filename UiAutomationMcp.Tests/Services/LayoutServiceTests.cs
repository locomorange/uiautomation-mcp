using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Services
{
    /// <summary>
    /// LayoutService単体テスト - ScrollElementIntoViewを含むLayoutService機能のテスト
    /// Mock使用により安全にUIAutomation依存を回避
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class LayoutServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<LayoutService>> _mockLogger;
        private readonly Mock<ILogger<SubprocessExecutor>> _mockExecutorLogger;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly LayoutService _layoutService;

        public LayoutServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<LayoutService>>();
            _mockExecutorLogger = new Mock<ILogger<SubprocessExecutor>>();
            
            // Create a SubprocessExecutor with a non-existent worker path for unit testing
            // This will cause operations to fail predictably, which is what we want for unit tests
            _subprocessExecutor = new SubprocessExecutor(_mockExecutorLogger.Object, "mock-worker-path.exe");
            
            _layoutService = new LayoutService(_mockLogger.Object, _subprocessExecutor);
        }

        #region ScrollElementIntoView Tests

        /// <summary>
        /// ScrollElementIntoViewAsync - エラーハンドリング：ワーカープロセスが見つからない場合
        /// SubprocessExecutorが適切にエラーを処理することをテスト
        /// </summary>
        [Fact]
        public async Task ScrollElementIntoViewAsync_Should_Handle_Worker_Not_Found_Error()
        {
            // Arrange
            var elementId = "test-scroll-element";
            var windowTitle = "Test Window";
            var processId = 1234;
            var timeoutSeconds = 1; // 短いタイムアウトで高速テスト

            // Act - 存在しないワーカーパスでの実行
            var result = await _layoutService.ScrollElementIntoViewAsync(automationId: elementId, processId: processId, timeoutSeconds: timeoutSeconds);

            // Assert - エラーが適切に処理されることを確認
            Assert.NotNull(result);
            
            // JSON形式で結果を検証
            var jsonResult = System.Text.Json.JsonSerializer.Serialize(result);
            _output.WriteLine($"Result JSON: {jsonResult}");
            
            // 結果をdynamic型でチェック
            if (result.GetType().GetProperty("Success") != null)
            {
                var success = result.GetType().GetProperty("Success")?.GetValue(result);
                Assert.False((bool?)success);
                
                var error = result.GetType().GetProperty("Error")?.GetValue(result);
                Assert.NotNull(error);
                
                _output.WriteLine($"ScrollElementIntoView error handling test completed: {error}");
            }
            else
            {
                _output.WriteLine($"Result type: {result.GetType().Name}");
                Assert.NotNull(result); // At minimum, we should get some result
            }
        }

        /// <summary>
        /// ScrollElementIntoViewAsync - デフォルトパラメータでの動作テスト
        /// </summary>
        [Fact]
        public async Task ScrollElementIntoViewAsync_Should_Handle_Default_Parameters()
        {
            // Arrange
            var elementId = "test-element";

            // Act - デフォルトパラメータでの実行
            var result = await _layoutService.ScrollElementIntoViewAsync(elementId);

            // Assert - デフォルトパラメータが適切に処理されることを確認
            Assert.NotNull(result);
            
            // 結果の検証（型安全な方法）
            if (result.GetType().GetProperty("Success") != null)
            {
                var success = result.GetType().GetProperty("Success")?.GetValue(result);
                Assert.False((bool?)success);
                
                var error = result.GetType().GetProperty("Error")?.GetValue(result);
                _output.WriteLine($"Default parameters test completed: {error}");
            }
            else
            {
                _output.WriteLine($"Result type: {result.GetType().Name}");
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// ScrollElementIntoViewAsync - ログ記録の確認
        /// エラー時にログが適切に記録されることをテスト
        /// </summary>
        [Fact]
        public async Task ScrollElementIntoViewAsync_Should_Log_Error_When_Operation_Fails()
        {
            // Arrange
            var elementId = "non-scrollable-element";

            // Act - 失敗する操作を実行
            var result = await _layoutService.ScrollElementIntoViewAsync(elementId);

            // Assert
            Assert.NotNull(result);
            
            // 結果の検証
            if (result.GetType().GetProperty("Success") != null)
            {
                var success = result.GetType().GetProperty("Success")?.GetValue(result);
                Assert.False((bool?)success);
                
                var error = result.GetType().GetProperty("Error")?.GetValue(result);
                _output.WriteLine($"Error logging test completed: {error}");
            }

            // エラーログが記録されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to scroll element into view")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// ScrollElementIntoViewAsync - 異なるパラメータでの動作確認
        /// </summary>
        [Theory]
        [InlineData("element1", "Window1", 1234)]
        [InlineData("element2", "", 0)]
        [InlineData("", "Window2", 5678)]
        public async Task ScrollElementIntoViewAsync_Should_Handle_Various_Parameters(string elementId, string windowTitle, int processId)
        {
            // Act - 様々なパラメータでの実行
            var result = await _layoutService.ScrollElementIntoViewAsync(automationId: elementId, processId: processId, timeoutSeconds: 1);

            // Assert - パラメータが適切に処理されることを確認
            Assert.NotNull(result);
            
            // 結果の検証
            if (result.GetType().GetProperty("Success") != null)
            {
                var success = result.GetType().GetProperty("Success")?.GetValue(result);
                Assert.False((bool?)success); // ワーカーが見つからないため失敗するが、パラメータ処理は正常
            }
            
            _output.WriteLine($"Parameter test completed for elementId:'{elementId}', processId:{processId}");
        }

        #endregion

        public void Dispose()
        {
            try
            {
                _subprocessExecutor?.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Disposal warning: {ex.Message}");
            }
            
            // テストクリーンアップ
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}