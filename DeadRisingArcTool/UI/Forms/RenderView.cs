using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;

namespace DeadRisingArcTool.Forms
{
    public partial class RenderView : Form
    {
        // Rendering interface.
        RenderManager renderer = null;

        // FPS counter data.
        long startTime;
        int framesPerSecond = 0;
        long lastInputPollTime = 0;

        // Flags the renderer to stop rendering when the window is closed.
        private bool isClosing = false;

        // Flags the form has resized and the renderer should adjust the viewport size.
        private bool hasResized = true;

        public RenderView(RenderViewType viewType, params DatumIndex[] renderDatums)
        {
            InitializeComponent();

            // Initialize the renderer.
            this.renderer = new RenderManager(this.Handle, this.ClientSize, viewType, renderDatums);

            // If the render style is level geometry then auto-maximize the window.
            if (viewType == RenderViewType.Level)
                this.WindowState = FormWindowState.Maximized;
        }

        private void RenderView_Load(object sender, EventArgs e)
        {
            // Show the form and gain focus.
            this.Show();
            this.Focus();

            // Initialize the rendering layer.
            if (this.renderer.InitializeGraphics() == false)
            {
                // Failed to initialize the renderer.
                throw new Exception("Failed to initialize rendering layer");
            }

            // Initialize time for the previous frame.
            this.renderer.RenderTime.CurrentTickCount = DateTime.Now.Ticks;

            // Show the form and enter the render loop.
            this.startTime = DateTime.Now.Ticks;
            while (this.isClosing == false && this.IsDisposed == false)
            {
                // If the form has resized then change the viewport size.
                if (this.hasResized == true)
                {
                    // Resize the viewport.
                    this.renderer.ResizeView(this.ClientSize);
                    this.hasResized = false;
                }

                // Only render a frame if the window is visible.
                if (this.Visible == true)
                {
                    // Draw a new frame, only poll for input if the window is focused.
                    this.renderer.DrawFrame(this.Focused);

                    // Check if we need to reset the fps counter.
                    if (this.renderer.RenderTime.CurrentTickCount >= this.startTime + TimeSpan.TicksPerSecond)
                    {
                        // Update the fps counter.
                        this.Text = string.Format("RenderView: {0} fps", this.framesPerSecond);

                        // Reset the fps counter.
                        this.framesPerSecond = 0;
                        this.startTime = this.renderer.RenderTime.CurrentTickCount;
                    }

                    // Increment the frame counter.
                    this.framesPerSecond++;
                }

                // Process the window message queue.
                Application.DoEvents();
            }

            // TODO: Cleanup resources.
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

        //public void SetDepthStencil(int index)
        //{
        //    switch (index)
        //    {
        //        case 0: this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilState, 0); break;
        //        case 1: this.device.ImmediateContext.OutputMerger.SetDepthStencilState(this.bleedingDepthStencilState, 0); break;
        //    }
        //}
    }
}
