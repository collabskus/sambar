/*
*   Utility to get Module File Name without admin elevated rights
*   Process?.MainModule?.FileName fails in non elevated environments
*   Relies on the undocumented NtQuerySystemInformation() function inside ntdll.dll
*   COMPILE: gcc ntquery.c -lntdll
*/

#include <windows.h>
#include <BaseTsd.h>
#include <ntdef.h>
#include <winternl.h>
#include <winbase.h>
#include <stdio.h>

struct SYSTEM_PROCESS_ID_INFORMATION
{
    HANDLE ProcessId;
    UNICODE_STRING ImageName;
};

// Function to convert NT path to DOS path
BOOL ConvertNtPathToDosPath(LPCWSTR ntPath, LPWSTR dosPath, DWORD dosPathSize) {
    WCHAR drives[256];
    WCHAR deviceName[256];
    WCHAR driveLetter[4] = L"A:";
    
    // Get all logical drives
    if (!GetLogicalDriveStringsW(sizeof(drives)/sizeof(WCHAR), drives)) {
        return FALSE;
    }
    
    // Check each drive
    for (WCHAR* drive = drives; *drive; drive += 4) { // Each drive is "C:\" format (4 chars)
        driveLetter[0] = drive[0];  // Extract drive letter
        
        // Query the NT device name for this drive letter
        if (QueryDosDeviceW(driveLetter, deviceName, sizeof(deviceName)/sizeof(WCHAR))) {
            size_t deviceLen = wcslen(deviceName);
            
            // Check if NT path starts with this device name
            if (wcsnicmp(ntPath, deviceName, deviceLen) == 0) {
                // Found matching device - construct DOS path
                LPCWSTR remainingPath = ntPath + deviceLen;
                
                // Use wcscpy and wcscat instead of swprintf
                dosPath[0] = drive[0];  // Drive letter
                dosPath[1] = L':';      // Colon
                dosPath[2] = L'\0';     // Null terminate
                
                // Add the remaining path
                wcscat_s(dosPath, dosPathSize, remainingPath);
                
                return TRUE;
            }
        }
    }
    
    // If no match found, just copy the original path
    wcscpy_s(dosPath, dosPathSize, ntPath);
    return FALSE;
}

int main() {
	printf("enter processId: ");
	DWORD processId;
	scanf("%lu", &processId);
    struct SYSTEM_PROCESS_ID_INFORMATION info = { (HANDLE)(ULONG_PTR)processId, { 0, 0x256, NULL } };
    
    info.ImageName.Buffer = (PWSTR)LocalAlloc(LMEM_FIXED, info.ImageName.MaximumLength);
    if(!info.ImageName.Buffer) {
        printf("Memory allocation failed\n");
        return 1;
    }
    
    NTSTATUS status = NtQuerySystemInformation(0x58, &info, sizeof(info), 0);
    
    printf("Status: %x\n", status);
    
    if (status == 0 && info.ImageName.Length > 0) {
        DWORD charCount = info.ImageName.Length / sizeof(WCHAR);
        DWORD maxChars = info.ImageName.MaximumLength / sizeof(WCHAR);
        
        if (charCount < maxChars) {
            info.ImageName.Buffer[charCount] = L'\0';
            
            // Convert to DOS path
            WCHAR dosPath[MAX_PATH];
            if (ConvertNtPathToDosPath(info.ImageName.Buffer, dosPath, MAX_PATH)) {
                printf("Process path: %ls\n", dosPath);
            } else {
                printf("Process path: %ls\n", info.ImageName.Buffer);
            }
        } else {
            printf("Process path: %.*ls\n", charCount, info.ImageName.Buffer);
        }
    } else {
        printf("Failed to get process information\n");
    }
    
    LocalFree(info.ImageName.Buffer);
    return 0;
}
