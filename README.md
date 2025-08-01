# win-UIAutomation-mcp

Windows UI Automation MCP (Model Context Protocol) Server - Microsoft Learn ガイドライン準拠

このプロジェクトは、Windows UI Automation 機能を MCP プロトコル経由で公開する C# サーバーです。Microsoft Learn の [Control Pattern Mapping for UI Automation Clients](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/control-pattern-mapping-for-ui-automation-clients) ガイドラインに基づいて実装されています。


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
- **System.Windows.Automation** - Windows UI Automation API（ワーカープロセス専用）
- **ModelContextProtocol** - MCP サーバー実装
- **Microsoft.Extensions.Hosting** - ホスティングとDI
- **プロセス間通信** - JSON-RPC via stdin/stdout



### 最適化された設計原則

1. **分離されたプロセス**: MCPプロトコル処理とUI Automation実行の完全分離
2. **冗長性の排除**: サーバー側からSystem.Windows.Automationを削除、ワーカー側に統合
3. **責任の分離**: 各パターン実行がワーカー側で専用エグゼキューターを使用
4. **依存性注入**: 全コンポーネントがDIコンテナで管理
5. **テスト可能性**: 各コンポーネントが独立してテスト可能（72テストすべて通過）
6. **保守性**: 機能別にファイルを分割し、理解しやすい構造

#