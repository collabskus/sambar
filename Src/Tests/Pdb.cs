using System;
using System.IO;
using System.Runtime.InteropServices;

public class Pdb
{
	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymInitialize(nint hProcess, string pdbPath, bool fInvadeProcess);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern ulong SymLoadModuleEx(
		nint hProcess,
		nint hFile,
		string imageName,
		string moduleName,
		ulong baseOfDll,
		uint dllSize,
		nint data,
		uint flags
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymFromName(nint hProcess, string symbolName, ref SYMBOL_INFO info);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint GetCurrentProcess();

	nint hProcess = GetCurrentProcess();

	public Pdb()
	{
		if (!SymInitialize(hProcess, "Taskbar.pdb", false))
		{
			Console.WriteLine($"SymInitialize() failed, win32: {Marshal.GetLastWin32Error()}");
		}

		var pdbFile = File.OpenRead("Taskbar.pdb");
		ulong baseAddressPdb = SymLoadModuleEx(
			hProcess,
			0,
			"Taskbar.pdb",
			null,
			0x10000000,
			(uint)pdbFile.Length,
			0,
			0
		);

		Console.WriteLine($"base address of Taskbar.pdb: {baseAddressPdb}");
	}

	public long GetRva(string name)
	{
		SYMBOL_INFO info = new();
		info.SizeOfStruct = (uint)Marshal.SizeOf<SYMBOL_INFO>();
		if (!SymFromName(hProcess, name, ref info))
		{
			Console.WriteLine($"SymFromName() failed, win32: {Marshal.GetLastWin32Error()}");
			return -1;
		}
		return (long)(info.Address - info.Address);
	}

	public static void Main()
	{
		Pdb pdb = new();
		string mangledName = "/?GetWindow@CTaskListWnd@@UEAAPEAUHWND__@@XZ";
		long rva = pdb.GetRva(mangledName);
		Console.WriteLine($"Rva: {rva}");
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct SYMBOL_INFO
{
	public uint SizeOfStruct;
	public uint TypeIndex;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public ulong[] Reserved;
	public uint Index;
	public uint Size;
	public ulong ModBase;
	public uint Flags;
	public ulong Value;
	public ulong Address;
	public uint Register;
	public uint Scope;
	public uint Tag;
	public uint NameLen;
	public uint MaxNameLen;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
	public string Name;
}

