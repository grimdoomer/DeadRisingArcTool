
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

const float4 gXfHighlightColor;
const dword gXfEnableHighlighting;

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

SamplerState XfSamplerAlbedoMap2
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

SamplerState XfSamplerNormalMap
{
	//Texture				= (XfNormalMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	MaxLOD = 0;
	Filter = ANISOTROPIC;
	MipLODBias = 0.0f;
	MaxAnisotropy = 3;
};

SamplerState XfSamplerMaskMap
{
	//Texture				= (XfMaskMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerLightMap
{
	//Texture				= (XfLightMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerLightMap2
{
	//Texture				= (XfLightMap2);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerShadowMap
{
	//Texture				= (XfShadowMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerShadowMap2
{
	//Texture				= (XfShadowMap2);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerAdditionalMap
{
	//Texture				= (XfAdditionalMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerDetailMap
{
	//Texture				= (XfAdditionalMap);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerEnvironmentMap
{
	//Texture				= (XfEnvironmentMap);
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_LINEAR;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
};

SamplerState XfSamplerScreenMap
{
	//Texture				= (XfScreenMap);
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	BorderColor = float4(0, 0, 0, 0);
	Filter = MIN_MAG_MIP_POINT;
	MaxAnisotropy = 1;
	MaxLOD = 0;
	MipLODBias = 0.0f;
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

SamplerState XfSamplerPrevMatrixMap
{
	//Texture				= (XfPrevMatrixMap);
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

/*
	gXfMatrixMapFactor.x = starting matrix index
	gXfMatrixMapFactor.y = line number?
	gXfMatrixMapFactor.z = size of 1 matrix unit (in uv range)
	gXfMatrixMapFactor.w = row size of matrix (in uv range)
*/

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

float3x4 getWorldMatrix8wtFromTex(
	float4		boneWeights0:	BLENDWEIGHT0,
	float4		boneWeights1 : BLENDWEIGHT1,
	int4		boneIndices0 : BLENDINDICES0,
	int4		boneIndices1 : BLENDINDICES1)
{
	float3x4 wmat;
	float4   ofs0 = ((float4)boneIndices0 + gXfMatrixMapFactor.x) * gXfMatrixMapFactor.z;
	float4   ofs1 = ((float4)boneIndices1 + gXfMatrixMapFactor.x) * gXfMatrixMapFactor.z;
	float    _line = gXfMatrixMapFactor.y;

	wmat = mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs0.x, _line), boneWeights0.x)
		+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs0.y, _line), boneWeights0.y)
		+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs0.z, _line), boneWeights0.z)
		+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs0.w, _line), boneWeights0.w);

	if (boneWeights1.x > 0) {

		wmat += mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs1.x, _line), boneWeights1.x)
			+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs1.y, _line), boneWeights1.y)
			+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs1.z, _line), boneWeights1.z)
			+ mul(getMatrixFromTexture(XfMatrixMap, XfSamplerMatrixMap, ofs1.w, _line), boneWeights1.w);

	}

	return wmat;
}

#endif
