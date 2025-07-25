using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Windows.Automation;
using UIAutomationMCP.Core.Models;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Monitor.Sessions
{
    /// <summary>
    /// Event monitoring session with type-safe event data
    /// </summary>
    public class EventMonitoringSession : IDisposable
    {
        private readonly string _sessionId;
        private readonly string[] _eventTypes;
        private readonly string? _automationId;
        private readonly string? _name;
        private readonly string? _controlType;
        private readonly string? _windowTitle;
        private readonly int? _processId;
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<EventMonitoringSession> _logger;

        private readonly List<IDisposable> _eventHandlers = new();
        private readonly ConcurrentQueue<TypedEventData> _capturedEvents = new();
        private readonly object _lockObject = new();
        private bool _isActive = false;
        private bool _disposed = false;

        public string SessionId => _sessionId;
        public bool IsActive => _isActive;
        public int EventCount => _capturedEvents.Count;
        public DateTime StartTime { get; private set; }

        public EventMonitoringSession(
            string sessionId,
            string[] eventTypes,
            string? automationId,
            string? name,
            string? controlType,
            string? windowTitle,
            int? processId,
            ElementFinderService elementFinderService,
            ILogger<EventMonitoringSession> logger)
        {
            _sessionId = sessionId;
            _eventTypes = eventTypes;
            _automationId = automationId;
            _name = name;
            _controlType = controlType;
            _windowTitle = windowTitle;
            _processId = processId;
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EventMonitoringSession));

            lock (_lockObject)
            {
                if (_isActive)
                    throw new InvalidOperationException("Session is already active");

                StartTime = DateTime.UtcNow;
                _isActive = true;
            }

            await Task.Run(() =>
            {
                try
                {
                    RegisterEventHandlers();
                    _logger.LogInformation("Event monitoring session {SessionId} started with {HandlerCount} handlers", 
                        _sessionId, _eventHandlers.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start event monitoring session {SessionId}", _sessionId);
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

            foreach (var handler in _eventHandlers)
            {
                try
                {
                    handler?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose event handler in session {SessionId}", _sessionId);
                }
            }
            _eventHandlers.Clear();

            _logger.LogInformation("Event monitoring session {SessionId} stopped. Captured {EventCount} events", 
                _sessionId, _capturedEvents.Count);
        }

        public List<TypedEventData> GetCapturedEvents(int maxCount = 100)
        {
            var events = new List<TypedEventData>();
            var count = 0;

            while (_capturedEvents.TryDequeue(out var eventData) && count < maxCount)
            {
                events.Add(eventData);
                count++;
            }

            return events;
        }

        public List<TypedEventData> PeekCapturedEvents(int maxCount = 100)
        {
            return _capturedEvents.Take(maxCount).ToList();
        }

        private void RegisterEventHandlers()
        {
            var targetElement = FindTargetElement();
            var rootElement = targetElement ?? AutomationElement.RootElement;

            foreach (var eventType in _eventTypes)
            {
                var handler = CreateEventHandler(eventType, rootElement);
                if (handler != null)
                {
                    _eventHandlers.Add(handler);
                }
            }
        }

        private AutomationElement? FindTargetElement()
        {
            if (string.IsNullOrEmpty(_automationId) && string.IsNullOrEmpty(_name))
                return null;

            try
            {
                return _elementFinderService.FindElement(
                    automationId: _automationId,
                    name: _name,
                    controlType: _controlType,
                    windowTitle: _windowTitle,
                    processId: _processId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to find target element for session {SessionId}", _sessionId);
                return null;
            }
        }

        private IDisposable? CreateEventHandler(string eventType, AutomationElement element)
        {
            try
            {
                return eventType.ToLower() switch
                {
                    "focus" => CreateFocusChangedHandler(),
                    "invoke" => CreateInvokeHandler(element),
                    "selection" => CreateSelectionHandler(element),
                    "text" => CreateTextChangedHandler(element),
                    "property" => CreatePropertyChangedHandler(element),
                    "structure" => CreateStructureChangedHandler(element),
                    _ => CreateGenericAutomationEventHandler(element, eventType)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create event handler for {EventType} in session {SessionId}", 
                    eventType, _sessionId);
                return null;
            }
        }

        private IDisposable CreateFocusChangedHandler()
        {
            AutomationFocusChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new FocusEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = "Focus changed",
                    SessionId = _sessionId
                };

                _capturedEvents.Enqueue(eventData);
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
                    _logger.LogError(ex, "Failed to remove focus changed event handler in session {SessionId}", _sessionId);
                }
            });
        }

        private IDisposable CreateInvokeHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new InvokeEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    EventId = e.EventId.ProgrammaticName
                };

                _capturedEvents.Enqueue(eventData);
            };

            Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent, element, TreeScope.Subtree, handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(InvokePattern.InvokedEvent, element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove invoke event handler in session {SessionId}", _sessionId);
                }
            });
        }

        private IDisposable CreateSelectionHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new SelectionEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    EventId = e.EventId.ProgrammaticName
                };

                _capturedEvents.Enqueue(eventData);
            };

            Automation.AddAutomationEventHandler(SelectionItemPattern.ElementSelectedEvent, element, TreeScope.Subtree, handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(SelectionItemPattern.ElementSelectedEvent, element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove selection event handler in session {SessionId}", _sessionId);
                }
            });
        }

        private IDisposable CreateTextChangedHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new TextChangedEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    EventId = e.EventId.ProgrammaticName
                };

                _capturedEvents.Enqueue(eventData);
            };

            Automation.AddAutomationEventHandler(TextPattern.TextChangedEvent, element, TreeScope.Subtree, handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(TextPattern.TextChangedEvent, element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove text changed event handler in session {SessionId}", _sessionId);
                }
            });
        }

        private IDisposable CreatePropertyChangedHandler(AutomationElement element)
        {
            AutomationPropertyChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new PropertyChangedEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    PropertyId = e.Property.ProgrammaticName,
                    NewValue = e.NewValue?.ToString() ?? "null",
                    OldValue = e.OldValue?.ToString() ?? "null"
                };

                _capturedEvents.Enqueue(eventData);
            };

            var properties = new[]
            {
                AutomationElement.NameProperty,
                AutomationElement.IsEnabledProperty,
                ValuePattern.ValueProperty,
                TogglePattern.ToggleStateProperty
            };

            Automation.AddAutomationPropertyChangedEventHandler(element, TreeScope.Subtree, handler, properties);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationPropertyChangedEventHandler(element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove property changed event handler in session {SessionId}", _sessionId);
                }
            });
        }

        private IDisposable CreateStructureChangedHandler(AutomationElement element)
        {
            StructureChangedEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new StructureChangedEventData
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    StructureChangeType = e.StructureChangeType.ToString(),
                    RuntimeId = e.GetRuntimeId()?.ToArray() ?? Array.Empty<int>()
                };

                _capturedEvents.Enqueue(eventData);
            };

            Automation.AddStructureChangedEventHandler(element, TreeScope.Subtree, handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveStructureChangedEventHandler(element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove structure changed event handler in session {SessionId}", _sessionId);
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
                _logger.LogWarning("Unknown event type: {EventType} in session {SessionId}", eventType, _sessionId);
                return null;
            }

            AutomationEventHandler handler = (sender, e) =>
            {
                if (!_isActive) return;

                var eventData = new GenericEventData(eventType)
                {
                    Timestamp = DateTime.UtcNow,
                    SourceElement = GetElementDescription(sender as AutomationElement),
                    SessionId = _sessionId,
                    EventId = e.EventId.ProgrammaticName
                };

                _capturedEvents.Enqueue(eventData);
            };

            Automation.AddAutomationEventHandler(automationEvent, element, TreeScope.Subtree, handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(automationEvent, element, handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove {EventType} event handler in session {SessionId}", eventType, _sessionId);
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

    /// <summary>
    /// Helper class for creating disposable actions
    /// </summary>
    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        private bool _disposed = false;

        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _action();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}