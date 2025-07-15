using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// FindElements検索結果のキャッシュサービス
    /// </summary>
    public class FindElementsCacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
        private readonly int _maxCacheSize = 100;

        /// <summary>
        /// キャッシュから結果を取得
        /// </summary>
        public List<ElementInfo>? GetFromCache(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    entry.LastAccessed = DateTime.UtcNow;
                    return entry.Elements;
                }
                else
                {
                    // 期限切れのエントリを削除
                    _cache.TryRemove(cacheKey, out _);
                }
            }
            
            return null;
        }

        /// <summary>
        /// キャッシュに結果を保存
        /// </summary>
        public void AddToCache(string cacheKey, List<ElementInfo> elements, TimeSpan? customExpiration = null)
        {
            // キャッシュサイズ制限をチェック
            if (_cache.Count >= _maxCacheSize)
            {
                RemoveOldestEntries();
            }

            var expiration = customExpiration ?? _defaultExpiration;
            var entry = new CacheEntry
            {
                Elements = new List<ElementInfo>(elements), // コピーを作成
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };

            _cache.AddOrUpdate(cacheKey, entry, (key, existing) => entry);
        }

        /// <summary>
        /// 検索パラメータからキャッシュキーを生成
        /// </summary>
        public string GenerateCacheKey(Dictionary<string, object>? parameters)
        {
            if (parameters == null) return "";

            // キー生成に使用するパラメータ（変動しやすいものは除外）
            var keyParams = new Dictionary<string, object>();
            var includeKeys = new[] 
            { 
                "searchText", "controlType", "windowTitle", "processId", 
                "className", "helpText", "acceleratorKey", "accessKey",
                "patternType", "searchMethod", "scope", "conditionOperator",
                "excludeText", "excludeControlType"
            };

            foreach (var key in includeKeys)
            {
                if (parameters.TryGetValue(key, out var value) && value != null)
                {
                    keyParams[key] = value;
                }
            }

            var json = JsonSerializationHelper.SerializeObject(keyParams);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 期限切れエントリを削除
        /// </summary>
        public void RemoveExpiredEntries()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _cache
                .Where(kvp => kvp.Value.ExpiresAt <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 最も古いエントリを削除
        /// </summary>
        private void RemoveOldestEntries()
        {
            var entriesToRemove = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .Take(_cache.Count / 4) // 25%を削除
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in entriesToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        private class CacheEntry
        {
            public List<ElementInfo> Elements { get; set; } = new();
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}