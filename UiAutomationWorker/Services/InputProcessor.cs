using System.Text.Json;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Configuration;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// 入力データの読み取りと解析を担当するサービス
    /// </summary>
    public class InputProcessor
    {
        private readonly ILogger<InputProcessor> _logger;

        public InputProcessor(ILogger<InputProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 標準入力からJSON データを読み取り、WorkerOperation にデシリアライズします
        /// </summary>
        /// <returns>解析されたWorkerOperation、または null（失敗時）</returns>
        public async Task<WorkerOperation?> ReadAndParseInputAsync()
        {
            try
            {
                // Read operation data from stdin
                var inputJson = await Console.In.ReadToEndAsync();
                
                if (string.IsNullOrEmpty(inputJson))
                {
                    _logger.LogError("[InputProcessor] No input data received");
                    await Console.Error.WriteLineAsync("No input data received");
                    return null;
                }

                _logger.LogInformation("[InputProcessor] Processing operation: {InputLength} chars", inputJson.Length);

                // Parse operation with UTF-8 encoding support
                var options = JsonSerializationConfig.GetInputOptions();
                var operation = JsonSerializer.Deserialize<WorkerOperation>(inputJson, options);
                
                if (operation == null)
                {
                    _logger.LogError("[InputProcessor] Failed to parse operation JSON");
                    await Console.Error.WriteLineAsync("Failed to parse operation JSON");
                    return null;
                }

                return operation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InputProcessor] Error reading or parsing input");
                await Console.Error.WriteLineAsync($"Error reading or parsing input: {ex.Message}");
                return null;
            }
        }
    }
}
