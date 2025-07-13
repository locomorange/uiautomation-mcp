using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    /// <summary>
    /// E2Eテストの基底クラス - 適切なリソース管理を提供
    /// </summary>
    public abstract class BaseE2ETest : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly UIAutomationTools Tools;
        private bool _disposed;

        protected BaseE2ETest(ITestOutputHelper output)
        {
            Output = output;
            ServiceProvider = MCPToolsE2ETests.CreateServiceProvider();
            Tools = ServiceProvider.GetRequiredService<UIAutomationTools>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    // ServiceProviderのDisposeを確実に実行
                    if (ServiceProvider is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    // SubprocessExecutorが適切にDisposeされているか確認
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // 念のため残っているWorkerプロセスをクリーンアップ
                    CleanupRemainingWorkerProcesses();
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Cleanup error in {GetType().Name}: {ex.Message}");
                }
            }

            _disposed = true;
        }

        private void CleanupRemainingWorkerProcesses()
        {
            try
            {
                var workerProcesses = Process.GetProcessesByName("UIAutomationMCP.Worker");
                foreach (var process in workerProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            Output.WriteLine($"Cleaning up remaining Worker process PID: {process.Id}");
                            process.Kill(entireProcessTree: true);
                            process.WaitForExit(1000);
                        }
                        process.Dispose();
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}