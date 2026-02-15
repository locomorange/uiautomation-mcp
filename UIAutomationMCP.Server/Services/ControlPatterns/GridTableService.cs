using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class GridTableService : IGridTableService
    {
        private readonly IGridService _gridService;
        private readonly ITableService _tableService;

        public GridTableService(IGridService gridService, ITableService tableService)
        {
            _gridService = gridService;
            _tableService = tableService;
        }

        public Task<ServerEnhancedResponse<ElementSearchResult>> GetGridItemAsync(string? automationId = null, string? name = null, int row = 0, int column = 0, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _gridService.GetGridItemAsync(automationId, name, row, column, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderAsync(string? automationId = null, string? name = null, int row = 0, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _gridService.GetRowHeaderAsync(automationId, name, row, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderAsync(string? automationId = null, string? name = null, int column = 0, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _gridService.GetColumnHeaderAsync(automationId, name, column, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<TableInfoResult>> GetTableInfoAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _tableService.GetTableInfoAsync(automationId, name, controlType, windowHandle, processId, timeoutSeconds);
    }
}
