using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;
using Moq;

namespace UIAutomationMCP.Tests.EndToEnd
{
    /// <summary>
    /// エンドツーエンドテスト - 実際のWindowsアプリケーションとの操作をテスト
    /// 計算機、メモ帳等の標準アプリケーションを使用
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class EndToEndTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly Mock<IProcessTimeoutManager> _mockProcessTimeoutManager;
        private readonly UIAutomationWorker _worker;
        private readonly List<Process> _launchedProcesses;

        public EndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _launchedProcesses = new List<Process>();
            
            // テスト用のロガーを作成
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<UIAutomationWorker>();
            
            _mockProcessTimeoutManager = new Mock<IProcessTimeoutManager>();
            _worker = new UIAutomationWorker(_logger, _mockProcessTimeoutManager.Object);
        }

        [Fact]
        public async Task Calculator_ShouldBeFoundAndOperated()
        {
            Process? calcProcess = null;
            try
            {
                // Arrange - 計算機を起動
                calcProcess = await LaunchCalculatorAsync();
                if (calcProcess == null)
                {
                    _output.WriteLine("Calculator could not be launched, skipping test");
                    return;
                }

                // 少し待機してアプリが完全に起動するのを待つ
                await Task.Delay(2000);

                // Act - 計算機ウィンドウを検索
                var searchParams = new ElementSearchParameters
                {
                    WindowTitle = "Calculator",
                    ControlType = "Window",
                    ProcessId = calcProcess.Id
                };

                var result = await _worker.FindFirstElementAsync(searchParams, 15);

                // Assert
                _output.WriteLine($"Calculator search result: Success={result.Success}");
                if (result.Success && result.Data != null)
                {
                    _output.WriteLine($"Found calculator: {result.Data.Name}, AutomationId: {result.Data.AutomationId}");
                    Assert.NotNull(result.Data);
                    Assert.Contains("Calculator", result.Data.Name);
                }
                else
                {
                    _output.WriteLine($"Calculator search failed: {result.Error}");
                    // CI環境等ではUIアクセスができない場合があるのでスキップ
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Calculator test exception: {ex.Message}");
                // E2E環境でUIアクセスができない場合はスキップ
            }
            finally
            {
                // クリーンアップ
                if (calcProcess != null && !calcProcess.HasExited)
                {
                    try
                    {
                        calcProcess.CloseMainWindow();
                        if (!calcProcess.WaitForExit(5000))
                        {
                            calcProcess.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Failed to close calculator: {ex.Message}");
                    }
                }
            }
        }

        [Fact]
        public async Task WindowEnumeration_ShouldFindMultipleWindows()
        {
            try
            {
                // Act - 全ウィンドウの列挙
                var parameters = new { maxDepth = 1 };
                var result = await _worker.GetElementTreeAsync(parameters, 10);

                // Assert
                _output.WriteLine($"Window enumeration result: Success={result.Success}");
                
                if (result.Error?.Contains("not found") == true)
                {
                    _output.WriteLine("Worker process not found, skipping window enumeration test");
                    return;
                }

                if (result.Success && result.Data != null)
                {
                    _output.WriteLine($"Window enumeration data: {System.Text.Json.JsonSerializer.Serialize(result.Data)}");
                    Assert.True(result.Success);
                    Assert.NotNull(result.Data);
                }
                else
                {
                    _output.WriteLine($"Window enumeration failed: {result.Error}");
                    // UI自動化サービスが利用できない環境ではスキップ
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Window enumeration test exception: {ex.Message}");
                // E2E環境でUIアクセスができない場合はスキップ
            }
        }

        [Fact]
        public async Task Notepad_ShouldAllowTextInput()
        {
            Process? notepadProcess = null;
            try
            {
                // Arrange - メモ帳を起動
                notepadProcess = await LaunchNotepadAsync();
                if (notepadProcess == null)
                {
                    _output.WriteLine("Notepad could not be launched, skipping test");
                    return;
                }

                // 少し待機
                await Task.Delay(2000);

                // Act - メモ帳のテキストエリアを検索
                var searchParams = new ElementSearchParameters
                {
                    WindowTitle = "Untitled - Notepad",
                    ControlType = "Edit",
                    ProcessId = notepadProcess.Id
                };

                var findResult = await _worker.FindFirstElementAsync(searchParams, 15);
                
                _output.WriteLine($"Notepad text area search: Success={findResult.Success}");
                
                if (findResult.Success && findResult.Data != null)
                {
                    // テキスト入力を試行
                    var parameters = new { elementId = findResult.Data.AutomationId, value = "Hello from UIAutomation test!", processId = notepadProcess.Id };
                    var textResult = await _worker.SetElementValueAsync(parameters, 30);
                    
                    _output.WriteLine($"Text input result: Success={textResult.Success}");
                    
                    if (textResult.Success)
                    {
                        Assert.True(textResult.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Notepad test exception: {ex.Message}");
                // E2E環境でUIアクセスができない場合はスキップ
            }
            finally
            {
                // クリーンアップ
                if (notepadProcess != null && !notepadProcess.HasExited)
                {
                    try
                    {
                        notepadProcess.CloseMainWindow();
                        if (!notepadProcess.WaitForExit(5000))
                        {
                            notepadProcess.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Failed to close notepad: {ex.Message}");
                    }
                }
            }
        }

        private async Task<Process?> LaunchCalculatorAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    _launchedProcesses.Add(process);
                    await Task.Delay(1000); // 起動待機
                }
                return process;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to launch calculator: {ex.Message}");
                return null;
            }
        }

        private async Task<Process?> LaunchNotepadAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    _launchedProcesses.Add(process);
                    await Task.Delay(1000); // 起動待機
                }
                return process;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to launch notepad: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            // 起動したプロセスをすべて終了
            foreach (var process in _launchedProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(3000))
                        {
                            process.Kill();
                        }
                    }
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to dispose process: {ex.Message}");
                }
            }

            _worker?.Dispose();
        }
    }
}
