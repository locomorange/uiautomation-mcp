using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// Wraps a Windows Job Object for process lifecycle management.
    /// When killOnClose is true and this object is disposed (or the process crashes),
    /// all assigned processes are automatically terminated by the OS.
    /// </summary>
    internal sealed class WindowsJobObject : IDisposable
    {
        private readonly SafeJobObjectHandle _handle;
        private readonly ILogger? _logger;
        private readonly bool _killOnClose;
        private bool _disposed;

        /// <summary>
        /// Creates a new Windows Job Object.
        /// </summary>
        /// <param name="killOnClose">
        /// If true, all assigned processes are killed when the Job Object handle is closed
        /// (including abnormal parent termination). Used for Worker/Monitor subprocess management.
        /// If false, the Job Object is used only for tracking. Used for LaunchApplication tracking.
        /// </param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        /// <param name="name">Optional name for the Job Object (useful for debugging).</param>
        public WindowsJobObject(bool killOnClose, ILogger? logger = null, string? name = null)
        {
            _killOnClose = killOnClose;
            _logger = logger;

            _handle = NativeMethods.CreateJobObject(IntPtr.Zero, name);

            if (_handle.IsInvalid)
            {
                var error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to create Job Object. Win32 error: {error}");
            }

            if (killOnClose)
            {
                ConfigureKillOnClose();
            }

            _logger?.LogDebug("WindowsJobObject created (killOnClose: {KillOnClose}, name: {Name})", killOnClose, name ?? "(unnamed)");
        }

        private void ConfigureKillOnClose()
        {
            var extendedInfo = new NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = NativeMethods.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                }
            };

            int length = Marshal.SizeOf<NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);

            try
            {
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!NativeMethods.SetInformationJobObject(
                    _handle,
                    NativeMethods.JobObjectInfoType.ExtendedLimitInformation,
                    extendedInfoPtr,
                    (uint)length))
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to set JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE. Win32 error: {error}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
            }
        }

        /// <summary>
        /// Assigns a process to this Job Object.
        /// Once assigned, the process is managed by the Job Object for its lifetime.
        /// </summary>
        public bool AssignProcess(Process process)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WindowsJobObject));

            ArgumentNullException.ThrowIfNull(process);

            bool result = NativeMethods.AssignProcessToJobObject(_handle, process.Handle);

            if (result)
            {
                _logger?.LogDebug("Process PID {ProcessId} assigned to Job Object", process.Id);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger?.LogWarning("Failed to assign process PID {ProcessId} to Job Object. Win32 error: {Error}", process.Id, error);
            }

            return result;
        }

        /// <summary>
        /// Terminates all processes in the Job Object.
        /// </summary>
        public bool TerminateAll(uint exitCode = 1)
        {
            if (_disposed)
                return false;

            bool result = NativeMethods.TerminateJobObject(_handle, exitCode);

            if (result)
            {
                _logger?.LogInformation("All processes in Job Object terminated");
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger?.LogWarning("Failed to terminate Job Object processes. Win32 error: {Error}", error);
            }

            return result;
        }

        /// <summary>
        /// Queries the list of process IDs currently in the Job Object.
        /// </summary>
        public int[] GetProcessIds()
        {
            if (_disposed)
                return Array.Empty<int>();

            // Start with space for 64 processes, grow if needed
            int bufferProcessCount = 64;

            while (true)
            {
                int structSize = Marshal.SizeOf<NativeMethods.JOBOBJECT_BASIC_PROCESS_ID_LIST>()
                    + (bufferProcessCount - 1) * IntPtr.Size;
                IntPtr buffer = Marshal.AllocHGlobal(structSize);

                try
                {
                    if (NativeMethods.QueryInformationJobObject(
                        _handle,
                        NativeMethods.JobObjectInfoType.BasicProcessIdList,
                        buffer,
                        (uint)structSize,
                        out _))
                    {
                        var list = Marshal.PtrToStructure<NativeMethods.JOBOBJECT_BASIC_PROCESS_ID_LIST>(buffer);
                        int count = (int)list.NumberOfProcessIdsInList;

                        if (count == 0)
                            return Array.Empty<int>();

                        var pids = new int[count];
                        int offset = Marshal.OffsetOf<NativeMethods.JOBOBJECT_BASIC_PROCESS_ID_LIST>(nameof(NativeMethods.JOBOBJECT_BASIC_PROCESS_ID_LIST.ProcessIdList)).ToInt32();

                        for (int i = 0; i < count; i++)
                        {
                            IntPtr pidPtr = Marshal.ReadIntPtr(buffer, offset + i * IntPtr.Size);
                            pids[i] = (int)pidPtr.ToInt64();
                        }

                        return pids;
                    }
                    else
                    {
                        var error = Marshal.GetLastWin32Error();
                        if (error == 234) // ERROR_MORE_DATA
                        {
                            bufferProcessCount *= 2;
                            continue;
                        }

                        _logger?.LogWarning("Failed to query Job Object process list. Win32 error: {Error}", error);
                        return Array.Empty<int>();
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // When _handle is disposed, if killOnClose was set,
            // the OS will automatically terminate all assigned processes.
            _handle.Dispose();

            _logger?.LogDebug("WindowsJobObject disposed (killOnClose: {KillOnClose})", _killOnClose);
        }

        /// <summary>
        /// Safe handle wrapper for Windows Job Object.
        /// </summary>
        private sealed class SafeJobObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeJobObjectHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.CloseHandle(handle);
            }
        }

        /// <summary>
        /// Native Windows API declarations for Job Object management.
        /// Uses DllImport for AOT compatibility.
        /// </summary>
        private static class NativeMethods
        {
            internal const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;

            internal enum JobObjectInfoType
            {
                BasicProcessIdList = 3,
                ExtendedLimitInformation = 9
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                public long PerProcessUserTimeLimit;
                public long PerJobUserTimeLimit;
                public uint LimitFlags;
                public UIntPtr MinimumWorkingSetSize;
                public UIntPtr MaximumWorkingSetSize;
                public uint ActiveProcessLimit;
                public UIntPtr Affinity;
                public uint PriorityClass;
                public uint SchedulingClass;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct IO_COUNTERS
            {
                public ulong ReadOperationCount;
                public ulong WriteOperationCount;
                public ulong OtherOperationCount;
                public ulong ReadTransferCount;
                public ulong WriteTransferCount;
                public ulong OtherTransferCount;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
                public IO_COUNTERS IoInfo;
                public UIntPtr ProcessMemoryLimit;
                public UIntPtr JobMemoryLimit;
                public UIntPtr PeakProcessMemoryUsed;
                public UIntPtr PeakJobMemoryUsed;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct JOBOBJECT_BASIC_PROCESS_ID_LIST
            {
                public uint NumberOfAssignedProcesses;
                public uint NumberOfProcessIdsInList;
                public IntPtr ProcessIdList; // First element of an array
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern SafeJobObjectHandle CreateJobObject(IntPtr lpJobAttributes, string? lpName);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetInformationJobObject(
                SafeJobObjectHandle hJob,
                JobObjectInfoType infoType,
                IntPtr lpJobObjectInfo,
                uint cbJobObjectInfoLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool AssignProcessToJobObject(SafeJobObjectHandle hJob, IntPtr hProcess);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool TerminateJobObject(SafeJobObjectHandle hJob, uint uExitCode);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryInformationJobObject(
                SafeJobObjectHandle hJob,
                JobObjectInfoType infoType,
                IntPtr lpJobObjectInfo,
                uint cbJobObjectInfoLength,
                out uint lpReturnLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr handle);
        }
    }
}
