using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#nullable enable

class HookerInjector
{
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandles, uint dwProcessId);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern int CloseHandle(nint handle);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern int GetModuleFileNameA(nint hModule, StringBuilder str, int nSize);

	static unsafe void Main()
	{
		Console.Write("processId: ");
		uint processId = Convert.ToUInt32(Console.ReadLine());
		const uint PROCESS_ALL_ACCESS = 0x1FFFFF;
		nint hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
		Console.WriteLine($"hProcess: {hProcess}");
		//GetModulesInProcess(hProcess);
		nint kernel32 = FindKernel32InProcess(hProcess);
		Console.WriteLine($"kernel32: {kernel32}");
		CloseHandle(hProcess);
	}

	[DllImport("psapi.dll", SetLastError = true)]
	static extern int EnumProcessModules(nint hProcess, [Out] nint ptrToModuleArray, int moduleArrayLength, [Out] nint sizeNeeded);

	static unsafe Dictionary<string, nint>? GetModulesInProcess(nint hProcess)
	{
		nint* modules = stackalloc nint[100];
		uint sizeNeeded = 0;
		if (EnumProcessModules(hProcess, (nint)modules, 100 * sizeof(nint), (nint)(&sizeNeeded)) == 0)
		{
			Console.WriteLine($"EnumProcessModules() failed, win32: {Marshal.GetLastWin32Error()}");
			return null;
		}
		Console.WriteLine("processes...");
		Dictionary<string, nint> dlls = new();
		for (int i = 0; i < 100; i++) {
			StringBuilder str = new(256);
			GetModuleFileNameA(modules[i], str, str.Capacity);
			//Console.WriteLine($"m: {str.ToString()}");
			if(str.Length > 1) dlls[str.ToString()] = modules[i];
		}
		return dlls;
	}

	static unsafe nint FindKernel32InProcess(nint hProcess) {
		var modules = GetModulesInProcess(hProcess);
		var kernel32 = modules.First(pair => pair.Key.ToLower().Contains("kernel32"));
		return kernel32.Value;
	}

	static void Inject()
	{
		// 1. call target's kernerl32!LoadLibrary with argument "hooker.dll"
		// 2. call hooker's Hook() function
	}
}
