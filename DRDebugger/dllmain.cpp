// dllmain.cpp : Defines the entry point for the DLL application.
#include <stdio.h>
#include <string>
#include <iostream>
#include <locale>
#include <codecvt>
#include <Windows.h>
#include <shellapi.h>
#include "DRDebugger.h"
#include "Misc/AsmHelpers.h"
#include "MtFramework/sResource.h"

// Global lua state.
sol::state g_LuaState;

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
		std::wstring sCommandUnic;
		CommandEntry commandInfo;
		LPWSTR *pArguments;
		int ArgCount;

		// Get the next command and convert it to unicode for parsing.
		wprintf(L">");
		std::getline(std::cin, sCommand);

		// Run the command in the lua engine.
		sol::protected_function_result result = g_LuaState.script(sCommand.c_str());

		//// Parse the command string.
		//pArguments = CommandLineToArgvW(sCommandUnic.c_str(), &ArgCount);
		//if (pArguments != NULL)
		//{
		//	// Make sure there is at least one argument to process.
		//	if (ArgCount == 0)
		//		goto CommandEnd;

		//	// Check if a command with the specified name exists.
		//	if (FindCommand(std::wstring(pArguments[0]), &commandInfo) == false)
		//	{
		//		// No matching command found.
		//		wprintf(L"\nUnknown command: %s\n\n", pArguments[0]);
		//		goto CommandEnd;
		//	}

		//	// Call the command handler.
		//	commandInfo.pHandlerFunction(&pArguments[1], ArgCount - 1);
		//	wprintf(L"\n");

		//CommandEnd:
		//	// Free the argument buffer.
		//	LocalFree(pArguments);
		//}

		// Sleep and loop.
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

		// Setup the lua environment.
		g_LuaState.open_libraries(sol::lib::base, sol::lib::coroutine, sol::lib::string, sol::lib::io);

		// Register all commands.
		RegisterCommands(g_CommandManagerCommands, g_CommandManagerCommandsLength);
		RegisterCommands(g_TypeInfoCommands, g_TypeInfoCommandsLength);
		RegisterCommands(g_sResourceCommands, g_sResourceCommandsLength);

		// Register all type info.
		RegisterTypeInfo(&cResourceTypeInfo);

		// Setup the console window.
		OutputDebugString(L"DRDebugger DllMain\n");
		SetupConsole();

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

