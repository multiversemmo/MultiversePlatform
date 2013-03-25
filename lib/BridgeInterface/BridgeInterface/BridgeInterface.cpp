// BridgeInterface.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "Socket.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	int rv = Socket::Startup();
	if (rv < 0)
		return false;
	return true;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

