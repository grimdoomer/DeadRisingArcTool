/*

*/

#pragma once
#include <Windows.h>
#include "CommandManager.h"
#include "Misc/TypeInfo.h"

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

const TypeInfo cResourceTypeInfo =
{
	"cResource", sizeof(cResource),
	{
		{ FieldType_Pointer, "*vtable", FIELD_OFFSET(cResource, vtable), sizeof(void*), nullptr },
		{ FieldType_String, "mPath", FIELD_OFFSET(cResource, mPath), sizeof(char), (void*)64 },
		{ FieldType_Number, "mRefCount", FIELD_OFFSET(cResource, mRefCount), sizeof(DWORD), nullptr },
		{ FieldType_Number, "mAttr", FIELD_OFFSET(cResource, mAttr), sizeof(DWORD), nullptr },
		{ FieldType_Number, "mState", FIELD_OFFSET(cResource, mState), sizeof(DWORD), nullptr },
		{ FieldType_Number, "mSize", FIELD_OFFSET(cResource, mSize), sizeof(DWORD), nullptr },
		{ FieldType_Number, "mID", FIELD_OFFSET(cResource, mID), sizeof(ULONGLONG), nullptr },
		{ FieldType_Terminator, nullptr, 0, 0, nullptr }
	}
};

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
	template<typename T> static T GetGameResourceAsType(void *pTypeDTI, char *psFileName, int dwFlags)
	{
		return (T)GetGameResourceAsType(pTypeDTI, psFileName, dwFlags);
	}

	static cResource* GetGameResourceAsType(void *pTypeDTI, char *psFileName, int dwFlags);

	static void IncrementResourceRefCount(cResource *pResource);
};

// Table of commands for sResource objects.
const int g_sResourceCommandsLength = 2;
extern const CommandEntry g_sResourceCommands[];