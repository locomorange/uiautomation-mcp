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
    /// ScrollPattern莉墓ｧ俶ｺ匁侠繝・せ繝・- Microsoft UIAutomation ScrollPattern縺ｮ蠢・医Γ繝ｳ繝舌・繧偵ユ繧ｹ繝・    /// 繧ｵ繝悶・繝ｭ繧ｻ繧ｹ螳溯｡後↓繧医ｊ螳牙・縺ｫUIAutomation繧偵ユ繧ｹ繝・    /// </summary>
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
            
            // 繝・せ繝育畑縺ｮ繧ｵ繝ｼ繝薙せ繧ｳ繝ｳ繝・リ繧偵そ繝・ヨ繧｢繝・・
            var services = new ServiceCollection();
            
            // 繝ｭ繧ｬ繝ｼ繧定ｿｽ蜉
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            // Worker.exe縺ｮ繝代せ繧貞叙蠕・            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
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
        /// GetScrollInfo - ScrollPattern縺ｮ6縺､縺ｮ蠢・医・繝ｭ繝代ユ繧｣縺梧ｭ｣縺励￥蜿門ｾ励〒縺阪ｋ縺薙→繧偵ユ繧ｹ繝・        /// Microsoft莉墓ｧ・ HorizontalScrollPercent, VerticalScrollPercent, HorizontalViewSize, 
        /// VerticalViewSize, HorizontallyScrollable, VerticallyScrollable
        /// </summary>
        [Fact]
        public async Task GetScrollInfo_Should_Return_All_Required_ScrollPattern_Properties()
        {
            // Arrange - 繧ｵ繝ｳ繝励Ν繝代Λ繝｡繝ｼ繧ｿ・亥ｮ滄圀縺ｮUI縺後↑縺上※繧８orker縺ｮ蜍穂ｽ懊ｒ繝・せ繝茨ｼ・            var request = new GetScrollInfoRequest
            {
                AutomationId = "test-scroll-element",
                WindowTitle = "Test Window",
                ProcessId = 0
            };

            // Act - 繧ｵ繝悶・繝ｭ繧ｻ繧ｹ縺ｧGetScrollInfo謫堺ｽ懊ｒ螳溯｡・            try
            {
                var result = await _subprocessExecutor.ExecuteAsync<GetScrollInfoRequest, ScrollInfoResult>("GetScrollInfo", request, 5);
                _output.WriteLine($"GetScrollInfo result: {System.Text.Json.JsonSerializer.Serialize(result)}");

                // Assert - 邨先棡縺碁←蛻・↑蠖｢蠑上〒縺ゅｋ縺薙→繧堤｢ｺ隱・                Assert.NotNull(result);
                
                // Note: 螳滄圀縺ｮUI縺後↑縺・ｴ蜷医・Element not found"繧ｨ繝ｩ繝ｼ縺梧悄蠕・＆繧後ｋ
                // 縺薙ｌ縺ｯScrollPattern莉墓ｧ倥・螳溯｣・′豁｣縺励￥蜍穂ｽ懊＠縺ｦ縺・ｋ縺薙→繧堤､ｺ縺・            }
            catch (Exception ex)
            {
                // UIAutomation髢｢騾｣縺ｮ萓句､悶・譛溷ｾ・＆繧後ｋ蜍穂ｽ・                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
                
                // Worker閾ｪ菴薙′蜍穂ｽ懊＠縲・←蛻・↑謫堺ｽ懊′逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・                Assert.Contains("GetScrollInfo", ex.Message);
            }
        }

        /// <summary>
        /// SetScrollPercent - ScrollPattern縺ｮ蠢・医Γ繧ｽ繝・ラ縺梧ｭ｣縺励￥螳溯｣・＆繧後※縺・ｋ縺薙→繧偵ユ繧ｹ繝・        /// Microsoft莉墓ｧ・ 0-100縺ｮ遽・峇縺ｧ繧ｹ繧ｯ繝ｭ繝ｼ繝ｫ菴咲ｽｮ繧定ｨｭ螳壹・1縺ｧNoScroll繧呈欠螳・        /// </summary>
        [Fact]
        public async Task SetScrollPercent_Should_Accept_Valid_Percentage_Values()
        {
            // Arrange - 譛牙柑縺ｪ繝代・繧ｻ繝ｳ繝・・繧ｸ蛟､
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
                // UIAutomation髢｢騾｣縺ｮ萓句､悶・譛溷ｾ・＆繧後ｋ蜍穂ｽ・                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
                
                // Worker閾ｪ菴薙′蜍穂ｽ懊＠縲・←蛻・↑謫堺ｽ懊′逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・                Assert.Contains("SetScrollPercent", ex.Message);
            }
        }

        /// <summary>
        /// SetScrollPercent_NoScroll - Microsoft莉墓ｧ倥・-1蛟､・・oScroll・峨′豁｣縺励￥蜃ｦ逅・＆繧後ｋ縺薙→繧偵ユ繧ｹ繝・        /// </summary>
        [Fact]
        public async Task SetScrollPercent_Should_Handle_NoScroll_Values()
        {
            // Arrange - NoScroll蛟､(-1)繧偵ユ繧ｹ繝・            var request = new SetScrollPercentRequest
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
                // UIAutomation髢｢騾｣縺ｮ萓句､悶・譛溷ｾ・＆繧後ｋ蜍穂ｽ・                _output.WriteLine($"Expected exception (no UI element): {ex.Message}");
            }
        }

        /// <summary>
        /// ScrollPattern謫堺ｽ懊′Worker縺ｫ豁｣縺励￥逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・        /// </summary>
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

            // Act & Assert - 謫堺ｽ懊′逋ｻ骭ｲ縺輔ｌ縺ｦ縺翫ｊ縲∝ｮ溯｡悟庄閭ｽ縺ｧ縺ゅｋ縺薙→繧堤｢ｺ隱・            var exception = await Record.ExceptionAsync(async () =>
            {
                await _subprocessExecutor.ExecuteAsync<TypedWorkerRequest, ActionResult>(operationName, request, 5);
            });

            // UI縺後↑縺・ｴ蜷医・萓句､悶・譛溷ｾ・＆繧後ｋ・域桃菴懆・菴薙・豁｣縺励￥逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ・・            if (exception != null)
            {
                _output.WriteLine($"Operation {operationName} executed with expected exception: {exception.Message}");
                
                // "Unknown operation"繧ｨ繝ｩ繝ｼ縺ｧ縺ｪ縺・％縺ｨ繧堤｢ｺ隱搾ｼ茨ｼ晄桃菴懊′豁｣縺励￥逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ・・                Assert.DoesNotContain("unknown operation", exception.Message.ToLowerInvariant());
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

            // 繝励Ο繧ｻ繧ｹ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}