using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos
{
    public class BoundingBox : IRenderable
    {
        private Vector4 boundMin;
        /// <summary>
        /// Minimum extents of the bounding box
        /// </summary>
        public Vector4 BoundMin { get { return this.boundMin; } set { this.boundMin = value; BuildVertexBuffer(); } }

        private Vector4 boundMax;
        /// <summary>
        /// Maximum extentds of the bounding box
        /// </summary>
        public Vector4 BoundMax { get { return this.boundMax; } set { this.boundMax = value; BuildVertexBuffer(); } }

        private Color4 color;
        /// <summary>
        /// Color of the bounding box mesh
        /// </summary>
        public Color4 Color { get { return this.color; } set { this.color = value; BuildVertexBuffer(); } }

        private RenderStyle style = RenderStyle.Wireframe;
        /// <summary>
        /// Render style for the box
        /// </summary>
        public RenderStyle Style { get { return this.style; } set { this.style = value; BuildVertexBuffer(); } }

        private D3DColoredVertex[] vertices;
        private int[] indices;
        private SharpDX.Direct3D11.Buffer vertexBuffer;
        private SharpDX.Direct3D11.Buffer indexBuffer;

        private Shader shader;

        public BoundingBox(Vector4 boundMin, Vector4 boundMax, Color4 color)
        {
            // Initialize fields.
            this.boundMin = boundMin;
            this.boundMax = boundMax;
            this.color = color;
        }

        private void BuildVertexBuffer()
        {
            // Compute the bounding box extents.
            SharpDX.BoundingBox box = new SharpDX.BoundingBox(this.BoundMin.ToVector3(), this.BoundMax.ToVector3());

            // Calculate the half-widths of the box.
            float halfWidth = box.Width / 2.0f;
            float halfHeight = box.Height / 2.0f;
            float halfDepth = box.Depth / 2.0f;

            // Build the vertex array from the bounding box info.
            this.vertices = new D3DColoredVertex[8];
            this.vertices[0].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y - halfHeight, box.Center.Z + halfDepth); // BL-R
            this.vertices[1].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y - halfHeight, box.Center.Z + halfDepth); // BR-R
            this.vertices[2].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y - halfHeight, box.Center.Z - halfDepth); // BR-F
            this.vertices[3].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y - halfHeight, box.Center.Z - halfDepth); // BL-F

            this.vertices[4].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y + halfHeight, box.Center.Z + halfDepth); // TL-R
            this.vertices[5].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y + halfHeight, box.Center.Z + halfDepth); // TR-R
            this.vertices[6].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y + halfHeight, box.Center.Z - halfDepth); // TR-F
            this.vertices[7].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y + halfHeight, box.Center.Z - halfDepth); // TL-F

            // Check the render style and build the index buffer accordingly.
            if (this.style == RenderStyle.Wireframe)
            {
                // Setup the index buffer as a line list.
                this.indices = new int[24]
                {
                    0, 1,
                    1, 2,
                    2, 3,
                    3, 0,

                    4, 5,
                    5, 6,
                    6, 7,
                    7, 4,

                    0, 4,
                    1, 5,
                    2, 6,
                    3, 7
                };
            }
            else
            {
                // Setup the index buffer as a triangle list.
                this.indices = new int[36]
                {
                    // Right face:
                    0, 4, 7,
                    7, 3, 0,

                    // Front face:
                    7, 6, 2,
                    3, 7, 2,

                    // Left face:
                    6, 5, 1,
                    2, 6, 1,

                    // Back face:
                    5, 4, 0,
                    1, 5, 0,

                    // Bottom:
                    0, 3, 2,
                    2, 1, 0,

                    // Top:
                    6, 7, 4,
                    4, 5, 6
                };
            }

            // Set vertex colors.
            for (int i = 0; i < this.vertices.Length; i++)
                this.vertices[i].Color = this.Color;
        }

        public bool InitializeGraphics(RenderManager manager)
        {
            // Build the initial vertex array.
            BuildVertexBuffer();

            // Setup the vertex buffer using the vertex data stream.
            BufferDescription desc = new BufferDescription();
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            desc.StructureByteStride = 28;
            desc.Usage = ResourceUsage.Default;
            this.vertexBuffer = SharpDX.Direct3D11.Buffer.Create<D3DColoredVertex>(manager.Device, BindFlags.VertexBuffer, this.vertices);

            // Setup the index buffer using the indice data.
            desc = new BufferDescription();
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            desc.StructureByteStride = 4;
            desc.Usage = ResourceUsage.Default;
            this.indexBuffer = SharpDX.Direct3D11.Buffer.Create<int>(manager.Device, BindFlags.IndexBuffer, this.indices);

            // Get the wireframe shader.
            this.shader = manager.ShaderCollection.GetShader(ShaderType.Wireframe);

            return true;
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Update the vertex and index buffers.
            manager.Device.ImmediateContext.UpdateSubresource(this.vertices, this.vertexBuffer);
            manager.Device.ImmediateContext.UpdateSubresource(this.indices, this.indexBuffer);

            // Set the primitive type based on the render style.
            manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = this.style == RenderStyle.Wireframe ? 
                SharpDX.Direct3D.PrimitiveTopology.LineList : SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            // Set the vertex and index buffers.
            manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, 28, 0));
            manager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            // Setup the wireframe shader.
            this.shader.DrawFrame(manager);

            // Draw the cube.
            manager.Device.ImmediateContext.DrawIndexed(this.indices.Length, 0, 0);

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public void CleanupGraphics(RenderManager manager)
        {
            throw new NotImplementedException();
        }
    }
}
