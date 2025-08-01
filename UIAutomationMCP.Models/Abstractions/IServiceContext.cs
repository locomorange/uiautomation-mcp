using System.Diagnostics;

namespace UIAutomationMCP.Models.Abstractions
{
    /// <summary>
    /// Interface for service execution context
    /// </summary>
    public interface IServiceContext
    {
        /// <summary>
        /// Unique operation identifier
        /// </summary>
        string OperationId { get; }
        
        /// <summary>
        /// Method name that initiated the operation
        /// </summary>
        string MethodName { get; }
        
        /// <summary>
        /// Timeout in seconds
        /// </summary>
        int TimeoutSeconds { get; }
        
        /// <summary>
        /// Operation start time
        /// </summary>
        DateTime StartedAt { get; }
        
        /// <summary>
        /// Stopwatch for measuring execution time
        /// </summary>
        Stopwatch Stopwatch { get; }
    }
    
    /// <summary>
    /// Default implementation of service context
    /// </summary>
    public class ServiceContext : IServiceContext
    {
        public string OperationId { get; }
        public string MethodName { get; }
        public int TimeoutSeconds { get; }
        public DateTime StartedAt { get; }
        public Stopwatch Stopwatch { get; }
        
        public ServiceContext(string methodName, int timeoutSeconds)
        {
            OperationId = Guid.NewGuid().ToString();
            MethodName = methodName;
            TimeoutSeconds = timeoutSeconds;
            StartedAt = DateTime.UtcNow;
            Stopwatch = Stopwatch.StartNew();
        }
    }
}