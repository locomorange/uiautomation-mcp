using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Core.Infrastructure
{
    /// <summary>
    /// Registry for managing operation types and creating instances
    /// </summary>
    public class OperationRegistry
    {
        private readonly Dictionary<string, Type> _operations = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OperationRegistry> _logger;

        public OperationRegistry(IServiceProvider serviceProvider, ILogger<OperationRegistry> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Register an operation type
        /// </summary>
        public void RegisterOperation<T>(string operationName) where T : class
        {
            _operations[operationName] = typeof(T);
            _logger.LogDebug("Registered operation: {OperationName} -> {OperationType}", operationName, typeof(T).Name);
        }

        /// <summary>
        /// Register all operations from an assembly by naming convention
        /// </summary>
        public void RegisterOperationsFromAssembly(Assembly assembly, string suffix = "Operation")
        {
            var operationTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith(suffix) && !t.IsAbstract && t.IsClass)
                .ToList();

            foreach (var type in operationTypes)
            {
                var operationName = type.Name.Replace(suffix, "");
                _operations[operationName] = type;
                _logger.LogDebug("Auto-registered operation: {OperationName} -> {OperationType}", operationName, type.Name);
            }

            _logger.LogInformation("Registered {Count} operations from assembly {AssemblyName}", operationTypes.Count, assembly.GetName().Name);
        }

        /// <summary>
        /// Create an operation instance
        /// </summary>
        public T CreateOperation<T>(string operationName) where T : class
        {
            if (!_operations.TryGetValue(operationName, out var operationType))
            {
                throw new InvalidOperationException($"Operation '{operationName}' is not registered");
            }

            if (!typeof(T).IsAssignableFrom(operationType))
            {
                throw new InvalidOperationException($"Operation '{operationName}' is not of type {typeof(T).Name}");
            }

            var instance = _serviceProvider.GetRequiredService(operationType) as T;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create operation instance for '{operationName}'");
            }

            return instance;
        }

        /// <summary>
        /// Check if an operation is registered
        /// </summary>
        public bool IsOperationRegistered(string operationName)
        {
            return _operations.ContainsKey(operationName);
        }

        /// <summary>
        /// Get all registered operation names
        /// </summary>
        public IEnumerable<string> GetRegisteredOperations()
        {
            return _operations.Keys;
        }
    }
}