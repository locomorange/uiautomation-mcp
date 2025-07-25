using Microsoft.Extensions.Logging;
using System.Text.Json;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Core.Infrastructure;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Monitor.Operations;

namespace UIAutomationMCP.Monitor.Infrastructure
{
    /// <summary>
    /// Host for Monitor process communication
    /// </summary>
    public class MonitorHost : BaseProcess
    {
        private readonly OperationRegistry _operationRegistry;
        private readonly ILogger<MonitorHost> _logger;

        public MonitorHost(OperationRegistry operationRegistry, ILogger<MonitorHost> logger)
        {
            _operationRegistry = operationRegistry;
            _logger = logger;

            // Register operations
            _operationRegistry.RegisterOperation<StartEventMonitoringOperation>("StartEventMonitoring");
            _operationRegistry.RegisterOperation<StopEventMonitoringOperation>("StopEventMonitoring");
            _operationRegistry.RegisterOperation<GetEventLogOperation>("GetEventLog");

            _logger.LogInformation("MonitorHost initialized with {Count} registered operations", 
                _operationRegistry.GetRegisteredOperations().Count());
        }

        protected override async Task<string> ProcessRequestAsync(string request)
        {
            try
            {
                _logger.LogDebug("Processing request: {RequestLength} characters", request.Length);

                // Deserialize the worker request
                var workerRequest = JsonSerializationHelper.Deserialize<WorkerRequest>(request);
                if (workerRequest == null)
                {
                    _logger.LogError("Failed to deserialize WorkerRequest from: {Request}", request);
                    return CreateErrorResponse("Invalid request format");
                }

                var operationName = workerRequest.Operation;
                if (string.IsNullOrEmpty(operationName))
                {
                    _logger.LogError("Operation name is null or empty");
                    return CreateErrorResponse("Operation name is required");
                }

                if (!_operationRegistry.IsOperationRegistered(operationName))
                {
                    _logger.LogWarning("Unknown operation requested: {OperationName}", operationName);
                    return CreateErrorResponse($"Unknown operation: {operationName}");
                }

                // Get the parameters JSON
                var parametersJson = workerRequest.ParametersJson ?? "";
                if (string.IsNullOrEmpty(parametersJson) && workerRequest.Parameters != null)
                {
                    parametersJson = JsonSerializationHelper.Serialize(workerRequest.Parameters);
                }

                // Execute the operation based on type
                switch (operationName)
                {
                    case "StartEventMonitoring":
                        var startOp = _operationRegistry.CreateOperation<StartEventMonitoringOperation>(operationName);
                        var startResult = await startOp.ExecuteAsync(parametersJson);
                        return JsonSerializationHelper.Serialize(startResult);

                    case "StopEventMonitoring":
                        var stopOp = _operationRegistry.CreateOperation<StopEventMonitoringOperation>(operationName);
                        var stopResult = await stopOp.ExecuteAsync(parametersJson);
                        return JsonSerializationHelper.Serialize(stopResult);

                    case "GetEventLog":
                        var logOp = _operationRegistry.CreateOperation<GetEventLogOperation>(operationName);
                        var logResult = await logOp.ExecuteAsync(parametersJson);
                        return JsonSerializationHelper.Serialize(logResult);

                    default:
                        _logger.LogWarning("Unhandled operation: {OperationName}", operationName);
                        return CreateErrorResponse($"Operation '{operationName}' is not implemented");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request: {ErrorMessage}", ex.Message);
                return CreateErrorResponse($"Internal error: {ex.Message}");
            }
        }

        protected override string CreateErrorResponse(string error)
        {
            var errorResponse = new OperationResult
            {
                Success = false,
                Error = error,
                Data = null
            };

            return JsonSerializationHelper.Serialize(errorResponse);
        }
    }
}