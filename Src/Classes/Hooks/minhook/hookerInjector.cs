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
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint GetProcAddress(nint hModule, string procName);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint LoadLibrary(string moduleName);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint CreateRemoteThread(
		nint hProcess,
		nint securityAttributes,
		nuint dwStackSize,
		nint localFn,
		nint fnArgs,
		uint dwCreationFlags,
		out uint threadId
	);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint VirtualAllocEx(
		nint hProcess,
		nint lpAddress,
		nuint dwSize,
		uint allocationType,
		uint protect
	);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern int WriteProcessMemory(
		nint hProcess,
		nint baseAddress,
		nint buffer,
		nuint size,
		nint bytesWritten
	);

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
		for (int i = 0; i < 100; i++)
		{
			StringBuilder str = new(256);
			GetModuleFileNameA(modules[i], str, str.Capacity);
			//Console.WriteLine($"m: {str.ToString()}");
			if (str.Length > 1) dlls[str.ToString()] = modules[i];
		}
		return dlls;
	}

	static nint FindModuleInProcess(nint hProcess, string moduleName)
	{
		var modules = GetModulesInProcess(hProcess);
		var kernel32 = modules.First(pair => pair.Key.ToLower().Contains(moduleName));
		return kernel32.Value;
	}

	static nint GetRVAOfProcInModule(nint hProcess, string dll, string procName)
	{
		var modules = GetModulesInProcess(hProcess);
		nint dllBase = modules[dll];
		return GetProcAddress(dllBase, procName);
	}

	static void CallFunctionInProcess(nint hProcess, nint fnAddressInProcess, nint fnArgsInProcess)
	{
		CreateRemoteThread(hProcess, 0, 0, fnAddressInProcess, fnArgsInProcess, 0, out uint threadId);
	}

	static unsafe void Inject()
	{
		Console.Write("processId: ");
		uint processId = Convert.ToUInt32(Console.ReadLine());
		const uint PROCESS_ALL_ACCESS = 0x1FFFFF;
		nint hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
		Console.WriteLine($"hProcess: {hProcess}");
		//CloseHandle(hProcess);

		// 1. call target's kernerl32!LoadLibrary with argument "hooker.dll"
		foreach (var item in GetModulesInProcess(hProcess))
		{
			Console.WriteLine(item.Key);
		}
		nint kernel32Base = FindModuleInProcess(hProcess, "kernel32");
		Console.WriteLine($"kernel32base: {kernel32Base}");
		nint loadLibraryRva = GetRVAOfProcInModule(hProcess, "KERNEL32", "LoadLibrary");
		Console.WriteLine($"loadLibraryRva: {loadLibraryRva}");
		string hookerDllName = "hooker.dll\0";
		byte[] data = Encoding.Unicode.GetBytes(hookerDllName);
		byte* dataPtr = (byte*)Marshal.AllocHGlobal(data.Length);
		//Marshal.StructureToPtr<byte[]>(data, dataPtr, false);
		for (int i = 0; i < data.Length; i++)
		{
			*(dataPtr + i) = data[i];
		}
		const uint MEM_RESERVE = 0x00002000;
		const uint MEM_COMMIT = 0x00001000;
		const uint PAGE_READWRITE = 0x04;
		nint argPtr = VirtualAllocEx(hProcess, 0, (nuint)data.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
		WriteProcessMemory(hProcess, argPtr, (nint)dataPtr, (nuint)data.Length, 0);
		CallFunctionInProcess(hProcess, kernel32Base + loadLibraryRva, argPtr);
		Marshal.FreeHGlobal((nint)dataPtr);
		// check if hooker.dll is loaded in target
		var mods = GetModulesInProcess(hProcess);
		foreach (var mod in mods)
		{
			Console.WriteLine(mod.Key);
		}
		// 2. call hooker's Hook() function
	}

	static void Main()
	{
		Inject();
	}
}
