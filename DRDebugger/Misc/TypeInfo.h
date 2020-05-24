/*

*/

#pragma once
#include <Windows.h>
#include <string>
#include <map>
#include "CommandManager.h"

enum FieldType
{
	FieldType_Pointer,
	FieldType_String,
	FieldType_Number,
	FieldType_Array,
	FieldType_Struct,

	FieldType_Category,
	
	FieldType_Terminator
};

struct ArrayFieldDefinition
{
	unsigned int	MaxLength;
	const char		*LengthFieldName;
};

struct FieldInfo
{
	FieldType		Type;			// Type of field
	const char		*Name;			// Name of the field
	DWORD			Offset;			// Offset of the field relative to the start of the object
	DWORD			ElementSize;	// Size of the field type
	void			*Definition;	// Pointer to additional data describing the field
};

struct TypeInfo
{
	const char		*Name;		// Name of the type
	DWORD			Size;		// Size of the type
	FieldInfo		Fields[];	// Array of fields in the type
};

bool RegisterTypeInfo(const TypeInfo *pTypeInfo);

const TypeInfo *GetInfoForType(std::string typeName);

// Command table info for TypeInfo related commands.
const int g_TypeInfoCommandsLength = 1;
extern const CommandEntry g_TypeInfoCommands[];