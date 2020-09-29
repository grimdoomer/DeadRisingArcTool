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
    /// A skinned rigid mesh with 4 bone weights per vertex
    /// </summary>
    [ShaderAttribute(ShaderType.SkinnedRigid4W)]
    public class SkinnedRigid4WMesh : Shader
    {
        public static readonly InputElement[] VertexFormat = new InputElement[]
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

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh4W.vs");
            this.VertexShader = new VertexShader(manager.Device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh4W.ps");
            this.PixelShader = new PixelShader(manager.Device, pixelByteCode);

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
            this.VertexSampleStates = new SamplerState[] { new SamplerState(manager.Device, vertexDesc) };

            SamplerStateDescription pixelDesc = new SamplerStateDescription();
            pixelDesc.AddressU = TextureAddressMode.Wrap;
            pixelDesc.AddressV = TextureAddressMode.Wrap;
            pixelDesc.AddressW = TextureAddressMode.Wrap;
            pixelDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            pixelDesc.MaximumLod = 0;
            pixelDesc.Filter = Filter.Anisotropic;
            pixelDesc.MipLodBias = 0;
            pixelDesc.MaximumAnisotropy = 3;
            this.PixelSampleStates = new SamplerState[] { new SamplerState(manager.Device, pixelDesc) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(manager.Device, vertexByteCode.Data, SkinnedRigid4WMesh.VertexFormat);

            // Successfully initialized.
            return true;
        }
    }

    /// <summary>
    /// A skinned rigid mesh with 8 bone weights per vertex
    /// </summary>
    [ShaderAttribute(ShaderType.SkinnedRigid8W)]
    public class SkinnedRigid8WMesh : Shader
    {
        public static readonly InputElement[] VertexFormat = new InputElement[]
        {
            // Id: 0x87a34e22
            new InputElement("POSITION",        0, Format.R16G16B16A16_SNorm,   0,  0),
            new InputElement("BLENDINDICES",    0, Format.R8G8B8A8_UInt,        8,  0),
            new InputElement("BLENDINDICES",    1, Format.R8G8B8A8_UInt,        12, 0),
            new InputElement("BLENDWEIGHT",     0, Format.R8G8B8A8_UNorm,       16, 0),
            new InputElement("BLENDWEIGHT",     1, Format.R8G8B8A8_UNorm,       20, 0),
            new InputElement("NORMAL",          0, Format.R16G16B16A16_SNorm,   24, 0),
            new InputElement("TEXCOORD",        0, Format.R16G16_SNorm,         32, 0),
            new InputElement("TANGENT",         0, Format.R16G16B16A16_SNorm,   0,  1),
            new InputElement("TEXCOORD",        1, Format.R16G16_SNorm,         8,  1),
            new InputElement("TEXCOORD",        2, Format.R32G32B32A32_Float,   0,  0),
            new InputElement("TEXCOORD",        3, Format.R32G32B32A32_Float,   0,  0),
        };

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh8W.vs");
            this.VertexShader = new VertexShader(manager.Device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMesh8W.ps");
            this.PixelShader = new PixelShader(manager.Device, pixelByteCode);

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
            this.VertexSampleStates = new SamplerState[] { new SamplerState(manager.Device, vertexDesc) };

            SamplerStateDescription pixelDesc = new SamplerStateDescription();
            pixelDesc.AddressU = TextureAddressMode.Wrap;
            pixelDesc.AddressV = TextureAddressMode.Wrap;
            pixelDesc.AddressW = TextureAddressMode.Wrap;
            pixelDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            pixelDesc.MaximumLod = 0;
            pixelDesc.Filter = Filter.Anisotropic;
            pixelDesc.MipLodBias = 0;
            pixelDesc.MaximumAnisotropy = 3;
            this.PixelSampleStates = new SamplerState[] { new SamplerState(manager.Device, pixelDesc) };

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(manager.Device, vertexByteCode.Data, SkinnedRigid8WMesh.VertexFormat);

            // Successfully initialized.
            return true;
        }
    }
}
