using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Serialization
{
    /// <summary>
    /// Provides centralized JSON serialization configuration for the UIAutomationMCP project.
    /// </summary>
    public static class JsonSerializationConfig
    {
        /// <summary>
        /// Gets the standard JSON serializer options used throughout the application.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // Changed to false for single-line output
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}