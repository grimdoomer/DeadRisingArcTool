using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.UI.Controls;
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
using ImVector2 = System.Numerics.Vector2;

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
        public DepthStencilState HighlightDepthStencilState { get; protected set; }

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

        // Boudning box of the view frustum used for culling.
        protected Vector4 viewFrustumMin;
        protected Vector4 viewFrustumMax;
        public FastBoundingBox ViewFrustumBoundingBox { get; protected set; }

        protected ImGuiRenderer uiRenderer = null;

        // List of resources to render each frame.
        protected DatumIndex[] datumsToRender;
        protected List<RenderableGameResource> resourcesToRender = new List<RenderableGameResource>();

        // Dictionary of file names to loaded GameResource objects.
        protected Dictionary<string, GameResource> loadedResources = new Dictionary<string, GameResource>();

        // File tree for files that we can render in the view.
        private ImGuiResourceSelectTree renderableFilesTree = null;

        protected List<IPickableObject> selectedObjects = new List<IPickableObject>();
        public IPickableObject[] SelectedObjects { get { return this.selectedObjects.ToArray(); } }

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

            // Initialize the view frustum bounding box.
            this.viewFrustumMin = new Vector4(-(this.ViewSize.Width / 2f), 0.0f, this.DrawDistanceMin, 0.0f);
            this.viewFrustumMax = new Vector4(this.ViewSize.Width / 2f, this.ViewSize.Height, this.DrawDistanceMax, 0.0f);
            this.ViewFrustumBoundingBox = new FastBoundingBox(this.viewFrustumMin.ToVector3(), this.viewFrustumMax.ToVector3());

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

            // Setup the depth stencil state for object bleeding.
            stateDesc = new DepthStencilStateDescription();
            stateDesc.IsDepthEnabled = true;
            stateDesc.DepthWriteMask = DepthWriteMask.All;
            stateDesc.DepthComparison = Comparison.LessEqual;
            stateDesc.IsStencilEnabled = false;
            stateDesc.StencilReadMask = 0xFF;
            stateDesc.StencilWriteMask = 0xFF;
            stateDesc.FrontFace.FailOperation = StencilOperation.Keep;
            stateDesc.FrontFace.DepthFailOperation = StencilOperation.Increment;
            stateDesc.FrontFace.PassOperation = StencilOperation.Keep;
            stateDesc.FrontFace.Comparison = Comparison.Less;
            stateDesc.BackFace.FailOperation = StencilOperation.Keep;
            stateDesc.BackFace.DepthFailOperation = StencilOperation.Decrement;
            stateDesc.BackFace.PassOperation = StencilOperation.Keep;
            stateDesc.BackFace.Comparison = Comparison.Less;
            this.HighlightDepthStencilState = new DepthStencilState(this.Device, stateDesc);

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

            // Initialize game data.
            return InitializeGameData();
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

        private bool InitializeGameData()
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

                // Create a renderable resource from the game file and initialize directx resources for it.
                RenderableGameResource renderableResource = new RenderableGameResource(resource);
                if (renderableResource.InitializeGraphics(this) == false)
                {
                    // Failed to initialize resources.
                    // TODO: bubble this up to the user.
                    throw new Exception("Failed to initialize directx resources for game file");
                }

                // If we are rendering a single model try to auto load an animation for it.
                if (i == 0 && this.ViewType == RenderViewType.SingleModel)
                {
                    Archive.Archive arcFile;
                    ArchiveFileEntry fileEntry;

                    // Best effort approach: check if the animation is in the same folder with the same file name.
                    string animationFileName = resource.FileName.Replace("rModel", "rMotionList").Replace("model", "motion");
                    ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(animationFileName, out arcFile, out fileEntry);

                    // If we found the animation set it as the animation for the model.
                    if (fileEntry != null)
                        ((rModel)resource).SetActiveAnimation(arcFile.GetFileAsResource<rMotionList>(fileEntry.FileName));

                    // Set the object properties UI to active by default for single models.
                    renderableResource.UIVisible = true;
                }

                // Add the resource to the collection to render.
                this.loadedResources.Add(resource.FileName, resource);
                this.resourcesToRender.Add(renderableResource);
            }

            // HACK: For now just cast the first resource to an rModel and use it for reference.
            rModel firstModel = (rModel)this.resourcesToRender[0].GameResource;

            // Position the camera and make it look at the model.
            this.Camera.Position = new Vector3(firstModel.primitives[0].BoundingBoxMin.X, firstModel.primitives[0].BoundingBoxMin.Y, firstModel.primitives[0].BoundingBoxMin.Z);
            this.Camera.LookAt = (firstModel.header.BoundingBoxMax - firstModel.header.BoundingBoxMin).ToVector3();
            this.Camera.SpeedModifier = Math.Abs(firstModel.primitives[0].BoundingBoxMin.X / 100000.0f);

            // Now that we have placed the camera reset the view frustum bounding box.
            this.ViewFrustumBoundingBox.Reset(Vector4.Transform(this.viewFrustumMin, this.Camera.ViewMatrix).ToVector3(), Vector4.Transform(this.viewFrustumMax, this.Camera.ViewMatrix).ToVector3());

            // If we are in level viewer mode build the file name tree for renderable files.
            if (this.ViewType == RenderViewType.Level)
            {
                // Build a list of file names that can be rendered.
                Dictionary<string, DatumIndex> fileNames = new Dictionary<string, DatumIndex>(StringComparer.InvariantCultureIgnoreCase);
                for (int i = 0; i < ArchiveCollection.Instance.Archives.Length; i++)
                {
                    // Loop through all of the files in the archive.
                    Archive.Archive archive = ArchiveCollection.Instance.Archives[i];
                    for (int x = 0; x < archive.FileEntries.Length; x++)
                    {
                        // Check the file type and handle accordingly.
                        if (archive.FileEntries[x].FileType == ResourceType.rAreaHitLayout || archive.FileEntries[x].FileType == ResourceType.rItemLayout)
                        {
                            // Add the file to the list.
                            if (fileNames.Keys.Contains(archive.FileEntries[x].FileName) == false)
                            {
                                // Add the file name to the list.
                                fileNames.Add(archive.FileEntries[x].FileName, new DatumIndex(archive.ArchiveId, archive.FileEntries[x].FileId));
                            }
                        }
                        else if (archive.FileEntries[x].FileType == ResourceType.rModel)
                        {
                            // Only add models that are in the scroll folder.
                            if (archive.FileEntries[x].FileName.StartsWith("scroll", StringComparison.InvariantCultureIgnoreCase) == true &&
                                fileNames.ContainsKey(archive.FileEntries[x].FileName) == false)
                            {
                                // Add the scroll model to the list.
                                fileNames.Add(archive.FileEntries[x].FileName, new DatumIndex(archive.ArchiveId, archive.FileEntries[x].FileId));
                            }
                        }
                    }
                }

                // Build the file name tree.
                FileNameTree fileNameTree = FileNameTree.BuildFileNameTree(fileNames);
                this.renderableFilesTree = new ImGuiResourceSelectTree(fileNameTree);
                this.renderableFilesTree.Checkboxes = true;
                this.renderableFilesTree.OnTreeNodeCheckedChanged += new OnTreeNodeCheckedChangedEvent(renderableFilesTree_OnTreeNodeCheckedChanged);
                this.renderableFilesTree.OnTreeNodeDoubleClicked += new OnTreeNodeDoubleClickedEvent(renderableFilesTree_OnTreeNodeDoubleClicked);
            }

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

                        // Update the bounding box for the view frustum.
                        this.ViewFrustumBoundingBox.Reset(Vector4.Transform(this.viewFrustumMin, this.Camera.ViewMatrix).ToVector3(), Vector4.Transform(this.viewFrustumMax, this.Camera.ViewMatrix).ToVector3());

                        // Check if the left mouse button was pressed/released.
                        if (this.InputManager.ButtonReleased(InputAction.LeftClick) == true)
                        {
                            // Check if the mouse moved during the button press or not.
                            if (this.InputManager.MouseDownPosition == this.InputManager.MouseUpPosition)
                            {
                                // If the control button is not being pressed then clear the selected item list.
                                if (this.InputManager.KeyboardState[(int)Keys.ControlKey] == false)
                                    this.selectedObjects.Clear();

                                // Calculate the picking ray.
                                Ray pickingRay = CalculatePickingRay(this.InputManager.MouseUpPosition, new Vector2(this.ViewSize.Width, this.ViewSize.Height));

                                // Loop through all of the resources to be rendered and perform a picking test.
                                for (int i = 0; i < this.resourcesToRender.Count; i++)
                                {
                                    // If this object implements IPickableObject perform the picking test.
                                    IPickableObject pickableObj = this.resourcesToRender[i].GameResource as IPickableObject;
                                    if ((pickableObj?.DoPickingTest(this, pickingRay) ?? false) == true)
                                    {
                                        // TODO: Add in a output float parameter for distance to DoPickingTest.

                                        // Add the object to the selected object list.
                                        this.selectedObjects.Add(pickableObj);
                                    }
                                }
                            }
                        }
                        else if (this.InputManager.ButtonPressed(InputAction.LeftClick) == true)
                        {
                            // If there are any selected items perform a hit test on them to handle interaction.
                            for (int i = 0; i < this.selectedObjects.Count; i++)
                            {

                            }
                        }
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

            // Loop through all of the resources to render and draw each one.
            for (int i = 0; i < this.resourcesToRender.Count; i++)
            {
                // Draw the model.
                this.resourcesToRender[i].DrawFrame(this);
            }

            // TODO: Move this to the ImGuiRenderer class
            // Update the WVP matrix so we are back at the origin for rendering the UI.
            this.ShaderConstants.gXfViewProj = Matrix.Transpose(this.WorldMatrix * this.Camera.ViewMatrix * this.ProjectionMatrix);
            UpdateShaderConstants();

            // Do ImGui rendering for UI.
            this.uiRenderer.DrawFrame(this);

            // Present the final frame.
            this.SwapChain.Present(0, PresentFlags.None);
        }

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

            // Reset the view frustum bounding box.
            this.viewFrustumMin = new Vector4(-(this.ViewSize.Width / 2f), 0.0f, this.DrawDistanceMin, 0.0f);
            this.viewFrustumMax = new Vector4(this.ViewSize.Width / 2f, this.ViewSize.Height, this.DrawDistanceMax, 0.0f);
            this.ViewFrustumBoundingBox.Reset(Vector4.Transform(this.viewFrustumMin, this.Camera.ViewMatrix).ToVector3(), Vector4.Transform(this.viewFrustumMax, this.Camera.ViewMatrix).ToVector3());
        }

        #endregion

        #region UI rendering

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
                // Update the mouse position and buttons.
                io.MousePos = new System.Numerics.Vector2(this.InputManager.MousePosition.X, this.InputManager.MousePosition.Y);
                io.MouseDown[0] = this.InputManager.ButtonState[(int)InputAction.LeftClick];
                io.MouseDown[1] = this.InputManager.ButtonState[(int)InputAction.RightClick];
                io.MouseDown[2] = this.InputManager.ButtonState[(int)InputAction.MiddleMouse];

                // Update mouse wheel position.
                if (this.InputManager.MousePositionDelta[2] != 0)
                {
                    io.MouseWheel += ((float)this.InputManager.MousePositionDelta[2] / 120.0f) / 20.0f;
                }

                // Update keyboard input special keys.
                io.KeyCtrl = this.InputManager.KeyboardState[(int)Keys.ControlKey];
                io.KeyShift = this.InputManager.KeyboardState[(int)Keys.ShiftKey];
                io.KeyAlt = this.InputManager.KeyboardState[(int)Keys.Menu];
                io.KeySuper = false;
            }

            // Draw ImGui layer.
            ImGui.NewFrame();
            {
                ImVector2 nextWindowPos;

                //ImGui.ShowDemoWindow();

                // Create the camera properties window.
                ImGui.Begin("Camera");
                {
                    // Set window size and position.
                    ImGui.SetWindowSize(new ImVector2((this.ViewSize.Width / 4) - 20 - 30, 0), ImGuiCond.Once);
                    ImGui.SetWindowPos(new ImVector2(10, 10), ImGuiCond.Once);

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

                    // Calculate the position of the next window.
                    ImVector2 cameraWndSize = ImGui.GetWindowSize();
                    nextWindowPos = new ImVector2(10, 20 + cameraWndSize.Y);
                }
                ImGui.End();

                // Check if we are rendering a single model or not and handle accordingly.
                if (this.ViewType == RenderViewType.Level)
                {
                    // Draw the resource select UI.
                    DrawResourceSelectUI(nextWindowPos);
                }

                // Loop and draw the UI for any objects with a properties window open.
                for (int i = 0; i < this.resourcesToRender.Count; i++)
                {
                    this.resourcesToRender[i].DrawObjectPropertiesUI(this);
                }
            }

            ImGui.Render();
        }

        private void DrawResourceSelectUI(ImVector2 position)
        {
            // Create the resource select window.
            ImGui.Begin("Objects");
            {
                // Set the window size and position.
                ImGui.SetWindowPos(position, ImGuiCond.Once);
                ImGui.SetWindowSize(new ImVector2((this.ViewSize.Width / 4) - 20 - 30, this.ViewSize.Height - position.Y - 10), ImGuiCond.Once);

                // Draw the resource select tree.
                this.renderableFilesTree.DrawControl();
            }
            ImGui.End();
        }

        void renderableFilesTree_OnTreeNodeCheckedChanged(FileNameTreeNode node)
        {
            // Check if the node has a datum assigned to it.
            if (node.FileDatum.Datum == DatumIndex.Unassigned)
                return;

            // Get the file name for the datum index.
            ArchiveCollection.Instance.GetArchiveFileEntryFromDatum(node.FileDatum, out Archive.Archive archive, out ArchiveFileEntry fileEntry);

            // Check if we are displaying or hiding the resource.
            if (node.Checked == true)
            {
                // Check if we already have the resource file loaded.
                if (this.loadedResources.ContainsKey(fileEntry.FileName) == false)
                {
                    // Load the resource and call graphics init.
                    GameResource resource = ArchiveCollection.Instance.GetFileAsResource<GameResource>(node.FileDatum);
                    if (resource.InitializeGraphics(this) == false)
                    {
                        // TODO: Bubble this up to the user.
                        throw new Exception("Failed to load resource");
                    }

                    // Add it to the list of loaded resources.
                    this.loadedResources.Add(fileEntry.FileName, resource);
                }

                // Add the resource to the list of resources to render.
                this.resourcesToRender.Add(new RenderableGameResource(this.loadedResources[fileEntry.FileName]));
            }
            else
            {
                // Remove the resource by name.
                this.resourcesToRender.RemoveAll(res => res.GameResource.FileName.Equals(fileEntry.FileName, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        void renderableFilesTree_OnTreeNodeDoubleClicked(FileNameTreeNode node)
        {
            // Try to find a renderable resource with this datum index.
            RenderableGameResource resource = this.resourcesToRender.FirstOrDefault(res => res.GameResource.Datum.Datum == node.FileDatum.Datum);
            if (resource != null)
            {
                // Show the object properties UI for the object.
                resource.UIVisible = true;
            }
        }

        #endregion

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

        #region Object Picking

        protected Ray CalculatePickingRay(System.Drawing.Point position, Vector2 bounds)
        {
            Vector3 pickRayDir;
            Vector3 pickRayPos = new Vector3(0.0f);

            // Convert the 2d position into a ray in projection space.
            float x = (((2.0f * (float)position.X) / bounds.X) - 1) / this.ProjectionMatrix.M11;
            float y = -(((2.0f * (float)position.Y) / bounds.Y) - 1) / this.ProjectionMatrix.M22;
            float z = -1.0f;
            pickRayDir = new Vector3(x, y, z);

            // Get the inverse of the view space matrix.
            Matrix invViewMatrix = this.Camera.ViewMatrix;
            invViewMatrix.Invert();

            // Transform the pick ray in projection space to be in world space.
            Ray pickingRay = new Ray(Vector3.TransformCoordinate(pickRayPos, invViewMatrix), Vector3.TransformNormal(pickRayDir, invViewMatrix));
            pickingRay.Direction.Normalize();

            return pickingRay;
        }

        #endregion
    }
}
