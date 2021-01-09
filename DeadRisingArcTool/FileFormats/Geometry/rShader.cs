using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.Utilities;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry
{
    #region Enums

    public enum ShaderTechnique : uint
    {
        tXfScreenClear = 0x17796f56,
        tXfScreenCopy = 0xcab2a170,
        tXfYUY2Copy = 0xfe9a5edf,
        tXfRGBICopy = 0x3e5389ee,
        tXfRGBICubeCopy = 0x26461f2a,
        tXfSubPixelCopy = 0x48e2d4bb,
        tXfReductionZCopy = 0x96f834af,
        tXfMaterialDebug = 0x3b810b10,
        tXfMaterialZPass = 0xaa626ffb,
        tXfMaterialVelocity = 0x97471581,
        tXfMaterialShadowReceiver = 0x53ae7416,
        tXfMaterialShadowCaster = 0x30b6bdb8,
        tXfMaterialStandard = 0x88367c19,
        tXfFilterStandard = 0x640e8a05,
        tXfFilterBloom = 0x2d5767f8,
        tXfFilterDOF = 0x185168f1,
        tXfFilterTVNoise = 0x2e1ad607,
        tXfFilterVolumeNoise = 0xaf9cb5e1,
        tXfFilterRadialBlur = 0x2bb2716a,
        tXfFilterFeedbackBlur = 0xf4db0ad3,
        tXfFilterToneMap = 0xe8b68a03,
        tXfFilterGaussianBlur = 0xf7c48941,
        tXfFilterMotionBlur = 0x5ce1a976,
        tXfFilterMerge = 0x9d1670d5,
        tXfFilterImagePlane = 0x414e40d3,
        tXfFilterColorCorrect = 0x2b6ec81b,
        tXfFilterFXAA = 0x35bf832f,
        tXfResolveDepth = 0xd5b6d6b3,
        ShaderTechnique_e41f5dd1 = 0xe41f5dd1,
        ShaderTechnique_cc11f30d = 0xcc11f30d,
        ShaderTechnique_493a954c = 0x493a954c,
        ShaderTechnique_1fe78683 = 0x1fe78683,
        tXfPrimStandard = 0xed2827cf,
        tXfEnvmapCubicBlur = 0x312cb4e1,
        tXfEnvmapBlend = 0xe5f39d43,
        ShaderTechnique_fb4f06ed = 0xfb4f06ed,
        tXfMaterialSky = 0x399a88f9,
        tXfAdhesionPart = 0x5eecea3d,
        ShaderTechnique_23e98e1c = 0x23e98e1c,
        ShaderTechnique_49c0b237 = 0x49c0b237,
        ShaderTechnique_05168f29 = 0x05168f29,
        ShaderTechnique_5fa8066f = 0x5fa8066f,
        ShaderTechnique_fbc055e4 = 0xfbc055e4,
        ShaderTechnique_6cd7aba0 = 0x6cd7aba0,
        ShaderTechnique_ae243cee = 0xae243cee,
        ShaderTechnique_83614817 = 0x83614817,
        ShaderTechnique_c4dd56de = 0xc4dd56de,
        ShaderTechnique_ef36423a = 0xef36423a,
        ShaderTechnique_0264bf16 = 0x0264bf16,
        ShaderTechnique_fbb2f636 = 0xfbb2f636,
        ShaderTechnique_5a2a6823 = 0x5a2a6823,
        tXfPrimGpuParticleBatch = 0x2752e134,
        ShaderTechnique_81f59164 = 0x81f59164,
    }

    public enum ShaderParameter : uint
    {
        gXfScreenOfs = 0x17b88abb,
        gXfScreenScale = 0xcc139534,
        gXfScreen = 0x266863d3,
        gXfTestParam = 0x41cef39a,
        gXfJointWorld = 0x0eb7060f,
        gXfWorld = 0xae179d10,
        gXfView = 0x64f0e7ed,
        gXfViewI = 0x7e5901f6,
        gXfProj = 0xb6e09f69,
        gXfProjI = 0x3fd13704,
        gXfViewProj = 0xc8f2867d,
        gXfViewProjI = 0x51e31e18,
        gXfPrevWorld = 0x0b34877b,
        gXfPrevView = 0xe8dd2cb8,
        gXfPrevProj = 0xd0a63683,
        gXfPrevViewProj = 0x4cdecb48,
        gXfViewportSizeInv = 0xdc565d6e,
        gXfFogEnable = 0x15ea3fc0,
        gXfQuantPosScale = 0xac8e145b,
        gXfQuantPosOffset = 0x0696c70b,
        gXfEnvMapFactor = 0x0e3ce267,
        XfSamplerAlbedoMap = 0x3101c8f5,
        XfSamplerAlbedoMap2 = 0x7e205495,
        XfSamplerDetailMap = 0x7222dce7,
        XfSamplerNormalMap = 0x0bad2e67,
        XfSamplerMaskMap = 0xb3ca6e13,
        XfSamplerLightMap = 0x10cd25ee,
        XfSamplerLightMap2 = 0x5debb18e,
        XfSamplerShadowMap = 0x83109a3c,
        XfSamplerShadowMap2 = 0xd02f25dc,
        XfSamplerAdditionalMap = 0x1a84b4c4,
        XfSamplerEnvironmentMap = 0x0c4429f3,
        XfSamplerMatrixMap = 0xdf5af353,
        XfSamplerPrevMatrixMap = 0xe5eff19e,
        XfSamplerScreenMap = 0x62e9968e,
        XfBaseSampler = 0x4c3fb8d3,
        XfBaseCubeSampler = 0x34324e0f,
        XfDepthSampler = 0x0a77c097,
        XfBaseSamplerLinear = 0x93d5cb06,
        gXfMatrixMapFactor = 0xcbeaafb7,
        gXfFogFactor = 0x1e9fff86,
        gXfFogColor = 0x48dad899,
        gXfDetailFactor = 0x6a46b840,
        gXfBaseMapScale = 0x7caa9242,
        gXfLightMapScale = 0xe10be5c6,
        gXfLightMapScale2 = 0xb1fdfe06,
        gXfLightMapLerps = 0x2ef32f82,
        gXfLightNum = 0xb294b7b9,
        gXfLightParam = 0xac001283,
        gXfEyePos = 0xf465aa10,
        gXfSHcAr = 0xda2785a8,
        gXfSHcAg = 0x8096c1e8,
        gXfSHcAb = 0x07e966f1,
        gXfSHcBr = 0x3358236f,
        gXfSHcBg = 0x9b2512a9,
        gXfSHcBb = 0x25e8dfdd,
        gXfSHcC = 0x25d00eb9,
        gXfSHcTransmit = 0xeaf68bd8,
        gXfBlendFactor = 0xf705b40d,
        gXfFresnelFactor = 0x18308164,
        gXfParallaxFactor = 0x5076d5df,
        gXfUVScroll = 0xc68504cd,
        XfDebugBaseSampler = 0x7d4a21b9,
        XfDebugCubeSampler = 0x4af09e6a,
        PointSampler0 = 0x9a6df2af,
        PointSampler1 = 0x2df6ee51,
        PointSampler2 = 0xc496bf0c,
        PointSampler3 = 0x6b894162,
        PointSampler4 = 0xbddef9f9,
        PointSampler5 = 0x5d827df8,
        PointSampler6 = 0x2e8c3ddc,
        PointSampler7 = 0xd5217e61,
        LinearSampler0 = 0x0773f7a2,
        LinearSampler1 = 0x9afcf344,
        LinearSampler2 = 0x319cc3ff,
        LinearSampler3 = 0xd88f4655,
        LinearSampler4 = 0x2ae4feec,
        LinearSampler5 = 0xca8882eb,
        LinearSampler6 = 0x9b9242cf,
        LinearSampler7 = 0x42278354,
        ToneCurveSampler = 0xa4800d9b,
        VolumeSampler = 0xcf453579,
        TVNoiseSampler = 0x6b1c101f,
        TVMaskSampler = 0x2b0c5724,
        CubeSampler = 0xed254bd3,
        FXAASampler = 0x4e578a69,
        XfDepthMapMSAAx2 = 0xb1445399,
        XfDepthMapMSAAx4 = 0xbdae9fb5,
        XfDepthMapMSAAx8 = 0xcfa4602d,
        gXfVelocityFactor = 0xfdd9261b,
        gXfVelocityStretchEnable = 0xab29e867,
        gXfPow = 0x47b65999,
        gXfGradateColor = 0xb627d42d,
        gXfGradateFactor = 0x96e50c2c,
        fxaaPixelSize = 0xf014f0cb,
        gXfGamma = 0x9cbf3d01,
        gXfShiftHSV = 0x0805fc17,
        gXfConversionMatrix = 0x67b90f4b,
        gXfInBlack = 0xe4df1476,
        gXfInGamma = 0xd623c9e5,
        gXfInWhite = 0x46375fae,
        gXfOutBlack = 0xabcceb16,
        gXfOutWhite = 0x9024f194,
        gXfFocus = 0x725a0b45,
        gXfNear = 0x63663d26,
        gXfFar = 0xb1649db8,
        gXfFarBlurLimit = 0xe1297203,
        gXfNearBlurLimit = 0x932b1171,
        gXfCoCScale = 0x158270dd,
        gXfCoCBias = 0xf361d707,
        gXfRadiusScale = 0x5a3be08b,
        gXfPixelSizeHigh = 0x51b9fd9e,
        gXfPixelSizeLow = 0xa42be9ad,
        gXfTVNoisePower = 0xd4aab724,
        gXfTVNoiseUVOffset = 0xd0d0a7cf,
        gXfTVNoiseScanline = 0x97f7e132,
        gXfTVNoiseBlankWidth = 0x7daa0cf9,
        gXfTVNoiseHVSync = 0x8355b707,
        gXfTVNoiseBlankColor = 0x749529dc,
        gXfTextureColor = 0xb849e1a7,
        gXfDirection = 0x51357713,
        gXfPosition = 0x7e1cf259,
        gXfNoiseUVWOffset = 0x30eeabe1,
        gXfNoiseColor = 0x2f5dc3e0,
        gXfNoiseScale = 0x0e19e980,
        gXfNoiseBias = 0x8b5090a4,
        gXfEase = 0xda1e0bf9,
        gXfSampleOffsets = 0xc31e9bcf,
        gXfSampleWeights = 0x0d4c3558,
        gXfSampleScales = 0x389359e3,
        gXfScreenCenter = 0x0a2c7df2,
        gXfBlurStart = 0xab48c07c,
        gXfBlurWidth = 0x325f53b3,
        gXfAlpha = 0x300d69c2,
        gXfEnableMaxLuminance = 0xe689f4bf,
        gXfEnableMinLuminance = 0x8c6d3cf7,
        gXfEnableLogLuminance = 0xd2488da3,
        gXfMaxLuminance = 0x4cdc0d9d,
        gXfMinLuminance = 0xf2bf55d5,
        gXfElapsedTime = 0xc3897e38,
        gXfMiddleGray = 0x32e80d9d,
        gXfBrightPassOffset = 0x13d48623,
        gXfBrightPassThreshold = 0xc9e18503,
        gXfScaleColor = 0xb7ff01bc,
        gXfDepthHeightFactor = 0x761fb40e,
        gXfSectioningEnabled = 0xe757acda,
        gXfSectioningPoint = 0xd82faf92,
        gXfSectioningEquation = 0x639d6a09,
        gXfSphericalSectioningCenter = 0xda1f9552,
        gXfSphericalSectioningRadius = 0x14ba3210,
        XfShadowTransparency = 0xc2d1df57,
        XfShadowType = 0x01b2c1e6,
        XfShadowMask = 0x0485b394,
        XfShadowColor = 0x1f9cbefe,
        XfShadowColorCaster = 0xcc7a4ddc,
        XfShadowColorNoCaster = 0x32dd88d1,
        ShaderParameter_9eb6b50f = 0x9eb6b50f,
        ShaderParameter_0951cdb0 = 0x0951cdb0,
        ViewZ = 0x5773a4d3,
        InvTextureSize = 0x89d2a325,
        ZofsTrans = 0x5f181128,
        DepthBlend = 0xca02cfa2,
        gXfPrimType0 = 0x6357be60,
        gXfPrimType1 = 0x5b7bd747,
        gXfPrimDepthBlend = 0x63e5a55c,
        InvViewportSize = 0x1df29a66,
        gXfPrimType2 = 0x3e3d4531,
        XfPrimDepthPointSampler = 0xb27ff634,
        gXfOcclusion = 0x1564a5ac,
        gXfVolumeDepth = 0xc9d41d3b,
        XfPrimBasePointSampler = 0x1fe75530,
        XfPrimNormalLinearSampler = 0x9e52b9a6,
        XfPrimMaskLinearSampler = 0x667cf66e,
        XfPrimBaseLinearSampler = 0xbcb92646,
        XfPrimSceneSampler = 0xa56452e1,
        XfSamplerEnvmapCubeMap = 0x2be80a31,
        gXfEnvmapUpVector = 0xc64b6067,
        gXfEnvmapDelta = 0xbc7bcfe5,
        gXfEnvmapPow = 0x97262967,
        gXfEnvmapMipLevel = 0xc9f8534f,
        XfSamplerEnvmapCubeMapSource0 = 0xeb5d1bdc,
        XfSamplerEnvmapCubeMapSource1 = 0xe38134c3,
        gXfEnvmapBlendFactor = 0x8712b473,
        XfSamplerShadowMapNear = 0xf1f3efab,
        XfSamplerShadowMapMiddle = 0xee9afe83,
        XfSamplerShadowMapFar = 0x66b3724a,
        XfShadowProjectionMatrix0 = 0xa1fe14d7,
        XfShadowProjectionMatrix1 = 0x45322ac2,
        XfShadowProjectionMatrix2 = 0xcd908eed,
        XfShadowSlopeDepthBias = 0xe858c445,
        XfShadowLightDir = 0xabcabed7,
        XfShadowPCFRadius = 0xf4097363,
        XfShadowMapRange = 0xd070bbdb,
        XfShadowLightPos = 0x3389bd28,
        XfShadowSpotLightParam = 0x3a7b59e7,
        XfShadowBorderGradation = 0xc734af8b,
        XfShadowMaskPow = 0x3d5005ce,
        XfShadowDepthBias = 0x1091f149,
        XfShadowAlphaThreshold = 0xf1f6ce55,
        ShaderParameter_3d78bde7 = 0x3d78bde7,
        ShaderParameter_d1740b5a = 0xd1740b5a,
        ShaderParameter_c70a6632 = 0xc70a6632,
        ShaderParameter_c8684138 = 0xc8684138,
        ShaderParameter_971bd76d = 0x971bd76d,
        ShaderParameter_1281e0e8 = 0x1281e0e8,
        ShaderParameter_ed16a4a0 = 0xed16a4a0,
        gXfAdhesionProj = 0xf9fb01c7,
        gXfAdhesionDir = 0x855e590b,
        AdhesionSampler = 0xedbab8b7,
        gXfAdhesionColor = 0x91bff7a5,
        ShaderParameter_94467edd = 0x94467edd,
        gXfAdhesionSpecularIntensity = 0x1e679a9a,
        ShaderParameter_f2821d90 = 0xf2821d90,
        ShaderParameter_f306e78f = 0xf306e78f,
        ShaderParameter_2b9469c9 = 0x2b9469c9,
        ShaderParameter_2c1933c8 = 0x2c1933c8,
        ShaderParameter_cc45a0ba = 0xcc45a0ba,
        ShaderParameter_49e73154 = 0x49e73154,
        ShaderParameter_da4f78de = 0xda4f78de,
        ShaderParameter_47e76dac = 0x47e76dac,
        ShaderParameter_adaaeb46 = 0xadaaeb46,
        ShaderParameter_5d09f067 = 0x5d09f067,
        ShaderParameter_a12b3ddf = 0xa12b3ddf,
        ShaderParameter_b007a4f1 = 0xb007a4f1,
        ShaderParameter_40aa808a = 0x40aa808a,
        ShaderParameter_3f9ed8d0 = 0x3f9ed8d0,
        ShaderParameter_b419bf00 = 0xb419bf00,
        ShaderParameter_8772a209 = 0x8772a209,
        ShaderParameter_861e5a9e = 0x861e5a9e,
        ShaderParameter_84336869 = 0x84336869,
        ShaderParameter_00b96d4b = 0x00b96d4b,
        ShaderParameter_090a6848 = 0x090a6848,
        ShaderParameter_d29f0c79 = 0xd29f0c79,
        ShaderParameter_3c9bef23 = 0x3c9bef23,
        ShaderParameter_ddaf49ab = 0xddaf49ab,
        ShaderParameter_7f1f535d = 0x7f1f535d,
        ShaderParameter_3c6711d9 = 0x3c6711d9,
        ShaderParameter_1e6c79b3 = 0x1e6c79b3,
        ShaderParameter_73a29ef8 = 0x73a29ef8,
        ShaderParameter_72118515 = 0x72118515,
        ShaderParameter_aab42a50 = 0xaab42a50,
        ShaderParameter_083eaa57 = 0x083eaa57,
        ShaderParameter_3ec5a28b = 0x3ec5a28b,
        ShaderParameter_b3990f12 = 0xb3990f12,
        ShaderParameter_a36c4911 = 0xa36c4911,
        ShaderParameter_a2fc633d = 0xa2fc633d,
        ShaderParameter_8c0d16d6 = 0x8c0d16d6,
        ShaderParameter_78ba5148 = 0x78ba5148,
        ShaderParameter_b40038eb = 0xb40038eb,
        ShaderParameter_9e099f93 = 0x9e099f93,
        ShaderParameter_dee93d6b = 0xdee93d6b,
        ShaderParameter_83e17f67 = 0x83e17f67,
        ShaderParameter_e0471871 = 0xe0471871,
        ShaderParameter_aaea968b = 0xaaea968b,
        ShaderParameter_8aacdca2 = 0x8aacdca2,
        ShaderParameter_232b6b5d = 0x232b6b5d,
        ShaderParameter_7b42160f = 0x7b42160f,
        ShaderParameter_8633235b = 0x8633235b,
        ShaderParameter_607f8c1b = 0x607f8c1b,
        ShaderParameter_b307ac09 = 0xb307ac09,
        ShaderParameter_6810d807 = 0x6810d807,
        ShaderParameter_b7c4ead4 = 0xb7c4ead4,
        ShaderParameter_c2f9a3ec = 0xc2f9a3ec,
        ShaderParameter_7bf882c7 = 0x7bf882c7,
        ShaderParameter_f721ac2b = 0xf721ac2b,
        ShaderParameter_596b8441 = 0x596b8441,
        ShaderParameter_319e0e4e = 0x319e0e4e,
        ShaderParameter_a8256703 = 0xa8256703,
        ShaderParameter_285b04d8 = 0x285b04d8,
        ShaderParameter_9bebc619 = 0x9bebc619,
        ShaderParameter_84d6b465 = 0x84d6b465,
        ShaderParameter_5a06e545 = 0x5a06e545,
        ShaderParameter_4d9ca65d = 0x4d9ca65d,
        ShaderParameter_f960a9ba = 0xf960a9ba,
        ShaderParameter_3574215a = 0x3574215a,
        ShaderParameter_351ce0ad = 0x351ce0ad,
        ShaderParameter_97171200 = 0x97171200,
        ShaderParameter_7fbe4353 = 0x7fbe4353,
        ShaderParameter_bb2ab940 = 0xbb2ab940,
        ShaderParameter_51554af2 = 0x51554af2,
        ShaderParameter_97abf77f = 0x97abf77f,
        ShaderParameter_cf0275e1 = 0xcf0275e1,
        ShaderParameter_c082886e = 0xc082886e,
        ShaderParameter_958fc017 = 0x958fc017,
        ShaderParameter_25a29dc2 = 0x25a29dc2,
        ShaderParameter_78f61bf9 = 0x78f61bf9,
        PatternSize = 0x02ac362b,
        PatternRowNum = 0x79b66eb3,
        PatternTotalNum = 0x0d73d626,
        XfShadowObjDepthBias = 0x759de99c
    }

    public enum ShaderParameterType : byte
    {
        SamplerState = 0,
        Boolean,
        Integer,
        Float,
        Vector,
        Array
    }

    #endregion

    // sizeof = 0xC
    public struct rShaderHeader
    {
        public const int kSizeOf = 0xC;
        public const int kMagic = 0x584642;     // 'BFX'
        public const int kVersion = 257;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short Version;
        /* 0x06 */ public short TechniqueCount;
        /* 0x08 */ public int ParameterCount;
    }

    // sizeof = 0x18
    public struct rShaderTechniqueDesc
    {
        [Hex]
        /* 0x00 */ public ShaderTechnique TechniqueId;
        /* 0x04 */ public int ShaderDescSize;
        /* 0x08 */ public int ByteCodeSize;
        /* 0x0C */ // padding
        /* 0x10*/ //public long pTechnique;

        public rShaderSetDesc ShaderDescriptor;
    }

    // sizeof = 0x8
    public struct rShaderParameterDesc
    {
        [Hex]
        /* 0x00 */ public ShaderParameter ParameterId;
        /* 0x04 */ public ShaderParameterType Type;
        /* 0x05 */ public byte RegCount;
        /* 0x06 */ // padding?
    }

    // sizeof = 0x28
    public struct rShaderSetDesc
    {
        public const int kSizeOf = 0x28;

        /* 0x00 */ public int ShaderIndexCount;
        /* 0x04 */ public int Pipeline;
        /* 0x08 */ public int PixelShaderCount;
        /* 0x0C */ public int VertexShaderCount;
        /* 0x10 */ public int ShaderIndexOffset;
        /* 0x14 */ // padding
        /* 0x18 */ public int VertexShaderDataOffset;
        /* 0x1C */ // padding
        /* 0x20 */ public int PixelShaderDataOffset;
        /* 0x24 */ // padding

        public rShaderSetIndex[] ShaderIndices;
        public rShaderVertexShaderDesc[] VertexShaders;
        public rShaderPixelShaderDesc[] PixelShaders;
    }

    // sizeof = 4
    public struct rShaderSetIndex
    {
        /* 0x00 */ public short PixelShaderIndex;
        /* 0x02 */ public short VertexShaderIndex;
    }

    // sizeof = 2
    public struct rShaderSemanticIndices
    {
        /* 0x00 */ public byte SemanticNameIndex;       // Index into VertexDeclarationSemanticNames array
        /* 0x01 */ public byte SemanticIndex;           // Integer index of the semantic, ex: TEXCOORD0, TEXCOORD1, etc.
    }

    // sizeof = 0x40
    public struct rShaderVertexShaderDesc
    {
        /* 0x00 */ public int ByteCodeSize;
        /* 0x04 */ // padding
        /* 0x08 */ // never read, set to memory address of shader byte code at runtime?
        /* 0x0C */ // padding
        /* 0x10 */ public int ByteCodeOffset;
        /* 0x14 */ // padding
        /* 0x18 */ public rShaderSemanticIndices[] SemanticIndices;     // 16 elements
        /* 0x38 */ public long Flags;       // Upper 14 bits are number of parameters

        public rShaderParameterReference[] Parameters;
    }

    // sizeof = 0x18
    public struct rShaderPixelShaderDesc
    {
        /* 0x00 */ public int ByteCodeSize;
        /* 0x04 */ // padding
        /* 0x08 */ public int ByteCodeOffset;
        /* 0x0C */ // padding
        /* 0x10 */ public long Flags;       // Upper 14 bits are number of parameters

        public rShaderParameterReference[] Parameters;
    }

    // sizeof = 0x8
    public struct rShaderParameterReference
    {
        /* 0x00 */ public short ParameterIndex;
        /* 0x02 */ public short Offset;             // Offset of variable in shader constants buffer?
        /* 0x04 */ public short Size;               // Size of the parameter in bytes, or sampler state index
        /* 0x06 */ // padding
    }

    [GameResourceParser(ResourceType.rShader)]
    public class rShader : GameResource
    {
        public static readonly string[] VertexDeclarationSemanticNames = new string[]
        {
            "POSITION",
            "BLENDWEIGHT",
            "BLENDINDICES",
            "NORMAL",
            "PSIZE",
            "TEXCOORD",
            "TANGENT",
            "BINORMAL",
            "TESSFACTOR",
            "POSITIONT",
            "COLOR",
            "FOG",
            "DEPTH",
            "SAMPLE"
        };

        public rShaderHeader header;
        public rShaderTechniqueDesc[] techniques;
        public rShaderParameterDesc[] parameters;

        public rShader(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static rShader FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to hold the header.
            if (buffer.Length < rShaderHeader.kSizeOf)
                return null;

            // Create a new rShader to read into.
            rShader shader = new rShader(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Read the shader header.
            shader.header.Magic = reader.ReadInt32();
            shader.header.Version = reader.ReadInt16();
            shader.header.TechniqueCount = reader.ReadInt16();
            shader.header.ParameterCount = reader.ReadInt32();

            // Make sure the header magic and version are correct.
            if (shader.header.Magic != rShaderHeader.kMagic || shader.header.Version != rShaderHeader.kVersion)
            {
                // Header magic is invalid or version is not supported.
                return null;
            }

            // Allocate and ready the shader parameters.
            shader.parameters = new rShaderParameterDesc[shader.header.ParameterCount];
            for (int i = 0; i < shader.header.ParameterCount; i++)
            {
                // Read the shader parameter info.
                shader.parameters[i].ParameterId = (ShaderParameter)reader.ReadUInt32();
                shader.parameters[i].Type = (ShaderParameterType)reader.ReadByte();
                shader.parameters[i].RegCount = reader.ReadByte();
                reader.BaseStream.Position += 2;
            }

            // Allocate and read the shader techniques.
            shader.techniques = new rShaderTechniqueDesc[shader.header.TechniqueCount];
            for (int i = 0; i < shader.header.TechniqueCount; i++)
            {
                // Read the shader technique info.
                shader.techniques[i].TechniqueId = (ShaderTechnique)reader.ReadUInt32();
                shader.techniques[i].ShaderDescSize = reader.ReadInt32();
                shader.techniques[i].ByteCodeSize = reader.ReadInt32();
                reader.BaseStream.Position += 12;
            }

            // Loop for all the shader techniques and read the shader data for it.
            for (int i = 0; i < shader.header.TechniqueCount; i++)
            {
                // Save the offset of the descriptor.
                int baseOffset = (int)reader.BaseStream.Position;

                // Read the shader set descriptor.
                shader.techniques[i].ShaderDescriptor.ShaderIndexCount = reader.ReadInt32();
                shader.techniques[i].ShaderDescriptor.Pipeline = reader.ReadInt32();
                shader.techniques[i].ShaderDescriptor.PixelShaderCount = reader.ReadInt32();
                shader.techniques[i].ShaderDescriptor.VertexShaderCount = reader.ReadInt32();
                shader.techniques[i].ShaderDescriptor.ShaderIndexOffset = reader.ReadInt32() + baseOffset;
                reader.BaseStream.Position += 4;
                shader.techniques[i].ShaderDescriptor.VertexShaderDataOffset = reader.ReadInt32() + baseOffset;
                reader.BaseStream.Position += 4;
                shader.techniques[i].ShaderDescriptor.PixelShaderDataOffset = reader.ReadInt32() + baseOffset;
                reader.BaseStream.Position += 4;

                // Allocate arrays for the vertex and pixel shader descriptors.
                shader.techniques[i].ShaderDescriptor.ShaderIndices = new rShaderSetIndex[shader.techniques[i].ShaderDescriptor.ShaderIndexCount];
                shader.techniques[i].ShaderDescriptor.VertexShaders = new rShaderVertexShaderDesc[shader.techniques[i].ShaderDescriptor.VertexShaderCount];
                shader.techniques[i].ShaderDescriptor.PixelShaders = new rShaderPixelShaderDesc[shader.techniques[i].ShaderDescriptor.PixelShaderCount];

                // Loop and read all of the shader set indices.
                reader.BaseStream.Position = shader.techniques[i].ShaderDescriptor.ShaderIndexOffset;
                for (int x = 0; x < shader.techniques[i].ShaderDescriptor.ShaderIndexCount; x++)
                {
                    // Read the shader set info.
                    shader.techniques[i].ShaderDescriptor.ShaderIndices[x].PixelShaderIndex = reader.ReadInt16();
                    shader.techniques[i].ShaderDescriptor.ShaderIndices[x].VertexShaderIndex = reader.ReadInt16();
                }

                // Loop and read all of the vertex shader descriptors.
                for (int x = 0; x < shader.techniques[i].ShaderDescriptor.VertexShaderCount; x++)
                {
                    // Seek to the next vertex shader descriptor offset and read it.
                    reader.BaseStream.Position = shader.techniques[i].ShaderDescriptor.VertexShaderDataOffset + (x * 8);
                    long descOffset = reader.ReadInt64();

                    // Seek to and read the vertex shader descriptor.
                    reader.BaseStream.Position = baseOffset + descOffset;
                    shader.techniques[i].ShaderDescriptor.VertexShaders[x].ByteCodeSize = reader.ReadInt32();
                    reader.BaseStream.Position += 12;
                    shader.techniques[i].ShaderDescriptor.VertexShaders[x].ByteCodeOffset = reader.ReadInt32();
                    reader.BaseStream.Position += 0x24;
                    shader.techniques[i].ShaderDescriptor.VertexShaders[x].Flags = reader.ReadInt64();

                    // Allocate and read all the parameters for this shader.
                    int parameterCount = (int)(shader.techniques[i].ShaderDescriptor.VertexShaders[x].Flags >> 50);
                    shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters = new rShaderParameterReference[parameterCount];
                    for (int z = 0; z < parameterCount; z++)
                    {
                        // Read the parameter reference struct.
                        shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].ParameterIndex = reader.ReadInt16();
                        shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].Offset = reader.ReadInt16();
                        shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].Size = reader.ReadInt16();
                        reader.BaseStream.Position += 2;
                    }
                }

                // Loop and read all of the pixel shader descriptors.
                for (int x = 0; x < shader.techniques[i].ShaderDescriptor.PixelShaderCount; x++)
                {
                    // Seek to the next pixel shader descriptor offset and read it.
                    reader.BaseStream.Position = shader.techniques[i].ShaderDescriptor.PixelShaderDataOffset + (x * 8);
                    long descOffset = reader.ReadInt64();

                    // Seek to and read the pixel shader descriptor.
                    reader.BaseStream.Position = baseOffset + descOffset;
                    shader.techniques[i].ShaderDescriptor.PixelShaders[x].ByteCodeSize = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    shader.techniques[i].ShaderDescriptor.PixelShaders[x].ByteCodeOffset = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    shader.techniques[i].ShaderDescriptor.PixelShaders[x].Flags = reader.ReadInt64();

                    // Allocate and read all the parameters for this shader.
                    int parameterCount = (int)(shader.techniques[i].ShaderDescriptor.PixelShaders[x].Flags >> 50);
                    shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters = new rShaderParameterReference[parameterCount];
                    for (int z = 0; z < parameterCount; z++)
                    {
                        // Read the parameter reference struct.
                        shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].ParameterIndex = reader.ReadInt16();
                        shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].Offset = reader.ReadInt16();
                        shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].Size = reader.ReadInt16();
                        reader.BaseStream.Position += 2;
                    }
                }

                // Seek to the next descriptor.
                reader.BaseStream.Position = baseOffset + shader.techniques[i].ShaderDescSize + shader.techniques[i].ByteCodeSize;
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the shader object.
            return shader;
        }
    }
}
