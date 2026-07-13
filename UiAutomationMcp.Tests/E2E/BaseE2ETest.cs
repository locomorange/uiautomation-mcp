using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    /// <summary>
    /// E2E テストの基底クラス - 共通のサービスプロバイダとツール初期化を提供
    /// 
    /// WindowsJobObject により Worker プロセスは自動管理されるため、
    /// 手動の GC.Collect やプロセスクリーンアップは不要。
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
                    if (ServiceProvider is IDisposable serviceProviderDisposable)
                    {
                        serviceProviderDisposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Cleanup error in {GetType().Name}: {ex.Message}");
                }
            }

            _disposed = true;
        }
    }
}

