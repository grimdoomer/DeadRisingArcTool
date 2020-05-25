/*

*/

#pragma once
#include <Windows.h>
#include "DRDebugger.h"

// sizeof = 0x60
struct cResource
{
	/* 0x00 */ void			*vtable;
	/* 0x08 */ char			mPath[64];
	/* 0x48 */ DWORD		mRefCount;
	/* 0x4C */ DWORD		mAttr;
	/* 0x50 */ DWORD		mState;
	/* 0x54 */ DWORD		mSize;
	/* 0x58 */ ULONGLONG	mID;
};
static_assert(sizeof(cResource) == 0x60, "cResource struct has incorrect size");

// sizeof = 0x24458
struct sResource
{
	/* 0x00 */ void					*vtable;
	/* 0x08 */ CRITICAL_SECTION		ListLock;

	/* 0x2248 */ cResource			**pResourceEntries;
};

class sResourceImpl
{
protected:

public:
	static void InitializeLua();

	template<typename T> static T GetGameResourceAsType(void *pTypeDTI, char *psFileName, int dwFlags)
	{
		return (T)GetGameResource(pTypeDTI, psFileName, dwFlags);
	}

	static cResource* GetGameResource(void *pTypeDTI, char *psFileName, int dwFlags);

	static cResource* GetGameResourceByIndex(int index);


	static void IncrementResourceRefCount(cResource *pResource);


	static void PrintLoadedResources(const char *psFilter = nullptr);
};