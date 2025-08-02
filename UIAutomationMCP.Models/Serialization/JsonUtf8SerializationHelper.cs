using System.Text;
using System.Text.Json;

namespace UIAutomationMCP.Models.Serialization
{
    /// <summary>
    /// JSON-UTF-8 serialization helper for internal subprocess communication
    /// Provides UTF-8 byte array serialization to solve Japanese character encoding issues
    /// while maintaining JSON format compatibility and AOT support
    /// 
    /// This helper leverages the existing JsonSerializationHelper and UIAutomationJsonContext
    /// for complete type support and consistency.
    /// </summary>
    public static class JsonUtf8SerializationHelper
    {
        /// <summary>
        /// Serialize an object to UTF-8 encoded JSON byte array using existing JsonSerializationHelper
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>UTF-8 encoded JSON byte array</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for serialization</exception>
        /// <exception cref="InvalidOperationException">Thrown when serialization fails</exception>
        public static byte[] SerializeToUtf8Bytes<T>(T obj) where T : notnull
        {
            try
            {
                // Use existing JsonSerializationHelper for type-safe serialization
                var json = JsonSerializationHelper.Serialize(obj);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize object of type {typeof(T).Name} to UTF-8 JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserialize UTF-8 encoded JSON byte array to an object using existing JsonSerializationHelper
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="utf8JsonBytes">The UTF-8 encoded JSON byte array</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for deserialization</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails</exception>
        public static T? DeserializeFromUtf8Bytes<T>(byte[] utf8JsonBytes) where T : notnull
        {
            try
            {
                if (utf8JsonBytes == null || utf8JsonBytes.Length == 0)
                    throw new ArgumentException("UTF-8 JSON data cannot be null or empty");

                // Convert UTF-8 bytes to string and use existing JsonSerializationHelper
                var json = Encoding.UTF8.GetString(utf8JsonBytes);
                return JsonSerializationHelper.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                var jsonString = Encoding.UTF8.GetString(utf8JsonBytes);
                throw new InvalidOperationException($"Failed to deserialize UTF-8 JSON data to type {typeof(T).Name}. JSON: {jsonString}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserialize UTF-8 encoded JSON byte array to an object (ReadOnlyMemory version)
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="utf8JsonBytes">The UTF-8 encoded JSON byte array</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for deserialization</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails</exception>
        public static T? DeserializeFromUtf8Bytes<T>(ReadOnlyMemory<byte> utf8JsonBytes) where T : notnull
        {
            try
            {
                if (utf8JsonBytes.IsEmpty)
                    throw new ArgumentException("UTF-8 JSON data cannot be empty");

                // Convert UTF-8 bytes to string and use existing JsonSerializationHelper
                var json = Encoding.UTF8.GetString(utf8JsonBytes.Span);
                return JsonSerializationHelper.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                var jsonString = Encoding.UTF8.GetString(utf8JsonBytes.Span);
                throw new InvalidOperationException($"Failed to deserialize UTF-8 JSON data to type {typeof(T).Name}. JSON: {jsonString}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Serialize an object to JSON string using existing JsonSerializationHelper (convenience method)
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>JSON string</returns>
        public static string SerializeToString<T>(T obj) where T : notnull
        {
            return JsonSerializationHelper.Serialize(obj);
        }

        /// <summary>
        /// Deserialize JSON string to an object using existing JsonSerializationHelper (convenience method)
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="json">The JSON string</param>
        /// <returns>The deserialized object</returns>
        public static T? DeserializeFromString<T>(string json) where T : notnull
        {
            return JsonSerializationHelper.Deserialize<T>(json);
        }
    }
}