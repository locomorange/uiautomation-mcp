using System.Diagnostics;
using Xunit;

namespace UiAutomationMcp.Tests.Infrastructure
{
    /// <summary>
    /// テストコレクション - プロセスのクリーンアップを確実に行う
    /// </summary>
    [CollectionDefinition("UIAutomationTestCollection")]
    public class UIAutomationTestCollection : ICollectionFixture<UIAutomationTestFixture>
    {
        // このクラスは実装を持たず、テストコレクションの定義のみを行います
    }

    /// <summary>
    /// テストフィクスチャ - テスト実行の前後でリソースの管理を行う
    /// </summary>
    public class UIAutomationTestFixture : IDisposable
    {
        public UIAutomationTestFixture()
        {
            // テスト開始前の初期化
            CleanupWorkerProcesses();
            
            // 複数回のガベージコレクションでリソース確実解放
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            // 短時間待機してプロセス終了を確実にする
            System.Threading.Thread.Sleep(500);
        }

        public void Dispose()
        {
            try
            {
                // 複数回のガベージコレクションでリソース確実解放
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                
                // 残っているWorkerプロセスを強制終了
                CleanupWorkerProcesses();
                
                // 最終的な待機
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fixture disposal error: {ex.Message}");
            }
        }
        
        private void CleanupWorkerProcesses()
        {
            try
            {
                // UIAutomationMCP.Worker プロセスを検索して終了
                var workerProcesses = Process.GetProcessesByName("UIAutomationMCP.Worker");
                foreach (var process in workerProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                            process.WaitForExit(2000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // 既に終了している場合は無視
                    }
                }
                
                // dotnet.exe プロセスで UIAutomationMCP.Worker.dll を実行しているものも検索
                var dotnetProcesses = Process.GetProcessesByName("dotnet");
                foreach (var process in dotnetProcesses)
                {
                    try
                    {
                        // コマンドラインに UIAutomationMCP.Worker が含まれているか確認
                        var commandLine = GetProcessCommandLine(process);
                        if (!string.IsNullOrEmpty(commandLine) && commandLine.Contains("UIAutomationMCP.Worker"))
                        {
                            if (!process.HasExited)
                            {
                                process.Kill(entireProcessTree: true);
                                process.WaitForExit(2000);
                            }
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // アクセス権限がない場合などは無視
                    }
                }
            }
            catch (Exception ex)
            {
                // クリーンアップエラーは無視（テストは続行）
                Console.WriteLine($"Worker process cleanup error: {ex.Message}");
            }
        }
        
        private string? GetProcessCommandLine(Process process)
        {
            try
            {
                // Windows Management Instrumentation を使用してコマンドラインを取得
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString();
                    }
                }
            }
            catch
            {
                // WMIアクセスエラーは無視
            }
            return null;
        }
    }
}
