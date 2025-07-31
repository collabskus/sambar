using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

#nullable enable

class HookerInjector
{
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandles, uint dwProcessId);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern int CloseHandle(nint handle);
	[DllImport("psapi.dll", SetLastError = true)]
	static extern int GetModuleFileNameExA(nint hProcess, nint hModule, StringBuilder str, int nSize);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint GetProcAddress(nint hModule, string procName);
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint LoadLibraryA(string moduleName);
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
	static extern int EnumProcessModulesEx(nint hProcess, [Out] nint ptrToModuleArray, int moduleArrayLength, [Out] nint sizeNeeded, uint flags);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern int WaitForSingleObject(nint hHandle, uint dwMilliseconds);

	static unsafe Dictionary<string, nint>? GetModulesInProcess(nint hProcess)
	{
		int initArrayLength = 1024;
		nint* modules = stackalloc nint[initArrayLength];
		int cb = initArrayLength * sizeof(nint);
		uint sizeNeeded = 0;
		uint LIST_MODULES_ALL = 0x03;
		if (EnumProcessModulesEx(hProcess, (nint)modules, cb, (nint)(&sizeNeeded), LIST_MODULES_ALL) > 0)
		{
			Dictionary<string, nint> dlls = new();
			for (int i = 0; i < sizeNeeded / sizeof(nint); i++)
			{
				StringBuilder str = new(1024);
				if (GetModuleFileNameExA(hProcess, modules[i], str, str.Capacity) > 0)
				{
					dlls[str.ToString()] = modules[i];
				}
			}
			Console.WriteLine("printing dlls...");
			for (int i = 0; i < dlls.Count; i++)
			{
				Console.WriteLine($"{i}: {dlls.ElementAt(i).Key}");
			}
			return dlls;
		}
		Console.WriteLine($"EnumProcessModules() failed, win32: {Marshal.GetLastWin32Error()}");
		return null;


	}

	static nint FindModuleInProcess(nint hProcess, string moduleName)
	{
		var modules = GetModulesInProcess(hProcess);
		var module = modules.First(pair => pair.Key.ToLower().Contains(moduleName));
		return module.Value;
	}

	static nint GetRVAOfProcInModule(string moduleName, string procName)
	{
		nint dllBase = LoadLibraryA($"{moduleName}.dll");
		return GetProcAddress(dllBase, procName) - dllBase;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool GetExitCodeThread(nint hThread, out uint exitCode);

	static void CallFunctionInProcess(nint hProcess, nint fnAddressInProcess, nint fnArgsInProcess)
	{
		nint remoteThread = 0;
		if ((remoteThread = CreateRemoteThread(hProcess, 0, 0, fnAddressInProcess, fnArgsInProcess, 0, out uint threadId)) == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		Console.WriteLine($"remoteThread: {remoteThread}");
		int exitReason = WaitForSingleObject(remoteThread, 5000);
		GetExitCodeThread(remoteThread, out uint exitCode);
		Console.WriteLine($"exitReason: {exitReason}, threadReturn: {exitCode}");
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
		nint kernel32Base = FindModuleInProcess(hProcess, "kernel32");
		Console.WriteLine($"kernel32base: {kernel32Base}");
		nint loadLibraryRva = GetRVAOfProcInModule("kernel32", "LoadLibraryW");
		Console.WriteLine($"loadLibraryRva: {loadLibraryRva}");

		string hookerDllPath = new FileInfo("hooker.dll").FullName + "\0";
		Console.WriteLine($"hookerDllPath: {hookerDllPath}");
		byte[] data = Encoding.Unicode.GetBytes(hookerDllPath);
		const uint MEM_RESERVE = 0x00002000;
		const uint MEM_COMMIT = 0x00001000;
		const uint PAGE_READWRITE = 0x04;
		nint argPtr = VirtualAllocEx(hProcess, 0, (nuint)data.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
		Console.WriteLine($"argPtr: {argPtr}");

		fixed (void* dataPtr = data)
		{
			if (WriteProcessMemory(hProcess, argPtr, (nint)dataPtr, (nuint)data.Length, 0) == 0)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		Console.WriteLine("calling loadlibrary in target ...");
		CallFunctionInProcess(hProcess, kernel32Base + loadLibraryRva, argPtr);
		GetModulesInProcess(hProcess);
		// check if hooker.dll is loaded in target
		// 2. call hooker's Hook() function
	}

	static void Main()
	{
		Inject();
	}
}
