//=============================================================================
//!	@file		XfZPassMaterial.fx
//! @brief		
//! @author     T.Ishida
//=============================================================================
#ifndef XF_ZPASS_MATERIAL_FX
#define XF_ZPASS_MATERIAL_FX

//=============================================================================
//Include
//=============================================================================
#include "XfGlobal.fx"
#include "XfMaterial.fx"

//=============================================================================
//
//=============================================================================

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct ZPassVS_INPUT
{
	float4		position:				POSITION;
	float2		texCoordBase:			TEXCOORD0;
	float4		boneWeights0:	BLENDWEIGHT0;
	int4		boneIndices0:	BLENDINDICES0;
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct ZPassVS_OUTPUT
{
	float4		position:				SV_POSITION;
	float2		texCoordBase:			TEXCOORD0;
	float4		wPosition:				TEXCOORD1;
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct ZPassPS_INPUT
{	
	float2		texCoordBase:			TEXCOORD0;
	float4		vPosition:				TEXCOORD1;
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
ZPassVS_OUTPUT XfZPassVS(ZPassVS_INPUT I)
{
	ZPassVS_OUTPUT O;
 
	float3x4 wmat;

	wmat = getWorldMatrix4wtFromTex(I.boneWeights0, I.boneIndices0);

	float3	wp = mul(wmat, float4(decodePosition(I.position.xyz), 1));
	//float3	wp = decodePosition(I.position.xyz);

	O.position = mul(float4(wp, 1), gXfViewProj);

	O.wPosition = O.position;
	O.texCoordBase = I.texCoordBase;

	return O;
}

//-----------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------
float4 XfZPassPS(ZPassVS_OUTPUT I, uniform const bool _alpha, uniform const float _threshold) : SV_Target
{
	const bool alpha = FPARAM_0;
	const float threshold = FPARAM_1;

	if (alpha) {
		float4 albedo = XfAlbedoMap.Sample(XfSamplerAlbedoMap, I.texCoordBase);
		float v = albedo.w - threshold;
		clip(v);
	}

	//float z = 1.0f - (I.wPosition.z / I.wPosition.w);	//qloc
	float z = (I.wPosition.z / I.wPosition.w);	//qloc	
	return float4(z, 0, 0, 1);	
}

//=============================================================================
//
//=============================================================================
technique11 tXfMaterialZPass
{
	pass P0
	{
		VertexShader = compile vs_5_0 XfZPassVS();
		PixelShader = compile ps_5_0 XfZPassPS(false, 0);
	}

	pass P1
	{
		VertexShader = compile vs_5_0 XfZPassVS();
		PixelShader = compile ps_5_0 XfZPassPS(true, 0.01f);
	}

	pass P2
	{
		VertexShader = compile vs_5_0 XfZPassVS();
		PixelShader = compile ps_5_0 XfZPassPS(true, 0.5f);
	}
}

#endif