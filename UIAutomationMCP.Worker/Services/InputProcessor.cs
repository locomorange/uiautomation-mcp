using System.Text.Json;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Configuration;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// Service responsible for reading and parsing input data from Server process
    /// </summary>
    public class InputProcessor
    {
        private readonly ILogger<InputProcessor> _logger;

        public InputProcessor(ILogger<InputProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reads JSON data from stdin and deserializes to WorkerOperation
        /// </summary>
        /// <returns>Parsed WorkerOperation, or null on failure</returns>
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
                var options = JsonSerializationConfig.GetOptions();
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
