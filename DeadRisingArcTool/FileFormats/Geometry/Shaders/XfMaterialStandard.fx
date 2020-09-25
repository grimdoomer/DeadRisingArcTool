
#ifndef XF_STANDARD_MATERIAL_FX
#define XF_STANDARD_MATERIAL_FX

//=============================================================================
//Include
//=============================================================================
#include "XfGlobal.fx"
#include "XfMaterial.fx"


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct VS_INPUT
{
	float3		position:		POSITION;
	float3      normal:			NORMAL;
	float3      tangent:		TANGENT;
	float2		texCoord0:		TEXCOORD0;
	float2		texCoord1:		TEXCOORD1;
	float2		texCoord2:		TEXCOORD2;
	float2		texCoord3:		TEXCOORD3;

#if (FUNC_SKIN != SKIN_NONE)
	float4		boneWeights0:	BLENDWEIGHT0;
	int4		boneIndices0:	BLENDINDICES0;
#endif

#if	(FUNC_SKIN == SKIN_8WT)
	float4		boneWeights1:	BLENDWEIGHT1;
	int4		boneIndices1:	BLENDINDICES1;
#endif
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4		position:				SV_POSITION;
	float3		texCoordBase:			TEXCOORD0;
	float4		wPosition:				TEXCOORD1;
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
struct PS_INPUT
{	
	float3		texCoordBase:			TEXCOORD0;
	float4		vPosition:				TEXCOORD1;
};

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
VS_OUTPUT XfStandardVS(VS_INPUT I)
{
	VS_OUTPUT O;
 
	float3x4 wmat;

#if (FUNC_SKIN == SKIN_4WT)

	wmat = getWorldMatrix4wtFromTex(I.boneWeights0, I.boneIndices0);

#elif (FUNC_SKIN == SKIN_8WT)

	wmat = getWorldMatrix8wtFromTex(I.boneWeights0, I.boneWeights1, I.boneIndices0, I.boneIndices1);
#endif

#if (FUNC_SKIN != SKIN_NONE)
	float3	pos = decodePosition(I.position.xyz);
	float3 wp = mul(wmat, float4(pos, 1));
#else
	float3 pos = I.position;
	float3 wp = pos;
#endif

	O.position = mul(float4(wp, 1), gXfViewProj);

	//O.wPosition = O.position;
	O.texCoordBase.xy = I.texCoord0.xy;

	O.texCoordBase.z = 1;

	return O;
}

//-----------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------
float4 XfStandardPS(VS_OUTPUT I) : SV_Target
{
	/*if (alpha) 
	{
		float4 albedo = XfAlbedoMap.Sample(XfSamplerAlbedoMap, I.texCoordBase);
		clip(albedo.a * alpha - 0.25);

		return albedo;
	}
	else*/
    if (gXfEnableHighlighting == 0)
	{
		float4 albedo = XfAlbedoMap.Sample(XfSamplerAlbedoMap, I.texCoordBase);
		clip(albedo.a * I.texCoordBase.z - 1.0 / 255.0);

		return albedo;
	}
    else
    {
        return gXfHighlightColor;
    }
}

//=============================================================================
//
//=============================================================================
technique11 tXfMaterialStandard
{
	pass P0
	{
		VertexShader = compile vs_5_0 XfStandardVS();
		PixelShader = compile ps_5_0 XfStandardPS();
	}
}

#endif