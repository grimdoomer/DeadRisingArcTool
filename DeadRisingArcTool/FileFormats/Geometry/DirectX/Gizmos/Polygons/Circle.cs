using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons
{
    public class Circle : Polygon
    {
        private float radius;
        public float Radius { get { return this.radius; } set { if (this.radius != value) { this.radius = value; this.IsDirty = true; } } }

        private Vector3 minorAxis;
        public Vector3 MinorAxis { get { return this.minorAxis; } set { this.minorAxis = value; this.IsDirty = true; } }

        private Vector3 majorAxis;
        public Vector3 MajorAxis { get { return this.majorAxis; } set { this.majorAxis = value; this.IsDirty = true; } }

        private Color4 color = new Color4(0xFF00FF00);
        public Color4 Color { get { return this.color; } set { if (this.color != value) { this.color = value; this.IsDirty = true; } } }

        private int ringSegments;

        public Circle(float radius, Vector3 minorAxis, Vector3 majorAxis, Vector3 position, Quaternion rotation)
            : base(32 * (((int)radius / 100) + 1), 2 * (32 * (((int)radius / 100) + 1)), position, rotation)
        {
            // Initialize fields.
            this.radius = radius;
            this.minorAxis = minorAxis;
            this.majorAxis = majorAxis;

            this.ringSegments = 32 * (((int)this.radius / 100) + 1);

            this.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
        }

        public override void BuildMesh(VertexStreamSplice<D3DColoredVertex> vertexBuffer, VertexStreamSplice<ushort> indexBuffer)
        {
            // Set the number of vertices and indices being used.
            this.VertexCount = 32 * (((int)radius / 100) + 1);
            this.IndexCount = 2 * this.VertexCount;

            float angleDelta = MathUtil.TwoPi / (float)this.ringSegments;
            Vector3 cosDelta = new Vector3((float)Math.Cos(angleDelta));
            Vector3 sinDelta = new Vector3((float)Math.Sin(angleDelta));

            Vector3 incrementalSin = new Vector3(0.0f);
            Vector3 incrementalCos = new Vector3(1.0f);

            for (int i = 0; i < this.ringSegments; i++)
            {
                Vector3 position = (majorAxis * incrementalCos);
                position = (minorAxis * incrementalSin) + position;

                vertexBuffer[i] = new D3DColoredVertex(position, this.Color);

                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                incrementalSin = newSin;
                incrementalCos = newCos;
            }

            // Loop and setup the indices.
            for (int i = 0; i < this.ringSegments; i++)
            {
                // Add the indices for the line.
                indexBuffer[(i * 2)] = (ushort)i;
                indexBuffer[(i * 2) + 1] = (ushort)(i + 1);
            }

            // Adjust the last index to point to the first vertex.
            indexBuffer[indexBuffer.Length - 1] = 0;

            // Flag that we are no longer dirty.
            this.IsDirty = false;
        }
    }
}
