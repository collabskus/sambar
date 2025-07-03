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
    public void CountersInit()
    {
        CpuCounter();
    }

    public void CpuCounter()
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
    }
}
