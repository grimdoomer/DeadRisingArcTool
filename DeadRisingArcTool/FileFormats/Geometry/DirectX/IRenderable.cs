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
        /// <param name="device">The DirectX <see cref="Device"/> used for rendering</param>
        /// <returns>True if initialization was successful, false otherwise</returns>
        bool InitializeGraphics(IRenderManager manager, Device device);

        /// <summary>
        /// Called during the render loop, draws the object to screen
        /// </summary>
        /// <param name="device">The DirectX <see cref="Device"/> used for rendering</param>
        /// <returns></returns>
        bool DrawFrame(IRenderManager manager, Device device);

        /// <summary>
        /// Called during teardown, dispose of any rendering resources created
        /// </summary>
        /// <param name="device">The DirectX <see cref="Device"/> used for rendering</param>
        void CleanupGraphics(IRenderManager manager, Device device);
    }
}
