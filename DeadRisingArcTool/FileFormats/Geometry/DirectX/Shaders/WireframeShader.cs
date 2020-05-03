using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders
{
    [BuiltInShader(BuiltInShaderType.Wireframe)]
    public class WireframeShader : BuiltInShader
    {
        public override bool InitializeGraphics(IRenderManager manager, SharpDX.Direct3D11.Device device)
        {
            // Compile our vertex and pixel shaders.
            ShaderBytecode vertexByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\Wireframe.vs");
            this.VertexShader = new VertexShader(device, vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(System.Windows.Forms.Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\Wireframe.ps");
            this.PixelShader = new PixelShader(device, pixelByteCode);

            // Setup our vertex declaration and bind it to the inputs for the vertex shader.
            this.VertexDeclaration = new InputLayout(device, vertexByteCode.Data, new InputElement[]
                    {
                    new InputElement("POSITION",        0, Format.R32G32B32_Float,   0,  0),
                    new InputElement("COLOR",           0, Format.R32G32B32A32_Float, 12, 0),
                    });

            // Successfully initialized.
            return true;
        }
    }
}
