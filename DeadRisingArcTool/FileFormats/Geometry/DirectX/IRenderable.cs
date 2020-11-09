using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public interface IRenderable
    {
        /// <summary>
        /// Called during DirectX initialization, create any resources needed to render this object
        /// </summary>
        /// <param name="manager">Rendering context</param>
        /// <returns>True if initialization was successful, false otherwise</returns>
        bool InitializeGraphics(RenderManager manager);

        /// <summary>
        /// Called during the render loop, draws the object to screen
        /// </summary>
        /// <param name="manager">Rendering context</param>
        /// <returns></returns>
        bool DrawFrame(RenderManager manager);

        /// <summary>
        /// Called during teardown, dispose of any rendering resources created
        /// </summary>
        /// <param name="manager">Rendering context</param>
        void CleanupGraphics(RenderManager manager);

        /// <summary>
        /// Called when the object properties UI for the object should be drawn.
        /// </summary>
        /// <param name="manager">Rendering context</param>
        void DrawObjectPropertiesUI(RenderManager manager);

        bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox);
    }
}
