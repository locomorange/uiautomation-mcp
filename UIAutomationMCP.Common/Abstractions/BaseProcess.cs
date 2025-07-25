using System.Text;
using System.Text.Json;

namespace UIAutomationMCP.Common.Abstractions
{
    /// <summary>
    /// Base class for process communication via stdin/stdout
    /// </summary>
    public abstract class BaseProcess
    {
        /// <summary>
        /// Process a JSON request and return JSON response
        /// </summary>
        protected abstract Task<string> ProcessRequestAsync(string request);

        /// <summary>
        /// Main process loop - reads from stdin, processes, writes to stdout
        /// </summary>
        public async Task RunAsync()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                string? line;
                while ((line = await Console.In.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var response = await ProcessRequestAsync(line);
                        await Console.Out.WriteLineAsync(response);
                        await Console.Out.FlushAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = CreateErrorResponse(ex.Message);
                        await Console.Out.WriteLineAsync(errorResponse);
                        await Console.Out.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log to stderr to not interfere with stdout communication
                await Console.Error.WriteLineAsync($"Fatal error in process loop: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Create a standardized error response
        /// </summary>
        protected virtual string CreateErrorResponse(string error)
        {
            var errorObj = new
            {
                success = false,
                error,
                data = (object?)null
            };

            return JsonSerializer.Serialize(errorObj);
        }
    }
}