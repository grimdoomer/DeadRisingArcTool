/*

*/

#include "sResource.h"
#include <stdio.h>
#include <string>
#include "Misc/AsmHelpers.h"
#include "MtDTI.h"
#include "Graphics/rModel.h"

// sResource singleton instance pointer.
void *g_sResourceSingletonInst = (void*)0x141CF27F8;

void sResourceImpl::InitializeLua()
{
	// Register cResource:
	g_LuaState.new_usertype<cResource>("cResource",
		"vtable", &cResource::vtable,
		"mPath", &cResource::mPath,
		"mRefCount", &cResource::mRefCount,
		"mAttr", &cResource::mAttr,
		"mState", &cResource::mState,
		"mSize", &cResource::mSize,
		"mID", &cResource::mID,
		
		"To_rModel", [](const cResource& self) { return (rModel*)&self; });

	// Register sResourceImpl:
	g_LuaState.new_usertype<sResourceImpl>("sResourceImpl",
		"GetGameResourceAsType", &sResourceImpl::GetGameResource,
		"GetGameResourceByIndex", &sResourceImpl::GetGameResourceByIndex,
		"IncrementResourceRefCount", &sResourceImpl::IncrementResourceRefCount,
		"PrintLoadedResources", sol::overload([]() { sResourceImpl::PrintLoadedResources(); }, [](const char *psFilter) { sResourceImpl::PrintLoadedResources(psFilter); }));
}

cResource* sResourceImpl::GetGameResource(void *pTypeDTI, char *psFileName, int dwFlags)
{
	return (cResource*)ThisPtrCall((void*)0x14063BC60, GetModulePointer<void*>(g_sResourceSingletonInst), pTypeDTI, psFileName, (void*)dwFlags, nullptr);
}

void sResourceImpl::IncrementResourceRefCount(cResource *pResource)
{
	ThisPtrCall((void*)0x14063B3B0, GetModulePointer<void*>(g_sResourceSingletonInst), pResource, nullptr, nullptr, nullptr);
}

void sResourceImpl::PrintLoadedResources(const char *psFilter)
{
	std::string sFilter;
	bool bFilterFront = false;

	// Check if there is a filter argument.
	if (psFilter != nullptr)
	{
		// Check if this is a front filter or rear filter.
		if (psFilter[0] == '*')
		{
			// Rear filter.
			sFilter = &psFilter[1];
		}
		else
		{
			// Front filter.
			sFilter = psFilter;
			sFilter = sFilter.substr(0, sFilter.size() - 1);
			bFilterFront = true;
		}
	}

	// Acquire the list lock.
	EnterCriticalSection((LPCRITICAL_SECTION)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 8));

	// Loop through the hash table and print each entry.
	cResource **pTableEntries = (cResource**)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 0x2248);
	for (int i = 0; i < 8192; i++)
	{
		// Check if this entry is allocated.
		if (pTableEntries[i] == nullptr)
			continue;

		// Check if there is a filter and handle accordingly.
		if (sFilter.size() > 0)
		{
			// Check if this is a front filter or rear filter.
			std::string sFileName(pTableEntries[i]->mPath);
			if (bFilterFront == true && sFileName.find(sFilter, 0) == 0)
			{
				// Print the name of the object.
				wprintf(L"Resource %d: %S\n", i, pTableEntries[i]->mPath);
			}
			else if (bFilterFront == false && sFileName.rfind(sFilter) == sFileName.size() - sFilter.size())
			{
				// Print the name of the object.
				wprintf(L"Resource %d: %S\n", i, pTableEntries[i]->mPath);
			}
		}
		else
		{
			// Print the name of the object.
			wprintf(L"Resource %d: %S\n", i, pTableEntries[i]->mPath);
		}
	}

	// Release the list lock.
	LeaveCriticalSection((LPCRITICAL_SECTION)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 8));
}

cResource* sResourceImpl::GetGameResourceByIndex(int index)
{
	cResource *pResource = nullptr;

	// Make sure the index argument is valid.
	if (index < 0 || index >= 8192)
	{
		// Index is out of range.
		wprintf(L"Index is out of range, must be [0, 8191]\n");
		return 0;
	}

	// Acquire the list lock.
	EnterCriticalSection((LPCRITICAL_SECTION)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 8));

	// Get the resource list table pointer and make sure the resource we want is not null.
	cResource **pTableEntries = (cResource**)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 0x2248);
	if (pTableEntries[index] != nullptr)
	{
		// Set the return value.
		pResource = pTableEntries[index];

		// Increment the ref count of the resource.
		sResourceImpl::IncrementResourceRefCount(pResource);
	}

	// Release the list lock.
	LeaveCriticalSection((LPCRITICAL_SECTION)(GetModulePointer<__int64>(g_sResourceSingletonInst) + 8));

	// Return the resource.
	return pResource;
}