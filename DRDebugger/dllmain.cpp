// dllmain.cpp : Defines the entry point for the DLL application.
#include <stdio.h>
#include <string>
#include <iostream>
#include <Windows.h>
#include <shellapi.h>
#include "DRDebugger.h"
#include "Lua/HelperFunctions.h"
#include "Misc/AsmHelpers.h"
#include "MtFramework/sResource.h"

// Global lua state.
sol::state g_LuaState;

int LuaExceptionHandler(lua_State* L, sol::optional<const std::exception&> maybe_exception, sol::string_view description)
{
	// If an exception was thrown use it for printing.
	if (maybe_exception)
	{
		// Print the exception info.
		printf("EXCEPTION: %s\n", maybe_exception->what());
	}
	else
	{
		// Print the error details.
		std::cout.write(description.data(), description.size());
	}

	// you must push 1 element onto the stack to be 
	// transported through as the error object in Lua
	// note that Lua -- and 99.5% of all Lua users and libraries -- expects a string
	// so we push a single string (in our case, the description of the error)
	return sol::stack::push(L, description);
}

int LoadFileRequire(lua_State* L)
{
	// Get the module name.
	std::string path = sol::stack::get<std::string>(L, 1);

	// Check the module name and handle accordingly.
	if (path == "inspect")
	{
		// Get the inspect code file into a buffer.
		std::string InspectCode = LuaInspectCodeStr;

		// Load the script buffer.
		luaL_loadbuffer(L, InspectCode.data(), InspectCode.size(), path.c_str());
		return 1;
	}

	sol::stack::push(L, "This is not the module you're looking for!");
	return 1;
}

void SetupConsole()
{
	// Create the console window.
	if (AllocConsole() == FALSE)
	{
		// Failed to create the console window.
		OutputDebugString(L"Failed to create console window!\n");
		return;
	}

	// Set the window title.
	SetConsoleTitle(L"DRDebugger");

	// Open input/output streams.
	freopen("CONIN$", "r", stdin);
	freopen("CONOUT$", "w", stdout);
	freopen("CONOUT$", "w", stderr);
}

DWORD __stdcall ProcessConsoleWorker(LPVOID)
{
	// Loop forever.
	while (true)
	{
		std::string sCommand;

		// Get the next command and convert it to unicode for parsing.
		wprintf(L">");
		std::getline(std::cin, sCommand);

		// Run the command in the lua engine.
		sol::protected_function_result result = g_LuaState.script(sCommand.c_str(), sol::script_pass_on_error);
		if (result.valid() == false)
		{
			// An error occured while parsing the script.
			sol::error error = result;
			printf("SCRIPT ERROR: %s\n", error.what());
		}

		// Sleep and loop.
		printf("\n");
		Sleep(50);
	}

	return 0;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
	{
		// Set the module handle.
		SnatcherModuleHandle = GetModuleHandle(NULL);

		// Setup the console window.
		OutputDebugString(L"DRDebugger DllMain\n");
		SetupConsole();

		// Setup the lua environment.
		g_LuaState.open_libraries(sol::lib::base, sol::lib::coroutine, sol::lib::string, sol::lib::io, sol::lib::package, sol::lib::math, sol::lib::table);
		g_LuaState.set_exception_handler(&LuaExceptionHandler);
		g_LuaState.clear_package_loaders();
		g_LuaState.add_package_loader(LoadFileRequire);

		// Load some helper scripts.
		g_LuaState.safe_script(LuaHelperFunctionsStr, sol::script_pass_on_error);
		g_LuaState.safe_script(R"( inspect = require("inspect") )");

		// Register all objects with the lua state.
		sResourceImpl::InitializeLua();

		// Create a worker thread to process console commands.
		HANDLE hThread = CreateThread(NULL, NULL, ProcessConsoleWorker, NULL, NULL, NULL);
		CloseHandle(hThread);
		break;
	}
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

