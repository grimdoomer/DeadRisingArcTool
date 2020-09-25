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
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Utilities;
using System.Threading;
using ImGuiNET;

namespace DeadRisingArcTool.Forms
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

    [StructLayout(LayoutKind.Sequential)]
    public struct ShaderConstants
    {
        public const int kSizeOf = (4 * 4 * 4) + (4 * 4) + (4 * 4) + (4 * 4) + (4 * 4) + 4;
        //public const int kElementCount = (4 * 4) + 4 + 4 + 4;

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

    public class RenderTime
    {
        // Global timing data:
        public long LastTickCount;                  // Tick count from the last frame
        public long CurrentTickCount;               // Tick count for the current frame
        public float TimeDelta;                     // Time that has elapsed since the previous frame

        // Animation timing data:
        public int SelectedAnimation;               // Index of the animation that is currently playing
        public float AnimationFrameRate;            // Playback rate of the animation i.e.: 10fps
        public float AnimationTimePerFrame;         // Time per frame of the animation
        public float AnimationTotalTime;            // Total time the animation will take to complete
        public float AnimationCurrentTime;          // Time of the current position in animation playback
        public float AnimationFrameCount;           // Number of frames in the animation
        public float AnimationCurrentFrame;         // Frame number of the current position in animation playback
    }

    public class UIState
    {
        public bool DockOptionsWindow = true;       // Dock the options window to the side of the render view

        public bool ShowJointSpheres = false;       // Toggles bounding spheres for joints
        public bool ShowBoundingBoxes = false;      // Toggles bounding boxes for primitives
    }

    public partial class RenderView : Form, IRenderManager
    {
        // UI style for the render view.
        RenderViewType viewType;

        // List of game resources to render.
        List<DatumIndex> renderDatums = new List<DatumIndex>();
        List<GameResource> resourcesToRender = new List<GameResource>();

        // Dictionary of file names to loaded IRenderable
        Dictionary<string, GameResource> loadedResources = new Dictionary<string, GameResource>();

        rMotionList modelAnimation = null;

        // DirectX interfaces for rendering.
        SharpDX.Direct3D11.Device device = null;
        SwapChain swapChain = null;
        Texture2D backBuffer = null;
        Texture2D depthStencil = null;
        RenderTargetView renderView = null;
        DepthStencilView depthStencilView = null;
        DepthStencilState depthStencilState = null;
        RasterizerState rasterState = null;

        // Depth stencil for object bleeding.
        DepthStencilState bleedingDepthStencilState = null;

        // Camera helper.
        Camera camera = null;
        Matrix projectionMatrix;
        Matrix worldGround;

        // FPS counter data.
        long startTime;
        int framesPerSecond = 0;
        long lastInputPollTime = 0;

        // Input manager.
        InputManager inputManager = null;

        // Built in shader collection.
        BuiltInShaderCollection shaderCollection = null;

        // Debug draw options.
        DebugDrawOptions debugDrawOptions = DebugDrawOptions.None;

        // Shader constant buffer.
        ShaderConstants shaderConsts = new ShaderConstants();
        SharpDX.Direct3D11.Buffer shaderConstantBuffer = null;

        // Timing values used for animating meshes.
        RenderTime renderTime = new RenderTime();

        bool isClosing = false;         // Indicates if we should quit the render loop
        bool hasResized = false;        // Indicates if the form has changed size

        // ImGui rendering interface.
        ImGuiRenderer imGui = null;
        UIState uiState = new UIState();

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref System.Drawing.Point p);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr handle, ref System.Drawing.Point p);

        public RenderView(RenderViewType viewType, params DatumIndex[] renderDatums)
        {
            // Initialize fields.
            this.viewType = viewType;
            this.renderDatums.AddRange(renderDatums);

            InitializeComponent();

            // If the render style is level geometry then auto-maximize the window.
            if (this.viewType == RenderViewType.Level)
                this.WindowState = FormWindowState.Maximized;
        }

        private void RenderView_Load(object sender, EventArgs e)
        {
            // Show the form and gain focus.
            this.Show();
            this.Focus();

            // Initialize D3D layer.
            InitializeD3D();

            // Create the rendering buffers from the model data.
            InitializeModelData();

            // Initialize time for the previous frame.
            this.renderTime.CurrentTickCount = DateTime.Now.Ticks;

            // Show the form and enter the render loop.
            this.startTime = DateTime.Now.Ticks;
            while (this.isClosing == false && this.IsDisposed == false)
            {
                // Render the next frame and process the windows message queue.
                Render();
                Application.DoEvents();
            }

            // Cleanup shader resources.
            this.shaderCollection.CleanupGraphics(this, device);
        }

        private void RenderView_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            // Flag that we are closing so the render loop can kill itself.
            this.isClosing = true;
        }

        private void RenderView_SizeChanged(object sender, System.EventArgs e)
        {
            // Make sure the form has a valid size.
            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
                return;

            // Flag that the form has resized so the directx thread can adjust it's frame buffer size.
            this.hasResized = true;
        }

        #region IRenderManager

        public GameResource GetResourceFromFileName(string fileName)
        {
            // Check to see if we have already loaded this game resource for rendering.
            if (this.loadedResources.ContainsKey(fileName) == true)
            {
                // The game resource has already been loaded.
                return this.loadedResources[fileName];
            }

            // Find the arc file the resource is in.
            ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(fileName, out Archive arcFile, out ArchiveFileEntry fileEntry);
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
            if (resource.InitializeGraphics(this, this.device) == false)
            {
                // Failed to initialize the resource for rendering.
                return null;
            }

            // Add the game resource to the loaded resources collection and return.
            this.loadedResources.Add(fileName, resource);
            return resource;
        }

        public BuiltInShader GetBuiltInShader(BuiltInShaderType type)
        {
            // Get the shader from the shader collection.
            return this.shaderCollection.GetShader(type);
        }

        public DebugDrawOptions GetDebugDrawOptions()
        {
            // Get the debug draw options.
            return this.debugDrawOptions;
        }

        public void SetMatrixMapFactor(Vector4 vec)
        {
            this.shaderConsts.gXfMatrixMapFactor = vec;

            // Update the shader constants buffer with the new data.
            this.device.ImmediateContext.UpdateSubresource(ref this.shaderConsts, this.shaderConstantBuffer);
        }

        public rMotionList GetMotionList()
        {
            return this.modelAnimation;
        }

        public RenderTime GetTime()
        {
            return this.renderTime;
        }

        public InputManager GetInputManager()
        {
            return this.inputManager;
        }

        public Size GetWindowSize()
        {
            return this.ClientSize;
        }

        public void SetDepthStencil(int index)
        {
            switch (index)
            {
                case 0: this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilState, 0); break;
                case 1: this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.bleedingDepthStencilState, 0); break;
            }
        }

        public ref ShaderConstants GetShaderConstants()
        {
            return ref this.shaderConsts;
        }

        public void UpdateShaderConstants()
        {
            // Update the shader constants buffer with the new data.
            this.device.ImmediateContext.UpdateSubresource(ref this.shaderConsts, this.shaderConstantBuffer);

            // Set the shader constants.
            this.device.ImmediateContext.VertexShader.SetConstantBuffer(0, this.shaderConstantBuffer);
            this.device.ImmediateContext.PixelShader.SetConstantBuffer(0, this.shaderConstantBuffer);
        }

        #endregion

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

#if DEBUG
            // Create the device and swapchain.
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, desc, out this.device, out this.swapChain);
#else
            // Create the device and swapchain.
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, desc, out this.device, out this.swapChain);
#endif

            // Setup the projection matrix.
            this.projectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(95.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 1.0f, 400000.0f);
            this.worldGround = Matrix.Identity;

            // Create our output texture for rendering.
            this.backBuffer = Texture2D.FromSwapChain<Texture2D>(this.swapChain, 0);
            this.renderView = new RenderTargetView(this.device, this.backBuffer);

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
            this.depthStencilState = new DepthStencilState(this.device, stateDesc);

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
            this.bleedingDepthStencilState = new DepthStencilState(this.device, stateDesc);

            // Setup the rasterizer state.
            RasterizerStateDescription rasterStateDesc = new RasterizerStateDescription();
            rasterStateDesc.FillMode = FillMode.Solid;
            rasterStateDesc.CullMode = CullMode.Front;
            rasterStateDesc.IsDepthClipEnabled = true;
            this.rasterState = new RasterizerState(this.device, rasterStateDesc);

            // Initialize the input manager.
            this.inputManager = new InputManager(this.Handle);
            this.inputManager.InitializeGraphics(this, this.device);

            // Initialize the shader collection.
            this.shaderCollection = new BuiltInShaderCollection();
            if (this.shaderCollection.InitializeGraphics(this, this.device) == false)
            {
                // Failed to initialize shaders.
            }

            // Round buffer size up to the nearest 16 bytes.
            int padding = 16 - (ShaderConstants.kSizeOf % 16);
            if (padding == 16)
                padding = 0;

            // Create the shader constants buffer.
            this.shaderConstantBuffer = SharpDX.Direct3D11.Buffer.Create(this.device, BindFlags.ConstantBuffer, ref this.shaderConsts, ShaderConstants.kSizeOf + padding);

            // Create the camera.
            this.camera = new Camera();
            this.camera.InitializeGraphics(this, this.device);

            // Create the ImGui rendering layer.
            this.imGui = new ImGuiRenderer(this.ClientSize.Width, this.ClientSize.Height);
            if (this.imGui.InitializeGraphics(this, this.device) == false)
            {
                // Failed to initialize ImGui rendering layer.
                throw new Exception("ImGui failed to initialize");
            }
        }

        private void InitializeModelData()
        {
            // If we are rendering a single model try to auto load an animation for it.
            if (this.viewType == RenderViewType.SingleModel)
            {
                Archive arcFile;
                ArchiveFileEntry fileEntry;

                ArchiveCollection.Instance.GetArchiveFileEntryFromDatum(this.renderDatums[0], out arcFile, out fileEntry);

                string animationFileName = fileEntry.FileName.Replace("rModel", "rMotionList").Replace("model", "motion");
                //animationFileName = "motion\\em\\em49\\em4900\\em4900.rMotionList";
                ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(animationFileName, out arcFile, out fileEntry);

                if (fileEntry != null)
                    this.modelAnimation = arcFile.GetFileAsResource<rMotionList>(fileEntry.FileName);
            }

            // Loop through all of the datums to render and setup each one for rendering.
            for (int i = 0; i < this.renderDatums.Count; i++)
            {
                // Create the game resource from the datum.
                GameResource resource = ArchiveCollection.Instance.GetFileAsResource<GameResource>(this.renderDatums[i]);
                if (resource == null)
                {
                    // Failed to load the required resource.
                    // TODO: Bubble this up to the user.
                    throw new NotImplementedException();
                }

                // Let the object initialize and required directx resources.
                if (resource.InitializeGraphics(this, this.device) == false)
                {
                    // Failed to initialize graphics for resource.
                    // TODO: Bubble this up to the user.
                    throw new NotImplementedException();
                }

                // Add the resource to the collection to render.
                this.resourcesToRender.Add(resource);
            }

            // HACK: For now just cast the first resource to an rModel and use it for reference.
            rModel firstModel = (rModel)this.resourcesToRender[0];

            // Position the camera and make it look at the model.
            this.camera.Position = new Vector3(firstModel.primitives[0].BoundingBoxMin.X, firstModel.primitives[0].BoundingBoxMin.Y, firstModel.primitives[0].BoundingBoxMin.Z);
            this.camera.LookAt = (firstModel.header.BoundingBoxMax - firstModel.header.BoundingBoxMin).ToVector3();
            //this.camera.Speed = Math.Abs(firstModel.primitives[0].BoundingBoxMin.X / 1000.0f);
            this.camera.SpeedModifier = Math.Abs(firstModel.primitives[0].BoundingBoxMin.X / 100000.0f);
        }

        private void Render()
        {
            // If the form is not visible do not render anything.
            if (this.Visible == false)
                return;

            // Update the time from the previous frame to the current frame.
            this.renderTime.LastTickCount = this.renderTime.CurrentTickCount;
            this.renderTime.CurrentTickCount = DateTime.Now.Ticks;
            this.renderTime.TimeDelta = (float)(this.renderTime.CurrentTickCount - this.renderTime.LastTickCount) / (float)TimeSpan.TicksPerSecond;

            // Check if we need to reset the fps counter.
            if (this.renderTime.CurrentTickCount >= this.startTime + TimeSpan.TicksPerSecond)
            {
                // Update the fps counter.
                this.Text = string.Format("RenderView: {0} fps", this.framesPerSecond);

                // Reset the fps counter.
                this.framesPerSecond = 0;
                this.startTime = this.renderTime.CurrentTickCount;
            }

            // Increment the frame counter.
            this.framesPerSecond++;

            // Check if the form has resized and if so reset our render state to accomidate the size change.
            if (this.hasResized == true)
                ResizeRenderTarget();

            // Render ImGui before polling for input so we can block out updating the view camera.
            RenderGuiLayer();

            // Cap input polling to 30 times per second.
            if ((this.renderTime.CurrentTickCount - this.lastInputPollTime) > (TimeSpan.TicksPerSecond / 30))
            {
                // Only move the camera if the window is visible.
                if (this.Visible == true && this.Focused == true)
                {
                    // Update input.
                    this.lastInputPollTime = DateTime.Now.Ticks;
                    this.inputManager.DrawFrame(this, this.device);

                    // Check if we need to update debug draw options.
                    if (this.inputManager.ButtonPressed(InputAction.MiscAction1) == true)
                    {
                        // Toggle joint bounding spheres.
                        if (this.debugDrawOptions.HasFlag(DebugDrawOptions.DrawJointBoundingSpheres) == true)
                            this.debugDrawOptions &= ~DebugDrawOptions.DrawJointBoundingSpheres;
                        else
                            this.debugDrawOptions |= DebugDrawOptions.DrawJointBoundingSpheres;
                    }
                    if (this.inputManager.ButtonPressed(InputAction.MiscAction2) == true)
                    {
                        // Toggle primitive bounding boxes.
                        if (this.debugDrawOptions.HasFlag(DebugDrawOptions.DrawPrimitiveBoundingBox) == true)
                            this.debugDrawOptions &= ~DebugDrawOptions.DrawPrimitiveBoundingBox;
                        else
                            this.debugDrawOptions |= DebugDrawOptions.DrawPrimitiveBoundingBox;
                    }

                    // If ImGui has focus, don't adjust the camera.
                    if (ImGui.GetIO().WantCaptureMouse == false)
                    {
                        // Update the camera.
                        this.camera.DrawFrame(this, this.device);
                    }
                }
            }

            // Set our render target to our swapchain buffer.
            this.device.ImmediateContext.OutputMerger.SetRenderTargets(this.depthStencilView, this.renderView);

            // Set the viewport.
            this.device.ImmediateContext.Rasterizer.SetViewport(0, 0, this.ClientSize.Width, this.ClientSize.Height, 0.0f, 1.0f);

            // Clear the backbuffer.
            this.device.ImmediateContext.ClearRenderTargetView(this.renderView, SharpDX.Color.CornflowerBlue);
            this.device.ImmediateContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

            // Set depth stencil and rasterizer states.
            this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilState, 0);
            this.device.ImmediateContext.Rasterizer.State = this.rasterState;

            // The pixel shader constants do not change between primtive draw calls, update them now.
            this.shaderConsts.gXfViewProj = Matrix.Transpose(this.worldGround * this.camera.ViewMatrix * this.projectionMatrix);

            // Loop through all of the resources to render and draw each one.
            for (int i = 0; i < this.resourcesToRender.Count; i++)
            {
                // Draw the model.
                this.resourcesToRender[i].DrawFrame(this, this.device);
            }

            // Do ImGui rendering for UI.
            this.imGui.DrawFrame(this, this.device);

            // Present the final frame.
            this.swapChain.Present(0, PresentFlags.None);
        }

        private void RenderGuiLayer()
        {
            // Get the IO structure.
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new System.Numerics.Vector2(this.ClientSize.Width / 1.0f, this.ClientSize.Height / 1.0f);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1.0f, 1.0f);
            io.DeltaTime = this.renderTime.TimeDelta;

            // Only update input if the window is in focus.
            if (this.Focused == true)
            {
                // Update cursor position.
                System.Drawing.Point cursorPos = new System.Drawing.Point();
                GetCursorPos(ref cursorPos);
                ScreenToClient(this.Handle, ref cursorPos);

                io.MousePos = new System.Numerics.Vector2(cursorPos.X, cursorPos.Y);
            }

            // Draw ImGui layer.
            ImGui.NewFrame();
            {
                //ImGui.ShowDemoWindow();

                // Create the main window.
                ImGui.Begin("Options");
                {
                    // Allow the options window to dock to the side of the render view.
                    ImGui.Checkbox("Dock window", ref this.uiState.DockOptionsWindow);
                    if (this.uiState.DockOptionsWindow == true)
                    {
                        // Set window size and position.
                        System.Numerics.Vector2 optionsSize = new System.Numerics.Vector2(350, this.ClientSize.Height - 20);
                        ImGui.SetWindowSize(optionsSize);
                        ImGui.SetWindowPos(new System.Numerics.Vector2(this.ClientSize.Width - optionsSize.X - 10, 10));
                    }
                    else
                    {
                        // Set window size and position.
                        System.Numerics.Vector2 optionsSize = new System.Numerics.Vector2(350, this.ClientSize.Height - 20);
                        ImGui.SetWindowSize(optionsSize, ImGuiCond.Once | ImGuiCond.Appearing);
                        ImGui.SetWindowPos(new System.Numerics.Vector2(this.ClientSize.Width - optionsSize.X - 10, 10), ImGuiCond.Once | ImGuiCond.Appearing);
                    }

                    // Camera options section.
                    if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen) == true)
                    {
                        System.Numerics.Vector3 camPosition = new System.Numerics.Vector3(this.camera.Position.X, this.camera.Position.Y, this.camera.Position.Z);
                        System.Numerics.Vector3 camAngle = new System.Numerics.Vector3(this.camera.LookAt.X, this.camera.LookAt.Y, this.camera.LookAt.Z);

                        // Position and angle.
                        ImGui.InputFloat3("Position", ref camPosition);
                        ImGui.InputFloat3("Angle", ref camAngle);
                    }

                    // Check if we are rendering a single model or not and handle accordingly.
                    if (this.viewType == RenderViewType.SingleModel)
                    {
                        // Create a section for the model info.
                        ImGui.Separator();
                        ImGui.Text(this.resourcesToRender[0].FileName);

                        if (ImGui.Checkbox("Show bounding spheres", ref this.uiState.ShowJointSpheres) == true)
                        {
                            // Toggle joint bounding spheres.
                            if (this.debugDrawOptions.HasFlag(DebugDrawOptions.DrawJointBoundingSpheres) == true)
                                this.debugDrawOptions &= ~DebugDrawOptions.DrawJointBoundingSpheres;
                            else
                                this.debugDrawOptions |= DebugDrawOptions.DrawJointBoundingSpheres;
                        }
                        if (ImGui.Checkbox("Show bounding boxes", ref this.uiState.ShowBoundingBoxes) == true)
                        {
                            // Toggle primitive bounding boxes.
                            if (this.debugDrawOptions.HasFlag(DebugDrawOptions.DrawPrimitiveBoundingBox) == true)
                                this.debugDrawOptions &= ~DebugDrawOptions.DrawPrimitiveBoundingBox;
                            else
                                this.debugDrawOptions |= DebugDrawOptions.DrawPrimitiveBoundingBox;
                        }

                        // Joints.
                        ((rModel)resourcesToRender[0]).CreateJointsImGui(this);
                    }
                }
                ImGui.End();
            }

            ImGui.Render();
            //this.imGui.DrawFrame(this, this.device);
        }

        private void CreateDepthStencil()
        {
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
        }

        private void ResizeRenderTarget()
        {
            // Clear the current device state.
            this.device.ImmediateContext.ClearState();

            // Release all references to the swap chain buffers.
            this.device.ImmediateContext.OutputMerger.ResetTargets();
            this.backBuffer.Dispose();
            this.renderView.Dispose();

            // Dispose of the old depth stencil texture and create a new one.
            this.depthStencil.Dispose();
            this.depthStencilView.Dispose();
            CreateDepthStencil();

            // Resize the swap chain.
            this.swapChain.ResizeBuffers(1, this.ClientSize.Width, this.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
            this.backBuffer = this.swapChain.GetBackBuffer<Texture2D>(0);

            // Create a new render target view using the back buffer.
            this.renderView = new RenderTargetView(this.device, this.backBuffer);

            // Update the projection matrix.
            this.projectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(95.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 1.0f, 400000.0f);
        }
    }
}
