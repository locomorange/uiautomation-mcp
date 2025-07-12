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
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Dispose()
        {
            // テスト完了後のクリーンアップ
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
