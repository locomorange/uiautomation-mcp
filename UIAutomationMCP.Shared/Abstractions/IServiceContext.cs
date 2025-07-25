using System.Diagnostics;

namespace UIAutomationMCP.Core.Abstractions
{
    /// <summary>
    /// Service execution context for tracking operation state
    /// </summary>
    public interface IServiceContext
    {
        string OperationId { get; }
        string MethodName { get; }
        Stopwatch Stopwatch { get; }
        DateTime StartedAt { get; }
        int TimeoutSeconds { get; }
    }
    
    /// <summary>
    /// Default implementation of service context
    /// </summary>
    public sealed class ServiceContext : IServiceContext
    {
        public string OperationId { get; }
        public string MethodName { get; }
        public Stopwatch Stopwatch { get; }
        public DateTime StartedAt { get; }
        public int TimeoutSeconds { get; }
        
        public ServiceContext(string methodName, int timeoutSeconds)
        {
            OperationId = Guid.NewGuid().ToString("N")[..8];
            MethodName = methodName;
            TimeoutSeconds = timeoutSeconds;
            StartedAt = DateTime.UtcNow;
            Stopwatch = Stopwatch.StartNew();
        }
    }
}