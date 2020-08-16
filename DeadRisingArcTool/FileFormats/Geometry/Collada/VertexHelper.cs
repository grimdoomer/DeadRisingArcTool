using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.Collada
{
    public class VertexHelper
    {
        public static Vector2 Decompress_R16G16_SNorm(byte[] buffer, int index)
        {
            // Get the vector components in compressed form from the buffer.
            short x = BitConverter.ToInt16(buffer, index);
            short y = BitConverter.ToInt16(buffer, index + 2);

            // Decompress and return as a vector.
            return new Vector2(SNorm16ToFloat(x), SNorm16ToFloat(y));
        }

        public static Vector4 Decompress_R16G16B16A16_SNorm(byte[] buffer, int index)
        {
            // Get the vector components in compressed form from the buffer.
            short x = BitConverter.ToInt16(buffer, index);
            short y = BitConverter.ToInt16(buffer, index + 2);
            short z = BitConverter.ToInt16(buffer, index + 4);
            short w = BitConverter.ToInt16(buffer, index + 6);

            // Decompress and return as a vector.
            return new Vector4(SNorm16ToFloat(x), SNorm16ToFloat(y), SNorm16ToFloat(z), SNorm16ToFloat(w));
        }

        public static float SNorm16ToFloat(short value)
        {
            // Map [-32768, 32767] to [-1, 1].
            return Math.Max(value / 32767.0f, -1.0f);
        }

        public static short[] TriangleStripToTriangleList(short[] stripIndices, int startIndex, int indexCount, int vertexBase)
        {
            // Create a list to hold the triangle list indices.
            List<short> triList = new List<short>();

            // Loop and convert the triangle strip to a triangle list.
            for (int i = 0; i < indexCount - 2; i++)
            {
                // Get the vertex indices for the current triangle.
                short v1 = (short)(stripIndices[startIndex + i] - vertexBase);
                short v2 = (short)(stripIndices[startIndex + i + 1] - vertexBase);
                short v3 = (short)(stripIndices[startIndex + i + 2] - vertexBase);

                // Check for degenerate triangle.
                if (v1 == v2 || v1 == v3 || v2 == v3)
                {
                    // Degenerate triangle, skip.
                    continue;
                }

                // Check for clockwise/counter clockwise orientation and handle accordingly.
                if (i % 2 != 0)
                {
                    triList.Add(v2);
                    triList.Add(v1);
                    triList.Add(v3);
                }
                else
                {
                    triList.Add(v1);
                    triList.Add(v2);
                    triList.Add(v3);
                }
            }

            // Return the triangle list.
            return triList.ToArray();
        }
    }
}
