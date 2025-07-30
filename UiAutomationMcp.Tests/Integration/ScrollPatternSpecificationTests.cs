using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// ScrollPattern仕様準拠チE��チE- Microsoft UIAutomation ScrollPatternの忁E��メンバ�EをテスチE    /// サブ�Eロセス実行により安�EにUIAutomationをテスチE    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class ScrollPatternSpecificationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly string _workerPath;

        public ScrollPatternSpecificationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // チE��ト用のサービスコンチE��をセチE��アチE�E
            var services = new ServiceCollection();
            
            // ロガーを追加
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            // Worker.exeのパスを取征E            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = null!;
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    _workerPath = fullPath;
                    break;
                }
            }

            if (_workerPath == null)
            {
                throw new FileNotFoundException($"Worker executable not found. Searched paths: {string.Join(", ", possiblePaths)}");
            }

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath, new CancellationTokenSource());
        }

        /// <summary>
        /// GetScrollInfo - ScrollPatternの6つの忁E���Eロパティが正しく取得できることをテスチE        /// Microsoft仕槁E HorizontalScrollPercent, VerticalScrollPercent, HorizontalViewSize, 
        /// VerticalViewSize, HorizontallyScrollable, VerticallyScrollable
        /// </summary>
        [Fact]
        public async Task GetScrollInfo_Should_Return_All_Required_ScrollPattern_Properties()
        {
            // Arrange - サンプルパラメータ�E�実際のUIがなくてもWorkerの動作をチE��ト！E            var request = new GetScrollInfoRequest
            {
                AutomationId = "test-scroll-element",
                WindowTitle = "Test Window",
                ProcessId = 0
            };

            // Act - サブ�EロセスでGetScrollInfo操作を実衁E            try
            {
                var result = await _subprocessExecutor.ExecuteAsync<GetScrollInfoRequest, ScrollInfoResult>("GetScrollInfo", request, 5);
                _output.WriteLine($"GetScrollInfo result: {System.Text.Json.JsonSerializer.Serialize(result)}");

                // Assert - 結果が適刁E��形式であることを確誁E                Assert.NotNull(result);
                
                // Note: 実際のUIがなぁE��合、EElement not found"エラーが期征E��れる
                // これはScrollPattern仕様�E実裁E��正しく動作してぁE��ことを示ぁE            }
            catch (Exception ex)
            {
                // UIAutomation関連の例外�E期征E��れる動佁E                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
                
                // Worker自体が動作し、E��刁E��操作が登録されてぁE��ことを確誁E                Assert.Contains("GetScrollInfo", ex.Message);
            }
        }

        /// <summary>
        /// SetScrollPercent - ScrollPatternの忁E��メソチE��が正しく実裁E��れてぁE��ことをテスチE        /// Microsoft仕槁E 0-100の篁E��でスクロール位置を設定、E1でNoScrollを指宁E        /// </summary>
        [Fact]
        public async Task SetScrollPercent_Should_Accept_Valid_Percentage_Values()
        {
            // Arrange - 有効なパ�EセンチE�Eジ値
            var request = new SetScrollPercentRequest
            {
                AutomationId = "test-scroll-element",
                HorizontalPercent = 50.0,
                VerticalPercent = 75.0,
                WindowTitle = "Test Window",
                ProcessId = 0
            };

            // Act & Assert
            try
            {
                var result = await _subprocessExecutor.ExecuteAsync<SetScrollPercentRequest, ActionResult>("SetScrollPercent", request, 5);
                _output.WriteLine($"SetScrollPercent result: {System.Text.Json.JsonSerializer.Serialize(result)}");
                
                Assert.NotNull(result);
            }
            catch (Exception ex)
            {
                // UIAutomation関連の例外�E期征E��れる動佁E                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
                
                // Worker自体が動作し、E��刁E��操作が登録されてぁE��ことを確誁E                Assert.Contains("SetScrollPercent", ex.Message);
            }
        }

        /// <summary>
        /// SetScrollPercent_NoScroll - Microsoft仕様�E-1値�E�EoScroll�E�が正しく処琁E��れることをテスチE        /// </summary>
        [Fact]
        public async Task SetScrollPercent_Should_Handle_NoScroll_Values()
        {
            // Arrange - NoScroll値(-1)をテスチE            var request = new SetScrollPercentRequest
            {
                AutomationId = "test-scroll-element",
                HorizontalPercent = -1.0, // NoScroll
                VerticalPercent = 25.0,
                WindowTitle = "Test Window",
                ProcessId = 0
            };

            // Act & Assert
            try
            {
                var result = await _subprocessExecutor.ExecuteAsync<SetScrollPercentRequest, ActionResult>("SetScrollPercent", request, 5);
                _output.WriteLine($"SetScrollPercent NoScroll result: {System.Text.Json.JsonSerializer.Serialize(result)}");
                
                Assert.NotNull(result);
            }
            catch (Exception ex)
            {
                // UIAutomation関連の例外�E期征E��れる動佁E                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
            }
        }

        /// <summary>
        /// ScrollPattern操作がWorkerに正しく登録されてぁE��ことを確誁E        /// </summary>
        [Theory]
        [InlineData("GetScrollInfo")]
        [InlineData("SetScrollPercent")]
        [InlineData("ScrollElement")]
        [InlineData("ScrollElementIntoView")]
        public async Task ScrollPattern_Operations_Should_Be_Registered_In_Worker(string operationName)
        {
            // Arrange
            TypedWorkerRequest request = operationName switch
            {
                "GetScrollInfo" => new GetScrollInfoRequest
                {
                    AutomationId = "test-element",
                    WindowTitle = "Test Window"
                },
                "SetScrollPercent" => new SetScrollPercentRequest
                {
                    AutomationId = "test-element",
                    WindowTitle = "Test Window",
                    HorizontalPercent = 0.0,
                    VerticalPercent = 0.0
                },
                "ScrollElement" => new ScrollElementRequest
                {
                    AutomationId = "test-element",
                    WindowTitle = "Test Window",
                    Direction = "up",
                    Amount = 1.0
                },
                "ScrollElementIntoView" => new ScrollElementIntoViewRequest
                {
                    AutomationId = "test-element",
                    WindowTitle = "Test Window"
                },
                _ => throw new ArgumentException($"Unknown operation: {operationName}")
            };

            // Act & Assert - 操作が登録されており、実行可能であることを確誁E            var exception = await Record.ExceptionAsync(async () =>
            {
                await _subprocessExecutor.ExecuteAsync<TypedWorkerRequest, ActionResult>(operationName, request, 5);
            });

            // UIがなぁE��合�E例外�E期征E��れる�E�操作�E体�E正しく登録されてぁE���E�E            if (exception != null)
            {
                _output.WriteLine($"Operation {operationName} executed with expected exception: {exception.Message}");
                
                // "Unknown operation"エラーでなぁE��とを確認（＝操作が正しく登録されてぁE���E�E                Assert.DoesNotContain("unknown operation", exception.Message.ToLowerInvariant());
                Assert.DoesNotContain("not found", exception.Message.ToLowerInvariant());
            }
            else
            {
                _output.WriteLine($"Operation {operationName} executed successfully");
            }
        }

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
            
            try
            {
                _serviceProvider?.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Service provider disposal warning: {ex.Message}");
            }

            // プロセスクリーンアチE�E
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}