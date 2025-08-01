using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// 謫堺ｽ懊↓髢｢騾｣縺吶ｋ繝ｭ繧ｰ繧貞庶髮・☆繧九・繝ｫ繝代・繧ｯ繝ｩ繧ｹ
    /// 迚ｹ螳壹・謫堺ｽ廬D縺ｫ髢｢騾｣縺吶ｋ繝ｭ繧ｰ繧ｨ繝ｳ繝医Μ繧定ｿｽ霍｡縺励√Ξ繧ｹ繝昴Φ繧ｹ縺ｫ蜷ｫ繧√ｋ縺溘ａ縺ｫ菴ｿ逕ｨ
    /// </summary>
    public class OperationLogCollector
    {
        private readonly ConcurrentDictionary<string, List<string>> _operationLogs = new();
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// 謫堺ｽ懊↓髢｢騾｣縺吶ｋ繝ｭ繧ｰ繧ｨ繝ｳ繝医Μ繧定ｿｽ蜉
        /// </summary>
        /// <param name="operationId">謫堺ｽ懊・荳諢剰ｭ伜挨蟄・/param>
        /// <param name="logMessage">繝ｭ繧ｰ繝｡繝・そ繝ｼ繧ｸ</param>
        public void AddLog(string operationId, string logMessage)
        {
            if (string.IsNullOrEmpty(operationId) || string.IsNullOrEmpty(logMessage))
                return;
                
            lock (_lockObject)
            {
                if (!_operationLogs.ContainsKey(operationId))
                {
                    _operationLogs[operationId] = new List<string>();
                }
                
                var timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                _operationLogs[operationId].Add($"[{timestamp}] {logMessage}");
                
                // 繝｡繝｢繝ｪ繝ｪ繝ｼ繧ｯ髦ｲ豁｢縺ｮ縺溘ａ縲∝商縺・Ο繧ｰ繧ｨ繝ｳ繝医Μ繧貞宛髯・                if (_operationLogs[operationId].Count > 100)
                {
                    _operationLogs[operationId] = _operationLogs[operationId].TakeLast(50).ToList();
                }
            }
        }
        
        /// <summary>
        /// 謫堺ｽ懊↓髢｢騾｣縺吶ｋ繝ｭ繧ｰ繧ｨ繝ｳ繝医Μ繧貞叙蠕・        /// </summary>
        /// <param name="operationId">謫堺ｽ懊・荳諢剰ｭ伜挨蟄・/param>
        /// <returns>繝ｭ繧ｰ繧ｨ繝ｳ繝医Μ縺ｮ繝ｪ繧ｹ繝・/returns>
        public List<string> GetLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return new List<string>();
                
            lock (_lockObject)
            {
                if (_operationLogs.TryGetValue(operationId, out var logs))
                {
                    return new List<string>(logs);
                }
                
                return new List<string>();
            }
        }
        
        /// <summary>
        /// 謫堺ｽ懊・繝ｭ繧ｰ繧ｨ繝ｳ繝医Μ繧偵け繝ｪ繧｢縺励※縲√Γ繝｢繝ｪ繧定ｧ｣謾ｾ
        /// </summary>
        /// <param name="operationId">謫堺ｽ懊・荳諢剰ｭ伜挨蟄・/param>
        public void ClearLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return;
                
            lock (_lockObject)
            {
                _operationLogs.TryRemove(operationId, out _);
            }
        }
        
        /// <summary>
        /// 蜿､縺・桃菴懊・繝ｭ繧ｰ繧偵け繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ壽悄螳溯｡梧耳螂ｨ・・        /// </summary>
        /// <param name="maxAgeMinutes">菫晄戟縺吶ｋ譛螟ｧ譎る俣・亥・・・/param>
        public void CleanupOldLogs(int maxAgeMinutes = 30)
        {
            lock (_lockObject)
            {
                // 螳溯｣・ｒ邁｡邏蛹悶☆繧九◆繧√√☆縺ｹ縺ｦ縺ｮ繝ｭ繧ｰ繧偵け繝ｪ繧｢
                // 螳滄圀縺ｮ驕狗畑縺ｧ縺ｯ縲√ち繧､繝繧ｹ繧ｿ繝ｳ繝励・繝ｼ繧ｹ縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺悟ｿ・ｦ・                if (_operationLogs.Count > 1000)
                {
                    _operationLogs.Clear();
                }
            }
        }
    }
    
    /// <summary>
    /// OperationLogCollector繧剃ｽｿ逕ｨ縺吶ｋ縺溘ａ縺ｮ繝倥Ν繝代・繝｡繧ｽ繝・ラ
    /// </summary>
    public static class LogCollectorExtensions
    {
        private static readonly OperationLogCollector _collector = new();
        
        /// <summary>
        /// 繧ｰ繝ｭ繝ｼ繝舌Ν縺ｪ繝ｭ繧ｰ繧ｳ繝ｬ繧ｯ繧ｿ繝ｼ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ
        /// </summary>
        public static OperationLogCollector Instance => _collector;
        
        /// <summary>
        /// ILogger繧呈僑蠑ｵ縺励※縲∵桃菴廬D縺ｨ蜈ｱ縺ｫ繝ｭ繧ｰ繧定ｨ倬鹸
        /// </summary>
        /// <param name="logger">繝ｭ繧ｬ繝ｼ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</param>
        /// <param name="operationId">謫堺ｽ廬D</param>
        /// <param name="logLevel">繝ｭ繧ｰ繝ｬ繝吶Ν</param>
        /// <param name="message">繝ｭ繧ｰ繝｡繝・そ繝ｼ繧ｸ</param>
        public static void LogWithOperation(this ILogger logger, string operationId, LogLevel logLevel, string message)
        {
            logger.Log(logLevel, message);
            _collector.AddLog(operationId, $"{logLevel}: {message}");
        }
        
        /// <summary>
        /// 諠・ｱ繝ｭ繧ｰ繧呈桃菴廬D縺ｨ蜈ｱ縺ｫ險倬鹸
        /// </summary>
        public static void LogInformationWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogInformation(message);
            _collector.AddLog(operationId, $"INFO: {message}");
        }
        
        /// <summary>
        /// 繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ繧呈桃菴廬D縺ｨ蜈ｱ縺ｫ險倬鹸
        /// </summary>
        public static void LogErrorWithOperation(this ILogger logger, string operationId, Exception ex, string message)
        {
            logger.LogError(ex, message);
            _collector.AddLog(operationId, $"ERROR: {message} - {ex.Message}");
        }
        
        /// <summary>
        /// 隴ｦ蜻翫Ο繧ｰ繧呈桃菴廬D縺ｨ蜈ｱ縺ｫ險倬鹸
        /// </summary>
        public static void LogWarningWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogWarning(message);
            _collector.AddLog(operationId, $"WARN: {message}");
        }
    }
}
