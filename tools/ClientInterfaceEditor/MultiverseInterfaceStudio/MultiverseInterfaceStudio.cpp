#include "stdafx.h"
#include "MultiverseInterfaceStudio.h"

#define MAX_LOADSTRING 100

#define APPNAME L"MultiverseInterfaceStudio"

typedef int (__cdecl  *STARTFCN)(LPSTR, LPWSTR, int, GUID *, WCHAR *pszSettings);
typedef int (__cdecl  *SETUPFCN)(LPSTR, LPWSTR, GUID *);
typedef int (__cdecl  *REMOVEFCN)(LPSTR, LPWSTR);

void ShowNoComponentError(HINSTANCE hInstance)
{
	WCHAR szErrorString[1000];
	WCHAR szCaption[1000];

	LoadStringW(hInstance, IDS_ERR_MSG_FATAL, szErrorString, 1000);
	LoadStringW(hInstance, IDS_ERR_FATAL_CAPTION, szCaption, 1000);
	
	MessageBoxW(NULL, szErrorString, szCaption, MB_OK|MB_ICONERROR);
}

int APIENTRY _tWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow)
{
    USES_CONVERSION;
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	int nRetVal = -1;
	WCHAR szExeFilePath[MAX_PATH];
	HKEY hKeyAppEnv90Hive = NULL;

	if(RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"Software\\Microsoft\\AppEnv\\9.0", 0, KEY_READ, &hKeyAppEnv90Hive) == ERROR_SUCCESS)
	{
		DWORD dwType;
		DWORD dwSize = MAX_PATH;
		RegQueryValueExW(hKeyAppEnv90Hive, L"AppenvStubDLLInstallPath", NULL, &dwType, (LPBYTE)szExeFilePath, &dwSize);
		RegCloseKey(hKeyAppEnv90Hive);
	}

	if(GetFileAttributesW(szExeFilePath) == INVALID_FILE_ATTRIBUTES)
	{
		//If we cannot find it at a registered location, then try in the same directory as the application
		GetModuleFileNameW(NULL, szExeFilePath, MAX_PATH);
		WCHAR *pszStartOfFileName = wcsrchr(szExeFilePath, '\\');
		if(!pszStartOfFileName)
		{
			return -1;
		}
		*pszStartOfFileName = 0;
		wcscat_s(szExeFilePath, MAX_PATH, L"\\appenvstub.dll");

		if(GetFileAttributesW(szExeFilePath) == INVALID_FILE_ATTRIBUTES)
		{
			//If the file cannot be found in the same directory as the calling exe, then error out.
			ShowNoComponentError(hInstance);
			return -1;
		}
	}

	HMODULE hModStubDLL = LoadLibraryW(szExeFilePath);
	if(!hModStubDLL)
	{
		ShowNoComponentError(hInstance);
		return -1;
	}

	//Check to see if the /setup arg was passed. If so, then call the Setup method 
	//	to prepare the registry for the AppID.
	int nArgs = 0;
	bool fDoSetup = false;
	bool fDoRemove = false;
	LPWSTR *szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
	for(int i = 0 ; i < nArgs ; i++)
	{
		if(_wcsicmp(szArglist[i], L"/setup") == 0)
		{
			fDoSetup = true;
		}
		if(_wcsicmp(szArglist[i], L"/remove") == 0)
		{
			fDoRemove = true;
		}
	}
	LocalFree(szArglist);

	if(fDoSetup && fDoRemove)
	{
		//Cannot have both /setup and /remove on the command line at the same time.
        return -1;
	}

	if(fDoSetup)
	{
		SETUPFCN Setup = (SETUPFCN)GetProcAddress(hModStubDLL, "Setup");
		if(!Setup)
		{
			ShowNoComponentError(hInstance);
			return -1;
		}

		nRetVal = Setup(T2A(lpCmdLine), APPNAME, NULL);
	}
	else if(fDoRemove)
	{
		REMOVEFCN Remove = (REMOVEFCN)GetProcAddress(hModStubDLL, "Remove");
		if(!Remove)
		{
			ShowNoComponentError(hInstance);
			return -1;
		}

		nRetVal = Remove(T2A(lpCmdLine), APPNAME);
	}
	else
	{
		USES_CONVERSION;
		STARTFCN Start = (STARTFCN)GetProcAddress(hModStubDLL, "Start");
		if(!Start)
		{
			ShowNoComponentError(hInstance);
			return -1;
		}

		nRetVal = Start(T2A(lpCmdLine), APPNAME, nCmdShow, NULL, NULL);
	}

	FreeLibrary(hModStubDLL);

    return nRetVal;
}




