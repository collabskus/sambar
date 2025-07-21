using System;
using System.Runtime.InteropServices;

enum MH_STATUS: int
{
    MH_UNKNOWN = -1,
    MH_OK = 0,
    MH_ERROR_ALREADY_INITIALIZED,
    MH_ERROR_NOT_INITIALIZED,
    MH_ERROR_ALREADY_CREATED,
    MH_ERROR_NOT_CREATED,
    MH_ERROR_ENABLED,
    MH_ERROR_DISABLED,
    MH_ERROR_NOT_EXECUTABLE,
    MH_ERROR_UNSUPPORTED_FUNCTION,
    MH_ERROR_MEMORY_ALLOC,
    MH_ERROR_MEMORY_PROTECT,
    MH_ERROR_MODULE_NOT_FOUND,
    MH_ERROR_FUNCTION_NOT_FOUND
}

class Hooker {

	[DllImport("Minhook.x64.dll", SetLastError=true)]
	static extern MH_STATUS MH_Initialize();
	[DllImport("Minhook.x64.dll", SetLastError=true)]
	static extern MH_STATUS MH_CreateHook(nint pTarget, nint pDetour, nint ppOriginal);
	[DllImport("Minhook.x64.dll", SetLastError=true)]
	static extern MH_STATUS MH_EnableHook(nint ptarget);

	[DllImport("kernel32.dll", SetLastError=true)]
	static extern nint LoadLibrary(string module);
	[DllImport("kernel32.dll", SetLastError=true)]
	static extern nint GetProcAddress(nint dllBase, string procName);
	[DllImport("user32.dll", SetLastError=true)]
	static extern int MessageBoxA(nint hWnd, string text, string caption, uint type);

	static void HandleError(MH_STATUS code) 
	{
		if(code != MH_STATUS.MH_OK) {
			throw new Exception($"MinHook Error: {code}");
		}
	}

	delegate int MessageBoxA_Delegate(nint hWnd, nint lpText, nint lpCaption, uint uType);
	static int MessageBoxA_Hook(nint hWnd, nint lpText, nint lpCaption, uint uType)
	{
		Console.WriteLine("No message box for you !");
		return 0;
	}

	static void Main() {

		nint user32Base = (nint)LoadLibrary("user32.dll");
		nint targetFnPtr = GetProcAddress(user32Base, "MessageBoxA");
		nint hookFnPtr = Marshal.GetFunctionPointerForDelegate<MessageBoxA_Delegate>(MessageBoxA_Hook);

		HandleError(MH_Initialize());
		HandleError(MH_CreateHook(targetFnPtr, hookFnPtr, 0));
		HandleError(MH_EnableHook(0));

		MessageBoxA(0, "hello", "message", (uint)0x00000000L);
	}
}


