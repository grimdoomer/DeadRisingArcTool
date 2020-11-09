using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc
{
    public class FastBoundingBox
    {
        protected Vector3 boxMinimum;
        public Vector3 BoxMinimum { get { return this.boxMinimum; } }

        protected Vector3 boxMaximum;
        public Vector3 BoxMaximum { get { return this.boxMaximum; } }

        public FastBoundingBox(Vector3 min, Vector3 max)
        {
            // Initialize fields.
            this.boxMinimum = min;
            this.boxMaximum = max;
        }

        public void Reset(Vector3 min, Vector3 max)
        {
            // Reset the box min and max.
            this.boxMinimum = min;
            this.boxMaximum = max;
        }

        public bool ClipTest(Vector3 min, Vector3 max)
        {
            // Check if any part of the mesh bounding box is intereseting with our bounding box.
            if (IntersectionTest(min.X, max.X, this.boxMinimum.X, this.boxMaximum.X) == true ||
                IntersectionTest(min.Y, max.Y, this.boxMinimum.Y, this.boxMaximum.Y) == true ||
                IntersectionTest(min.Z, max.Z, this.boxMinimum.Z, this.boxMaximum.Z) == true)
            {
                // There is an intersection.
                return true;
            }

            // No interesection.
            return false;
        }

        private bool IntersectionTest(float min1, float max1, float min2, float max2)
        {
            return (min1 >= min2 && min1 <= max2) || (max1 >= min2 && max1 <= max2) || (min2 >= min1 && min2 <= max1) || (max2 >= min1 && max2 <= max1);
        }
    }
}
