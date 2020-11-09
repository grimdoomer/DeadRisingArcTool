using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc
{
    public interface IPickableObject
    {
        bool DoPickingTest(RenderManager manager, Ray pickingRay);
    }
}
