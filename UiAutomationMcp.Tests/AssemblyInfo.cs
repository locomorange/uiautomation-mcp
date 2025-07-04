using Xunit;

// テストの並列実行を無効にして、プロセス競合を防ぐ
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]

// テスト実行時間を制限
[assembly: TestCaseOrderer("Xunit.Sdk.DefaultTestCaseOrderer", "xunit.core")]
