using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders
{
    /// <summary>
    /// Shader used for level geometry
    /// </summary>
    [BuiltInShader(BuiltInShaderType.Game_LevelGeometry1)]
    public class LevelGeometry1Shader : BuiltInShader
    {
        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfLevelMesh.vs");
            this.VertexShader = new VertexShader(device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfLevelMesh.ps");
            this.PixelShader = new PixelShader(device, pixelByteCode);

            // Setup the sampler states for the vertex shader.
            SamplerStateDescription samplerDesc = new SamplerStateDescription();
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc.MaximumLod = 0;
            samplerDesc.Filter = Filter.Anisotropic;
            samplerDesc.MipLodBias = 0;
            samplerDesc.MaximumAnisotropy = 3;
            this.PixelSampleStates = new SamplerState[] { new SamplerState(device, samplerDesc) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(device, vertexByteCode.Data, new InputElement[]
                    {
                        // Id: 0x3259609d
                    new InputElement("POSITION",    0, Format.R32G32B32_Float,      0,  0),
                    new InputElement("NORMAL",      0, Format.R16G16B16A16_SNorm,   12, 0),
                    new InputElement("TANGENT",     0, Format.R16G16B16A16_SNorm,   0,  1),
                    new InputElement("TEXCOORD",    0, Format.R32G32_Float,         20, 0),
                    new InputElement("TEXCOORD",    1, Format.R32G32_Float,         8,  1),
                    new InputElement("TEXCOORD",    2, Format.R16G16_SNorm,         16, 1),
                    new InputElement("TEXCOORD",    3, Format.R32G32_Float,         20, 1),
                    });

            // Successfully initialized.
            return true;
        }
    }

    [BuiltInShader(BuiltInShaderType.Game_LevelGeometry2)]
    public class LevelGeometry2Shader : BuiltInShader
    {
        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh.vs");
            this.VertexShader = new VertexShader(device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh.ps");
            this.PixelShader = new PixelShader(device, pixelByteCode);

            // Setup the sampler states for the vertex shader.
            SamplerStateDescription samplerDesc = new SamplerStateDescription();
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc.MaximumLod = 0;
            samplerDesc.Filter = Filter.Anisotropic;
            samplerDesc.MipLodBias = 0;
            samplerDesc.MaximumAnisotropy = 3;
            this.PixelSampleStates = new SamplerState[] { new SamplerState(device, samplerDesc) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(device, vertexByteCode.Data, new InputElement[]
                    {
                        /*
                            Id: 0x87a34e22
                                Elem 0: Slot=0 Offset=0 Format=R16G16B16A16_SNORM SemanticName=POSITION Index=0
                                Elem 1: Slot=0 Offset=8 Format=R8G8B8A8_UINT SemanticName=BLENDINDICES Index=0
                                Elem 2: Slot=0 Offset=12 Format=R8G8B8A8_UINT SemanticName=BLENDINDICES Index=1
                                Elem 3: Slot=0 Offset=16 Format=R8G8B8A8_UNORM SemanticName=BLENDWEIGHT Index=0
                                Elem 4: Slot=0 Offset=20 Format=R8G8B8A8_UNORM SemanticName=BLENDWEIGHT Index=1
                                Elem 5: Slot=0 Offset=24 Format=R16G16B16A16_SNORM SemanticName=NORMAL Index=0
                                Elem 6: Slot=0 Offset=32 Format=R16G16_SNORM SemanticName=TEXCOORD Index=0
                                Elem 7: Slot=1 Offset=0 Format=R16G16B16A16_SNORM SemanticName=TANGENT Index=0
                                Elem 8: Slot=1 Offset=8 Format=R16G16_SNORM SemanticName=TEXCOORD Index=1
                        */

                    new InputElement("POSITION",        0, Format.R16G16B16A16_SNorm,   0,  0),
                    new InputElement("TEXCOORD",        0, Format.R16G16_SNorm,         24, 0),
                    new InputElement("BLENDWEIGHT",     0, Format.R8G8B8A8_UNorm,       12, 0),
                    new InputElement("BLENDINDICES",    0, Format.R8G8B8A8_UInt,        8,  0),

                    new InputElement("NORMAL",          0, Format.R16G16B16A16_SNorm,   16, 0),
                    new InputElement("TANGENT",         0, Format.R16G16B16A16_SNorm,   0,  0),
                    new InputElement("TEXCOORD",        1, Format.R16G16_SNorm,         8,  0),
                    new InputElement("TEXCOORD",        2, Format.R16G16_SNorm,         0,  0),
                    new InputElement("TEXCOORD",        3, Format.R16G16_SNorm,         0,  0),
                    });

            // Successfully initialized.
            return true;
        }
    }
}
