/*

*/

#pragma once

extern "C" void *SnatcherModuleHandle;

template<typename T> T GetModuleAddress(void *pAddress)
{
	return (T)((__int64)pAddress - ((__int64)0x140000000 - (__int64)SnatcherModuleHandle));
}

template<typename T> T GetModulePointer(void *pAddress)
{
	return *(T*)GetModuleAddress<void*>(pAddress);
}

extern "C" __int64 __stdcall ThisPtrCall(void *functionPtr, void *thisPtr, void *arg1, void *arg2, void *arg3, void *arg4);