/*

*/

#pragma once
#include <Windows.h>

// sizeof = 0x38
struct MtDTI
{
	/* 0x00 */ void		*vtable;
	/* 0x08 */ char		*pObjectName;
	/* 0x10 */ void		*pUnknown1;
	/* 0x18 */ void		*pUnknown2;
	/* 0x20 */ MtDTI	*pParentObject;

	union
	{
		/* 0x28 */ DWORD	ObjectSize : 24;		// Upper 8 bits are flags, lower 24 bits are object size
		/* 0x2B */ DWORD	Flags : 8;
	};

	/* 0x2C */ DWORD	UnkId;		// Unique ID for the object?
	/* 0x30 */ void		*pUnknown3;
};
static_assert(sizeof(MtDTI) == 0x38, "MtDTI struct has incorrect size");

class MtDTIImpl
{
public:
	// MtDTI(this, char *psObjectName, MtDTI *pParentObject, DWORD dwObjectSize);
};