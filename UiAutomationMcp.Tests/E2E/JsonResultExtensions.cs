using System.Text.Json;

namespace UIAutomationMCP.Tests.E2E
{
    /// <summary>
    /// MCP ツールは AOT 互換のためシリアライズ済み JSON 文字列 (camelCase) を返す。
    /// テストからツール結果を JsonElement として扱うための変換ヘルパー。
    /// </summary>
    internal static class JsonResultExtensions
    {
        public static JsonElement ToJsonElement(this object? result)
        {
            if (result is string json)
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.Clone();
            }
            return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// プロパティ名の casing (camelCase/PascalCase) に依存しない TryGetProperty。
        /// </summary>
        public static bool TryGetPropertyCI(this JsonElement element, string name, out JsonElement value)
        {
            value = default;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
