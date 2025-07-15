using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace UIAutomationMCP.Shared.Configuration
{
    /// <summary>
    /// AOT-compatible configuration binding helpers
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Get value with default fallback
        /// </summary>
        public static T GetValue<T>(this IConfigurationSection section, string key, T defaultValue)
        {
            var value = section[key];
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Bind configuration section to object using reflection-free approach
        /// </summary>
        public static void Bind(this IConfigurationSection section, object target, params (string Key, Action<object, string> Setter)[] bindings)
        {
            if (!section.Exists()) return;

            foreach (var (key, setter) in bindings)
            {
                var value = section[key];
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        setter(target, value);
                    }
                    catch
                    {
                        // Ignore conversion errors
                    }
                }
            }
        }

        /// <summary>
        /// Create typed property setter
        /// </summary>
        public static Action<object, string> Setter<T>(Action<T> propertySetter)
        {
            return (target, value) =>
            {
                if (target is T typedTarget && TryParse<T>(value, out var parsedValue))
                {
                    propertySetter(parsedValue);
                }
            };
        }

        /// <summary>
        /// Create simple property setter
        /// </summary>
        public static Action<object, string> CreateSetter<T>(Action<T> setter)
        {
            return (_, value) =>
            {
                if (TryParse<T>(value, out var parsedValue))
                {
                    setter(parsedValue);
                }
            };
        }

        /// <summary>
        /// Generic parsing with fallback
        /// </summary>
        private static bool TryParse<T>(string value, out T result)
        {
            result = default(T)!;
            
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                var type = typeof(T);
                
                // Handle nullable types
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                
                if (underlyingType == typeof(string))
                {
                    result = (T)(object)value;
                    return true;
                }
                
                if (underlyingType == typeof(bool))
                {
                    if (bool.TryParse(value, out var boolResult))
                    {
                        result = (T)(object)boolResult;
                        return true;
                    }
                }
                
                if (underlyingType == typeof(int))
                {
                    if (int.TryParse(value, out var intResult))
                    {
                        result = (T)(object)intResult;
                        return true;
                    }
                }
                
                if (underlyingType == typeof(double))
                {
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
                    {
                        result = (T)(object)doubleResult;
                        return true;
                    }
                }
                
                // Fallback to Convert.ChangeType
                result = (T)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}