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
        public static readonly InputElement[] VertexFormat = new InputElement[]
        {
            // Id: 0x7976290a
            new InputElement("POSITION",    0, Format.R32G32B32_Float,      0,  0),
            new InputElement("NORMAL",      0, Format.R16G16B16A16_SNorm,   12, 0),
            new InputElement("TANGENT",     0, Format.R16G16B16A16_SNorm,   0,  1),
            new InputElement("TEXCOORD",    0, Format.R32G32_Float,         20, 0),
            new InputElement("TEXCOORD",    1, Format.R32G32_Float,         8,  1),
            new InputElement("TEXCOORD",    2, Format.R16G16_SNorm,         16, 1),
            new InputElement("TEXCOORD",    3, Format.R32G32_Float,         20, 1),
        };

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
            this.VertexDeclaration = new InputLayout(device, vertexByteCode.Data, LevelGeometry1Shader.VertexFormat);

            // Successfully initialized.
            return true;
        }
    }
}
