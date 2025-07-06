using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Services
{
    public interface IDiagnosticService
    {
        Task<object> DiagnoseProcessIdAsync(int processId);
        Task<object> ListAccessibleProcessesAsync(int timeoutSeconds = 30);
        Task<object> TestProcessIdAccessAsync(int processId, int timeoutSeconds = 30);
    }

    public class DiagnosticService : IDiagnosticService
    {
        private readonly AutomationHelper _automationHelper;
        private readonly IUIAutomationService _uiAutomationService;

        public DiagnosticService(AutomationHelper automationHelper, IUIAutomationService uiAutomationService)
        {
            _automationHelper = automationHelper;
            _uiAutomationService = uiAutomationService;
        }

        public async Task<object> DiagnoseProcessIdAsync(int processId)
        {
            try
            {
                await Task.Run(() => _automationHelper.DiagnoseProcessId(processId));
                
                return new
                {
                    Success = true,
                    Message = $"Diagnostic information for ProcessId {processId} has been logged. Check the application logs for detailed analysis.",
                    ProcessId = processId
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Error = $"Failed to diagnose ProcessId {processId}: {ex.Message}",
                    ProcessId = processId
                };
            }
        }

        public async Task<object> ListAccessibleProcessesAsync(int timeoutSeconds = 30)
        {
            try
            {
                var result = await Task.Run(() =>
                {
                    var processes = System.Diagnostics.Process.GetProcesses()
                        .Where(p => !p.HasExited && p.MainWindowHandle != IntPtr.Zero)
                        .Select(p => new
                        {
                            ProcessId = p.Id,
                            ProcessName = p.ProcessName,
                            MainWindowTitle = p.MainWindowTitle,
                            MainWindowHandle = p.MainWindowHandle.ToString()
                        })
                        .OrderBy(p => p.ProcessName)
                        .ToList();

                    return new
                    {
                        Success = true,
                        ProcessCount = processes.Count,
                        Processes = processes
                    };
                });

                return result;
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Error = $"Failed to list accessible processes: {ex.Message}"
                };
            }
        }

        public async Task<object> TestProcessIdAccessAsync(int processId, int timeoutSeconds = 30)
        {
            try
            {
                await Task.Run(() => _automationHelper.DiagnoseProcessId(processId));

                var elementResult = await _uiAutomationService.FindElementsAsync(null, null, null, processId, timeoutSeconds);

                var alternatives = new List<object>();

                if (!elementResult.Success)
                {
                    var targetProcess = System.Diagnostics.Process.GetProcesses()
                        .FirstOrDefault(p => p.Id == processId);

                    if (targetProcess != null)
                    {
                        var sameNameProcesses = System.Diagnostics.Process.GetProcessesByName(targetProcess.ProcessName)
                            .Where(p => p.Id != processId && !p.HasExited)
                            .Select(p => new
                            {
                                ProcessId = p.Id,
                                ProcessName = p.ProcessName,
                                MainWindowTitle = p.MainWindowTitle,
                                HasMainWindow = p.MainWindowHandle != IntPtr.Zero
                            })
                            .ToList();

                        alternatives.AddRange(sameNameProcesses);
                    }
                }

                return new
                {
                    Success = true,
                    ProcessId = processId,
                    AccessTest = new
                    {
                        CanAccessElements = elementResult.Success,
                        ElementCount = elementResult.Success && elementResult.Data is System.Collections.IList list ? list.Count : 0,
                        Error = elementResult.Error
                    },
                    Alternatives = alternatives,
                    Recommendation = alternatives.Count > 0 
                        ? $"ProcessId {processId} is not accessible. Try using one of the alternative ProcessIds listed above."
                        : elementResult.Success 
                            ? $"ProcessId {processId} is working correctly."
                            : $"ProcessId {processId} is not accessible and no alternatives found."
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    ProcessId = processId,
                    Error = $"Failed to test ProcessId access: {ex.Message}"
                };
            }
        }
    }
}