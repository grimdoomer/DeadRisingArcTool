using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using ImGuiNET;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public struct ImGuiShaderConstants
    {
        public const int kSizeOf = 16;

        public float[] mvp;
    }

    public class ImGuiRenderer : IRenderable
    {
        public Size WindowSize { get; set; }

        private Buffer vertexBuffer;
        private Buffer indexBuffer;

        private int vertexBufferSize = 5000;
        private int indexBufferSize = 10000;

        private ImDrawVert[] vertices;
        private ushort[] indices;

        private VertexShader vertexShader;
        private PixelShader pixelShader;

        private InputLayout inputLayout;

        private ImGuiShaderConstants shaderConstants;
        private Buffer shaderConstantsBuffer;

        //private DepthStencilState depthStencilState;
        private RasterizerState rasterState;
        private BlendState blendState;

        private Texture2D texture;
        private ShaderResourceView resourceView;
        private SamplerState samplerState;

        public ImGuiRenderer(int width, int height)
        {
            // Initialize fields.
            this.WindowSize = new Size(width, height);
        }

        public bool InitializeGraphics(RenderManager manager)
        {
            // Initialize ImGui stuff.
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.StyleColorsClassic();

            // Setup ImGui rendering info.
            ImGuiIOPtr io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            //ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;    // Enable Keyboard Controls
            //io.ConfigFlags |= ImGuiConfigFlags_NavEnableGamepad;      // Enable Gamepad Controls

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.Prior;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.Next;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Insert] = (int)Keys.Insert;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Back;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Return;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.KeyPadEnter] = (int)Keys.Return;
            io.KeyMap[(int)ImGuiKey.A] = 'A';
            io.KeyMap[(int)ImGuiKey.C] = 'C';
            io.KeyMap[(int)ImGuiKey.V] = 'V';
            io.KeyMap[(int)ImGuiKey.X] = 'X';
            io.KeyMap[(int)ImGuiKey.Y] = 'Y';
            io.KeyMap[(int)ImGuiKey.Z] = 'Z';

            // Initialize fonts.
            ImFontPtr font = io.Fonts.AddFontDefault();
            bool result = io.Fonts.Build();

            // Create a vertex buffer for drawing.
            this.vertices = new ImDrawVert[this.vertexBufferSize];
            this.vertexBuffer = Buffer.Create<ImDrawVert>(manager.Device, BindFlags.VertexBuffer, this.vertices);

            // Create the index buffer.
            this.indices = new ushort[this.indexBufferSize];
            this.indexBuffer = Buffer.Create<ushort>(manager.Device, BindFlags.IndexBuffer, this.indices);

            // Compile the vertex and pixel shaders.
            CompilationResult vertexByteCode = ShaderBytecode.CompileFromFile("FileFormats\\Geometry\\Shaders\\ImGuiShader.fx", "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            this.vertexShader = new VertexShader(manager.Device, vertexByteCode.Bytecode);

            CompilationResult pixelByteCode = ShaderBytecode.CompileFromFile("FileFormats\\Geometry\\Shaders\\ImGuiShader.fx", "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            this.pixelShader = new PixelShader(manager.Device, pixelByteCode.Bytecode);

            // Setup the input layout for the vertex declaration.
            this.inputLayout = new InputLayout(manager.Device, vertexByteCode.Bytecode, new InputElement[]
                {
                    new InputElement("POSITION",    0, SharpDX.DXGI.Format.R32G32_Float,    0, 0),
                    new InputElement("TEXCOORD",    0, SharpDX.DXGI.Format.R32G32_Float,    8, 0),
                    new InputElement("COLOR",       0, SharpDX.DXGI.Format.R8G8B8A8_UNorm,  16, 0)
                });

            // Create the shader constant buffer.
            this.shaderConstants.mvp = new float[16];
            this.shaderConstantsBuffer = Buffer.Create<float>(manager.Device, BindFlags.ConstantBuffer, this.shaderConstants.mvp);

            // Setup the rasterizer state.
            RasterizerStateDescription rasterStateDesc = new RasterizerStateDescription();
            rasterStateDesc.FillMode = FillMode.Solid;
            rasterStateDesc.CullMode = CullMode.None;
            rasterStateDesc.IsScissorEnabled = true;
            rasterStateDesc.IsDepthClipEnabled = true;
            this.rasterState = new RasterizerState(manager.Device, rasterStateDesc);

            // Setup the blending state.
            BlendStateDescription blendDesc = new BlendStateDescription();
            blendDesc.AlphaToCoverageEnable = false;
            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseSourceAlpha;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            this.blendState = new BlendState(manager.Device, blendDesc);

            // Get the font image info from imgui layer.
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int fontWidth, out int fontHeight, out int bytesPerPixel);

            // Create the font texture.
            Texture2DDescription textureDesc = new Texture2DDescription();
            textureDesc.Width = fontWidth;
            textureDesc.Height = fontHeight;
            textureDesc.MipLevels = 1;
            textureDesc.ArraySize = 1;
            textureDesc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            textureDesc.SampleDescription.Count = 1;
            textureDesc.Usage = ResourceUsage.Default;
            textureDesc.BindFlags = BindFlags.ShaderResource;
            this.texture = new Texture2D(manager.Device, textureDesc);

            // Setup the databox that wraps the pixel buffer.
            DataBox box = new DataBox(pixels, fontWidth * 4, fontWidth * 4 * fontHeight);
            manager.Device.ImmediateContext.UpdateSubresource(box, this.texture);

            // Setup the resource view for the font texture.
            this.resourceView = new ShaderResourceView(manager.Device, this.texture);

            // Set the texture id so we can retreive it later.
            io.Fonts.TexID = this.resourceView.NativePointer;

            // Create the shader sampler state.
            SamplerStateDescription samplerDesc = new SamplerStateDescription();
            samplerDesc.Filter = Filter.MinMagMipLinear;
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.MipLodBias = 0.0f;
            samplerDesc.ComparisonFunction = Comparison.Always;
            samplerDesc.MinimumLod = 0.0f;
            samplerDesc.MaximumLod = 0.0f;
            this.samplerState = new SamplerState(manager.Device, samplerDesc);

            // Successfully initialized resources.
            return true;
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Get the IO structure.
            ImGuiIOPtr io = ImGui.GetIO();

            // Get the ImGui draw data.
            ImDrawDataPtr drawData = ImGui.GetDrawData();

            // Make sure the vertex buffer can handle the data size requested.
            if (this.vertexBufferSize < drawData.TotalVtxCount)
            {
                // Free the old vertex buffer.
                this.vertexBuffer.Dispose();

                // Update the vertex buffer size.
                this.vertexBufferSize = drawData.TotalVtxCount + 1000;

                // Resize the vertex buffer.
                this.vertices = new ImDrawVert[this.vertexBufferSize];
                this.vertexBuffer = Buffer.Create<ImDrawVert>(manager.Device, BindFlags.VertexBuffer, this.vertices);
            }

            // Make sure the index buffer is large enough.
            if (this.indexBufferSize < drawData.TotalIdxCount)
            {
                // Free the old index buffer.
                this.indexBuffer.Dispose();

                // Update the index buffer size.
                this.indexBufferSize = drawData.TotalIdxCount + 1000;

                // Resize the index buffer.
                this.indices = new ushort[this.indexBufferSize];
                this.indexBuffer = Buffer.Create<ushort>(manager.Device, BindFlags.IndexBuffer, this.indices);
            }

            int vertexPosition = 0;
            int indexPosition = 0;

            // Loop through the command lists and copy all the vertex and index data for each one into our buffers.
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                // Get the command list info.
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];

                unsafe
                {
                    // Copy the vertex buffer for the command.
                    ImDrawVert* pSrcVert = (ImDrawVert*)cmdList.VtxBuffer.Data.ToPointer();
                    for (int x = 0; x < cmdList.VtxBuffer.Size; x++)
                        this.vertices[vertexPosition + x] = pSrcVert[x];

                    // Copy the index buffer for the command.
                    ushort* pSrcIndex = (ushort*)cmdList.IdxBuffer.Data.ToPointer();
                    for (int x = 0; x < cmdList.IdxBuffer.Size; x++)
                        this.indices[indexPosition + x] = pSrcIndex[x];
                }

                // Update the buffer indices.
                vertexPosition += cmdList.VtxBuffer.Size;
                indexPosition += cmdList.IdxBuffer.Size;
            }

            // Update vertex and index buffers.
            manager.Device.ImmediateContext.UpdateSubresource(this.vertices, this.vertexBuffer);
            manager.Device.ImmediateContext.UpdateSubresource(this.indices, this.indexBuffer);

            // Setup the orthographic projection matrix and copy it to the shader constants buffer.
            Matrix.OrthoOffCenterLH(
                drawData.DisplayPos.X,
                drawData.DisplayPos.X + drawData.DisplaySize.X,
                drawData.DisplayPos.Y + drawData.DisplaySize.Y,
                drawData.DisplayPos.Y,
                1.0f, -1.0f, out Matrix mvp);

            manager.Device.ImmediateContext.UpdateSubresource(mvp.ToArray(), this.shaderConstantsBuffer);

            // Setup the directx rendering state.
            manager.Device.ImmediateContext.InputAssembler.InputLayout = this.inputLayout;
            manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, 20, 0));
            manager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            manager.Device.ImmediateContext.VertexShader.Set(this.vertexShader);
            manager.Device.ImmediateContext.PixelShader.Set(this.pixelShader);

            manager.Device.ImmediateContext.VertexShader.SetConstantBuffer(0, this.shaderConstantsBuffer);
            manager.Device.ImmediateContext.PixelShader.SetConstantBuffer(0, this.shaderConstantsBuffer);
            manager.Device.ImmediateContext.PixelShader.SetSampler(0, this.samplerState);

            manager.Device.ImmediateContext.OutputMerger.SetBlendState(this.blendState, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), 0xFFFFFFFF);
            manager.Device.ImmediateContext.Rasterizer.State = this.rasterState;

            // Reset the vertex and index buffer positions.
            vertexPosition = 0;
            indexPosition = 0;

            drawData.ScaleClipRects(new System.Numerics.Vector2(1.0f, 1.0f));

            int vcount = drawData.TotalVtxCount;
            int icount = drawData.TotalIdxCount;
            bool valid = drawData.Valid;

            // Loop through all the command lists and render each batch of vertices.
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                // Get the command list info.
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];

                // Loop through all the commands in this list.
                for (int x = 0; x < cmdList.CmdBuffer.Size; x++)
                {
                    // Check if there is a user callback for this command.
                    if (cmdList.CmdBuffer[x].UserCallback != IntPtr.Zero)
                    {
                        // TODO:
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // Apply scissor/clipping rectangle.
                        manager.Device.ImmediateContext.Rasterizer.SetScissorRectangle(
                            (int)(cmdList.CmdBuffer[x].ClipRect.X - drawData.DisplayPos.X),
                            (int)(cmdList.CmdBuffer[x].ClipRect.Y - drawData.DisplayPos.Y),
                            (int)(cmdList.CmdBuffer[x].ClipRect.Z - drawData.DisplayPos.X),
                            (int)(cmdList.CmdBuffer[x].ClipRect.W - drawData.DisplayPos.Y));

                        // Check if this mesh is being rendered with a texture or not.
                        if (cmdList.CmdBuffer[x].TextureId != IntPtr.Zero)
                        {
                            // Bind the font texture.
                            manager.Device.ImmediateContext.PixelShader.SetShaderResource(0, CppObject.FromPointer<ShaderResourceView>(cmdList.CmdBuffer[x].TextureId));
                        }

                        // Draw primitives.
                        manager.Device.ImmediateContext.DrawIndexed((int)cmdList.CmdBuffer[x].ElemCount, (int)cmdList.CmdBuffer[x].IdxOffset + indexPosition, (int)cmdList.CmdBuffer[x].VtxOffset + vertexPosition);
                    }
                }

                // Update the vertex and index positions.
                vertexPosition += cmdList.VtxBuffer.Size;
                indexPosition += cmdList.IdxBuffer.Size;

                // TODO: Dispose of handle?
            }

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public void CleanupGraphics(RenderManager manager)
        {
            throw new NotImplementedException();
        }

        public bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox)
        {
            return false;
        }
    }
}
