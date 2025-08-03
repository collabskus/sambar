using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

public class QueryPdb
{
	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymInitialize(
		nint hProcess,
		string pdbPath,
		bool fInvadeProcess
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern uint SymSetOptions(
		uint SymOptions
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern ulong SymLoadModuleEx(
		nint hProcess,
		[Optional] nint hFile,
		string imageName,
		string moduleName,
		ulong baseOfDll,
		uint dllSize,
		nint data,
		uint flags
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymFromName(nint hProcess, [MarshalAs(UnmanagedType.LPTStr)] string symbolName, ref SYMBOL_INFO info);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint GetCurrentProcess();

	public delegate bool PsymEnumeratesymbolsCallback(ref SYMBOL_INFO info, uint symbolSize, [In, Optional] nint userContext);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymEnumSymbols(
		nint hProcess,
		ulong baseOfDll,
		string mask,
		PsymEnumeratesymbolsCallback callback,
		nint userContext
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymGetSearchPath(
		nint hProcess,
		StringBuilder SearchPath,
		uint SearchPathLength
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	public static extern bool SymGetSymbolFile(
		nint hProcess,
		string SymPath,
		string ImageFile,
		int Type,
		StringBuilder SymbolFile,
		int SymbolFileSize,
		StringBuilder DbgFile,
		int DbgFileSize
	);

	[DllImport("dbghelp.dll", SetLastError = true)]
	static extern bool SymSetSearchPath(IntPtr hProcess, string SearchPath);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint LoadLibraryEx(string lpFileName, nint hFile, uint dwFlags);

	public static void Main()
	{
		string pdbFilePath = "Taskbar.pdb";
		long pdbFileSize = File.OpenRead(pdbFilePath).Length;
		nint hProcess = GetCurrentProcess();
		if (!SymInitialize(hProcess, pdbFilePath, false))
		{
			Console.WriteLine($"hProcess() failed: {Marshal.GetLastWin32Error()}");
			return;
		}
		uint dllBase = 0x10000000;
		SymLoadModuleEx(
			hProcess,
			0,
			pdbFilePath,
			null,
			(ulong)dllBase,
			(uint)pdbFileSize,
			0,
			0
		);

		// set mask to *GetWindow* or *CTaskListWnd* to resolve class methods
		SymEnumSymbols(hProcess, dllBase, "*CTaskListWnd*", (ref SYMBOL_INFO info, uint size, nint ctx) =>
		{
			Console.WriteLine($"[ NAME ]: {info.Name}, [ RVA ]: {info.Address - info.ModBase}");
			return true;
		}, 0);

		/// <summary>
		/// Taskbar.dll
		/// </summary>

		//string dllFile = "Taskbar.dll";
		//nint dllBase = LoadLibraryEx(dllFile, 0, 0x00000020);
		//nint hProcess = GetCurrentProcess();
		//if (!SymInitialize(hProcess, null, false))
		//{
		//	Console.WriteLine($"SymInitialize failed: 0x{Marshal.GetLastWin32Error():X}");
		//	return;
		//}
		//ulong symbolBase = SymLoadModuleEx(
		//	hProcess,
		//	IntPtr.Zero,
		//	dllFile,
		//	null,
		//	(ulong)dllBase,
		//	0,
		//	IntPtr.Zero,
		//	0
		//);

		//if (symbolBase == 0)
		//{
		//	Console.WriteLine($"SymLoadModuleEx failed: 0x{Marshal.GetLastWin32Error():X}");
		//	return;
		//}
		//SymEnumSymbols(hProcess, symbolBase, "*", (ref SYMBOL_INFO info, uint size, nint ctx) =>
		//{
		//	Console.WriteLine($"[SYMBOL] {info.Name}");
		//	return true;
		//}, IntPtr.Zero);
	}
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct SYMBOL_INFO
{
	public uint SizeOfStruct;
	public uint TypeIndex;
	public ulong Reserved0;
	private ulong Reserved1;
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
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2000)]
	public string Name;
	public static SYMBOL_INFO Default = new() { SizeOfStruct = (uint)Marshal.SizeOf(typeof(SYMBOL_INFO)), MaxNameLen = 2000 };
}

public enum SymTagEnum : ulong
{
	SymTagNull = 0,
	SymTagExe = 1,
	SymTagCompiland = 2,
	SymTagCompilandDetails = 3,
	SymTagCompilandEnv = 4,
	SymTagFunction = 5,
	SymTagBlock = 6,
	SymTagData = 7,
	SymTagAnnotation = 8,
	SymTagLabel = 9,
	SymTagPublicSymbol = 10,
	SymTagUDT = 11,
	SymTagEnum = 12,
	SymTagFunctionType = 13,
	SymTagPointerType = 14,
	SymTagArrayType = 15,
	SymTagBaseType = 16,
	SymTagTypedef = 17,
	SymTagBaseClass = 18,
	SymTagFriend = 19,
	SymTagFunctionArgType = 20,
	SymTagFuncDebugStart = 21,
	SymTagFuncDebugEnd = 22,
	SymTagUsingNamespace = 23,
	SymTagVTableShape = 24,
	SymTagVTable = 25,
	SymTagCustom = 26,
	SymTagThunk = 27,
	SymTagCustomType = 28,
	SymTagManagedType = 29,
	SymTagDimension = 30
}

