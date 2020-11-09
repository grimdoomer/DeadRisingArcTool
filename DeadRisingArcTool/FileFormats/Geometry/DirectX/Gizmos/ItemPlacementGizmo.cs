using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
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
    public class ItemPlacementGizmo : IRenderable
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        private Matrix rotationX;
        private Matrix rotationY;
        private Matrix rotationZ;
        private Quaternion quatRotation;

        public rModel ItemModel { get; private set; }

        private Color4 boxCornerColor = new Color4(0xFF00FF00);
        public Color4 BoxCornerColor { get { return this.boxCornerColor; } set { if (this.boxCornerColor != value) { this.boxCornerColor = value; UpdateColors(); } } }

        private Color4 boxCornerHighlightColor = new Color4(0xFF31B0F5);
        public Color4 BoxCornerHighlightColor { get { return this.boxCornerHighlightColor; } set { if (this.boxCornerHighlightColor != value) { this.boxCornerHighlightColor = value; UpdateColors(); } } }

        private Color4 rotationCircleColor = new Color4(0xFF00FF00);
        public Color4 RotationCircleColor { get { return this.rotationCircleColor; } set { if (this.rotationCircleColor != value) { this.rotationCircleColor = value; UpdateColors(); } } }

        private Color4 rotationCircleHighlightColor = new Color4(0xFF00FF00);
        public Color4 RotationCircleHighlightColor { get { return this.rotationCircleHighlightColor; } set { if (this.rotationCircleHighlightColor != value) { this.rotationCircleHighlightColor = value; UpdateColors(); } } }

        private bool isFocused = false;
        public bool IsFocused { get { return this.isFocused; } set { if (this.isFocused != value) { this.isFocused = value; UpdateColors(); } } }

        public bool ShowRotationCircles { get; set; } = false;

        // Bounding box for the game model.
        private SharpDX.BoundingBox boundingBox;

        // Rotational circle data.
        private int ringSegments = 32;
        private float ringRadius;

        // Rotational arrow data.
        private float arrowHeight;
        private float arrowWidth;

        private D3DColoredVertex[] vertices;
        private int[] indices;
        private SharpDX.Direct3D11.Buffer vertexBuffer;
        private SharpDX.Direct3D11.Buffer indexBuffer;

        // Bounding box corners data:
        private int boxCornerBaseVertex = 0;
        private int boxCornerBaseIndex = 0;
        private int boxCornerIndexCount = 0;

        // Rotation circle vertex data.
        private int[] rotationCircleBaseVertex = new int[3];
        private int[] rotationCircleBaseIndex = new int[3];
        private int[] rotationCircleIndexCount = new int[3];

        private Shader shader;

        public ItemPlacementGizmo(Vector3 position, Vector3 rotation, rModel itemModel)
        {
            // Initialize fields.
            this.Position = position;
            this.Rotation = rotation;
            this.ItemModel = itemModel;

            // Initialize the rotation matrices.
            this.rotationX = Matrix.RotationX(this.Rotation.X);
            this.rotationY = Matrix.RotationY(this.Rotation.Y);
            this.rotationZ = Matrix.RotationZ(this.Rotation.Z);
            this.quatRotation = Quaternion.RotationMatrix(this.rotationX * this.rotationY * this.rotationZ);

            // If a game resource was specified use its bounding box, else use default size.
            if (this.ItemModel != null)
            {
                // Initialize the bounding box for the game model.
                this.boundingBox = new SharpDX.BoundingBox(this.ItemModel.header.BoundingBoxMin.ToVector3(), this.ItemModel.header.BoundingBoxMax.ToVector3());
            }
            else
            {
                // Use default size.
                this.boundingBox = new SharpDX.BoundingBox(new Vector3(0.0f), new Vector3(5.0f, 5.0f, 5.0f));
            }

            // Get the largest side of the bounding box.
            float largestSideLength = Math.Max(this.boundingBox.Depth, Math.Max(this.boundingBox.Height, this.boundingBox.Width));

            // Setup the rotational ring fields.
            this.ringRadius = largestSideLength + 10.0f;
            this.ringSegments = 32 * (((int)this.ringRadius / 100) + 1);

            // Setup the rotation arrow fields.
            this.arrowHeight = this.ringRadius / 2.0f;
        }

        private void BuildVertexBuffer()
        {
            // Create temporary lists for the vertex and index buffers.
            List<D3DColoredVertex> vertexArray = new List<D3DColoredVertex>();
            List<int> indexArray = new List<int>();

            // Setup the major rotational axises.
            Vector3 xaxis = new Vector3(this.ringRadius, 0.0f, 0.0f);
            Vector3 yaxis = new Vector3(0.0f, this.ringRadius, 0.0f);
            Vector3 zaxis = new Vector3(0.0f, 0.0f, this.ringRadius);

            // Build the vertex buffer for the bounding box corners.
            BuildBoxCorners(vertexArray, indexArray);

            // Build the vertex buffer for the rotational circles.
            BuildRotationCircle(vertexArray, indexArray, 0, zaxis, xaxis, new Color4(0xFF00FF00));      // Yaw
            BuildRotationCircle(vertexArray, indexArray, 1, xaxis, yaxis, new Color4(0xFFFF0000));      // Pitch
            BuildRotationCircle(vertexArray, indexArray, 2, yaxis, zaxis, new Color4(0xFF0000FF));      // Roll

            // Save the index and vertex buffers.
            this.vertices = vertexArray.ToArray();
            this.indices = indexArray.ToArray();
        }

        private void UpdateColors()
        {
            // Loop and change the colors for the bounding box corners.
            for (int i = 0; i < this.boxCornerIndexCount; i++)
            {
                // Set the color accordingly.
                this.vertices[this.boxCornerBaseVertex + this.indices[this.boxCornerBaseIndex + i]].Color = this.IsFocused ? this.BoxCornerHighlightColor : this.BoxCornerColor;
            }
        }

        #region Bounding box corners

        private void BuildBoxCorners(List<D3DColoredVertex> vertices, List<int> indices)
        {
            // Line length for corners should be 10 pixels long or the length of the shortest side.
            float smallestSideLength = Math.Min(this.boundingBox.Depth, Math.Min(this.boundingBox.Height, this.boundingBox.Width));
            float cornerLength = Math.Min(10.0f, smallestSideLength);

            // Calculate the half-widths of the box.
            float halfWidth = this.boundingBox.Width / 2.0f;
            float halfHeight = this.boundingBox.Height / 2.0f;
            float halfDepth = this.boundingBox.Depth / 2.0f;

            // Create 3 lines for all 8 corners of the bounding box.
            this.boxCornerBaseVertex = vertices.Count;
            vertices.AddRange(new D3DColoredVertex[]
            {
                // Top left front:
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight, -halfDepth + cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight - cornerLength, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth - cornerLength, halfHeight, -halfDepth), this.BoxCornerColor),

                // Top Right front:
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight, -halfDepth + cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight - cornerLength, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth + cornerLength, halfHeight, -halfDepth), this.BoxCornerColor),

                // Bottom left front:
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight, -halfDepth + cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight + cornerLength, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth - cornerLength, -halfHeight, -halfDepth), this.BoxCornerColor),

                // Bottom Right front:
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight, -halfDepth + cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight + cornerLength, -halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth + cornerLength, -halfHeight, -halfDepth), this.BoxCornerColor),

                // Top left back:
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight, halfDepth - cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, halfHeight - cornerLength, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth - cornerLength, halfHeight, halfDepth), this.BoxCornerColor),

                // Top Right back:
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight, halfDepth - cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, halfHeight - cornerLength, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth + cornerLength, halfHeight, halfDepth), this.BoxCornerColor),

                // Bottom left back:
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight, halfDepth - cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth, -halfHeight + cornerLength, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(halfWidth - cornerLength, -halfHeight, halfDepth), this.BoxCornerColor),

                // Bottom Right back:
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight, halfDepth - cornerLength), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth, -halfHeight + cornerLength, halfDepth), this.BoxCornerColor),
                new D3DColoredVertex(new Vector3(-halfWidth + cornerLength, -halfHeight, halfDepth), this.BoxCornerColor),
            });

            int[] idx = new int[]
            {
                0, 1,
                0, 2,
                0, 3,

                4, 5,
                4, 6,
                4, 7,

                8, 9,
                8, 10,
                8, 11,

                12, 13,
                12, 14,
                12, 15,

                16, 17,
                16, 18,
                16, 19,

                20, 21,
                20, 22,
                20, 23,

                24, 25,
                24, 26,
                24, 27,

                28, 29,
                28, 30,
                28, 31
            };

            // Save the index buffer position and count.
            this.boxCornerBaseIndex = indices.Count;
            this.boxCornerIndexCount = idx.Length;

            // Add the indices to the buffer.
            indices.AddRange(idx);
        }

        #endregion

        private void BuildRotationCircle(List<D3DColoredVertex> vertices, List<int> indices, int index, Vector3 majorAxis, Vector3 minorAxis, Color4 axisColor)
        {
            float angleDelta = MathUtil.TwoPi / (float)this.ringSegments;
            Vector3 cosDelta = new Vector3((float)Math.Cos(angleDelta));
            Vector3 sinDelta = new Vector3((float)Math.Sin(angleDelta));

            Vector3 incrementalSin = new Vector3(0.0f);
            Vector3 incrementalCos = new Vector3(1.0f);

            this.rotationCircleBaseVertex[index] = vertices.Count;
            for (int i = 0; i < this.ringSegments; i++)
            {
                Vector3 position = (majorAxis * incrementalCos);
                position = (minorAxis * incrementalSin) + position;

                vertices.Add(new D3DColoredVertex(position, axisColor));

                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                incrementalSin = newSin;
                incrementalCos = newCos;
            }

            // Set the index starting index position.
            this.rotationCircleBaseIndex[index] = indices.Count;

            // Loop and setup the indices.
            for (int i = 0; i < this.ringSegments; i++)
            {
                // Add the indices for the line.
                indices.AddRange(new int[] { i, i + 1 });
            }

            // Adjust the last index to point to the first vertex.
            indices[indices.Count - 1] = 0;

            // Create identity vectors for the major and minor axises.
            Vector3 majorIdentity = majorAxis; majorIdentity.Normalize();
            Vector3 minorIdentity = minorAxis; minorIdentity.Normalize();

            // Setup the vertices for the rotation arrow.
            Vector3 p0 = minorAxis + (new Vector3(this.arrowHeight + 5.0f) * minorIdentity);
            Vector3 p1 = minorAxis + (new Vector3(5.0f) * minorIdentity) + (new Vector3(this.arrowHeight / 2.0f) * majorIdentity);
            Vector3 p2 = minorAxis + (new Vector3(5.0f + (this.arrowHeight / 5.0f)) * minorIdentity);
            Vector3 p3 = minorAxis + (new Vector3(5.0f) * minorIdentity) - (new Vector3(this.arrowHeight / 2.0f) * majorIdentity);

            int arrowStartVertex = vertices.Count - this.rotationCircleBaseVertex[index];
            vertices.AddRange(new D3DColoredVertex[]
            {
                new D3DColoredVertex(p0, axisColor),
                new D3DColoredVertex(p1, axisColor),
                new D3DColoredVertex(p2, axisColor),
                new D3DColoredVertex(p3, axisColor),
            });

            // Loop and setup indices for the arrow.
            for (int i = 0; i < 4; i++)
            {
                // Add the indices for the line.
                indices.AddRange(new int[] { arrowStartVertex + i, arrowStartVertex + i + 1 });
            }

            // Adjust the last index to point to the first vertex for the arrow.
            indices[indices.Count - 1] = arrowStartVertex;

            // Set the index count.
            this.rotationCircleIndexCount[index] = indices.Count - this.rotationCircleBaseIndex[index];
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

            // Successfully initialized.
            return true;
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Update the vertex and index buffers.
            manager.Device.ImmediateContext.UpdateSubresource(this.vertices, this.vertexBuffer);
            manager.Device.ImmediateContext.UpdateSubresource(this.indices, this.indexBuffer);

            // Update the WVP matrix to position and rotate the gizmo.
            Matrix world = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, this.quatRotation, this.Position);
            manager.ShaderConstants.gXfViewProj = Matrix.Transpose(world * manager.Camera.ViewMatrix * manager.ProjectionMatrix);
            manager.UpdateShaderConstants();

            // Set the vertex and index buffers.
            manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, 28, 0));
            manager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            // Setup the wireframe shader.
            this.shader.DrawFrame(manager);

            // Draw the bounding box corners.
            manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            manager.Device.ImmediateContext.DrawIndexed(this.boxCornerIndexCount, this.boxCornerBaseIndex, this.boxCornerBaseVertex);

            // Check if we should draw the rotation circles.
            if (this.ShowRotationCircles == true)
            {
                // Draw the rotation circles.
                for (int i = 0; i < 3; i++)
                {
                    // Draw the circle.
                    manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
                    manager.Device.ImmediateContext.DrawIndexed(this.rotationCircleIndexCount[i], this.rotationCircleBaseIndex[i], this.rotationCircleBaseVertex[i]);
                }
            }

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {
            throw new NotImplementedException();
        }

        public void CleanupGraphics(RenderManager manager)
        {
            throw new NotImplementedException();
        }

        public bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox)
        {
            return false;
        }
    }
}
