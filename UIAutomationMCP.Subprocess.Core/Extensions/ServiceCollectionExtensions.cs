using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Models.Requests;

namespace UIAutomationMCP.Subprocess.Core.Extensions
{
    /// <summary>
    /// Extension methods for operation registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register an operation with automatic operation name extraction from Request type
        /// </summary>
        public static IServiceCollection AddOperation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TOperation, TRequest>(
            this IServiceCollection services)
            where TOperation : class, IUIAutomationOperation
            where TRequest : TypedWorkerRequest, new()
        {
            // Create instance of request to get operation name
            var request = new TRequest();
            var operationName = request.Operation;
            
            // Register as keyed service
            services.AddKeyedTransient<IUIAutomationOperation, TOperation>(operationName);
            
            return services;
        }
    }
}

