
#ifndef	XF_GLOBAL_FX
#define	XF_GLOBAL_FX

//=============================================================================
//
//=============================================================================
const float4x4 gXfViewProj : register(c0);

float3 decodeRGBY(float4 rgby)
{
	return rgby.xyz * rgby.w;
}

#endif
