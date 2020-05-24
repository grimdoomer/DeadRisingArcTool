//"use strict";

/*
	.load jsprovider
	.scriptload "D:\Visual Studio 2015\DeadRisingArcTool\DeadRisingArcTool\Windbg\DeadRisingDbgExtensions.js"
	dx @$myScript = Debugger.State.Scripts.DeadRisingDbgExtensions.Contents
	dx @$myScript.dumpVertexDecl()

*/

var DXGI_FORMAT = [
	"DXGI_FORMAT_UNKNOWN",
	"DXGI_FORMAT_R32G32B32A32_TYPELESS",
	"DXGI_FORMAT_R32G32B32A32_FLOAT",
	"DXGI_FORMAT_R32G32B32A32_UINT",
	"DXGI_FORMAT_R32G32B32A32_SINT",
	"DXGI_FORMAT_R32G32B32_TYPELESS",
	"DXGI_FORMAT_R32G32B32_FLOAT",
	"DXGI_FORMAT_R32G32B32_UINT",
	"DXGI_FORMAT_R32G32B32_SINT",
	"DXGI_FORMAT_R16G16B16A16_TYPELESS",
	"DXGI_FORMAT_R16G16B16A16_FLOAT",
	"DXGI_FORMAT_R16G16B16A16_UNORM",
	"DXGI_FORMAT_R16G16B16A16_UINT",
	"DXGI_FORMAT_R16G16B16A16_SNORM",
	"DXGI_FORMAT_R16G16B16A16_SINT",
	"DXGI_FORMAT_R32G32_TYPELESS",
	"DXGI_FORMAT_R32G32_FLOAT",
	"DXGI_FORMAT_R32G32_UINT",
	"DXGI_FORMAT_R32G32_SINT",
	"DXGI_FORMAT_R32G8X24_TYPELESS",
	"DXGI_FORMAT_D32_FLOAT_S8X24_UINT",
	"DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS",
	"DXGI_FORMAT_X32_TYPELESS_G8X24_UINT",
	"DXGI_FORMAT_R10G10B10A2_TYPELESS",
	"DXGI_FORMAT_R10G10B10A2_UNORM",
	"DXGI_FORMAT_R10G10B10A2_UINT",
	"DXGI_FORMAT_R11G11B10_FLOAT",
	"DXGI_FORMAT_R8G8B8A8_TYPELESS",
	"DXGI_FORMAT_R8G8B8A8_UNORM",
	"DXGI_FORMAT_R8G8B8A8_UNORM_SRGB",
	"DXGI_FORMAT_R8G8B8A8_UINT",
	"DXGI_FORMAT_R8G8B8A8_SNORM",
	"DXGI_FORMAT_R8G8B8A8_SINT",
	"DXGI_FORMAT_R16G16_TYPELESS",
	"DXGI_FORMAT_R16G16_FLOAT",
	"DXGI_FORMAT_R16G16_UNORM",
	"DXGI_FORMAT_R16G16_UINT",
	"DXGI_FORMAT_R16G16_SNORM",
	"DXGI_FORMAT_R16G16_SINT",
	"DXGI_FORMAT_R32_TYPELESS",
	"DXGI_FORMAT_D32_FLOAT",
	"DXGI_FORMAT_R32_FLOAT",
	"DXGI_FORMAT_R32_UINT",
	"DXGI_FORMAT_R32_SINT",
	"DXGI_FORMAT_R24G8_TYPELESS",
	"DXGI_FORMAT_D24_UNORM_S8_UINT",
	"DXGI_FORMAT_R24_UNORM_X8_TYPELESS",
	"DXGI_FORMAT_X24_TYPELESS_G8_UINT",
	"DXGI_FORMAT_R8G8_TYPELESS",
	"DXGI_FORMAT_R8G8_UNORM",
	"DXGI_FORMAT_R8G8_UINT",
	"DXGI_FORMAT_R8G8_SNORM",
	"DXGI_FORMAT_R8G8_SINT",
	"DXGI_FORMAT_R16_TYPELESS",
	"DXGI_FORMAT_R16_FLOAT",
	"DXGI_FORMAT_D16_UNORM",
	"DXGI_FORMAT_R16_UNORM",
	"DXGI_FORMAT_R16_UINT",
	"DXGI_FORMAT_R16_SNORM",
	"DXGI_FORMAT_R16_SINT",
	"DXGI_FORMAT_R8_TYPELESS",
	"DXGI_FORMAT_R8_UNORM",
	"DXGI_FORMAT_R8_UINT",
	"DXGI_FORMAT_R8_SNORM",
	"DXGI_FORMAT_R8_SINT",
	"DXGI_FORMAT_A8_UNORM",
	"DXGI_FORMAT_R1_UNORM",
	"DXGI_FORMAT_R9G9B9E5_SHAREDEXP",
	"DXGI_FORMAT_R8G8_B8G8_UNORM",
	"DXGI_FORMAT_G8R8_G8B8_UNORM",
	"DXGI_FORMAT_BC1_TYPELESS",
	"DXGI_FORMAT_BC1_UNORM",
	"DXGI_FORMAT_BC1_UNORM_SRGB",
	"DXGI_FORMAT_BC2_TYPELESS",
	"DXGI_FORMAT_BC2_UNORM",
	"DXGI_FORMAT_BC2_UNORM_SRGB",
	"DXGI_FORMAT_BC3_TYPELESS",
	"DXGI_FORMAT_BC3_UNORM",
	"DXGI_FORMAT_BC3_UNORM_SRGB",
	"DXGI_FORMAT_BC4_TYPELESS",
	"DXGI_FORMAT_BC4_UNORM",
	"DXGI_FORMAT_BC4_SNORM",
	"DXGI_FORMAT_BC5_TYPELESS",
	"DXGI_FORMAT_BC5_UNORM",
	"DXGI_FORMAT_BC5_SNORM",
	"DXGI_FORMAT_B5G6R5_UNORM",
	"DXGI_FORMAT_B5G5R5A1_UNORM",
	"DXGI_FORMAT_B8G8R8A8_UNORM",
	"DXGI_FORMAT_B8G8R8X8_UNORM",
	"DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM",
	"DXGI_FORMAT_B8G8R8A8_TYPELESS",
	"DXGI_FORMAT_B8G8R8A8_UNORM_SRGB",
	"DXGI_FORMAT_B8G8R8X8_TYPELESS",
	"DXGI_FORMAT_B8G8R8X8_UNORM_SRGB",
	"DXGI_FORMAT_BC6H_TYPELESS",
	"DXGI_FORMAT_BC6H_UF16",
	"DXGI_FORMAT_BC6H_SF16",
	"DXGI_FORMAT_BC7_TYPELESS",
	"DXGI_FORMAT_BC7_UNORM",
	"DXGI_FORMAT_BC7_UNORM_SRGB",
	"DXGI_FORMAT_AYUV",
	"DXGI_FORMAT_Y410",
	"DXGI_FORMAT_Y416",
	"DXGI_FORMAT_NV12",
	"DXGI_FORMAT_P010",
	"DXGI_FORMAT_P016",
	"DXGI_FORMAT_420_OPAQUE",
	"DXGI_FORMAT_YUY2",
	"DXGI_FORMAT_Y210",
	"DXGI_FORMAT_Y216",
	"DXGI_FORMAT_NV11",
	"DXGI_FORMAT_AI44",
	"DXGI_FORMAT_IA44",
	"DXGI_FORMAT_P8",
	"DXGI_FORMAT_A8P8",
	"DXGI_FORMAT_B4G4R4A4_UNORM",
	"DXGI_FORMAT_P208",
	"DXGI_FORMAT_V208",
	"DXGI_FORMAT_V408",
	"DXGI_FORMAT_FORCE_UINT"
]

function dumpVertexDecl()
{
	// Print the arguments for reference.
	var rdx = host.currentThread.Registers.User.rdx;
	var r8 = host.currentThread.Registers.User.r8;
	host.diagnostics.debugLog("rdx=", rdx.toString(), " r8=", r8.toString(), "\n");
	
	// Print the number of vertex declarations.
	host.diagnostics.debugLog("Dumping vertex decl with ", r8, " items...\n");
	
	// Loop and print each item.
	for (i = 0; i < r8; i++)
	{
		// Read the vertex decl data.
		var ptr = rdx + (i * 32);
		var NamePtr = host.memory.readMemoryValues(ptr, 1, 8)[0];
		//host.diagnostics.debugLog("NamePtr=", NamePtr);
		var SemanticName = host.memory.readString(NamePtr);
		var SemanticIndex = host.memory.readMemoryValues(ptr + 8, 1, 4)[0];
		var Format = host.memory.readMemoryValues(ptr + 12, 1, 4)[0];
		var InputSlot = host.memory.readMemoryValues(ptr + 16, 1, 4)[0];
		var AlignedByteOffset = host.memory.readMemoryValues(ptr + 20, 1, 4)[0];
		var InputSlotClassification = host.memory.readMemoryValues(ptr + 24, 1, 4)[0];
		var InstanceDataStepRate = host.memory.readMemoryValues(ptr + 28, 1, 4)[0];
		
		// Print formatted info to console.
		host.diagnostics.debugLog("\t", SemanticName, "\t\t", SemanticIndex, "\t", DXGI_FORMAT[Format], 
			"\t", InputSlot, "\t", AlignedByteOffset, "\t", InputSlotClassification, "\t", InstanceDataStepRate, "\n")
	}
	
	// Print new line for spacing.
	host.diagnostics.debugLog("\n");
}

//function getModuleAddress()
//{
//	// Why the cock fucking suck doesn't this work?
//	var modules = host.currentProcess.Modules.Where(function (k) { return k.Name == "Snatcher.exe"; });
//	var mod = modules.First();
//	return mod.BaseAddress;
//}

function convertModuleAddress(address)
{
	// I don't have to explain myself here.
	return address - 0x850000;
}

function dumpShaderIds()
{
	var rsi = host.memory.readMemoryValues(convertModuleAddress(0x141D179A0), 1, 8)[0]; //host.currentThread.Registers.User.rsi;
	var shaderCount = host.memory.readMemoryValues(rsi + 0xB29C, 1, 4)[0];
	
	// Loop and print the shader info.
	host.diagnostics.debugLog("Found ", shaderCount.toString(), " shaders:\n");
	for (i = 0; i < shaderCount; i++)
	{
		// Read the shader name and id.
		var shaderNameAddress = host.memory.readMemoryValues(rsi + 0x62A0 + (i * 0x18), 1, 8)[0];
		var shaderId = host.memory.readMemoryValues(rsi + 0x62A0 + (i * 0x18) + 8, 1, 4)[0];
		
		var shaderName = "null";
		if (shaderNameAddress != 0)
			shaderName = host.memory.readString(shaderNameAddress);
		
		
		// Print the shader.
		host.diagnostics.debugLog(shaderName, " = 0x", shaderId.toString(16), "\n");
	}
	
	// Print new line for spacing.
	host.diagnostics.debugLog("\n");
}