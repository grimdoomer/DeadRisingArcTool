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
        public Vector4 BoundMin { get; set; }
        public Vector4 BoundMax { get; set; }

        public Color4 Color { get; set; }

        private D3DColoredVertex[] vertices;
        private SharpDX.Direct3D11.Buffer vertexBuffer;
        private SharpDX.Direct3D11.Buffer indexBuffer;

        private BuiltInShader shader;

        // Bounding box vertices to be transformed.
        private readonly Vector4[] BoxVertices = new Vector4[8]
        {
            new Vector4(-1.0f, -1.0f, -1.0f, 0.0f),
            new Vector4(1.0f, -1.0f, -1.0f, 0.0f),
            new Vector4(1.0f, -1.0f, 1.0f, 0.0f),
            new Vector4(-1.0f, -1.0f, 1.0f, 0.0f), 
            new Vector4(-1.0f, 1.0f, -1.0f, 0.0f),
            new Vector4(1.0f, 1.0f, -1.0f, 0.0f),
            new Vector4(1.0f, 1.0f, 1.0f, 0.0f), 
            new Vector4(-1.0f, 1.0f, 1.0f, 0.0f)
        };

        // Bounding box triangle indices.
        private readonly int[] BoxIndices = new int[24]
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

        public BoundingBox(Vector4 boundMin, Vector4 boundMax, Color4 color)
        {
            // Initialize fields.
            this.BoundMin = boundMin;
            this.BoundMax = boundMax;
            this.Color = color;
        }

        public bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Compute the bounding box extents.
            SharpDX.BoundingBox box = new SharpDX.BoundingBox(this.BoundMin.ToVector3(), this.BoundMax.ToVector3());

            // Allocate the vertex array.
            this.vertices = new D3DColoredVertex[8];

            // Calculate the half-widths of the box.
            float halfWidth = box.Width / 2.0f;
            float halfHeight = box.Height / 2.0f;
            float halfDepth = box.Depth / 2.0f;

            // Build the vertex array from the bounding box info.
            this.vertices[0].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y - halfHeight, box.Center.Z + halfDepth);
            this.vertices[1].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y - halfHeight, box.Center.Z + halfDepth);
            this.vertices[2].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y - halfHeight, box.Center.Z - halfDepth);
            this.vertices[3].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y - halfHeight, box.Center.Z - halfDepth);

            this.vertices[4].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y + halfHeight, box.Center.Z + halfDepth);
            this.vertices[5].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y + halfHeight, box.Center.Z + halfDepth);
            this.vertices[6].Position = new Vector3(box.Center.X + halfWidth, box.Center.Y + halfHeight, box.Center.Z - halfDepth);
            this.vertices[7].Position = new Vector3(box.Center.X - halfWidth, box.Center.Y + halfHeight, box.Center.Z - halfDepth);

            // Setup the vertex buffer using the vertex data stream.
            BufferDescription desc = new BufferDescription();
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            desc.StructureByteStride = 28;
            desc.Usage = ResourceUsage.Default;
            this.vertexBuffer = SharpDX.Direct3D11.Buffer.Create<D3DColoredVertex>(device, BindFlags.VertexBuffer, this.vertices);

            // Setup the index buffer using the indice data.
            desc = new BufferDescription();
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            desc.StructureByteStride = 4;
            desc.Usage = ResourceUsage.Default;
            this.indexBuffer = SharpDX.Direct3D11.Buffer.Create<int>(device, BindFlags.IndexBuffer, this.BoxIndices);

            // Get the wireframe shader.
            this.shader = manager.GetBuiltInShader(BuiltInShaderType.Wireframe);

            return true;
        }

        public bool DrawFrame(IRenderManager manager, Device device)
        {
            // Compute the bounding box extents.
            SharpDX.BoundingBox box = new SharpDX.BoundingBox(this.BoundMin.ToVector3(), this.BoundMax.ToVector3());

            // Compute the transformation matrix that will be used to transform the box vertices.
            Matrix transMatrix = Matrix.Scaling(box.Width, box.Height, box.Depth);
            transMatrix.Row4 = new Vector4(box.Center, transMatrix.Row4.W);

            // Loop and transform the vertices.
            for (int i = 0; i < this.vertices.Length; i++)
            {
                // Transform the current vertex.
                //this.vertices[i].Position = Vector4.Transform(this.BoxVertices[i], transMatrix).ToVector3();
                this.vertices[i].Color = this.Color;
            }

            // Update the vertex buffer.
            device.ImmediateContext.UpdateSubresource(this.vertices, this.vertexBuffer);

            // Set the primitive type to line list.
            device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;

            // Set the vertex and index buffers.
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, 28, 0));
            device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            // Setup the wireframe shader.
            this.shader.DrawFrame(manager, device);

            // Draw the cube.
            device.ImmediateContext.DrawIndexed(24, 0, 0);

            return true;
        }

        public void CleanupGraphics(IRenderManager manager, Device device)
        {
            throw new NotImplementedException();
        }
    }
}
