//=============================================================================
//!	@file		XfMaterialStandard.fx
//=============================================================================
#ifndef XF_STANDARD_MATERIAL_FX
#define XF_STANDARD_MATERIAL_FX

//=============================================================================
//Include
//=============================================================================
#include "XfGlobal.fx"
#include "XfMaterial.fx"

cbuffer AlphaTest : register (b1)
{
	int gXfEnableAlphaTest;
	float gXfAlphaThreshold;
	float4 gXfClipPlane;
}

//=============================================================================
//=============================================================================

#define LIGHTING_NONE       0	
#define LIGHTING_SH			1	
#define LIGHTING_4SPOT		2
#define LIGHTING_SH4SPOT	3
#define LIGHTING_EMITSH4SPOT 4

#define NORMALMAP_NONE		0	
#define NORMALMAP_STANDARD	1	
#define NORMALMAP_DETAIL	2	
#define NORMALMAP_PARALLAX	3	

#define SPECULAR_NONE		0	
#define SPECULAR_STANDARD	1	
#define SPECULAR_MIRROR     2   
#define SPECULAR_POWMAP		3  

#define LIGHTMAP_NONE		 0	
#define LIGHTMAP_STANDARD	 1	
#define LIGHTMAP_SHADOW		 2
#define LIGHTMAP_BLEND		 3
#define LIGHTMAP_BLENDSHADOW 4	
#define LIGHTMAP_COLOR		 5	

#define MULTITEXTURE_NONE	0	
#define MULTITEXTURE_ALPHA	1	
#define MULTITEXTURE_BASE	2	
#define MULTITEXTURE_GLASS	3	

//=============================================================================
//=============================================================================
/*
#define FUNC_LIGHTING		LIGHTING_SH4SPOT
#define FUNC_SKIN			SKIN_4WT
#define FUNC_NORMALMAP		NORMALMAP_PARALLAX
#define FUNC_SPECULAR		SPECULAR_ENVLIGHT
#define FUNC_LIGHTMAP		LIGHTMAP_BLENDSHADOW
#define FUNC_MULTITEXTURE	MULTITEXTURE_BASE
*/


//=============================================================================
//=============================================================================

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
struct VS_INPUT
{
	float3		position:		POSITION;
	float3      normal:			NORMAL;
	float3      tangent:		TANGENT;
	float2		texCoord0:		TEXCOORD0;
	float2		texCoord1:		TEXCOORD1;
	float2		texCoord2:		TEXCOORD2;
	float2      texCoord3:		TEXCOORD3;

	float4		boneWeights0:	BLENDWEIGHT0;	
	int4		boneIndices0:	BLENDINDICES0;
};

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4		position:			SV_POSITION;
	float3		texCoordBase:		TEXCOORD0;	
	float3		wPosition:			TEXCOORD1;	

	float3		texCoordDetail:		TEXCOORD2;	

	float2		texCoordUnique:		TEXCOORD3;	

	float4      texCoordAdditional: TEXCOORD4;	
	
	float3		mTangentSpaceT:		TEXCOORD5;
	float3		mTangentSpaceB:		TEXCOORD6;
	float3		mTangentSpaceN:		TEXCOORD7;

	float clip : SV_ClipDistance0;		//qloc
};

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
struct PS_INPUT
{
	float3		texCoordBase:		TEXCOORD0;	
	float3		wPosition:			TEXCOORD1;

	float3		texCoordDetail:		TEXCOORD2;	

	float2		texCoordUnique:		TEXCOORD3;	

	float4      texCoordAdditional: TEXCOORD4;	

	float3		mTangentSpaceT:		TEXCOORD5;	
	float3		mTangentSpaceB:		TEXCOORD6;
	float3		mTangentSpaceN:		TEXCOORD7;
};

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
struct PS_OUTPUT
{
	float4 color0 : SV_Target;
};

//=============================================================================
//=============================================================================
VS_OUTPUT XfStandardVS(VS_INPUT I)
{
	VS_OUTPUT O;

	float3x4 wmat;

#if	((FUNC_SKIN == SKIN_4WT) || (FUNC_SKIN == SKIN_4WT_64))

#if (FUNC_SKIN == SKIN_4WT)
	wmat = getWorldMatrix4wtFromTex(I.boneWeights0, I.boneIndices0);
#else
	wmat = getWorldMatrix4wtFromConstant(I.boneWeights0, I.boneIndices0);
#endif	

#elif (FUNC_SKIN == SKIN_8WT)

	wmat = getWorldMatrix8wtFromTex(I.boneWeights0, I.boneWeights1, I.boneIndices0, I.boneIndices1);

#elif (FUNC_SKIN == SKIN_1WT_64)

	wmat = getWorldMatrix1wtFromConstant(I.boneWeights0, I.boneIndices0);

#else
	wmat = gXfWorld;
#endif

#if	(FUNC_SKIN == SKIN_NONE)
	float3 pos = I.position.xyz;
#else
	float3 pos = decodePosition(I.position.xyz);
#endif	

	float3 wp = mul(wmat, float4(pos, 1));
	float4 vp = mul(float4(wp, 1), gXfViewProj);
#if (FUNC_MULTITEXTURE == MULTITEXTURE_GLASS)
	vp.z -= (0.001f * vp.w);//(gXfDepthBias * vp.w);
#endif
	O.position = vp;
	O.wPosition = wp;

	float3 wn = mul(wmat, float4(I.normal.xyz, 0));

#if (FUNC_SKIN != SKIN_NONE)	
	wn = normalize(wn);
#endif	

#if (FUNC_NORMALMAP != NORMALMAP_NONE)

	float3 wt = mul(wmat, float4(I.tangent.xyz, 0));

#if (FUNC_SKIN != SKIN_NONE)	
	wt = normalize(wt);
#endif	

	float3	wb = cross(wt, wn);	wb = normalize(wb);

	O.mTangentSpaceT = wt;
	O.mTangentSpaceN = wn;
	O.mTangentSpaceB = wb * gXfParallaxFactor.z;

#else
	O.wNormal = wn;
#endif

		float3 wDirVec = wp - gXfEyePos.xyz;
		float  dist = length(wDirVec);
		float3 wDirEye = normalize(wDirVec);

		O.texCoordBase.xy = I.texCoord0.xy;
#if (FUNC_MULTITEXTURE == MULTITEXTURE_GLASS)
		O.texCoordBase.z = gXfBlendFactor * checkSphericalSectionFreezing(pos);
#else
		O.texCoordBase.z = gXfBlendFactor;
#endif
		O.texCoordBase.xy += gXfUVScroll.xy;

#if (FUNC_NORMALMAP != NORMALMAP_NONE) || (FUNC_SPECULAR != SPECULAR_NONE) || (FUNC_LIGHTING == LIGHTING_EMITSH4SPOT)
		float fresnel = saturate(getFresnel(wn, wDirEye));
		O.texCoordDetail = float3(I.texCoord1, 1.0f - fresnel);

		O.texCoordDetail.xy += gXfUVScroll.xy;

#endif

#if (FUNC_LIGHTMAP != LIGHTMAP_NONE && FUNC_LIGHTMAP != LIGHTMAP_COLOR)
#if (FUNC_NORMALMAP != NORMALMAP_NONE) || (FUNC_SPECULAR != SPECULAR_NONE) || (FUNC_LIGHTING == LIGHTING_EMITSH4SPOT)
		O.texCoordUnique = I.texCoord2;
#else
		O.texCoordUnique = I.texCoord1;
#endif
#endif

#if (FUNC_SPECULAR == SPECULAR_MIRROR)
		O.texCoordAdditional = O.position;
#else
#if (FUNC_MULTITEXTURE == MULTITEXTURE_GLASS)
		O.texCoordAdditional = O.position;
#elif (FUNC_MULTITEXTURE != MULTITEXTURE_NONE)

#if (FUNC_NORMALMAP != NORMALMAP_NONE) && (FUNC_LIGHTMAP != LIGHTMAP_NONE && FUNC_LIGHTMAP != LIGHTMAP_COLOR)
		O.texCoordAdditional = float4(I.texCoord3, 0, 0);
#elif (FUNC_NORMALMAP != NORMALMAP_NONE) || (FUNC_LIGHTMAP != LIGHTMAP_NONE && FUNC_LIGHTMAP != LIGHTMAP_COLOR)
		O.texCoordAdditional = float4(I.texCoord2, 0, 0);
#else
		O.texCoordAdditional = float4(I.texCoord1, 0, 0);
#endif

		O.texCoordAdditional.xy += gXfUVScroll.xy;
#endif
#endif

#if (FUNC_LIGHTMAP == LIGHTMAP_COLOR)
		O.color = I.color;
		O.texCoordBase.z *= I.color.a;
#endif

		O.clip = dot(vp, gXfClipPlane);		//qloc
		return O;
}

//=============================================================================
//=============================================================================
PS_OUTPUT XfStandardPS(VS_OUTPUT I)
{
	float3 wEye = I.wPosition.xyz - gXfEyePos.xyz;
	float  wDistEye = length(wEye);
	float3 wDirEye = normalize(wEye);
	float alpha = I.texCoordBase.z;
	
#if (FUNC_NORMALMAP != NORMALMAP_NONE)
	float3x3 mTangentSpace = { I.mTangentSpaceT, I.mTangentSpaceB, I.mTangentSpaceN };
#endif

#if (FUNC_NORMALMAP == NORMALMAP_PARALLAX)
	float4 tP = XfAdditionalMap.Sample(XfSamplerAdditionalMap, I.texCoordDetail.xy);//HeightMap
	float3 tDirEye;
	tDirEye.x = dot(wDirEye, I.mTangentSpaceT);
	tDirEye.y = dot(wDirEye, I.mTangentSpaceB);
	tDirEye.z = dot(wDirEye, I.mTangentSpaceN);

	float2 tParallax = tDirEye.xy * (tP.r * gXfParallaxFactor.xy);
	I.texCoordBase.xy += tParallax;
	I.texCoordDetail.xy += tParallax;
#endif

	float4 albedo = XfAlbedoMap.Sample(XfSamplerAlbedoMap, I.texCoordBase);

	albedo.xyz *= gXfBaseMapScale.xyz * max(albedo.w, gXfBaseMapScale.w);
	albedo.w = max(albedo.w, 1.0f - gXfBaseMapScale.w);

#if (FUNC_MULTITEXTURE == MULTITEXTURE_ALPHA)
	albedo.w = XfAdditionalMap.Sample(XfSamplerAdditionalMap, I.texCoordAdditional).x;
#endif

	float3 wN;
	float3 tN = 0;

#if (FUNC_NORMALMAP != NORMALMAP_NONE)
	tN = XfNormalMap.Sample(XfSamplerNormalMap, I.texCoordDetail.xy);

#if (FUNC_NORMALMAP == NORMALMAP_DETAIL)
	tN += XfAdditionalMap.Sample(XfSamplerDetailMap, I.texCoordDetail.xy * gXfDetailFactor.y) * gXfDetailFactor.x;
#endif

#ifdef _XBOX
	tN = color2vector(tN);
#endif

	tN.z = gXfDetailFactor.z;
	tN = normalize(tN);

	wN = tN.x * I.mTangentSpaceT + tN.y * I.mTangentSpaceB + tN.z * I.mTangentSpaceN;
	wN = normalize(wN);
#else
	wN = normalize(I.wNormal);
#endif

	float3 wReflection = reflect(wDirEye, wN);

		float3 diffuse = 0;
		float3 specular = 0;
		float  specularPow = gXfFresnelFactor.z;

		float fogfactor = 0.0f;
		if (gXfFogEnable)
			fogfactor = max(min((wDistEye - gXfFogFactor.x) * gXfFogFactor.z, gXfFogColor.a), 0) * gXfBlendFactor;

		//SH
#if (FUNC_LIGHTING == LIGHTING_SH4SPOT) || (FUNC_LIGHTING == LIGHTING_SH) || (FUNC_LIGHTING == LIGHTING_EMITSH4SPOT)
		diffuse += getSHdiffuse(float4(wN, 1.0f));
#endif

		float4 mask;
#if (FUNC_SPECULAR != SPECULAR_NONE)
		mask = XfMaskMap.Sample(XfSamplerMaskMap, I.texCoordDetail.xy);
#if (FUNC_SPECULAR == SPECULAR_POWMAP)?		?)
		specularPow *= (1 - mask.x);
		mask = float4(1, 1, 1, 1);
#endif
#else
		mask = float4(1, 1, 1, 1);
#endif
?		?
#if (FUNC_LIGHTING == LIGHTING_EMITSH4SPOT)
			float3 emit = XfAdditionalMap.Sample(XfSamplerAdditionalMap, I.texCoordDetail.xy).xyz;
			diffuse += emit * gXfLightMapScale;
#endif

			//PerPixel
#if (FUNC_LIGHTING == LIGHTING_SH4SPOT) || (FUNC_LIGHTING == LIGHTING_4SPOT) || (FUNC_LIGHTING == LIGHTING_EMITSH4SPOT)
			int iLight = 0;
			for (int i = 0; i < gXfLightNum; i++) {
				float3 d, s;
				float4 LightColor = gXfLightParam[iLight * 4 + 0];
				float4 LightPos = gXfLightParam[iLight * 4 + 1];
				float4 LightDir = gXfLightParam[iLight * 4 + 2];
				float4 LightSpot = gXfLightParam[iLight * 4 + 3];
				float2 LightAtten = float2(LightColor.w, LightPos.w);
				doLighting(I.wPosition, wN, wDirEye,
					LightColor, LightPos, LightDir, LightAtten, LightSpot, specularPow, d, s);
				/*
				doLighting(I.wPosition, wN, wDirEye,
							gXfLightColor[iLight], gXfLightPos[iLight], gXfLightDir[iLight], LightAtten, gXfLightSpot[iLight], specularPow,
								d, s);
				*/
				diffuse += d;
				specular += s;

				iLight++;
			}
#endif

#if (FUNC_SPECULAR == SPECULAR_NONE)
			specular = float3(0, 0, 0);
#endif

			float shadow;
#if (FUNC_LIGHTMAP != LIGHTMAP_NONE && FUNC_LIGHTMAP != LIGHTMAP_COLOR)

			float4 lightMap;

#if (FUNC_LIGHTMAP == LIGHTMAP_BLEND) || (FUNC_LIGHTMAP == LIGHTMAP_BLENDSHADOW)

			float3 lightmap1 = decodeRGBY(XfLightMap.Sample(XfSamplerLightMap, I.texCoordUnique)) * gXfLightMapScale;
			float3 lightmap2 = decodeRGBY(XfLightMap2.Sample(XfSamplerLightMap2, I.texCoordUnique)) * gXfLightMapScale2;

			lightMap.xyz = lerp(lightmap1, lightmap2, gXfLightMapLerps);
#else
			lightMap.xyz = decodeRGBY(XfLightMap.Sample(XfSamplerLightMap, I.texCoordUnique)) * gXfLightMapScale;
#endif

#if (FUNC_LIGHTMAP == LIGHTMAP_SHADOW) || (FUNC_LIGHTMAP == LIGHTMAP_BLENDSHADOW)

#if (FUNC_LIGHTMAP == LIGHTMAP_BLENDSHADOW)
			shadow = lerp(XfShadowMap.Sample(XfSamplerShadowMap, I.texCoordUnique).r, XfShadowMap2.Sample(XfSamplerShadowMap2, I.texCoordUnique).r, gXfLightMapLerps);
#else
			shadow = XfShadowMap.Sample(XfSamplerShadowMap, I.texCoordUnique).r;
#endif
#else
			shadow = 1;
#endif

			diffuse += lightMap.xyz;
#else
#if (FUNC_LIGHTMAP == LIGHTMAP_COLOR)
			diffuse += I.color * gXfLightMapScale;
			shadow = I.color.r;
#else
			shadow = 1;
#endif
#endif

#if (FUNC_SPECULAR == SPECULAR_STANDARD)
			wReflection = reflect(wDirEye, wN);
			float4 envTexCoord = float4(wReflection, gXfEnvMapFactor);
			float3 environment0 = decodeRGBI(XfEnvironmentMap.SampleLevel(XfSamplerEnvironmentMap, wReflection, gXfEnvMapFactor));
			specular += environment0 * gXfFresnelFactor.w;
#endif

#if (FUNC_SPECULAR == SPECULAR_MIRROR) || (FUNC_MULTITEXTURE == MULTITEXTURE_GLASS)
			float2 vpos0 = I.texCoordAdditional.xy / I.texCoordAdditional.w;
			vpos0 = float2(0.5f, -0.5f) * vpos0 + float2(0.5f, 0.5f);
#if (FUNC_NORMALMAP != NORMALMAP_NONE)
			vpos0 -= tN.xy * gXfDetailFactor.xy;
#endif
#endif

#if (FUNC_SPECULAR == SPECULAR_MIRROR)
			float4 vRefrA = XfScreenMap.Sample(XfSamplerScreenMap, vpos0.xy);
			specular += decodeRGBI(vRefrA);
#endif

			specular *= shadow;

				float fresnel;
#if (FUNC_SPECULAR != SPECULAR_NONE) || (FUNC_NORMALMAP != NORMALMAP_NONE)
			fresnel = I.texCoordDetail.z;
#else
			fresnel = 1.0f;
#endif

			PS_OUTPUT O;
#if (FUNC_MULTITEXTURE == MULTITEXTURE_GLASS)
			float4 glassAlbedo = XfAdditionalMap.Sample(XfSamplerAdditionalMap, I.texCoordBase);

			float4 bgglass = XfScreenMap.Sample(XfSamplerScreenMap, vpos0.xy);
			float3 bgpicture;
#ifdef _XBOX
			bgglass.rgb = decode7e3(bgglass.rgb);
#else
			bgglass.rgb = decodeRGBI(bgglass);
#endif

			float3 albedoFactor = (diffuse)* glassAlbedo.rgb;
			float3 specularFactor = (fresnel * mask * specular);

			clip(albedo.a * alpha - 1.0 / 255.0);

			O.color0 = float4(lerp(lerp(bgglass.rgb, albedoFactor + specularFactor, albedo.a * glassAlbedo.a * fresnel), gXfFogColor, fogfactor), alpha);
#else

			float3 albedoFactor = (diffuse)* albedo.xyz;
			float3 specularFactor = (fresnel * mask * specular);

			if (gXfEnableAlphaTest == 1)
			{
				clip(albedo.a * alpha - gXfAlphaThreshold);
			}

			O.color0 = float4(lerp(albedoFactor + specularFactor, gXfFogColor, fogfactor), albedo.a * alpha);
#endif

			return O;
}

//=============================================================================
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