namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// 型安全なWorkerRequestファクトリ
    /// </summary>
    public static class TypedWorkerRequestFactory
    {
        public static WorkerRequest CreateInvokeElement(string elementId, string windowTitle = "", int? processId = null)
        {
            return new InvokeElementRequest
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSetElementValue(string elementId, string value, string windowTitle = "", int? processId = null)
        {
            return new SetElementValueRequest
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateFindElements(
            string? windowTitle = null,
            int? processId = null,
            string? searchText = null,
            string? automationId = null,
            string? controlType = null,
            string? className = null,
            string scope = "descendants",
            int timeoutSeconds = 30,
            bool useCache = true,
            bool useRegex = false,
            bool useWildcard = false)
        {
            return new FindElementsRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId,
                SearchText = searchText,
                AutomationId = automationId,
                ControlType = controlType,
                ClassName = className,
                Scope = scope,
                TimeoutSeconds = timeoutSeconds,
                UseCache = useCache,
                UseRegex = useRegex,
                UseWildcard = useWildcard
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateWindowAction(string action, string? windowTitle = null, int? processId = null)
        {
            return new WindowActionRequest
            {
                Action = action,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSetRangeValue(string elementId, double value, string windowTitle = "", int? processId = null)
        {
            return new SetRangeValueRequest
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSelectText(string elementId, int startIndex, int length, string windowTitle = "", int? processId = null)
        {
            return new SelectTextRequest
            {
                ElementId = elementId,
                StartIndex = startIndex,
                Length = length,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateMoveElement(string elementId, double x, double y, string windowTitle = "", int? processId = null)
        {
            return new MoveElementRequest
            {
                ElementId = elementId,
                X = x,
                Y = y,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateWaitForInputIdle(string? windowTitle = null, int? processId = null, int timeoutMilliseconds = 10000)
        {
            return new WaitForInputIdleRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutMilliseconds = timeoutMilliseconds
            }.ToWorkerRequest();
        }
    }
}