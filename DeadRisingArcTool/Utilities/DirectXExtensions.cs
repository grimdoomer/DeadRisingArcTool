using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Utilities
{
    public static class DirectXExtensions
    {
        public static Vector3 ToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
