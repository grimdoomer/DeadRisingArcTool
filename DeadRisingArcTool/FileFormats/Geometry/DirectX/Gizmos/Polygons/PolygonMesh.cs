using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons
{
    public class PolygonMesh : IRenderable
    {
        /// <summary>
        /// This struct is "glue" between Polygon and PolygonStream.
        /// </summary>
        public struct PolygonMeshInfo
        {
            public int BaseVertex;
            public int BaseIndex;
        }

        /// <summary>
        /// Array of polygons in the stream.
        /// </summary>
        public Polygon[] Polygons { get; private set; }
        private PolygonMeshInfo[] polygonMeshInfo;

        private Vector3 position;
        /// <summary>
        /// Base position of the polygon stream.
        /// </summary>
        public Vector3 Position { get { return this.position; } set { this.position = value; UpdateTransformationMatrix(); } }

        private Quaternion rotation;
        /// <summary>
        /// Base rotation of the polygon stream.
        /// </summary>
        public Quaternion Rotation { get { return this.rotation; } set { this.rotation = value; UpdateTransformationMatrix(); } }

        /// <summary>
        /// Number of vertices the mesh can hold.
        /// </summary>
        public int VertexCount { get { return (this.vertexStream != null ? this.vertexStream.Vertices.Length : 0); } }
        /// <summary>
        /// Number of vertex indices the mesh can hold.
        /// </summary>
        public int IndexCount { get { return (this.vertexStream != null ? this.vertexStream.Indices.Length : 0); } }

        // Transformation matrix for the stream, only updated when Position or Rotation change.
        private Matrix transformationMatrix;

        // Vertex and index buffers that hold all polygon data.
        private VertexStream<D3DColoredVertex, ushort> vertexStream;
        private Buffer vertexBuffer = null;
        private Buffer indexBuffer = null;

        // Shader instance.
        private Shader wireframeShader;

        public PolygonMesh(Vector3 position, Quaternion rotation, params Polygon[] polygons)
        {
            // Initialize fields.
            this.Position = position;
            this.Rotation = rotation;
            this.Polygons = polygons;
        }

        private void UpdateTransformationMatrix()
        {
            // Calculate the transformation matrix.
            this.transformationMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, this.rotation, this.position);
        }

        #region IRenderable

        public bool InitializeGraphics(RenderManager manager)
        {
            // Allocate the mesh info array.
            this.polygonMeshInfo = new PolygonMeshInfo[this.Polygons.Length];

            // Loop through all of the polygons and compute the size needed for the vertex and index buffers.
            int vertexCount = 0, indexCount = 0;
            for (int i = 0; i < this.Polygons.Length; i++)
            {
                // Update the mesh info starting positions.
                this.polygonMeshInfo[i].BaseVertex = vertexCount;
                this.polygonMeshInfo[i].BaseIndex = indexCount;

                // Update the counters.
                vertexCount += this.Polygons[i].MaxVertexCount;
                indexCount += this.Polygons[i].MaxIndexCount;
            }

            // Create the vertex stream and fill it with the polygon data.
            this.vertexStream = new VertexStream<D3DColoredVertex, ushort>(vertexCount, indexCount);
            for (int i = 0; i < this.Polygons.Length; i++)
            {
                // Create a new splice for the vertex and index data for this polygon.
                VertexStreamSplice<D3DColoredVertex> vertexData = this.vertexStream.SpliceVertexBuffer(this.polygonMeshInfo[i].BaseVertex, this.Polygons[i].MaxVertexCount);
                VertexStreamSplice<ushort> indexData = this.vertexStream.SpliceIndexBuffer(this.polygonMeshInfo[i].BaseIndex, this.Polygons[i].MaxIndexCount);

                // Build the polygon mesh which will update the vertex stream.
                this.Polygons[i].BuildMesh(vertexData, indexData);
            }

            // Create the vertex and index buffers.
            this.vertexBuffer = Buffer.Create(manager.Device, BindFlags.VertexBuffer, this.vertexStream.Vertices, accessFlags: CpuAccessFlags.Write);
            this.indexBuffer = Buffer.Create(manager.Device, BindFlags.IndexBuffer, this.vertexStream.Indices, accessFlags: CpuAccessFlags.Write);

            // Get the wireframe shader.
            this.wireframeShader = manager.ShaderCollection.GetShader(ShaderType.Wireframe);

            // Successfully initialized.
            return true;
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Loop and check if any of the polygons are dirty and require updating.
            bool isDirty = false;
            for (int i = 0; i < this.Polygons.Length; i++)
            {
                // If the polygon is dirty flag that we need to update and rebuild the polygon.
                if (this.Polygons[i].IsDirty == true)
                {
                    // Flag that we need to update.
                    isDirty = true;

                    // Create a new splice for the vertex and index data for this polygon.
                    VertexStreamSplice<D3DColoredVertex> vertexData = this.vertexStream.SpliceVertexBuffer(this.polygonMeshInfo[i].BaseVertex, this.Polygons[i].MaxVertexCount);
                    VertexStreamSplice<ushort> indexData = this.vertexStream.SpliceIndexBuffer(this.polygonMeshInfo[i].BaseIndex, this.Polygons[i].MaxIndexCount);

                    // Rebuild the polygon mesh which updates the vertex buffer.
                    this.Polygons[i].BuildMesh(vertexData, indexData);
                }
            }

            // If one or more polygons are dirty update the entire vertex and index buffers.
            if (isDirty == true)
            {
                // Update the vertex and index buffers.
                manager.Device.ImmediateContext.UpdateSubresource(this.vertexStream.Vertices, this.vertexBuffer);
                manager.Device.ImmediateContext.UpdateSubresource(this.vertexStream.Indices, this.indexBuffer);
            }

            // Set the vertex and index buffers.
            manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, D3DColoredVertex.kSizeOf, 0));
            manager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            // Setup the wireframe shader.
            this.wireframeShader.DrawFrame(manager);

            // Loop and draw each polygon.
            for (int i = 0; i < this.Polygons.Length; i++)
            {
                // Compute the transformation matrix and update shader constants.
                manager.ShaderConstants.gXfViewProj = Matrix.Transpose((this.transformationMatrix * this.Polygons[i].TransformationMatrix) * manager.Camera.ViewMatrix * manager.ProjectionMatrix);
                manager.UpdateShaderConstants();

                // TODO: This should be more efficient than updating the shaders constants buffer for every polygon. Perhaps create another buffer
                //          that has all the transformation matrices in it.

                // Set the primitive type based on the render style.
                manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = this.Polygons[i].PrimitiveTopology;

                // Draw the polygon.
                manager.Device.ImmediateContext.DrawIndexed(this.Polygons[i].IndexCount, this.polygonMeshInfo[i].BaseIndex, this.polygonMeshInfo[i].BaseVertex);
            }

            // Mesh rendered successfully.
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
            // Always return true since we we handle clipping in the DrawFrame function.
            return true;
        }

        #endregion
    }
}
