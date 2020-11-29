using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons
{
    public class Arrow : Polygon, IPickableObject
    {
        private float arrowHeight;
        public float Height { get { return this.arrowHeight; } set { if (this.arrowHeight != value) { this.arrowHeight = value; this.IsDirty = true; } } }

        private Color4 color = new Color4(0xFF00FF00);
        public Color4 Color { get { return this.color; } set { if (this.color != value) { this.color = value; this.IsDirty = true; } } }

        // Vertex array for quick access for hit tests.
        private D3DColoredVertex[] vertices = new D3DColoredVertex[4];

        public Arrow(float height, Vector3 position, Quaternion rotation)
            : base(4, 8, position, rotation)
        {
            // Initialize fields.
            this.arrowHeight = height;
        }

        public override void BuildMesh(VertexStreamSplice<D3DColoredVertex> vertexBuffer, VertexStreamSplice<ushort> indexBuffer)
        {
            // Build the vertex buffer.
            vertexBuffer[0] = this.vertices[0] = new D3DColoredVertex(new Vector3(0f, 0f, -(this.arrowHeight / 2f)), this.color);
            vertexBuffer[1] = this.vertices[1] = new D3DColoredVertex(new Vector3(-(this.arrowHeight / 2f), 0f, this.arrowHeight / 2f), this.color);
            vertexBuffer[2] = this.vertices[2] = new D3DColoredVertex(new Vector3(0f, 0f, this.arrowHeight / 5.0f), this.color);
            vertexBuffer[3] = this.vertices[3] = new D3DColoredVertex(new Vector3(this.arrowHeight / 2f, 0f, this.arrowHeight / 2f), this.color);

            // Check the draw style and handle accordingly.
            if (this.Style == PolygonDrawStyle.Outline)
            {
                // Set the number of vertices and indices being used.
                this.VertexCount = 4;
                this.IndexCount = 8;

                // Set primitive topology.
                this.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;

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

                // Set primitive topology.
                this.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

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

        #region IPickableObject

        public bool DoPickingTest(RenderManager manager, Ray pickingRay, out float distance, out object context)
        {
            // Invert the arrow transformation so we can transform the picking ray to local space.
            Matrix arrowTransform = this.TransformationMatrix;
            arrowTransform.Invert();

            // Transform the picking ray to be in local space.
            Ray newPickingRay = new Ray(Vector3.TransformCoordinate(pickingRay.Position, arrowTransform), Vector3.TransformNormal(pickingRay.Direction, arrowTransform));
            newPickingRay.Direction.Normalize();

            // Perform hit detection with both triangles for the arrow.
            bool hitTest = newPickingRay.Intersects(ref this.vertices[0].Position, ref this.vertices[1].Position, ref this.vertices[2].Position) ||
                newPickingRay.Intersects(ref this.vertices[0].Position, ref this.vertices[3].Position, ref this.vertices[2].Position);

            // If we had a hit set the distance to the arrow.
            if (hitTest == true)
                distance = this.Position.Z;
            else
                distance = float.MaxValue;

            // Return the hit test result.
            context = null;
            return hitTest;
        }

        public void SelectObject(RenderManager manager, object context)
        {

        }

        public bool DeselectObject(RenderManager manager, object context)
        {
            return true;
        }

        public bool HandleInput(RenderManager manager)
        {
            return false;
        }

        #endregion
    }
}
