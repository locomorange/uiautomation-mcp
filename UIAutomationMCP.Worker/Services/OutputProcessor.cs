using System.Text.Json;
using Microsoft.Extensions.Logging;
using UiAutomationWorker.Configuration;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// 出力データのシリアライゼーションと出力を担当するサービス
    /// </summary>
    public class OutputProcessor
    {
        private readonly ILogger<OutputProcessor> _logger;

        public OutputProcessor(ILogger<OutputProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 結果をJSON形式で標準出力に書き出します
        /// </summary>
        /// <param name="result">出力する結果</param>
        public async Task WriteResultAsync(object result)
        {
            try
            {
                var outputOptions = JsonSerializationConfig.GetOptions();
                var resultJson = JsonSerializer.Serialize(result, outputOptions);
                await Console.Out.WriteLineAsync(resultJson);
                
                _logger.LogInformation("[OutputProcessor] Result successfully written to output");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OutputProcessor] Error writing result to output");
                await Console.Error.WriteLineAsync($"Error writing result: {ex.Message}");
                throw;
            }
        }
    }
}
