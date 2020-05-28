/*

*/

#pragma once
#include "Lua/sol.hpp"

// Global lua state.
extern sol::state g_LuaState;


// Macros for building lua usertypes for C++ objects:
#define CPP_FIELD(class, field)		#field, &class::field

struct Vector3
{
	float x, y, z;

	static void InitializeLua()
	{
		// Register Vector3 usertype:
		g_LuaState.new_usertype<Vector3>("Vector3",
			"x", &Vector3::x,
			"y", &Vector3::y,
			"z", &Vector3::z);
	}
};

struct Vector4
{
	float x, y, z, w;

	static void InitializeLua()
	{
		// Register Vector4 usertype:
		g_LuaState.new_usertype<Vector4>("Vector3",
			"x", &Vector4::x,
			"y", &Vector4::y,
			"z", &Vector4::z,
			"w", &Vector4::w);
	}
};