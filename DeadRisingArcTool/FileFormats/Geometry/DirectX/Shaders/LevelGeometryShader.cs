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
    [ShaderAttribute(ShaderType.Game_LevelGeometry1)]
    public class LevelGeometry1Shader : Shader
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

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfLevelMesh.vs");
            this.VertexShader = new VertexShader(manager.Device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfLevelMesh.ps");
            this.PixelShader = new PixelShader(manager.Device, pixelByteCode);

            // Setup the sampler states for the vertex shader.
            SamplerStateDescription samplerDesc1 = new SamplerStateDescription();
            samplerDesc1.AddressU = TextureAddressMode.Wrap;
            samplerDesc1.AddressV = TextureAddressMode.Wrap;
            samplerDesc1.AddressW = TextureAddressMode.Wrap;
            samplerDesc1.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc1.MaximumLod = 0;
            samplerDesc1.Filter = Filter.Anisotropic;
            samplerDesc1.MipLodBias = 0;
            samplerDesc1.MaximumAnisotropy = 3;

            SamplerStateDescription samplerDesc2 = new SamplerStateDescription();
            samplerDesc2.AddressU = TextureAddressMode.Wrap;
            samplerDesc2.AddressV = TextureAddressMode.Wrap;
            samplerDesc2.AddressW = TextureAddressMode.Wrap;
            samplerDesc2.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc2.MaximumLod = 0;
            samplerDesc2.Filter = Filter.MinMagMipLinear;
            samplerDesc2.MipLodBias = 0;
            samplerDesc2.MaximumAnisotropy = 1;
            samplerDesc2.ComparisonFunction = Comparison.Never;

            this.PixelSampleStates = new SamplerState[] { new SamplerState(manager.Device, samplerDesc1), new SamplerState(manager.Device, samplerDesc2), new SamplerState(manager.Device, samplerDesc2) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(manager.Device, vertexByteCode.Data, LevelGeometry1Shader.VertexFormat);

            // Successfully initialized.
            return true;
        }
    }
}
