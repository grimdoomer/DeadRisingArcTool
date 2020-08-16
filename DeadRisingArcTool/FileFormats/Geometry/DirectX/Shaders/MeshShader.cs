using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders
{
    /// <summary>
    /// Shader used for all game meshes other than level geometry
    /// </summary>
    [BuiltInShader(BuiltInShaderType.Game_Mesh)]
    public class MeshShader : BuiltInShader
    {
        public static readonly InputElement[] VertexFormat = new InputElement[9]
        {
            // Id: 0x0550228e
            new InputElement("POSITION",        0, Format.R16G16B16A16_SNorm,   0,  0),
            new InputElement("NORMAL",          0, Format.R16G16B16A16_SNorm,   16, 0),
            new InputElement("TANGENT",         0, Format.R16G16B16A16_SNorm,   0,  1),
            new InputElement("TEXCOORD",        0, Format.R16G16_SNorm,         24, 0),
            new InputElement("TEXCOORD",        1, Format.R16G16_SNorm,         8,  1),
            new InputElement("BLENDWEIGHT",     0, Format.R8G8B8A8_UNorm,       12, 0),
            new InputElement("BLENDINDICES",    0, Format.R8G8B8A8_SInt,        8,  0),
            new InputElement("TEXCOORD",        2, Format.R32G32B32A32_Float,   0,  0),
            new InputElement("TEXCOORD",        3, Format.R32G32B32A32_Float,   0,  0),
        };

        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh.vs");
            this.VertexShader = new VertexShader(device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh.ps");
            this.PixelShader = new PixelShader(device, pixelByteCode);

            // Setup the sampler states for the vertex shader.
            SamplerStateDescription vertexDesc = new SamplerStateDescription();
            vertexDesc.AddressU = TextureAddressMode.Wrap;
            vertexDesc.AddressV = TextureAddressMode.Wrap;
            vertexDesc.AddressW = TextureAddressMode.Wrap;
            vertexDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            vertexDesc.MaximumLod = 13;
            vertexDesc.Filter = Filter.MinMagMipPoint;
            vertexDesc.MipLodBias = 0;
            vertexDesc.MaximumAnisotropy = 1;
            vertexDesc.ComparisonFunction = Comparison.Never;
            this.VertexSampleStates = new SamplerState[] { new SamplerState(device, vertexDesc) };

            SamplerStateDescription pixelDesc = new SamplerStateDescription();
            pixelDesc.AddressU = TextureAddressMode.Wrap;
            pixelDesc.AddressV = TextureAddressMode.Wrap;
            pixelDesc.AddressW = TextureAddressMode.Wrap;
            pixelDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            pixelDesc.MaximumLod = 0;
            pixelDesc.Filter = Filter.Anisotropic;
            pixelDesc.MipLodBias = 0;
            pixelDesc.MaximumAnisotropy = 3;
            this.PixelSampleStates = new SamplerState[] { new SamplerState(device, pixelDesc) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(device, vertexByteCode.Data, MeshShader.VertexFormat);

            // Successfully initialized.
            return true;
        }
    }
}
