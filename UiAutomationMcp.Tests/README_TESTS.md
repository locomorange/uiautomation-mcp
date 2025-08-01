# UIAutomationMCP Tests

このディレクトリには、UIAutomationMCPプロジェクトの安全なテストが含まれています。

## テスト安全性ポリシー

**重要**: UIAutomationのworker実行はハングする可能性があるため、すべてのテストは以下のいずれかの方法で実装されています：

### ✅ 安全なテスト方法

1. **サブプロセス実行**: `SubprocessExecutor`を使用してWorkerを別プロセスで実行
2. **UIAutomationのモック**: `Mock<T>`を使用して実際のUIAutomation呼び出しを回避
3. **純粋なロジックテスト**: UIAutomationに依存しない単純なロジックのテスト
4. **インターフェースのモック**: 具象クラスではなくインターフェースをモック化

### ❌ 危険な方法（使用禁止）

- `System.Windows.Automation`の直接使用・インポート
- `ElementFinderService`などのUIAutomation依存サービスの直接インスタンス化
- 実際のUIAutomationAPIの同期呼び出し
- `AutomationElement`、`TablePattern`等の具象クラスの直接使用
- Workerプロセス以外でのUIAutomation操作の実行

## テストカテゴリ

### 1. Unit Tests (`Trait("Category", "Unit")`)
単一のクラスやメソッドの動作を検証するテスト。外部依存関係はすべてモック化。

#### テスト対象
- **ビジネスロジック**: パラメータ検証、データ変換、エラーハンドリング
- **サービス層**: リクエスト構築、レスポンス処理、ログ出力
- **データモデル**: シリアライゼーション、型変換、バリデーション
- **ユーティリティ**: ヘルパーメソッド、共通処理

#### 特徴
- 実行時間: 100ms以内
- 外部依存: なし（すべてモック）
- テスト範囲: 単一メソッド/クラス
- 並列実行: 可能

### 2. Integration Tests (`Trait("Category", "Integration")`)
複数のコンポーネント間の連携を検証するテスト。サブプロセス実行を使用。

#### テスト対象
- **プロセス間通信**: Server-Worker間のメッセージング
- **タイムアウト処理**: プロセスハング時の強制終了
- **エラー伝播**: Worker側エラーのServer側での処理
- **リソース管理**: プロセスのライフサイクル管理

#### 特徴
- 実行時間: 30秒以内（タイムアウト込み）
- 外部依存: Workerプロセス
- テスト範囲: コンポーネント間連携
- 並列実行: 制限あり

### 3. E2E Tests (`Trait("Category", "E2E")`)
実際のアプリケーションに対する完全なシナリオテスト。

#### テスト対象
- **実際のUI操作**: ボタンクリック、テキスト入力、要素選択
- **UIパターンの動作**: 各パターンの実アプリケーションでの振る舞い
- **エンドツーエンドシナリオ**: ユーザーストーリーの完全な実行
- **パフォーマンス**: 実環境での応答時間

#### 特徴
- 実行時間: 数分程度
- 外部依存: 実際のアプリケーション
- テスト範囲: システム全体
- 並列実行: 不可（環境依存）

## テストコレクション

すべてのテストは`UIAutomationTestCollection`コレクションに属しており、以下の利点があります：

- プロセスのクリーンアップが確実に実行される
- ガベージコレクションによるメモリ管理
- テスト間のリソース競合を回避

## テスト実行確認済み状況

✅ **クリーンアップ完了**: 2025年1月16日
- 危険なUIAutomation依存テスト: 完全削除
- 新しい安全なテスト: 15個すべて成功
- プロセス管理: 強化されたクリーンアップ実装
- 並列実行: 安全性を優先した単一スレッド実行

## テスト実行方法

```bash
# すべてのテストを実行
dotnet test

# ユニットテストのみ実行（推奨）
dotnet test --filter "Category=Unit"

# 統合テストのみ実行
dotnet test --filter "Category=Integration"

# E2Eテストのみ実行（環境依存のため注意）
dotnet test --filter "Category=E2E"

# 特定のパターンのテストを実行
dotnet test --filter "FullyQualifiedName~ParameterValidation"
```

## テスト追加時の注意事項

新しいテストを追加する際は、必ず以下を確認してください：

### 共通ルール
1. **安全性の確保**
   - UIAutomationの直接使用を避ける
   - ハング可能性のある処理はサブプロセスで実行
   - 適切なタイムアウト設定

### 2. **テストの品質基準**
   - **外部依存の排除**: UIAutomation APIに依存しない設計
   - **決定論的動作**: 同じ入力で常に同じ結果を返す
   - **独立性**: 他のテストの実行状況に影響されない
   - **高速実行**: 100ms以内での完了を目標
   - **明確な意図**: テスト名と実装から目的が明確に理解できる

3. **テスト構造**
   - 適切な`[Collection("UIAutomationTestCollection")]`属性を追加
   - `IDisposable`を実装してリソースをクリーンアップ
   - 明確な命名規則: `メソッド名_期待される動作_条件`
   - AAA(Arrange-Act-Assert)パターンの厳守

## プロセス管理とクリーンアップ

### 自動プロセス管理
- すべてのテストは`UIAutomationTestCollection`で実行される
- テスト開始時と終了時に自動的にプロセスクリーンアップが実行される
- 予期しないプロセス残存を防ぐため、強制終了機能を搭載

### リソース解放ガイドライン
1. **必須**: すべてのテストクラスで`IDisposable`を実装
2. **推奨**: `using`文またはtry-finallyでの確実なリソース解放
3. **必須**: モックオブジェクトの適切な解放

### カテゴリ別ルール

#### Unit Tests
- `[Trait("Category", "Unit")]`属性を追加
- すべての外部依存関係をモック化
- 単一責任の原則に従う
- 実行時間は100ms以内を目標
- 例: サービスのリクエスト構築ロジック、パラメータ検証

#### Integration Tests
- `[Trait("Category", "Integration")]`属性を追加
- サブプロセス実行を使用
- タイムアウトを適切に設定（通常30秒）
- プロセス終了の確実な処理
- 例: Server-Worker間の通信、エラー伝播

#### E2E Tests
- `[Trait("Category", "E2E")]`属性を追加
- 実際のアプリケーションを使用
- **重要**: テスト中に実際のアプリを起動した場合は、必ずテスト終了時に終了させること
- テスト前後の環境クリーンアップを確実に実行
- 他のテストに影響しないよう独立性を保つ
- 例: 実際のUIコントロールの操作、ユーザーシナリオ

### テストの構造

```csharp
[Collection("UIAutomationTestCollection")]
[Trait("Category", "Unit")] // または "Integration", "E2E"
public class MyPatternTests : IDisposable
{
    // Setup
    public MyPatternTests()
    {
        // 初期化処理
    }

    [Fact]
    public async Task MyMethod_ShouldDoSomething_WhenCondition()
    {
        // Arrange
        // Act
        // Assert
    }

    public void Dispose()
    {
        // クリーンアップ処理
    }
}
```

条件を満たさないテストは削除される可能性があります。