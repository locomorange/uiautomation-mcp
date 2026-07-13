using Xunit;

namespace UiAutomationMcp.Tests.Infrastructure
{
    /// <summary>
    /// テストコレクション定義 - テストクラス間でフィクスチャを共有
    /// </summary>
    [CollectionDefinition("UIAutomationTestCollection")]
    public class UIAutomationTestCollection : ICollectionFixture<UIAutomationTestFixture>
    {
        // Collection marker class
    }

    /// <summary>
    /// テストフィクスチャ - テストコレクションの共有状態を管理
    /// 
    /// WindowsJobObject (JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE) が SubprocessExecutor に統合されたため、
    /// Worker プロセスは親プロセスの終了時に OS によって自動的に終了される。
    /// クラッシュシナリオでも孤児プロセスは発生しないため、手動のプロセスクリーンアップは不要。
    /// </summary>
    public class UIAutomationTestFixture : IDisposable
    {
        public UIAutomationTestFixture()
        {
            // WindowsJobObject により Worker プロセスのライフサイクルは自動管理されるため、
            // 手動の GC.Collect やプロセスクリーンアップは不要
        }

        public void Dispose()
        {
            // WindowsJobObject が KILL_ON_JOB_CLOSE で Worker を自動終了するため、
            // 明示的なクリーンアップは不要
        }
    }
}

