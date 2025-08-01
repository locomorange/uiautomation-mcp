namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Error handler registry for operations
    /// </summary>
    public static class ErrorHandlerRegistry
    {
        public static void RegisterHandlers() { }
        
        public static T Handle<T>(Func<T> operation) where T : BaseOperationResult, new()
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                var result = new T();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        public static bool ValidateElementId(string? elementId)
        {
            return !string.IsNullOrWhiteSpace(elementId);
        }
        
        public static bool ValidateElementId(string? elementId, string context)
        {
            return !string.IsNullOrWhiteSpace(elementId);
        }
    }
}