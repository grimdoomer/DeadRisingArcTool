using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX;
using System.Runtime.InteropServices;
using DeadRisingArcTool.Graphics;
using DeadRisingArcTool.FileFormats.Bitmaps;

namespace DeadRisingArcTool.Forms
{
    struct MyVertex
    {
        public Vector3 Position;
        public Vector4 Color;
    }

#if REAL_CODE
    public class ShaderConstants
    {
        public const int kSizeOf = (4 * 4 * 4) + (4 * 4) + (4 * 4) + (4 * 4);
        public const int kElementCount = (4 * 4) + 4 + 4 + 4;

        public Matrix gXfViewProj = new Matrix();
        public Vector4 gXfMatrixMapFactor = new Vector4();
        public Vector3 gXfQuantPosScale = new Vector3();
        public Vector3 gXfQuantPosOffset = new Vector3();

        public float[] ToBufferData()
        {
            // Allocate a float array to copy all the values to.
            float[] buffer = new float[kElementCount];

            // gXfViewProj:
            buffer[0] = this.gXfViewProj[0, 0];
            buffer[1] = this.gXfViewProj[1, 0];
            buffer[2] = this.gXfViewProj[2, 0];
            buffer[3] = this.gXfViewProj[3, 0];
            buffer[4] = this.gXfViewProj[0, 1];
            buffer[5] = this.gXfViewProj[1, 1];
            buffer[6] = this.gXfViewProj[2, 1];
            buffer[7] = this.gXfViewProj[3, 1];
            buffer[8] = this.gXfViewProj[0, 2];
            buffer[9] = this.gXfViewProj[1, 2];
            buffer[10] = this.gXfViewProj[2, 2];
            buffer[11] = this.gXfViewProj[3, 2];
            buffer[12] = this.gXfViewProj[0, 3];
            buffer[13] = this.gXfViewProj[1, 3];
            buffer[14] = this.gXfViewProj[2, 3];
            buffer[15] = this.gXfViewProj[3, 3];

            // gXfMatrixMapFactor:
            buffer[16] = this.gXfMatrixMapFactor[0];
            buffer[17] = this.gXfMatrixMapFactor[1];
            buffer[18] = this.gXfMatrixMapFactor[2];
            buffer[19] = this.gXfMatrixMapFactor[3];

            // gXfQuantPosScale:
            buffer[20] = this.gXfQuantPosScale[0];
            buffer[21] = this.gXfQuantPosScale[1];
            buffer[22] = this.gXfQuantPosScale[2];

            // gXfQuantPosOffset:
            buffer[24] = this.gXfQuantPosOffset[0];
            buffer[25] = this.gXfQuantPosOffset[1];
            buffer[26] = this.gXfQuantPosOffset[2];

            // Return the buffer.
            return buffer;
        }
    }
#else
    public class ShaderConstants
    {
        public Matrix WVP = new Matrix();
        public Matrix World = new Matrix();

        public float[] ToBufferData()
        {
            List<float> buffer = new List<float>();
            buffer.AddRange(this.WVP.ToArray());
            buffer.AddRange(this.World.ToArray());

            return buffer.ToArray();
        }
    }
#endif

    public partial class RenderView : Form
    {
        // List of currently loaded arc files.
        ArcFile arcFile = null;

        // Model to render.
        rModel model = null;

        // DirectX interfaces for rendering.
        SharpDX.Direct3D11.Device device = null;
        SwapChain swapChain = null;
        Texture2D backBuffer = null;
        Texture2D depthStencil = null;
        RenderTargetView renderView = null;
        DepthStencilView depthStencilView = null;
        DepthStencilState depthStencilState = null;
        RasterizerState rasterState = null;

        // Camera helper.
        Camera camera = null;
        Matrix projectionMatrix;
        Matrix worldGround;

        // Shaders used for rendering.
        ShaderBytecode vertexByteCode = null;
        VertexShader vertexShader = null;
        PixelShader pixelShader = null;
        SamplerState shaderSampler = null;

        // Vertex and index buffers for rendering.
        InputLayout vertexDecl = null;
        SharpDX.Direct3D11.Buffer primaryVertexBuffer = null;
        SharpDX.Direct3D11.Buffer secondaryVertexBuffer = null;
        SharpDX.Direct3D11.Buffer indexBuffer = null;

        // Shader constant buffer.
        ShaderConstants shaderConsts = new ShaderConstants();
        SharpDX.Direct3D11.Buffer shaderConstantBuffer = null;

        // Texture array used for materials.
        DataStream[] textureStreams = null;
        Texture2D[] textures = null;
        ShaderResourceView[] shaderResources = null;

        // Indicates if we should quit the render loop.
        bool isClosing = false;

        public RenderView(ArcFile arcFile, rModel model)
        {
            // Initialize fields.
            this.arcFile = arcFile;
            this.model = model;

            InitializeComponent();
        }

        private void RenderView_Load(object sender, EventArgs e)
        {
            // Initialize D3D layer.
            this.Show();
            InitializeD3D();

            // Create the rendering buffers from the model data.
            InitializeModelData();

            // Show the form and enter the render loop.
            while (this.isClosing == false)
            {
                // Render the next frame and process the windows message queue.
                Render();
                Application.DoEvents();
            }
        }

        private void RenderView_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            // Flag that we are closing so the render loop can kill itself.
            this.isClosing = true;
        }

        private void InitializeD3D()
        {
            // Setup the swapchain description structure.
            SwapChainDescription desc = new SwapChainDescription();
            desc.BufferCount = 1;
            desc.ModeDescription = new ModeDescription(this.ClientSize.Width, this.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
            desc.OutputHandle = this.Handle;
            desc.SampleDescription = new SampleDescription(1, 0);
            desc.SwapEffect = SwapEffect.Discard;
            desc.Usage = Usage.RenderTargetOutput;
            desc.IsWindowed = true;

            // Create the device and swapchain.
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.Debug, desc, out this.device, out this.swapChain);

            // Setup the projection matrix.
            this.projectionMatrix = Matrix.PerspectiveFovRH(Camera.DegreesToRadian(95.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 1.0f, 10000.0f);
            this.worldGround = Matrix.Identity;

            // Ignore all window events.
            // TODO:

            // Create our output texture for rendering.
            this.backBuffer = Texture2D.FromSwapChain<Texture2D>(this.swapChain, 0);
            this.renderView = new RenderTargetView(this.device, this.backBuffer);

            // Create a texture for the dpeth stencil.
            Texture2DDescription depthStencilDesc = new Texture2DDescription();
            depthStencilDesc.Width = this.ClientSize.Width;
            depthStencilDesc.Height = this.ClientSize.Height;
            depthStencilDesc.MipLevels = 1;
            depthStencilDesc.ArraySize = 1;
            depthStencilDesc.Format = Format.D32_Float;
            depthStencilDesc.SampleDescription.Count = 1;
            depthStencilDesc.SampleDescription.Quality = 0;
            depthStencilDesc.Usage = ResourceUsage.Default;
            depthStencilDesc.BindFlags = BindFlags.DepthStencil;
            this.depthStencil = new Texture2D(this.device, depthStencilDesc);

            // Create the depth stencil view.
            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription();
            depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
            depthStencilViewDesc.Format = Format.D32_Float;
            this.depthStencilView = new DepthStencilView(this.device, this.depthStencil, depthStencilViewDesc);
            this.device.ImmediateContext.OutputMerger.SetRenderTargets(this.depthStencilView, this.renderView);

            // Setup the depth stencil state.
            DepthStencilStateDescription stateDesc = new DepthStencilStateDescription();
            stateDesc.IsDepthEnabled = true;
            stateDesc.DepthWriteMask = DepthWriteMask.All;
            stateDesc.DepthComparison = Comparison.LessEqual;
            stateDesc.IsStencilEnabled = false;
            stateDesc.StencilReadMask = 0xFF;
            stateDesc.StencilWriteMask = 0xFF;
            stateDesc.FrontFace.FailOperation = StencilOperation.Increment;
            stateDesc.FrontFace.DepthFailOperation = StencilOperation.Increment;
            stateDesc.FrontFace.PassOperation = StencilOperation.Decrement;
            stateDesc.FrontFace.Comparison = Comparison.LessEqual;
            stateDesc.BackFace.FailOperation = StencilOperation.Increment;
            stateDesc.BackFace.DepthFailOperation = StencilOperation.Increment;
            stateDesc.BackFace.PassOperation = StencilOperation.Decrement;
            stateDesc.BackFace.Comparison = Comparison.LessEqual;
            this.depthStencilState = new DepthStencilState(this.device, stateDesc);

            // Setup the rasterizer state.
            RasterizerStateDescription rasterStateDesc = new RasterizerStateDescription();
            rasterStateDesc.FillMode = FillMode.Solid;
            rasterStateDesc.CullMode = CullMode.Front;
            rasterStateDesc.IsDepthClipEnabled = true;
            this.rasterState = new RasterizerState(this.device, rasterStateDesc);

            // Set the viewport.
            this.device.ImmediateContext.Rasterizer.SetViewport(0, 0, (float)this.ClientSize.Width, (float)this.ClientSize.Height, 0.0f, 1.0f);

#if REAL_CODE
            // Compile out vertex and pixel shaders.
            this.vertexByteCode = ShaderBytecode.FromFile(Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMaterialZPass.vs");
            this.vertexShader = new VertexShader(this.device, this.vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\XfMaterialZPass.ps");
            this.pixelShader = new PixelShader(this.device, pixelByteCode);
#else
            // Compile out vertex and pixel shaders.
            this.vertexByteCode = ShaderBytecode.FromFile(Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\BasicShader.vs");
            this.vertexShader = new VertexShader(this.device, this.vertexByteCode);

            ShaderBytecode pixelByteCode = ShaderBytecode.FromFile(Application.StartupPath + "\\FileFormats\\Geometry\\Shaders\\BasicShader.ps");
            this.pixelShader = new PixelShader(this.device, pixelByteCode);
#endif

            // Setup the sampler state for the vertex shader.
            SamplerStateDescription samplerDesc = new SamplerStateDescription();
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc.MaximumLod = 0;
            samplerDesc.Filter = Filter.Anisotropic;
            samplerDesc.MipLodBias = 0;
            samplerDesc.MaximumAnisotropy = 3;
            this.shaderSampler = new SamplerState(this.device, samplerDesc);

            // Initialize the camera and set starting position.
            this.camera = new Camera(this);
            this.camera.speed = 0.002f;

            this.camera.ComputePosition();
        }

        private void InitializeModelData()
        {
#if REAL_CODE

            // Setup the vertex declaration.
            this.vertexDecl = new InputLayout(this.device, this.vertexByteCode.Data, new InputElement[]
                {
                    new InputElement("POSITION", 0, Format.R16G16B16A16_SNorm, 0, 0),
                    new InputElement("NORMAL", 0, Format.R16G16B16A16_SNorm, 16, 0),
                    new InputElement("TANGENT", 0, Format.R16G16B16A16_SNorm, 0, 1),
                    new InputElement("TEXCOORD", 0, Format.R16G16_SNorm, 24, 0),
                    new InputElement("TEXCOORD", 1, Format.R16G16_SNorm, 8, 1),
                    new InputElement("BLENDWEIGHT", 0, Format.R8G8B8A8_UNorm, 12, 0),
                    new InputElement("BLENDINDICES", 0, Format.R8G8B8A8_UInt, 8, 0),
                });

            // Create the vertex and index buffer from the model data streams.
            this.primaryVertexBuffer = SharpDX.Direct3D11.Buffer.Create<byte>(this.device, BindFlags.VertexBuffer, this.model.vertexData1);
            this.secondaryVertexBuffer = SharpDX.Direct3D11.Buffer.Create<byte>(this.device, BindFlags.VertexBuffer, this.model.vertexData2);
            this.indexBuffer = SharpDX.Direct3D11.Buffer.Create<short>(this.device, BindFlags.IndexBuffer, this.model.indiceBuffer);

            // Set the bounding box parameters to the vertex shader constant buffer.
            bool test = true;
            if (test == true)
            {
                this.shaderConsts.gXfQuantPosScale = this.model.header.BoundingBoxMax - this.model.header.BoundingBoxMin;
                this.shaderConsts.gXfQuantPosOffset = this.model.header.BoundingBoxMin;
                this.shaderConsts.gXfMatrixMapFactor = new Vector4(144.0f, 0.0f, 0.00293f, 0.00098f);
            }
            else
            {
                this.shaderConsts.gXfQuantPosScale = new Vector3(1.0f, 1.0f, 1.0f);// this.model.header.BoundingBoxMax - this.model.header.BoundingBoxMin;
                this.shaderConsts.gXfQuantPosOffset = new Vector3(1.0f, 1.0f, 1.0f);// this.model.header.BoundingBoxMin;
                this.shaderConsts.gXfMatrixMapFactor = new Vector4(144.0f, 0.0f, 0.00293f, 0.00098f);
            }

#else

            // Setup the vertex declaration.
            this.vertexDecl = new InputLayout(this.device, this.vertexByteCode, new InputElement[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
                });

            // Vertex buffer.
            MyVertex[] vertices = new MyVertex[]
                {
                    //new MyVertex() { Position = new Vector3(0.0f, 0.5f, 0.0f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },
                    //new MyVertex() { Position = new Vector3(0.5f, -0.5f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    //new MyVertex() { Position = new Vector3(-0.5f, -0.5f, 0.0f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }

                    new MyVertex() { Position = new Vector3(-1f, -1f, 0.0f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(-1f, 1f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, 1f, 0.0f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, 1f, 0.0f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(-1f, -1f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, -1f, 0.0f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) },

                    new MyVertex() { Position = new Vector3(1f, -1f, 0.0f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, 1f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, 1f, 2f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, 1f, 2f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, -1f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new MyVertex() { Position = new Vector3(1f, -1f, 2f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }
                };

            // Create the vertex buffer.
            this.primaryVertexBuffer = SharpDX.Direct3D11.Buffer.Create<MyVertex>(this.device, BindFlags.VertexBuffer, vertices);

#endif

            // Create the shader constants buffer.
            this.shaderConstantBuffer = SharpDX.Direct3D11.Buffer.Create(this.device, BindFlags.ConstantBuffer, this.shaderConsts.ToBufferData());

            // Allocate the texture list, first texture is reserved.
            this.textureStreams = new DataStream[this.model.header.NumberOfTextures + 1];
            this.textures = new Texture2D[this.model.header.NumberOfTextures + 1];
            this.shaderResources = new ShaderResourceView[this.model.header.NumberOfTextures + 1];

            // Loop and load all of the textures into directx resources.
            for (int i = 0; i < this.textures.Length; i++)
            {
                // First texture is reserved?
                if (i == 0)
                    continue;

                // Decompress and parse the current texture.
                string textureFileName = this.model.textureFileNames[i - 1];
                byte[] decompressedData = this.arcFile.DecompressFileEntry(textureFileName);
                rTexture texture = rTexture.FromGameResource(decompressedData, textureFileName, FileFormats.ResourceType.rTexture, this.arcFile.Endian == IO.Endianness.Big);
                if (texture != null)
                {
                    // TODO: This currently will not work for cube maps.
                    // Create the data stream which will pin the pixel buffer to an IntPtr we can use for sub resource mapping.
                    this.textureStreams[i] = texture.PixelDataStream;

                    // Setup common texture description properties.
                    Texture2DDescription desc = new Texture2DDescription();
                    desc.Width = texture.header.Width;
                    desc.Height = texture.header.Height;
                    desc.MipLevels = texture.header.MipMapCount;
                    desc.Format = rTexture.DXGIFromTextureFormat(texture.header.Format);
                    desc.Usage = ResourceUsage.Default;
                    desc.BindFlags = BindFlags.ShaderResource;
                    desc.SampleDescription.Count = 1;
                    desc.ArraySize = texture.FaceCount;

                    // Create the texture using the description and resource data we setup.
                    this.textures[i] = new Texture2D(this.device, desc);
                    this.device.ImmediateContext.UpdateSubresource(texture.SubResources[0], this.textures[i]);

                    // Create the shader resource that will use this texture.
                    this.shaderResources[i] = new ShaderResourceView(this.device, this.textures[i]);
                }
            }
        }

        private void Render()
        {
            // Only move the camera if the window is visible.
            if (this.Visible == true && this.Focused == true)
            {
                // Update the camera.
                this.camera.move();
            }

            // Clear the backbuffer.
            this.device.ImmediateContext.ClearRenderTargetView(this.renderView, SharpDX.Color.CornflowerBlue);
            this.device.ImmediateContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

            // Set depth stencil and rasterizer states.
            this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilState, 0);
            this.device.ImmediateContext.Rasterizer.State = this.rasterState;

            // Set the shaders.
            this.device.ImmediateContext.VertexShader.Set(this.vertexShader);
            this.device.ImmediateContext.PixelShader.Set(this.pixelShader);

            // Set the shader samplers.
            this.device.ImmediateContext.VertexShader.SetSampler(0, this.shaderSampler);
            this.device.ImmediateContext.PixelShader.SetSampler(0, this.shaderSampler);

            // Set the viewport.
            this.device.ImmediateContext.Rasterizer.SetViewport(0, 0, this.ClientSize.Width, this.ClientSize.Height, 0.0f, 1.0f);

            // Set output target.
            //this.device.ImmediateContext.OutputMerger.SetTargets(this.renderView);
            this.device.ImmediateContext.OutputMerger.SetRenderTargets(this.depthStencilView, this.renderView);

            // Set the vertex and index buffers.
            this.device.ImmediateContext.InputAssembler.InputLayout = this.vertexDecl;
#if REAL_CODE
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, 
                new VertexBufferBinding(this.primaryVertexBuffer, this.model.primitives[0].VertexStride1, 0),
                new VertexBufferBinding(this.secondaryVertexBuffer, this.model.primitives[0].VertexStride2, 0));
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R16_UInt, 0);

            // Set the primitive type.
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            // The pixel shader constants do not change between primtive draw calls, update them now.
            this.shaderConsts.gXfViewProj = this.worldGround * this.camera.ViewMatrix * this.projectionMatrix;
            this.shaderConsts.gXfMatrixMapFactor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
#else
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.primaryVertexBuffer, 28, 0));

            // Set the primitive type.
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            // Update the world matrix based on the camera info.
            this.shaderConsts.WVP = Matrix.Transpose(this.worldGround * this.camera.ViewMatrix * this.projectionMatrix);
            this.shaderConsts.World = Matrix.Transpose(this.worldGround);
#endif

#if REAL_CODE
            // Loop through all of the primitives for the model.
            for (int i = 0; i < this.model.primitives.Length; i++)
            {
                // Check if the primitive is enabled?
                if (this.model.primitives[i].Enabled == 0)
                    continue;

                // Get the material for this primtive.
                Material material = this.model.materials[this.model.primitives[i].MaterialIndex];

                // Set the texture being used by the material.
                this.device.ImmediateContext.PixelShader.SetShaderResources(0, this.shaderResources[material.TextureIndex1]);

                // Update the shader constants buffer with the new data.
                this.device.ImmediateContext.UpdateSubresource(this.shaderConsts.ToBufferData(), this.shaderConstantBuffer);

                // Set the shader constants.
                this.device.ImmediateContext.VertexShader.SetConstantBuffer(0, this.shaderConstantBuffer);
                this.device.ImmediateContext.PixelShader.SetConstantBuffer(0, this.shaderConstantBuffer);

                // Draw the primtive.
                this.device.ImmediateContext.DrawIndexed(this.model.primitives[i].StartingIndexLocation, this.model.primitives[i].IndexCount, 0);
                //this.device.ImmediateContext.DrawIndexed(this.model.primitives[i].IndexCount, this.model.primitives[i].StartingIndexLocation, 0);
            }
#else
            // Update the shader constants buffer with the new data.
            this.device.ImmediateContext.UpdateSubresource(this.shaderConsts.ToBufferData(), this.shaderConstantBuffer);

            // Set the shader constants.
            this.device.ImmediateContext.VertexShader.SetConstantBuffer(0, this.shaderConstantBuffer);

            // Draw the primtive.
            this.device.ImmediateContext.Draw(12, 0);
#endif

            // Present the final frame.
            this.swapChain.Present(0, PresentFlags.None);
        }
    }
}
