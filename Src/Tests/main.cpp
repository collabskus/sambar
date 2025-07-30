#include <iostream>
#include <windows.h>

using namespace std;

typedef HWND(*__cdecl pGetWindow)();

int main() {
	HANDLE dllBase = LoadLibrary("Taskbar.dll");
	pGetWindow GetWindow = (pGetWindow)((char*)dllBase + 0x8B740);
	
	void* instance = new char[1024];
	memset(instance, 0, 1024);
	
	HWND hWnd = NULL;

	_asm {
		mov rcx, _pcls
		call GetWindow
		mov hWnd eax
	}

	cout << "hWnd: " << hWnd << endl;
}
