using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons
{
    /// <summary>
    /// Style for how a polygon is drawn
    /// </summary>
    public enum PolygonDrawStyle
    {
        //Wireframe,
        Outline,
        Solid
    }

    /// <summary>
    /// Represents a closed N sided geometric shape that can be drawn as part of a <see cref="PolygonMesh"/>
    /// </summary>
    public abstract class Polygon
    {
        private Vector3 position;
        /// <summary>
        /// Position of the polygon relative to the stream root position.
        /// </summary>
        public Vector3 Position { get { return this.position; } set { this.position = value; UpdateTransformationMatrix(); } }

        private Quaternion rotation;
        /// <summary>
        /// Rotation of the polygon relative to the stream root position.
        /// </summary>
        public Quaternion Rotation { get { return this.rotation; } set { this.rotation = value; UpdateTransformationMatrix(); } }
        /// <summary>
        /// Precalculated transformation matrix.
        /// </summary>
        public Matrix TransformationMatrix { get; private set; }
        /// <summary>
        /// True if the polygon should be drawn, false if it should be hidden.
        /// </summary>
        public bool Visible { get; set; } = true;

        private PolygonDrawStyle style = PolygonDrawStyle.Outline;
        /// <summary>
        /// Draw style of the polygon.
        /// </summary>
        public PolygonDrawStyle Style { get { return this.style; } set { this.style = value; this.IsDirty = true; } }

        /// <summary>
        /// DirectX primitive topology type the polygon should be rendered with.
        /// </summary>
        public PrimitiveTopology PrimitiveTopology { get; protected set; } = PrimitiveTopology.Undefined;

        /// <summary>
        /// Indicates if the mesh data is stale and needs to be rebuilt.
        /// </summary>
        public bool IsDirty { get; protected set; } = true;

        protected readonly int maxVertexCount;
        /// <summary>
        /// Maximum number of vertices the polygon requires.
        /// </summary>
        public int MaxVertexCount { get { return this.maxVertexCount; } }

        protected readonly int maxIndexCount;
        /// <summary>
        /// Maximum number of indices the polygon requires.
        /// </summary>
        public int MaxIndexCount { get { return this.maxIndexCount; } }

        /// <summary>
        /// Number of vertices the polygon is currently using.
        /// </summary>
        public int VertexCount { get; protected set; }
        /// <summary>
        /// Number of indices the polygon is currently using.
        /// </summary>
        public int IndexCount { get; protected set; }

        /// <summary>
        /// Initializes a polygon object using the position and rotation
        /// </summary>
        /// <param name="maxVertexCount">Maximum number of vertices the polygon needs to render all styles</param>
        /// <param name="maxIndexCount">Maximum number of indices the polygon needs to render all styles</param>
        /// <param name="position">Initial position of the polygon</param>
        /// <param name="rotation">Initial rotation of the polygon</param>
        public Polygon(int maxVertexCount, int maxIndexCount, Vector3 position, Quaternion rotation)
        {
            // Initialize fields.
            this.maxVertexCount = maxVertexCount;
            this.maxIndexCount = maxIndexCount;
            this.position = position;
            this.rotation = rotation;

            // Update the transformation matrix.
            UpdateTransformationMatrix();
        }

        private void UpdateTransformationMatrix()
        {
            // Calculate the transformation matrix.
            this.TransformationMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, this.rotation, this.position);
        }

        /// <summary>
        /// Rebuilds the mesh buffer and clears the dirty state.
        /// </summary>
        /// <param name="vertexBuffer">Vertex buffer to write vertices to</param>
        /// <param name="indexBuffer">Index buffer to write vertex indices to</param>
        public abstract void BuildMesh(VertexStreamSplice<D3DColoredVertex> vertexBuffer, VertexStreamSplice<ushort> indexBuffer);

        public virtual void DrawFrame(RenderManager manager)
        {
            // Set the default raster state.
            if (this.Style == PolygonDrawStyle.Outline)
                manager.Device.ImmediateContext.Rasterizer.State = manager.RasterizerState;
            else
                manager.Device.ImmediateContext.Rasterizer.State = manager.NoCullingRasterizerState;
        }
    }
}
