using System.Text.Json;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using System.Reflection;
using System.Collections.Concurrent;

namespace UIAutomationMCP.Shared
{
    /// <summary>
    /// WorkerRequestの型安全な拡張メソッド
    /// </summary>
    public static class WorkerRequestExtensions
    {
        private static readonly ConcurrentDictionary<string, Type> _operationTypeMap = new();
        private static readonly object _initializationLock = new();
        private static bool _initialized = false;

        static WorkerRequestExtensions()
        {
            InitializeOperationTypeMap();
        }

        private static void InitializeOperationTypeMap()
        {
            if (_initialized) return;
            
            lock (_initializationLock)
            {
                if (_initialized) return;
                
                var assembly = Assembly.GetExecutingAssembly();
                var requestTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(TypedWorkerRequest).IsAssignableFrom(t))
                    .ToList();

                foreach (var type in requestTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type) as TypedWorkerRequest;
                        if (instance != null)
                        {
                            _operationTypeMap[instance.Operation] = type;
                        }
                    }
                    catch
                    {
                        // Skip types that can't be instantiated with default constructor
                    }
                }
                
                _initialized = true;
            }
        }
        /// <summary>
        /// WorkerRequestから型安全なリクエストオブジェクトを取得（設定値を適用）
        /// </summary>
        public static T? GetTypedRequest<T>(this WorkerRequest request, IOptions<UIAutomationOptions> options) where T : TypedWorkerRequest
        {
            if (request.Parameters == null)
                return null;

            try
            {
                var json = JsonSerializer.Serialize(request.Parameters);
                var typedRequest = JsonSerializer.Deserialize<T>(json);
                
                if (typedRequest != null)
                {
                    RequestConfigurationDefaults.ApplyConfigurationDefaults(typedRequest, options.Value);
                }
                
                return typedRequest;
            }
            catch (JsonException)
            {
                return null;
            }
        }


        /// <summary>
        /// 操作名に基づいて適切な型安全リクエストを取得（設定値を適用）
        /// </summary>
        public static TypedWorkerRequest? GetTypedRequestByOperation(this WorkerRequest request, IOptions<UIAutomationOptions> options)
        {
            if (!_operationTypeMap.TryGetValue(request.Operation, out var requestType))
            {
                return null;
            }

            // Use reflection to call the generic GetTypedRequest method
            var method = typeof(WorkerRequestExtensions)
                .GetMethod(nameof(GetTypedRequest), BindingFlags.Static | BindingFlags.Public)
                ?.MakeGenericMethod(requestType);

            return method?.Invoke(null, new object[] { request, options }) as TypedWorkerRequest;
        }

        /// <summary>
        /// 型安全リクエストからWorkerRequestを作成
        /// </summary>
        public static WorkerRequest ToWorkerRequest(this TypedWorkerRequest typedRequest)
        {
            var parametersJson = JsonSerializer.Serialize(typedRequest);
            var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);

            return new WorkerRequest
            {
                Operation = typedRequest.Operation,
                Parameters = parameters
            };
        }
    }

}