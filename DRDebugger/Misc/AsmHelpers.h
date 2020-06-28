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

template<typename T, typename S> S* GetModulePointer(void *pAddress, int offset = 0)
{
	return (S*)(GetModulePointer<T>(pAddress) + offset);
}

extern "C" __int64 __stdcall ThisPtrCall(void *functionPtr, void *thisPtr, void *arg1 = nullptr, void *arg2 = nullptr, void *arg3 = nullptr, void *arg4 = nullptr);

extern "C" __int64 __stdcall ThisPtrCallNoFixup(void *functionPtr, void *thisPtr, void *arg1 = nullptr, void *arg2 = nullptr, void *arg3 = nullptr, void *arg4 = nullptr);