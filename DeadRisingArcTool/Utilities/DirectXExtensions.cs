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

        public static float AngleBetweenLine(this Vector3 @this, Vector3 vector)
        {
            // Compute the dot product of the two rays.
            float dotProduct = Vector3.Dot(@this, vector);

            // Get the magnitude of the two lines.
            float lengthA = @this.Length();
            float lengthB = vector.Length();

            // Calculate the angle between the two lines..
            float cosAngle = dotProduct / (lengthA * lengthB);
            float angle = (float)Math.Acos(cosAngle);

            // If the angle value is invalid bail out.
            if (float.IsNaN(angle) == true)
                return float.NaN;

            // Convert the angle from [0, pi] to [0, 2*pi].
            return angle;// * 2.0f;
        }

        //public static bool IntersectsWithPlane(this Ray @this, Plane plane, Vector3 p0, out Vector3 pointOfIntersection)
        //{
        //    // Satisfy the compiler.
        //    pointOfIntersection = Vector3.Zero;

        //    float denom = Vector3.Dot(plane.Normal, @this.Direction);
        //    if (Math.Abs(denom) <= 0.0001f)
        //        return false;

        //    // Calculate the distance from the ray to the plane.
        //    float distance = Vector3.Dot(p0 - @this.Position, plane.Normal) / denom;
        //    if (distance <= 0.0001f)
        //        return false;

        //    // Calculate the point of intersection using the distance and ray.
        //    pointOfIntersection = @this.Position + (@this.Direction * distance);
        //    return true;
        //}
    }
}
