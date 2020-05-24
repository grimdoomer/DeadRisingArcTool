/*

*/

#include "TypeInfo.h"
#include <locale>
#include <codecvt>

// Forward declarations for command table.
__int64 PrintTypeInfo(WCHAR **argv, int argc);

const CommandEntry g_TypeInfoCommands[g_TypeInfoCommandsLength] =
{
	{ L"dt", L"Prints type info for the specified type or variable", PrintTypeInfo }
};

std::map<std::string, const TypeInfo*> g_TypeInfoDictionary;

bool RegisterTypeInfo(const TypeInfo *pTypeInfo)
{
	// Make sure we don't already have an entry for this type.
	if (g_TypeInfoDictionary.find(std::string(pTypeInfo->Name)) != g_TypeInfoDictionary.end())
		return false;

	// Add the type info to the dictionary.
	g_TypeInfoDictionary.emplace(std::string(pTypeInfo->Name), pTypeInfo);
	return true;
}

const TypeInfo *GetInfoForType(std::string typeName)
{
	// Check to see if the dictionary contains the type name.
	if (g_TypeInfoDictionary.find(typeName) != g_TypeInfoDictionary.end())
	{
		// Return the type info object.
		return g_TypeInfoDictionary.at(typeName);
	}

	// A type with matching name was not found in the dictionary.
	return nullptr;
}

__int64 PrintTypeInfo(WCHAR **argv, int argc)
{
	const TypeInfo *pTypeInfo = nullptr;

	// Setup the unicode converter.
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> unicConvert;

	// Make sure there is at least one argument to process.
	if (argc != 1)
	{
		// Invalid syntax.
		wprintf(L"Invalid command syntax\n");
		return 0;
	}

	// Check if the argument is a local variable or regular type.
	std::wstring sTypeToDisplay = argv[0];
	if (sTypeToDisplay.size() > 0 && sTypeToDisplay.at(0) == '$')
	{
		// Argument is a local variable.
		const LocalVariable *pLocal = GetLocalVar(sTypeToDisplay);
		if (pLocal == nullptr)
		{
			// No local variable found with matching name.
			wprintf(L"No local variable with name '%s' found\n", sTypeToDisplay.c_str());
			return 0;
		}

		// Try to get the type info from the local variable type.
		pTypeInfo = GetInfoForType(unicConvert.to_bytes(pLocal->TypeName));
		if (pTypeInfo == nullptr)
		{
			// No type info found for local variable type.
			wprintf(L"No type info found for type '%s'\n", pLocal->TypeName.c_str());
			return 0;
		}

		// Print the type info header.
		wprintf(L"%s : %p\n", pLocal->TypeName.c_str(), pLocal->Value);

		// Loop and print all of the fields for the type.
		const FieldInfo *pField = &pTypeInfo->Fields[0];
		while (pField->Type != FieldType_Terminator)
		{
			// Check the field type and handle accordingly.
			switch (pField->Type)
			{
			case FieldType_Number:
			{
				// Check the field size and read accordingly.
				__int64 fieldValue = 0;
				BYTE *pFieldPtr = (BYTE*)pLocal->Value + pField->Offset;
				switch (pField->ElementSize)
				{
				case 1: fieldValue = *(BYTE*)pFieldPtr; break;
				case 2: fieldValue = *(WORD*)pFieldPtr; break;
				case 4: fieldValue = *(DWORD*)pFieldPtr; break;
				case 8: fieldValue = *(ULONGLONG*)pFieldPtr; break;
				default: DebugBreak(); break;
				}

				wprintf(L"\t[0x%x] %S: %llu\n", pField->Offset, pField->Name, fieldValue);
				break;
			}
			case FieldType_String:
			{
				wprintf(L"\t[0x%x] %S: %S\n", pField->Offset, pField->Name, (CHAR*)pLocal->Value + pField->Offset);
				break;
			}
			}

			// Next field.
			pField++;
		}
	}
	else
	{
		// Argument is a type name, check if we have typeinfo registered for it.
		pTypeInfo = GetInfoForType(unicConvert.to_bytes(sTypeToDisplay));
	}

	return 0;
}