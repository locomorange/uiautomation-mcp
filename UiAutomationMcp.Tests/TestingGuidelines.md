# Testing Guidelines for UI Automation MCP Server

## Testing Strategy Overview

### 1. Unit Tests (単体テスト)
**対象**: 純粋な業務ロジック
**実行**: 高速、CI/CD、開発中
**ツール**: xUnit + Moq（必要に応じて）

```csharp
[Trait("Category", "Unit")]
public class ParameterValidationTests
{
    [Fact]
    public void ValidateElementId_WithEmptyString_ShouldReturnFalse()
    {
        // 純粋なロジックのテスト
    }
}
```

### 2. Server Layer Tests (サーバー層テスト)
**対象**: MCPプロトコル処理、UIAutomationToolsクラス
**実行**: 高速、MockUIAutomationWorkerを使用
**目的**: プロトコル変換、エラーハンドリング、レスポンス形式

```csharp
[Trait("Category", "ServerLayer")]
public class UIAutomationToolsTests
{
    private readonly UIAutomationTools _tools;
    private readonly MockUIAutomationWorker _mockWorker;

    [Fact]
    public async Task InvokeElement_ShouldReturnProperMcpResponse()
    {
        // MCPプロトコルのテスト - MockWorkerを使用
        var result = await _tools.InvokeElement("btn1", "TestWindow");
        Assert.NotNull(result);
        // レスポンス形式の検証
    }
}
```

### 3. Integration Tests (統合テスト)  
**対象**: 実際のUI Automation操作
**実行**: 遅い、CI環境またはローカル
**要件**: 実際のテストアプリケーション

```csharp
[Trait("Category", "Integration")]
public class RealUIAutomationTests : IClassFixture<TestApplicationFixture>
{
    [Fact]
    public async Task InvokeButton_OnRealApp_ShouldTriggerAction()
    {
        // 実際のアプリケーションでのテスト
        // MockUIAutomationWorkerは使用しない
    }
}
```

## Moq vs 手動モック実装

### ✅ Moqを使用すべき理由

1. **簡潔性**: 300行の手動実装 → 数行のセットアップ
2. **柔軟性**: テストごとに異なる動作を簡単に設定
3. **保守性**: インターフェース変更時の自動追従
4. **検証機能**: メソッド呼び出しの検証が簡単
5. **標準的**: 業界標準のモックライブラリ

### Moqの基本的な使用例

```csharp
[Fact]
public async Task UIAutomationTools_InvokeElement_ShouldReturnValidMcpResponse()
{
    // Arrange - Moqで簡潔にセットアップ
    var mockWorker = new Mock<IUIAutomationWorker>();
    mockWorker
        .Setup(w => w.InvokeElementAsync("btn1", null, null, 20))
        .ReturnsAsync(new OperationResult<string> { Success = true, Data = "Clicked" });

    // Act
    var result = await _uiAutomationTools.InvokeElement("btn1");
    
    // Assert - 呼び出し検証も簡単
    Assert.NotNull(result);
    mockWorker.Verify(w => w.InvokeElementAsync("btn1", null, null, 20), Times.Once);
}
```

### エラーケースのテスト

```csharp
[Fact]
public async Task UIAutomationTools_WithWorkerError_ShouldReturnErrorResponse()
{
    // エラーシナリオも簡単に設定
    _mockWorker
        .Setup(w => w.InvokeElementAsync("nonexistent", null, null, 20))
        .ReturnsAsync(new OperationResult<string> { Success = false, Error = "Element not found" });
              
    var result = await _uiAutomationTools.InvokeElement("nonexistent");
    // エラーレスポンスの検証
}
```

### パラメータマッチングの例

```csharp
[Fact]
public async Task FlexibleParameterMatching()
{
    // 柔軟なパラメータマッチング
    mockWorker
        .Setup(w => w.SetElementValueAsync(
            It.IsAny<string>(),                    // 任意の要素ID
            It.Is<string>(v => v.Length > 0),      // 空でない値
            It.IsAny<string>(),                    // 任意のウィンドウタイトル
            It.IsAny<int?>(),                      // 任意のプロセスID
            It.IsAny<int>()))                      // 任意のタイムアウト
        .ReturnsAsync(new OperationResult<string> { Success = true });
}
```

### ❌ 不適切な使用例

1. **UI Automation APIの動作テスト**
```csharp
// これは意味がない - 実際のUI操作をテストしていない
[Fact]
public async Task TextPattern_ShouldSelectText()
{
    // MockはUI Automation APIの実際の動作を再現できない
}
```

2. **PatternExecutorの内部ロジック**
```csharp
// これも意味がない - 実際のパターン実行をテストしていない
[Fact]
public async Task WindowPatternExecutor_ShouldSetWindowState()
{
    // 実際のWindowPatternの動作をテストできない
}
```

## テスト実行戦略

### 開発時
```bash
# 高速フィードバック - Unit + ServerLayer
dotnet test --filter "Category=Unit|Category=ServerLayer"
```

### PR前
```bash
# 包括的テスト
dotnet test --filter "Category!=Integration"
```

### CI/CD
```bash
# 統合テストも含む（テスト環境があれば）
dotnet test
```

## テストアプリケーション要件

統合テスト用の簡単なアプリケーション：

```csharp
// TestApp/MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // 予測可能なAutomationIdを設定
        testButton.SetValue(AutomationProperties.AutomationIdProperty, "TestButton");
        testTextBox.SetValue(AutomationProperties.AutomationIdProperty, "TestTextBox");
    }
}
```

## まとめ

- **MockUIAutomationWorker**: サーバー層のテストで有用、削除不要
- **PatternExecutorのモック**: 無意味、削除済み
- **統合テスト**: 実際のUI操作には不可欠
- **テスト戦略**: 目的に応じた適切な分離が重要