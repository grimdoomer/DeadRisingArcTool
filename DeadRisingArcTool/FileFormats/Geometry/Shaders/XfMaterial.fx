//=============================================================================
//!	@file		XfMaterial.fx
//! @brief		
//! @author     T.Ishida
//=============================================================================
#include "XfGlobal.fx"

#ifndef XF_MATERIAL_FX
#define XF_MATERIAL_FX

//=============================================================================
//=============================================================================

#define SKIN_NONE	0		
#define SKIN_4WT	1	
#define SKIN_8WT	2	
#define SKIN_1WT_64	3	
#define SKIN_4WT_64	4

//=============================================================================
//=============================================================================


const float4   gXfMatrixMapFactor;

const float3 gXfQuantPosScale;
const float3 gXfQuantPosOffset;

//=============================================================================
//=============================================================================
shared Texture2D		XfAlbedoMap;
shared Texture2D	    XfMatrixMap;

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
SamplerState XfSamplerAlbedoMap
{
	//Texture				= (XfAlbedoMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	MaxLOD = 0;
	Filter = ANISOTROPIC;
	MipLODBias = 0.0f;
	MaxAnisotropy = 3;
};

SamplerState XfSamplerMatrixMap
{
	//Texture				= (XfMatrixMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_POINT;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
float3 decodePosition(in float3 pos)
{
	return pos * gXfQuantPosScale + gXfQuantPosOffset;
}

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
float3x4 getMatrixFromTexture(Texture2D tex, SamplerState ss, float index, float _line)
{
	float3x4 mat;
	float4 uv0 = float4(index, _line, 0, 0);
	float4 uv1 = float4(index + gXfMatrixMapFactor.w, _line, 0, 0);
	float4 uv2 = float4(index + gXfMatrixMapFactor.w + gXfMatrixMapFactor.w, _line, 0, 0);

	mat[0] = tex.SampleLevel(ss, uv0, 0);
	mat[1] = tex.SampleLevel(ss, uv1, 0);
	mat[2] = tex.SampleLevel(ss, uv2, 0);

	return mat;
}

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
float3x4 getWorldMatrix4wtFromTex(
	float4		boneWeights0:	BLENDWEIGHT0,
	int4		boneIndices0 : BLENDINDICES0)
{
	float3x4 wmat;
	float4   ofs = ((float4)boneIndices0 + gXfMatrixMapFactor.x) * gXfMatrixMapFactor.z;
	float    _line = gXfMatrixMapFactor.y;

	wmat = mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs.x, _line), boneWeights0.x)
		+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs.y, _line), boneWeights0.y);

	if (boneWeights0.z > 0) {

		wmat += mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs.z, _line), boneWeights0.z)
			+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs.w, _line), boneWeights0.w);

	}

	return wmat;
}

#endif
