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
        private readonly IElementSearchService _elementSearchService;

        public DiagnosticService(AutomationHelper automationHelper, IElementSearchService elementSearchService)
        {
            _automationHelper = automationHelper;
            _elementSearchService = elementSearchService;
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

                var elementResult = await _elementSearchService.FindElementsAsync(null, null, null, processId, timeoutSeconds);

                var alternatives = new List<object>();
                bool canAccessElements = false;
                int elementCount = 0;
                string? error = null;

                // Check if the result indicates success
                if (elementResult is IDictionary<string, object> resultDict)
                {
                    canAccessElements = resultDict.ContainsKey("Success") && (bool)resultDict["Success"];
                    if (canAccessElements && resultDict.ContainsKey("Data") && resultDict["Data"] is System.Collections.IList list)
                    {
                        elementCount = list.Count;
                    }
                    if (!canAccessElements && resultDict.ContainsKey("Error"))
                    {
                        error = resultDict["Error"]?.ToString();
                    }
                }

                if (!canAccessElements)
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
                        CanAccessElements = canAccessElements,
                        ElementCount = elementCount,
                        Error = error
                    },
                    Alternatives = alternatives,
                    Recommendation = alternatives.Count > 0 
                        ? $"ProcessId {processId} is not accessible. Try using one of the alternative ProcessIds listed above."
                        : canAccessElements 
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