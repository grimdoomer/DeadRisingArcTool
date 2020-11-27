using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons
{
    public class Arrow : Polygon
    {
        private float arrowHeight;
        public float Height { get { return this.arrowHeight; } set { if (this.arrowHeight != value) { this.arrowHeight = value; this.IsDirty = true; } } }

        private Color4 color = new Color4(0xFF00FF00);
        public Color4 Color { get { return this.color; } set { if (this.color != value) { this.color = value; this.IsDirty = true; } } }

        public Arrow(float height, Vector3 position, Quaternion rotation)
            : base(4, 8, position, rotation)
        {
            // Initialize fields.
            this.arrowHeight = height;
        }

        public override void BuildMesh(VertexStreamSplice<D3DColoredVertex> vertexBuffer, VertexStreamSplice<ushort> indexBuffer)
        {
            // Build the vertex buffer.
            vertexBuffer[0] = new D3DColoredVertex(new Vector3(0f, 0f, -(this.arrowHeight / 2f)), this.color);
            vertexBuffer[1] = new D3DColoredVertex(new Vector3(-(this.arrowHeight / 2f), 0f, this.arrowHeight / 2f), this.color);
            vertexBuffer[2] = new D3DColoredVertex(new Vector3(0f, 0f, this.arrowHeight / 5.0f), this.color);
            vertexBuffer[3] = new D3DColoredVertex(new Vector3(this.arrowHeight / 2f, 0f, this.arrowHeight / 2f), this.color);

            // Check the draw style and handle accordingly.
            if (this.Style == PolygonDrawStyle.Outline)
            {
                // Set the number of vertices and indices being used.
                this.VertexCount = 4;
                this.IndexCount = 8;

                // Build the index buffer.
                for (int i = 0; i < 4; i++)
                {
                    indexBuffer[(i * 2)] = (ushort)i;
                    indexBuffer[(i * 2) + 1] = (ushort)(i + 1);
                }

                // Correct the last index.
                indexBuffer[7] = 0;
            }
            else
            {
                // Set the number of vertices and indices being used.
                this.VertexCount = 4;
                this.IndexCount = 6;

                // Build the index buffer.
                indexBuffer[0] = 0;
                indexBuffer[1] = 1;
                indexBuffer[2] = 2;

                indexBuffer[3] = 0;
                indexBuffer[4] = 2;
                indexBuffer[5] = 3;
            }

            // Flag that we are no longer dirty.
            this.IsDirty = false;
        }
    }
}
