using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Forms;
using DeadRisingArcTool.Utilities;
using ImGuiNET;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public enum RenderViewType
    {
        /// <summary>
        /// Render a single model with animations
        /// </summary>
        SingleModel,
        /// <summary>
        /// Render level sections (stages) with item placements
        /// </summary>
        Level
    }

    public class RenderTime
    {
        // Global timing data:
        public long LastTickCount;                  // Tick count from the last frame
        public long CurrentTickCount;               // Tick count for the current frame
        public long InputPollTime;                  // Tick count for the last time input was polled at
        public float TimeDelta;                     // Time that has elapsed since the previous frame
    }

    public class UIState
    {
        public bool DockOptionsWindow = true;       // Dock the options window to the side of the render view

        public bool ShowJointSpheres = false;       // Toggles bounding spheres for joints
        public bool ShowBoundingBoxes = false;      // Toggles bounding boxes for primitives
    }

    [Flags]
    public enum DebugDrawOptions : int
    {
        None = 0,
        DrawJointBoundingSpheres = 1,
        DrawPrimitiveBoundingBox = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShaderConstants
    {
        public const int kSizeOf = (4 * 4 * 4) + (4 * 4) + (4 * 4) + (4 * 4) + (4 * 4) + 4;

        public Matrix gXfViewProj;
        public Vector4 gXfMatrixMapFactor;
        public Vector4 gXfQuantPosScale;
        public Vector4 gXfQuantPosOffset;

        public Vector4 gXfHighlightColor;       // Color used for highlighting objects
        public uint gXfHighlightingEnabled;     // Enables object highlighting
        public uint pad1;
        public uint pad2;
        public uint pad3;
    }

    public class RenderManager
    {
        /// <summary>
        /// Native handle for the UI component to draw to.
        /// </summary>
        public IntPtr OwnerHandle { get; private set; }
        /// <summary>
        /// Size of the image to render.
        /// </summary>
        public Size ViewSize { get; set; }
        /// <summary>
        /// Style of UI to render.
        /// </summary>
        public RenderViewType ViewType { get; private set; }

        /// <summary>
        /// D3D11 rendering device.
        /// </summary>
        public Device Device = null;

        public SwapChain SwapChain = null;
        public Texture2D BackBuffer { get; protected set; }
        public RenderTargetView RenderView { get; protected set; }
        public RasterizerState RasterizerState { get; protected set; }

        public Texture2D DepthStencilTexture { get; protected set; }
        public DepthStencilView DepthStencilView { get; protected set; }
        public DepthStencilState DepthStencilState { get; protected set; }

        private Texture2D checkerboardTexture = null;
        private ShaderResourceView checkerboardTextureResource = null;
        public ShaderResourceView CheckerboardTextureResource { get { return this.checkerboardTextureResource; } }


        /// <summary>
        /// Shader variables used during VS/PS stages.
        /// </summary>
        public ShaderConstants ShaderConstants = new ShaderConstants();
        /// <summary>
        /// Buffer used for sending shader constants to the shader pipeline.
        /// </summary>
        public Buffer ShaderConstantsBuffer { get; protected set; }
        /// <summary>
        /// Collection of shaders to render with.
        /// </summary>
        public ShaderCollection ShaderCollection { get; protected set; }

        /// <summary>
        /// Timing values for rendering and animation playback.
        /// </summary>
        public RenderTime RenderTime { get; set; } = new RenderTime();
        /// <summary>
        /// Input handler for polling mouse/keyboard/controller input.
        /// </summary>
        public InputManager InputManager { get; protected set; }

        /// <summary>
        /// Camera view of the rendered scene.
        /// </summary>
        public Camera Camera { get; set; }
        /// <summary>
        /// Projection matrix for the scene, based on field of view and render view size.
        /// </summary>
        public Matrix ProjectionMatrix { get; protected set; }
        /// <summary>
        ///  World matrix
        /// </summary>
        public Matrix WorldMatrix { get; protected set; } = Matrix.Identity;

        private float fieldOfView = 80.0f;
        /// <summary>
        /// Field of view for the projection matrix.
        /// </summary>
        public float FieldOfView { get { return this.fieldOfView; } set { this.fieldOfView = value; ResizeView(this.ViewSize); } }

        private float drawDistanceMin = 1.0f;
        /// <summary>
        /// Minimum distance to draw at
        /// </summary>
        public float DrawDistanceMin { get { return this.drawDistanceMin; } set { this.drawDistanceMin = value; ResizeView(this.ViewSize); } }

        private float drawDistanceMax = 400000.0f;
        /// <summary>
        /// Maximum distance to draw at
        /// </summary>
        public float DrawDistanceMax { get { return this.drawDistanceMax; } set { this.drawDistanceMax = value; ResizeView(this.ViewSize); } }

        protected ImGuiRenderer uiRenderer = null;

        // List of resources to render.
        protected DatumIndex[] datumsToRender;
        protected List<GameResource> resourcesToRender = new List<GameResource>();

        // Dictionary of file names to loaded IRenderable objects.
        protected Dictionary<string, GameResource> loadedResources = new Dictionary<string, GameResource>();

        public RenderManager(IntPtr formHandle, Size viewSize, RenderViewType viewType, params DatumIndex[] datumsToRender)
        {
            // Initialize fields.
            this.OwnerHandle = formHandle;
            this.ViewSize = viewSize;
            this.ViewType = viewType;
            this.datumsToRender = datumsToRender;
        }

        #region D3D Init

        public bool InitializeGraphics()
        {
            // Setup the swapchain description structure.
            SwapChainDescription desc = new SwapChainDescription();
            desc.BufferCount = 1;
            desc.ModeDescription = new ModeDescription(this.ViewSize.Width, this.ViewSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
            desc.OutputHandle = this.OwnerHandle;
            desc.SampleDescription = new SampleDescription(1, 0);
            desc.SwapEffect = SwapEffect.Discard;
            desc.Usage = Usage.RenderTargetOutput;
            desc.IsWindowed = true;

#if DEBUG
            // Create the device and swapchain.
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, desc, out this.Device, out this.SwapChain);
#else
            // Create the device and swapchain.
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, desc, out this.Device, out this.SwapChain);
#endif

            // Setup the projection matrix.
            this.ProjectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(this.FieldOfView), 
                (float)this.ViewSize.Width / (float)this.ViewSize.Height, this.DrawDistanceMin, this.DrawDistanceMax);

            // Create our output texture for rendering.
            this.BackBuffer = Texture2D.FromSwapChain<Texture2D>(this.SwapChain, 0);
            this.RenderView = new RenderTargetView(this.Device, this.BackBuffer);

            // Create the depth stencil and depth stencil view.
            CreateDepthStencil();

            // Setup the depth stencil state.
            DepthStencilStateDescription stateDesc = new DepthStencilStateDescription();
            stateDesc.IsDepthEnabled = true;
            stateDesc.DepthWriteMask = DepthWriteMask.All;
            stateDesc.DepthComparison = Comparison.LessEqual;
            stateDesc.IsStencilEnabled = false;
            stateDesc.StencilReadMask = 0xFF;
            stateDesc.StencilWriteMask = 0xFF;
            this.DepthStencilState = new DepthStencilState(this.Device, stateDesc);

            //// Setup the depth stencil state for object bleeding.
            //stateDesc = new DepthStencilStateDescription();
            //stateDesc.IsDepthEnabled = true;
            //stateDesc.DepthWriteMask = DepthWriteMask.All;
            //stateDesc.DepthComparison = Comparison.LessEqual;
            //stateDesc.IsStencilEnabled = false;
            //stateDesc.StencilReadMask = 0xFF;
            //stateDesc.StencilWriteMask = 0xFF;
            //stateDesc.FrontFace.FailOperation = StencilOperation.Keep;
            //stateDesc.FrontFace.DepthFailOperation = StencilOperation.Increment;
            //stateDesc.FrontFace.PassOperation = StencilOperation.Keep;
            //stateDesc.FrontFace.Comparison = Comparison.Less;
            //stateDesc.BackFace.FailOperation = StencilOperation.Keep;
            //stateDesc.BackFace.DepthFailOperation = StencilOperation.Decrement;
            //stateDesc.BackFace.PassOperation = StencilOperation.Keep;
            //stateDesc.BackFace.Comparison = Comparison.Less;
            //this.bleedingDepthStencilState = new DepthStencilState(this.Device, stateDesc);

            // Setup the rasterizer state.
            RasterizerStateDescription rasterStateDesc = new RasterizerStateDescription();
            rasterStateDesc.FillMode = FillMode.Solid;
            rasterStateDesc.CullMode = CullMode.Front;
            rasterStateDesc.IsDepthClipEnabled = true;
            this.RasterizerState = new RasterizerState(this.Device, rasterStateDesc);

            // Initialize the input manager.
            this.InputManager = new InputManager(this.OwnerHandle);
            if (this.InputManager.InitializeGraphics(this) == false)
            {
                // Failed to initialize the input manager.
                throw new Exception("Failed to initialize input manager");
            }

            // Initialize the shader collection.
            this.ShaderCollection = new ShaderCollection();
            if (this.ShaderCollection.InitializeGraphics(this) == false)
            {
                // Failed to initialize shaders.
                throw new Exception("Failed to build shader collection");
            }

            // Round buffer size up to the nearest 16 bytes.
            int padding = 16 - (ShaderConstants.kSizeOf % 16);
            if (padding == 16)
                padding = 0;

            // Create the shader constants buffer.
            this.ShaderConstantsBuffer = SharpDX.Direct3D11.Buffer.Create(this.Device, BindFlags.ConstantBuffer, ref this.ShaderConstants, ShaderConstants.kSizeOf + padding);

            // Create the camera.
            this.Camera = new Camera();
            this.Camera.InitializeGraphics(this);

            // Create the ImGui rendering layer.
            this.uiRenderer = new ImGuiRenderer(this.ViewSize.Width, this.ViewSize.Height);
            if (this.uiRenderer.InitializeGraphics(this) == false)
            {
                // Failed to initialize ImGui rendering layer.
                throw new Exception("ImGui failed to initialize");
            }

            // Load textures for UI components.
            LoadTexture(Properties.Resources.CheckerBoard, out this.checkerboardTexture, out this.checkerboardTextureResource);

            // Initialize model data.
            return InitializeModelData();
        }

        private void CreateDepthStencil()
        {
            // Create a texture for the dpeth stencil.
            Texture2DDescription depthStencilDesc = new Texture2DDescription();
            depthStencilDesc.Width = this.ViewSize.Width;
            depthStencilDesc.Height = this.ViewSize.Height;
            depthStencilDesc.MipLevels = 1;
            depthStencilDesc.ArraySize = 1;
            depthStencilDesc.Format = Format.D32_Float;
            depthStencilDesc.SampleDescription.Count = 1;
            depthStencilDesc.SampleDescription.Quality = 0;
            depthStencilDesc.Usage = ResourceUsage.Default;
            depthStencilDesc.BindFlags = BindFlags.DepthStencil;
            this.DepthStencilTexture = new Texture2D(this.Device, depthStencilDesc);

            // Create the depth stencil view.
            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription();
            depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
            depthStencilViewDesc.Format = Format.D32_Float;
            this.DepthStencilView = new DepthStencilView(this.Device, this.DepthStencilTexture, depthStencilViewDesc);
        }

        private bool LoadTexture(byte[] data, out Texture2D texture, out ShaderResourceView resource)
        {
            // Satisfy the compiler.
            texture = null;
            resource = null;

            // Parse the dds image from memory.
            DDSImage ddsImage = DDSImage.FromBuffer(data);
            if (ddsImage == null)
            {
                // Failed to load the texture from file.
                return false;
            }

            // Convert the image to an rTexture file, counter productive but it just makes things easier.
            rTexture imageAsTexture = rTexture.FromDDSImage(ddsImage, "", new DatumIndex(DatumIndex.Unassigned), ResourceType.rTexture, false);
            if (imageAsTexture == null)
            {
                // Failed to convert the texture.
                return false;
            }

            // Create the texture description.
            Texture2DDescription desc = new Texture2DDescription();
            desc.Width = imageAsTexture.Width;
            desc.Height = imageAsTexture.Height;
            desc.MipLevels = imageAsTexture.MipMapCount;
            desc.ArraySize = imageAsTexture.FaceCount;
            desc.Format = rTexture.DXGIFromTextureFormat(imageAsTexture.Format);
            desc.Usage = ResourceUsage.Default;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.SampleDescription.Count = 1;
            texture = new Texture2D(this.Device, desc);

            // Update the texture pixel buffer.
            this.Device.ImmediateContext.UpdateSubresource(imageAsTexture.SubResources[0], texture);

            // Create the shader resource view.
            resource = new ShaderResourceView(this.Device, texture);

            // Successfully loaded the texture.
            return true;
        }

        private bool InitializeModelData()
        {
            // If there are no models to render bail out.
            if (this.datumsToRender.Length == 0)
                return true;

            // Loop through all of the datums to render and setup each one for rendering.
            for (int i = 0; i < this.datumsToRender.Length; i++)
            {
                // Create the game resource from the datum.
                GameResource resource = ArchiveCollection.Instance.GetFileAsResource<GameResource>(this.datumsToRender[i]);
                if (resource == null)
                {
                    // Failed to load the required resource.
                    // TODO: Bubble this up to the user.
                    throw new NotImplementedException();
                }

                // Let the object initialize and required directx resources.
                if (resource.InitializeGraphics(this) == false)
                {
                    // Failed to initialize graphics for resource.
                    // TODO: Bubble this up to the user.
                    throw new NotImplementedException();
                }

                // If we are rendering a single model try to auto load an animation for it.
                if (i == 0 && this.ViewType == RenderViewType.SingleModel)
                {
                    Archive.Archive arcFile;
                    ArchiveFileEntry fileEntry;

                    ArchiveCollection.Instance.GetArchiveFileEntryFromDatum(this.datumsToRender[0], out arcFile, out fileEntry);

                    string animationFileName = fileEntry.FileName.Replace("rModel", "rMotionList").Replace("model", "motion");
                    //animationFileName = "motion\\em\\em49\\em4900\\em4900.rMotionList";
                    ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(animationFileName, out arcFile, out fileEntry);

                    if (fileEntry != null)
                        ((rModel)resource).SetActiveAnimation(arcFile.GetFileAsResource<rMotionList>(fileEntry.FileName));
                }

                // Add the resource to the collection to render.
                this.resourcesToRender.Add(resource);
            }

            // HACK: For now just cast the first resource to an rModel and use it for reference.
            rModel firstModel = (rModel)this.resourcesToRender[0];

            // Position the camera and make it look at the model.
            this.Camera.Position = new Vector3(firstModel.primitives[0].BoundingBoxMin.X, firstModel.primitives[0].BoundingBoxMin.Y, firstModel.primitives[0].BoundingBoxMin.Z);
            this.Camera.LookAt = (firstModel.header.BoundingBoxMax - firstModel.header.BoundingBoxMin).ToVector3();
            this.Camera.SpeedModifier = Math.Abs(firstModel.primitives[0].BoundingBoxMin.X / 100000.0f);

            // Successfully initialized model data.
            return true;
        }

        #endregion

        #region Rendering

        public void DrawFrame(bool isFocused)
        {
            // Update the time from the previous frame to the current frame.
            this.RenderTime.LastTickCount = this.RenderTime.CurrentTickCount;
            this.RenderTime.CurrentTickCount = DateTime.Now.Ticks;
            this.RenderTime.TimeDelta = (float)(this.RenderTime.CurrentTickCount - this.RenderTime.LastTickCount) / (float)TimeSpan.TicksPerSecond;

            // Render ImGui before polling for input so we can block out updating the view camera.
            RenderGuiLayer(isFocused);

            // Cap input polling to 30 times per second.
            if ((this.RenderTime.CurrentTickCount - this.RenderTime.InputPollTime) > (TimeSpan.TicksPerSecond / 30))
            {
                // Only move the camera if the window is visible.
                if (isFocused == true)
                {
                    // Update input.
                    this.RenderTime.InputPollTime = DateTime.Now.Ticks;
                    this.InputManager.DrawFrame(this);

                    // If ImGui has focus, don't adjust the camera.
                    if (ImGui.GetIO().WantCaptureMouse == false)
                    {
                        // Update the camera.
                        this.Camera.DrawFrame(this);
                    }
                }
            }

            // Set our render target to our swapchain buffer.
            this.Device.ImmediateContext.OutputMerger.SetRenderTargets(this.DepthStencilView, this.RenderView);

            // Set the viewport.
            this.Device.ImmediateContext.Rasterizer.SetViewport(0, 0, this.ViewSize.Width, this.ViewSize.Height, 0.0f, 1.0f);

            // Clear the backbuffer.
            this.Device.ImmediateContext.ClearRenderTargetView(this.RenderView, SharpDX.Color.CornflowerBlue);
            this.Device.ImmediateContext.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

            // Set depth stencil and rasterizer states.
            this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(this.DepthStencilState, 0);
            this.Device.ImmediateContext.Rasterizer.State = this.RasterizerState;

            // The pixel shader constants do not change between primtive draw calls, update them now.
            this.ShaderConstants.gXfViewProj = Matrix.Transpose(this.WorldMatrix * this.Camera.ViewMatrix * this.ProjectionMatrix);

            // Loop through all of the resources to render and draw each one.
            for (int i = 0; i < this.resourcesToRender.Count; i++)
            {
                // Draw the model.
                this.resourcesToRender[i].DrawFrame(this);
            }

            // Do ImGui rendering for UI.
            this.uiRenderer.DrawFrame(this);

            // Present the final frame.
            this.SwapChain.Present(0, PresentFlags.None);
        }

        private void RenderGuiLayer(bool isFocused)
        {
            // Get the IO structure.
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new System.Numerics.Vector2(this.ViewSize.Width / 1.0f, this.ViewSize.Height / 1.0f);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1.0f, 1.0f);
            io.DeltaTime = this.RenderTime.TimeDelta;

            // Only update input if the window is in focus.
            if (isFocused == true)
            {
                // Update the mouse position.
                io.MousePos = new System.Numerics.Vector2(this.InputManager.MousePosition.X, this.InputManager.MousePosition.Y);
            }

            // Draw ImGui layer.
            ImGui.NewFrame();
            {
                // Create the camera properties window.
                ImGui.Begin("Camera");
                {
                    // Set window size and position.
                    System.Numerics.Vector2 optionsSize = new System.Numerics.Vector2(470.0f, 180.0f);
                    ImGui.SetWindowSize(optionsSize, ImGuiCond.Appearing);
                    ImGui.SetWindowPos(new System.Numerics.Vector2(10, 10), ImGuiCond.Appearing);

                    // Position:
                    Vector3 camPosition = this.Camera.Position;
                    if (ImGui.InputFloat3("Position", ref camPosition) == true)
                        this.Camera.Position = camPosition;

                    // Angle:
                    Vector2 camRotation = this.Camera.Rotation;
                    if (ImGui.InputFloat2("Rotation", ref camRotation) == true)
                        this.Camera.Rotation = camRotation;

                    // Speed:
                    float camSpeed = this.Camera.Speed;
                    if (ImGui.InputFloat("Movement Speed", ref camSpeed) == true)
                        this.Camera.Speed = camSpeed;

                    // Field of view:
                    if (ImGui.InputFloat("Field of view", ref this.fieldOfView, 1.0f) == true)
                        this.ResizeView(this.ViewSize);

                    // Draw distance min:
                    if (ImGui.InputFloat("Draw distance min", ref this.drawDistanceMin) == true)
                        this.ResizeView(this.ViewSize);

                    // Draw distance max:
                    if (ImGui.InputFloat("Draw distance max", ref this.drawDistanceMax) == true)
                        this.ResizeView(this.ViewSize);
                }
                ImGui.End();

                // Check if we are rendering a single model or not and handle accordingly.
                if (this.ViewType == RenderViewType.SingleModel)
                {
                    // Joints.
                    ((rModel)resourcesToRender[0]).DrawUI(this);
                }
            }

            ImGui.Render();
        }

        #endregion

        public void ResizeView(Size newSize)
        {
            // Update the view size.
            this.ViewSize = newSize;

            // Clear the current device state.
            this.Device.ImmediateContext.ClearState();

            // Release all references to the swap chain buffers.
            this.Device.ImmediateContext.OutputMerger.ResetTargets();
            this.BackBuffer.Dispose();
            this.RenderView.Dispose();

            // Dispose of the old depth stencil texture and create a new one.
            this.DepthStencilTexture.Dispose();
            this.DepthStencilView.Dispose();
            CreateDepthStencil();

            // Resize the swap chain.
            this.SwapChain.ResizeBuffers(1, this.ViewSize.Width, this.ViewSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
            this.BackBuffer = this.SwapChain.GetBackBuffer<Texture2D>(0);

            // Create a new render target view using the back buffer.
            this.RenderView = new RenderTargetView(this.Device, this.BackBuffer);

            // Update the projection matrix.
            this.ProjectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(this.FieldOfView), 
                (float)this.ViewSize.Width / (float)this.ViewSize.Height, this.DrawDistanceMin, this.DrawDistanceMax);
        }

        #region Manager Functions

        /// <summary>
        /// Gets the game resource with the specified file name if it exists or null otherwise
        /// </summary>
        /// <param name="fileName">File name of the resource to get</param>
        /// <returns></returns>
        public GameResource GetResourceFromFileName(string fileName)
        {
            // Check to see if we have already loaded this game resource for rendering.
            if (this.loadedResources.ContainsKey(fileName) == true)
            {
                // The game resource has already been loaded.
                return this.loadedResources[fileName];
            }

            // Find the arc file the resource is in.
            ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(fileName, out Archive.Archive arcFile, out ArchiveFileEntry fileEntry);
            if (arcFile == null || fileEntry == null)
            {
                // Failed to find a resource with the specified name.
                return null;
            }

            // Parse the game resource and create a new GameResource object we can render.
            GameResource resource = arcFile.GetFileAsResource<GameResource>(fileName);
            if (resource == null)
            {
                // Failed to find and load a resource with the name specified.
                return null;
            }

            // Initialize the resource in case the calling resource needs data from it.
            if (resource.InitializeGraphics(this) == false)
            {
                // Failed to initialize the resource for rendering.
                return null;
            }

            // Add the game resource to the loaded resources collection and return.
            this.loadedResources.Add(fileName, resource);
            return resource;
        }

        public void UpdateShaderConstants()
        {
            // Update the shader constants buffer with the new data.
            this.Device.ImmediateContext.UpdateSubresource(ref this.ShaderConstants, this.ShaderConstantsBuffer);

            // Set the shader constants.
            this.Device.ImmediateContext.VertexShader.SetConstantBuffer(0, this.ShaderConstantsBuffer);
            this.Device.ImmediateContext.PixelShader.SetConstantBuffer(0, this.ShaderConstantsBuffer);
        }

        #endregion
    }
}
