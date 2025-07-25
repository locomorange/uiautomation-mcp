using System.Collections.Concurrent;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従った継続的なイベント監視操作
    /// バックグラウンドで継続的にイベントを監視し、セッション管理を行う
    /// </summary>
    public class StartEventMonitoringOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<StartEventMonitoringOperation> _logger;
        
        // MS Learn推奨: 複数のイベントセッションを安全に管理
        private static readonly ConcurrentDictionary<string, EventMonitoringSession> _activeSessions = new();

        public StartEventMonitoringOperation(
            ElementFinderService elementFinderService,
            ILogger<StartEventMonitoringOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                _logger.LogInformation($"[EventMonitor Worker] StartEventMonitoringOperation.ExecuteAsync started. ParametersJson length: {parametersJson?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(parametersJson))
                {
                    _logger.LogError($"[EventMonitor Worker] ParametersJson is null or empty");
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "ParametersJson is null or empty",
                        Data = new EventMonitoringStartResult()
                    };
                }
                
                var request = JsonSerializationHelper.Deserialize<StartEventMonitoringRequest>(parametersJson);
                if (request == null)
                {
                    _logger.LogError($"[EventMonitor Worker] Failed to deserialize StartEventMonitoringRequest. ParametersJson: {parametersJson}");
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Failed to deserialize StartEventMonitoringRequest",
                        Data = new EventMonitoringStartResult()
                    };
                }

                var sessionId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation($"[EventMonitor Worker] Generated sessionId: {sessionId}. Request details - EventTypes: [{string.Join(", ", request.EventTypes)}], AutomationId: {request.AutomationId ?? "null"}, Name: {request.Name ?? "null"}, ControlType: {request.ControlType ?? "null"}, ProcessId: {request.ProcessId?.ToString() ?? "null"}");
                _logger.LogInformation($"[EventMonitor Worker] Current active sessions count: {_activeSessions.Count}");

                // MS Learn推奨: セッション管理とスレッドセーフティ
                var session = new EventMonitoringSession(sessionId, request, _elementFinderService, _logger);
                
                _logger.LogInformation($"[EventMonitor Worker] Created EventMonitoringSession {sessionId}. Attempting to add to active sessions...");
                
                if (!_activeSessions.TryAdd(sessionId, session))
                {
                    _logger.LogError($"[EventMonitor Worker] Failed to add session {sessionId} to active sessions. Current sessions: [{string.Join(", ", _activeSessions.Keys)}]");
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Failed to create monitoring session",
                        Data = new EventMonitoringStartResult()
                    };
                }

                _logger.LogInformation($"[EventMonitor Worker] Successfully added session {sessionId} to active sessions. Total sessions: {_activeSessions.Count}");

                try
                {
                    _logger.LogInformation($"[EventMonitor Worker] Starting session {sessionId}...");
                    await session.StartAsync();
                    _logger.LogInformation($"[EventMonitor Worker] Session {sessionId} started successfully");

                    var result = new EventMonitoringStartResult
                    {
                        Success = true,
                        EventType = string.Join(", ", request.EventTypes),
                        ElementId = request.AutomationId,
                        WindowTitle = request.WindowTitle,
                        ProcessId = request.ProcessId,
                        SessionId = sessionId,
                        MonitoringStatus = "Started"
                    };

                    return new OperationResult 
                    { 
                        Success = true, 
                        Data = result
                    };
                }
                catch
                {
                    // 失敗した場合はセッションを削除
                    _logger.LogError($"[EventMonitor Worker] Session {sessionId} startup failed. Removing from active sessions...");
                    _activeSessions.TryRemove(sessionId, out _);
                    session.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EventMonitor Worker] StartEventMonitoring operation failed. Exception: {ex.Message}");
                return new OperationResult 
                { 
                    Success = false, 
                    Error = $"StartEventMonitoring failed: {ex.Message}",
                    Data = new EventMonitoringStartResult()
                };
            }
        }

        /// <summary>
        /// アクティブなセッションを取得（他のOperationから使用）
        /// </summary>
        public static EventMonitoringSession? GetSession(string sessionId)
        {
            _activeSessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// セッションを停止・削除
        /// </summary>
        public static bool RemoveSession(string sessionId)
        {
            if (_activeSessions.TryRemove(sessionId, out var session))
            {
                session.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 全てのアクティブセッションを取得
        /// </summary>
        public static IEnumerable<EventMonitoringSession> GetActiveSessions()
        {
            return _activeSessions.Values.ToList();
        }
    }

    /// <summary>
    /// MS Learn ベストプラクティスに従ったイベント監視セッション管理
    /// </summary>
    public class EventMonitoringSession : IDisposable
    {
        private readonly string _sessionId;
        private readonly StartEventMonitoringRequest _request;
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger _logger;
        private readonly List<IDisposable> _eventHandlers = new();
        private readonly ConcurrentQueue<EventData> _capturedEvents = new();
        private readonly object _lockObject = new();
        private bool _isActive = false;
        private bool _disposed = false;

        public string SessionId => _sessionId;
        public bool IsActive => _isActive;
        public int EventCount => _capturedEvents.Count;
        public DateTime StartTime { get; private set; }

        public EventMonitoringSession(
            string sessionId, 
            StartEventMonitoringRequest request, 
            ElementFinderService elementFinderService, 
            ILogger logger)
        {
            _sessionId = sessionId;
            _request = request;
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public string GetEventType() => string.Join(", ", _request.EventTypes);
        public string? GetAutomationId() => _request.AutomationId;
        public string? GetName() => _request.Name;
        public string? GetControlType() => _request.ControlType;
        public string? GetWindowTitle() => _request.WindowTitle;
        public int? GetProcessId() => _request.ProcessId;

        public async Task StartAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EventMonitoringSession));

            _logger.LogInformation($"[EventMonitor Session] StartAsync called for session {_sessionId}");

            lock (_lockObject)
            {
                if (_isActive)
                {
                    _logger.LogWarning($"[EventMonitor Session] Session {_sessionId} is already active");
                    throw new InvalidOperationException("Session is already active");
                }

                StartTime = DateTime.UtcNow;
                _isActive = true;
                _logger.LogInformation($"[EventMonitor Session] Session {_sessionId} marked as active. StartTime: {StartTime}");
            }

            // MS Learn推奨: 非UIスレッドでイベントハンドラを登録
            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation($"[EventMonitor Session] Registering event handlers for session {_sessionId}...");
                    RegisterEventHandlers();
                    _logger.LogInformation($"[EventMonitor Session] Event monitoring session {_sessionId} started successfully with {_eventHandlers.Count} handlers");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[EventMonitor Session] Failed to start event monitoring session {_sessionId}: {ex.Message}");
                    _isActive = false;
                    throw;
                }
            });
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isActive) return;
                _isActive = false;
            }

            // MS Learn推奨: 安全にイベントハンドラを解除
            foreach (var handler in _eventHandlers)
            {
                try
                {
                    handler?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to dispose event handler in session {_sessionId}");
                }
            }
            _eventHandlers.Clear();

            _logger.LogInformation($"Event monitoring session {_sessionId} stopped. Captured {_capturedEvents.Count} events");
        }

        public List<EventData> GetCapturedEvents(int maxCount = 100)
        {
            var events = new List<EventData>();
            var count = 0;
            
            while (_capturedEvents.TryDequeue(out var eventData) && count < maxCount)
            {
                events.Add(eventData);
                count++;
            }
            
            return events;
        }

        public List<EventData> PeekCapturedEvents(int maxCount = 100)
        {
            return _capturedEvents.Take(maxCount).ToList();
        }

        private void RegisterEventHandlers()
        {
            _logger.LogInformation($"[EventMonitor Session] RegisterEventHandlers started for session {_sessionId}. EventTypes: [{string.Join(", ", _request.EventTypes)}]");
            
            AutomationElement? targetElement = null;

            // 特定要素の監視が指定されている場合
            if (!string.IsNullOrEmpty(_request.AutomationId) || !string.IsNullOrEmpty(_request.Name))
            {
                _logger.LogInformation($"[EventMonitor Session] Searching for target element - AutomationId: {_request.AutomationId ?? "null"}, Name: {_request.Name ?? "null"}, ControlType: {_request.ControlType ?? "null"}, WindowTitle: {_request.WindowTitle ?? "null"}, ProcessId: {_request.ProcessId?.ToString() ?? "null"}");
                
                targetElement = _elementFinderService.FindElement(
                    automationId: _request.AutomationId,
                    name: _request.Name,
                    controlType: _request.ControlType,
                    windowTitle: _request.WindowTitle,
                    processId: _request.ProcessId);

                if (targetElement == null)
                {
                    _logger.LogWarning($"[EventMonitor Session] Target element not found for session {_sessionId}: AutomationId={_request.AutomationId}, Name={_request.Name}. Will monitor desktop (RootElement)");
                    // 要素が見つからなくてもデスクトップ全体の監視は続行
                }
                else
                {
                    _logger.LogInformation($"[EventMonitor Session] Target element found for session {_sessionId}: {GetElementDescription(targetElement)}");
                }
            }
            else
            {
                _logger.LogInformation($"[EventMonitor Session] No specific target element specified for session {_sessionId}. Will monitor desktop (RootElement)");
            }

            var rootElement = targetElement ?? AutomationElement.RootElement;
            _logger.LogInformation($"[EventMonitor Session] Using element for monitoring: {(targetElement != null ? "Target Element" : "RootElement")}");

            // 各イベントタイプに対してハンドラを登録
            foreach (var eventType in _request.EventTypes)
            {
                _logger.LogInformation($"[EventMonitor Session] Creating event handler for eventType: {eventType} in session {_sessionId}");
                
                var handler = CreateEventHandler(eventType, rootElement);
                if (handler != null)
                {
                    _eventHandlers.Add(handler);
                    _logger.LogInformation($"[EventMonitor Session] Successfully created and added event handler for eventType: {eventType} in session {_sessionId}");
                }
                else
                {
                    _logger.LogWarning($"[EventMonitor Session] Failed to create event handler for eventType: {eventType} in session {_sessionId}");
                }
            }
            
            _logger.LogInformation($"[EventMonitor Session] RegisterEventHandlers completed for session {_sessionId}. Total handlers created: {_eventHandlers.Count}");
        }

        private IDisposable? CreateEventHandler(string eventType, AutomationElement element)
        {
            try
            {
                _logger.LogInformation($"[EventMonitor Session] CreateEventHandler called for eventType: {eventType} in session {_sessionId}");
                
                var handler = eventType.ToLower() switch
                {
                    "focus" => CreateFocusChangedHandler(),
                    "invoke" => CreateInvokeHandler(element),
                    "selection" => CreateSelectionHandler(element),
                    "text" => CreateTextChangedHandler(element),
                    "property" => CreatePropertyChangedHandler(element),
                    "structure" => CreateStructureChangedHandler(element),
                    _ => CreateGenericAutomationEventHandler(element, eventType)
                };
                
                if (handler != null)
                {
                    _logger.LogInformation($"[EventMonitor Session] Successfully created event handler for eventType: {eventType} in session {_sessionId}");
                }
                else
                {
                    _logger.LogWarning($"[EventMonitor Session] Handler creation returned null for eventType: {eventType} in session {_sessionId}");
                }
                
                return handler;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EventMonitor Session] Failed to create event handler for {eventType} in session {_sessionId}: {ex.Message}");
                return null;
            }
        }

        private IDisposable CreateFocusChangedHandler()
        {
            AutomationFocusChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Focus",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = "Focus changed",
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId
                    }
                });
            };

            Automation.AddAutomationFocusChangedEventHandler(handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationFocusChangedEventHandler(handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove focus changed event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable CreateInvokeHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Invoke",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["EventId"] = e.EventId.ProgrammaticName
                    }
                });
            };

            Automation.AddAutomationEventHandler(
                InvokePattern.InvokedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        InvokePattern.InvokedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove invoke event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable CreateSelectionHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Selection",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["EventId"] = e.EventId.ProgrammaticName
                    }
                });
            };

            Automation.AddAutomationEventHandler(
                SelectionItemPattern.ElementSelectedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        SelectionItemPattern.ElementSelectedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove selection event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable CreateTextChangedHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Text",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["EventId"] = e.EventId.ProgrammaticName
                    }
                });
            };

            Automation.AddAutomationEventHandler(
                TextPattern.TextChangedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        TextPattern.TextChangedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove text changed event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable CreatePropertyChangedHandler(AutomationElement element)
        {
            AutomationPropertyChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Property",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["PropertyId"] = e.Property.ProgrammaticName,
                        ["NewValue"] = e.NewValue?.ToString() ?? "null",
                        ["OldValue"] = e.OldValue?.ToString() ?? "null"
                    }
                });
            };

            var properties = new[]
            {
                AutomationElement.NameProperty,
                AutomationElement.IsEnabledProperty,
                ValuePattern.ValueProperty,
                TogglePattern.ToggleStateProperty
            };

            Automation.AddAutomationPropertyChangedEventHandler(
                element,
                TreeScope.Subtree,
                handler,
                properties);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationPropertyChangedEventHandler(
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove property changed event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable CreateStructureChangedHandler(AutomationElement element)
        {
            StructureChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = "Structure",
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["StructureChangeType"] = e.StructureChangeType.ToString(),
                        ["RuntimeId"] = e.GetRuntimeId()?.ToArray() ?? Array.Empty<int>()
                    }
                });
            };

            Automation.AddStructureChangedEventHandler(
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveStructureChangedEventHandler(
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove structure changed event handler in session {_sessionId}");
                }
            });
        }

        private IDisposable? CreateGenericAutomationEventHandler(AutomationElement element, string eventType)
        {
            var eventMap = new Dictionary<string, AutomationEvent>
            {
                ["window.opened"] = WindowPattern.WindowOpenedEvent,
                ["window.closed"] = WindowPattern.WindowClosedEvent,
                ["menu.opened"] = AutomationElement.MenuOpenedEvent,
                ["menu.closed"] = AutomationElement.MenuClosedEvent,
                ["tooltip.opened"] = AutomationElement.ToolTipOpenedEvent,
                ["tooltip.closed"] = AutomationElement.ToolTipClosedEvent
            };

            if (!eventMap.TryGetValue(eventType.ToLower(), out var automationEvent))
            {
                _logger.LogWarning($"Unknown event type: {eventType} in session {_sessionId}");
                return null;
            }

            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;
                
                _capturedEvents.Enqueue(new EventData
                {
                    EventType = eventType,
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    EventDataProperties = new Dictionary<string, object>
                    {
                        ["SessionId"] = _sessionId,
                        ["EventId"] = e.EventId.ProgrammaticName
                    }
                });
            };

            Automation.AddAutomationEventHandler(
                automationEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        automationEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove {eventType} event handler in session {_sessionId}");
                }
            });
        }

        private string GetElementDescription(AutomationElement? element)
        {
            if (element == null) return "Unknown element";

            try
            {
                var name = element.GetCurrentPropertyValue(AutomationElement.NameProperty)?.ToString() ?? "";
                var controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty)?.ToString() ?? "";
                var automationId = element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty)?.ToString() ?? "";
                
                return $"{controlType} '{name}' (AutomationId: {automationId})";
            }
            catch
            {
                return "Element description unavailable";
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            Stop();
            _disposed = true;
        }
    }
}
