﻿using SharpDX;
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

        public static Vector4 ToVector4(this Quaternion quat)
        {
            return new Vector4(quat.X, quat.Y, quat.Z, quat.W);
        }

        public static Quaternion ToQuaternion(this Vector4 vector)
        {
            return new Quaternion(vector);
        }

        public static Vector4 SinEst(this Vector4 vector)
        {
            return new Vector4((float)Math.Sin(vector.X), (float)Math.Sin(vector.Y), (float)Math.Sin(vector.Z), (float)Math.Sin(vector.W));
        }

        public static Matrix MatrixFromVectors(Vector4[] vectors)
        {
            return new Matrix(vectors[0].X, vectors[0].Y, vectors[0].Z, vectors[0].W,
                vectors[1].X, vectors[1].Y, vectors[1].Z, vectors[1].W,
                vectors[2].X, vectors[2].Y, vectors[2].Z, vectors[2].W,
                vectors[3].X, vectors[3].Y, vectors[3].Z, vectors[3].W);
        }
    }
}
