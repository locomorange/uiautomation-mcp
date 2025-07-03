# win-UIAutomation-mcp

Windows UI Automation MCP (Model Context Protocol) Server - Microsoft Learn ガイドライン準拠

このプロジェクトは、Windows UI Automation 機能を MCP プロトコル経由で公開する C# サーバーです。Microsoft Learn の [Control Pattern Mapping for UI Automation Clients](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/control-pattern-mapping-for-ui-automation-clients) ガイドラインに基づいて実装されています。

## サポートされている UI Automation パターン

### 基本パターン（推奨実装）
- **InvokePattern** - ボタン、メニュー項目の実行
- **ValuePattern** - テキストボックスの値設定/取得
- **TogglePattern** - チェックボックスの状態変更
- **SelectionItemPattern** - リスト項目の選択
- **ExpandCollapsePattern** - ツリー項目の展開/折りたたみ

### 高度なパターン
- **ScrollPattern** - スクロール可能要素の操作
- **ScrollItemPattern** - 項目をビューにスクロール
- **RangeValuePattern** - スライダー、プログレスバーの値操作
- **TextPattern** - テキスト操作と選択
- **WindowPattern** - ウィンドウの状態変更（最小化、最大化、閉じる）
- **TransformPattern** - 要素の移動、リサイズ、回転
- **DockPattern** - ドッキング可能要素の位置設定

### 新規追加パターン（Microsoft Learn ガイドライン準拠）
- **MultipleViewPattern** - 複数ビューの切り替え
- **VirtualizedItemPattern** - 仮想化された項目の実現
- **ItemContainerPattern** - コンテナ内の項目検索
- **SynchronizedInputPattern** - 同期入力処理

### 計画中のパターン
- **GridPattern** - データグリッドの操作
- **TablePattern** - テーブルの操作
- **SelectionPattern** - 複数選択要素の操作

## 使用方法

### MCP クライアントからの利用

```json
{
  "tool": "ExecuteElementPattern",
  "arguments": {
    "elementId": "myButton",
    "patternName": "invoke",
    "windowTitle": "My Application"
  }
}
```

### パターン固有パラメータの例

#### ValuePattern
```json
{
  "patternName": "value",
  "parameters": {
    "value": "入力するテキスト"
  }
}
```

#### ExpandCollapsePattern
```json
{
  "patternName": "expandcollapse",
  "parameters": {
    "expand": true
  }
}
```

#### TransformPattern
```json
{
  "patternName": "transform",
  "parameters": {
    "action": "move",
    "x": 100,
    "y": 200
  }
}
```

#### MultipleViewPattern
```json
{
  "patternName": "multipleview",
  "parameters": {
    "viewId": 1
  }
}
```

## Microsoft Learn ガイドラインとの適合性

このプロジェクトは、Microsoft の公式ドキュメントに記載されているコントロールパターンマッピングに準拠しています：

| コントロールタイプ | 必須パターン | 条件付きパターン | 非サポート |
|---|---|---|---|
| Button | None | Invoke, Toggle, ExpandCollapse | None |
| CheckBox | Toggle | None | None |
| ComboBox | ExpandCollapse | Selection, Value | Scroll |
| Edit | None | Text, RangeValue, Value | None |
| List | None | Grid, MultipleView, Scroll, Selection | Table |

詳細は [Microsoft Learn のドキュメント](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/control-pattern-mapping-for-ui-automation-clients) を参照してください。

## ビルドと実行

```powershell
# プロジェクトのビルド
dotnet build UiAutomationMcpServer.csproj --verbosity quiet --nologo

# サーバーの実行
.\bin\Debug\net8.0-windows\UiAutomationMcpServer.exe
```

## 技術仕様

- **.NET 8.0** - Windows専用フレームワーク
- **System.Windows.Automation** - Windows UI Automation API
- **ModelContextProtocol** - MCP サーバー実装
- **Microsoft.Extensions.Hosting** - ホスティングとDI

## ライセンス

MIT License - 詳細は [LICENSE](LICENSE) ファイルを参照してください。