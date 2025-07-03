using System.Diagnostics;
using System.Runtime.InteropServices;

namespace sambar;

public partial class Api
{
    /// <summary>
    /// PerfCounters or performance counters allow us 
    /// to query performance metrics [CPU, GC Collections, Memory etc]
    /// We use native win32 apis instead of .NET's perfcounter class
    /// 
    /// https://learn.microsoft.com/en-us/windows/win32/perfctrs/specifying-a-counter-path 
    /// Counter PATH:
    /// "\\Computer\PerfObject(ParentInstance/ObjectInstance#InstanceIndex)\Counter"
    /// "\\computer\object(parent/instance#index)\counter"
    /// </summary>
    int cpuCores = 0;
    public void CountersInit()
    {
        cpuCores = GetCpuCount();
        GetCoreUsages();
    }

    public int GetCpuCount()
    {
        SYSTEM_BASIC_INFORMATION sbi = new();
        Ntdll.NtQuerySystemInformation(
            SYSTEM_INFORMATION_CLASS.SystemBasicInformation, 
            ref sbi, 
            (uint)Marshal.SizeOf<SYSTEM_BASIC_INFORMATION>(), 
            out uint returnLength
        );
        int coreCount = sbi.NumberOfProcessors;
        Debug.WriteLine($"CORECOUNT: {coreCount}");
        return coreCount;
    }

    public delegate void CpuPerformanceEventHandler(long[] cpuStats);
    public event CpuPerformanceEventHandler CPU_PERFORMANCE_NOTIFIED = (stats) => { };
    /// <summary>
    /// Get CPU Usages
    /// https://www.codeproject.com/Articles/9113/Get-CPU-Usage-with-GetSystemTimes
    /// </summary>
    public void GetCoreUsages()
    {
        if (cpuCores == 0) return;

        CancellationTokenSource cts = new();
        long[] cpuStats = new long[cpuCores];
        Task.Run(async () =>
        {
            long[] usr = new long[cpuCores];
            long[] kernel = new long[cpuCores];
            long[] idle = new long[cpuCores];

            long[] _usr = new long[cpuCores];
            long[] _kernel = new long[cpuCores];
            long[] _idle = new long[cpuCores];

            long[] _delta_usr = new long[cpuCores];
            long[] _delta_kernel = new long[cpuCores];
            long[] _delta_idle = new long[cpuCores];
            
            while(true)
            {
                int size = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
                nint bufferPtr = Marshal.AllocHGlobal(size*cpuCores);
                Ntdll.NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemProcessorPerformanceInformation,
                    bufferPtr,
                    (uint)(size*cpuCores),
                    out uint returnLength
                );

                for (int i = 0; i < cpuCores; i++)
                {
                    SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION proc = 
                        Marshal.PtrToStructure<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(bufferPtr + size*i);

                    _usr[i] = usr[i];
                    _kernel[i] = kernel[i];
                    _idle[i] = idle[i];

                    usr[i] = proc.UserTime;
                    kernel[i] = proc.KernelTime;
                    idle[i] = proc.IdleTime;

                    _delta_usr[i] = usr[i] - _usr[i];
                    _delta_kernel[i] = kernel[i] - _kernel[i];
                    _delta_idle[i] = idle[i] - _idle[i];

                    long total = _delta_usr[i] + _delta_kernel[i];
                    long active = total - _delta_idle[i];
                    cpuStats[i] = active * 100 / total;
                }

                CPU_PERFORMANCE_NOTIFIED(cpuStats);
                long cpuTotalUsage = 0;
                cpuStats.ToList().ForEach(x => cpuTotalUsage += x);
                Debug.WriteLine($"CPU TOTAL: {cpuTotalUsage/cpuCores}%");
                await Task.Delay(1000);
            }
        }, cts.Token);
    }
}



